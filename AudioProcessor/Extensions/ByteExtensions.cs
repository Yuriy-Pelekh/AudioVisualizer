using System;

namespace AudioProcessor.Extensions
{
  public static class ByteExtensions
  {
    /// <summary>
    /// Verifies bit value at specific position in a byte.
    /// </summary>
    /// <param name="b">Current byte.</param>
    /// <param name="position">Bit to check.</param>
    /// <returns>A bit value at a specific position.</returns>
    public static byte CheckBit(this byte b, byte position)
    {
      switch (position)
      {
        case 0:
          return (byte)(b & 0x01);
        case 1:
          return (byte)((b & 0x02) >> 1);
        case 2:
          return (byte)((b & 0x04) >> 2);
        case 3:
          return (byte)((b & 0x08) >> 3);
        case 4:
          return (byte)((b & 0x10) >> 4);
        case 5:
          return (byte)((b & 0x20) >> 5);
        case 6:
          return (byte)((b & 0x40) >> 6);
        case 7:
          return (byte)((b & 0x80) >> 7);
        default:
          throw new ArgumentException("Value should be in range [0..7]", "position");
      }
    }

    /// <summary>
    /// Set specific bit into 1.
    /// </summary>
    /// <param name="b">Current bite to edit.</param>
    /// <param name="position">Bit position to set.</param>
    /// <returns>Edited byte.</returns>
    public static byte SetBit(this byte b, byte position)
    {
      switch (position)
      {
        case 0:
          return (byte) (b | 0x01);
        case 1:
          return (byte) (b | 0x02);
        case 2:
          return (byte) (b | 0x04);
        case 3:
          return (byte) (b | 0x08);
        case 4:
          return (byte) (b | 0x10);
        case 5:
          return (byte) (b | 0x20);
        case 6:
          return (byte) (b | 0x40);
        case 7:
          return (byte) (b | 0x80);
        default:
          throw new ArgumentException("Value should be in range [0..7]", "position");
      }
    }
  }
}
