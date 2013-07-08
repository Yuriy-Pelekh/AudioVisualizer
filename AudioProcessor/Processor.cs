using System;
using System.Collections.Generic;
using System.Linq;

namespace AudioProcessor
{
  public class Processor
  {
    private const int SampleLength = 2;
    private const int ChunkLength = 512;
    private const int Shift = SampleLength*ChunkLength;
    private readonly byte[] originalBytes;
    private const int HeaderSize = 44;
    private const double QuasiEpsilon = 3500.0;
    private readonly Mode mode;

    #region TestData

    public byte[] OriginalWaterMarkBits { get; private set; }
    public byte[] ExtractedWaterMarkBits { get; private set; }

    public List<int> InseredWaterMarkIndexes { get; private set; }
    public List<int> ExtractedWaterMarkIndexes { get; private set; }

    #endregion

    public Processor(byte[] bytes, Mode mode)
    {
      this.mode = mode;
      originalBytes = new byte[bytes.Length];
      bytes.CopyTo(originalBytes, 0);

      InseredWaterMarkIndexes = new List<int>();
      ExtractedWaterMarkIndexes = new List<int>();
    }

    public byte[] GetWaterMarkedBytes(string waterMark)
    {
      InseredWaterMarkIndexes.Clear();

      var waterMarkedBytes = new byte[originalBytes.Length];
      originalBytes.CopyTo(waterMarkedBytes, 0);

      var waterMarkBits = WaterMark.FromString(waterMark);
      OriginalWaterMarkBits = waterMarkBits;
      var waterMarkBitIndex = 0;

      if (mode == Mode.Direct)
      {
        for (var index = HeaderSize + 4; index + Shift < waterMarkedBytes.Length; index += Shift)
        {
          var startIndex = index;
          var endIndex = index + Shift;

          var maxAmplitudeIndex = FindMaxAmplitudeIndex(waterMarkedBytes, startIndex, endIndex);

          InseredWaterMarkIndexes.Add(maxAmplitudeIndex);
          SetMark(waterMarkedBytes, waterMarkBits[waterMarkBitIndex], maxAmplitudeIndex);

          waterMarkBitIndex = waterMarkBitIndex < waterMarkBits.Length - 1
                                ? waterMarkBitIndex + 1
                                : 0;
        }
      }
      else if (mode == Mode.Quasi)
      {
        var quasi = new QuasiStationary(waterMarkedBytes.Skip(HeaderSize).Take(waterMarkedBytes.Length - HeaderSize).ToArray(), Shift, QuasiEpsilon);
        var quasiIndexes = quasi.Split();

        for (var i = 0; i < quasiIndexes.Count; i++)
        {
          var startIndex = quasiIndexes[i] * Shift;
          var endIndex = (i + 1) < quasiIndexes.Count
                           ? quasiIndexes[i + 1] * Shift - 1 // Start index of next quasi area * segment length - 1
                           : waterMarkedBytes.Length - 1; // End of signal

          var maxAmplitudeIndex = FindMaxAmplitudeIndex(waterMarkedBytes, startIndex, endIndex);

          InseredWaterMarkIndexes.Add(maxAmplitudeIndex);
          SetMark(waterMarkedBytes, waterMarkBits[waterMarkBitIndex], maxAmplitudeIndex);

          waterMarkBitIndex = waterMarkBitIndex < waterMarkBits.Length - 1
                                ? waterMarkBitIndex + 1
                                : 0;
        }
      }
      else
      {
        throw new NotImplementedException();
      }

      return waterMarkedBytes;
    }

