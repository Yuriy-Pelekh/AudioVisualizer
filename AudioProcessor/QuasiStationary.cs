using System;
using System.Collections.Generic;
using System.Linq;

namespace AudioProcessor
{
  public sealed class QuasiStationary
  {
    private readonly List<byte[]> segments;
    private readonly double epsilon;

    private QuasiStationary(double epsilon)
    {
      this.epsilon = epsilon;
      segments = new List<byte[]>();
    }

    public QuasiStationary(byte[] vector, int segmentLength, double epsilon)
      : this(epsilon)
    {
      for (var i = 0; i + segmentLength < vector.Length; i += segmentLength)
      {
        segments.Add(vector.Skip(i).Take(segmentLength).ToArray());
      }
    }

    /// <summary>
    /// Returns start indexes of quasistationary areas.
    /// </summary>
    /// <returns></returns>
    public List<int> Split()
    {
      var quasiIndexes = new List<int> {0};

      for (var i = 1; i < segments.Count; i++)
      {
        var quasiSegmentValue = GetQuasiSegmentValue(segments[quasiIndexes.Last()], segments[i]);
        if (quasiSegmentValue > epsilon)
        {
          quasiIndexes.Add(i);
        }
      }

      return quasiIndexes;
    }

    private double GetQuasiSegmentValue(byte[] segment1, byte[] segment2)
    {
      if (segment1.Length != segment2.Length)
      {
        throw new ArgumentException("Segments should be the same length.");
      }
      return Math.Abs(Math.Sqrt(segment1.Select((item, index) => (Math.Pow(Math.Abs(item - segment2[index]), 2))).Sum()));
    }
  }
}
