using System;
using System.Collections.Generic;
using AudioProcessor;

namespace AudioVisualizer
{
  public class AudioProcessorHelper
  {
    private readonly string waterMark;
    private readonly Mode mode;

    public AudioProcessorHelper(string waterMark, Mode mode)
    {
      this.waterMark = waterMark;
      this.mode = mode;
    }

    public string Process(byte[] originalBytes, out byte[] markedBytesArray, out byte[] unWaterMarkedBytesArray, out int[] markedIndexes, out int[] unMarkedIndexes)
    {
      var processor = new Processor(originalBytes, mode);

      markedBytesArray = processor.GetWaterMarkedBytes(waterMark);
      unWaterMarkedBytesArray = processor.ExtractWaterMark(markedBytesArray);

      markedIndexes = processor.InseredWaterMarkIndexes.ToArray();
      unMarkedIndexes = processor.ExtractedWaterMarkIndexes.ToArray();

      var percentage = CalculateResult(processor.OriginalWaterMarkBits, processor.ExtractedWaterMarkBits);
      var percentageByIndexes = ResultByIndexes(markedIndexes, unMarkedIndexes);
      var extractedWaterMark = WaterMark.FromBitArray(processor.ExtractedWaterMarkBits);
      return string.Format("Identity: {0}% / {1}% -> {2}", percentage.ToString("F0"), percentageByIndexes.ToString("F0"), extractedWaterMark);
    }

    private double CalculateResult(byte[] originalWaterMarkBits, byte[] extractedWaterMarkBits)
    {
      var assertBitsCount = extractedWaterMarkBits.Length;
      var persentage = 0.0;

      for (var i = 0; i < assertBitsCount; i++)
      {
        if (extractedWaterMarkBits[i%extractedWaterMarkBits.Length] ==
            originalWaterMarkBits[i%originalWaterMarkBits.Length])
        {
          persentage++;
        }
      }

      persentage /= assertBitsCount;

      return persentage*100;
    }

    private double ResultByIndexes(int[] markedIndexes, int[] unMarkedIndexes)
    {
      var count = Math.Min(markedIndexes.Length, unMarkedIndexes.Length);
      var result = 0.0;
      for (int i = 0; i < count; i++)
      {
        if (markedIndexes[i] == unMarkedIndexes[i])
        {
          result++;
        }
      }
      result /= count;
      result *= 100;
      return result;
    }
  }
}