    public byte[] ExtractWaterMark(byte[] waterMarkedBytes)
    {
      ExtractedWaterMarkIndexes.Clear();

      var unwaterMarkedBytes = new byte[waterMarkedBytes.Length];
      waterMarkedBytes.CopyTo(unwaterMarkedBytes, 0);

      var waterMarkBits = new List<byte>();

      if (mode == Mode.Direct)
      {
        for (var index = HeaderSize + 4; index + Shift < unwaterMarkedBytes.Length; index += Shift)
        {
          var startIndex = index;
          var endIndex = index + Shift;

          var maxAmplitudeIndex = FindMaxAmplitudeIndex(unwaterMarkedBytes, startIndex, endIndex);

          ExtractedWaterMarkIndexes.Add(maxAmplitudeIndex);
          waterMarkBits.Add(GetMark(unwaterMarkedBytes, maxAmplitudeIndex));
        }
      }
      else if (mode == Mode.Quasi)
      {
        var quasi = new QuasiStationary(waterMarkedBytes.Skip(HeaderSize).Take(waterMarkedBytes.Length - HeaderSize).ToArray(), Shift, QuasiEpsilon);
        var quasiIndexes = quasi.Split();

        for (var i = 0; i < quasiIndexes.Count; i++)
        {
          var startIndex = quasiIndexes[i] * Shift;
          var endIndex = (i + 1) < quasiIndexes.Count
                           ? quasiIndexes[i + 1] * Shift - 1 // Start index of next quasi area * segment length - 1
                           : waterMarkedBytes.Length - 1; // End of signal

          var maxAmplitudeIndex = FindMaxAmplitudeIndex(waterMarkedBytes, startIndex, endIndex);

          ExtractedWaterMarkIndexes.Add(maxAmplitudeIndex);
          waterMarkBits.Add(GetMark(unwaterMarkedBytes, maxAmplitudeIndex));
        }
      }
      else
      {
        throw new NotImplementedException();
      }
     
      ExtractedWaterMarkBits = waterMarkBits.ToArray();

      return unwaterMarkedBytes;
    }

    private int FindMaxAmplitudeIndex(byte[] bytes, int startIndex, int endIndex)
    {
      var maxAmplitude = Math.Abs(bytes[3*SampleLength] - bytes[4*SampleLength]);
      var maxAmplitudeIndex = startIndex + 3 * SampleLength;

      for (var i = startIndex + 3*SampleLength; i < endIndex - 3*SampleLength; i += SampleLength)
      {
        var currentAmplitude = Math.Abs(bytes[i] - bytes[i + SampleLength]);

        if (currentAmplitude > maxAmplitude)
        {
          maxAmplitude = currentAmplitude;
          maxAmplitudeIndex = i;
        }
      }

      return maxAmplitudeIndex;
    }

    private void SetMark(byte[] bytes, byte mark, int markIndex)
    {
      if (mark > 1)
      {
        throw new ArgumentException("Mark byte should be in a range [0, 1]");
      }

      bytes[markIndex] ^= mark;

      if (mark == 0)
      {
        bytes[markIndex - 1*SampleLength] &= 0xFE;
        bytes[markIndex - 2*SampleLength] &= 0xFE;
        bytes[markIndex - 3*SampleLength] &= 0xFE;
      }
      else
      {
        bytes[markIndex + 1*SampleLength] &= 0xFE;
        bytes[markIndex + 2*SampleLength] &= 0xFE;
        bytes[markIndex + 3*SampleLength] &= 0xFE;
      }
    }

    private byte GetMark(byte[] bytes, int markIndex)
    {
      var leftSum = (bytes[markIndex - 1*SampleLength] & 0x01) +
                    (bytes[markIndex - 2*SampleLength] & 0x01) +
                    (bytes[markIndex - 3*SampleLength] & 0x01);
      var rightSum = (bytes[markIndex + 1*SampleLength] & 0x01) +
                     (bytes[markIndex + 2*SampleLength] & 0x01) +
                     (bytes[markIndex + 3*SampleLength] & 0x01);

      var markBit = (byte) (rightSum > leftSum ? 0 : 1);

      bytes[markIndex] ^= markBit;

      return markBit;
    }
  }
}
