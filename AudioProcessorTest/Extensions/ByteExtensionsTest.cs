using AudioProcessor.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AudioProcessorTest.Extensions
{
  [TestClass]
  public class ByteExtensionsTest
  {
    [TestMethod]
    public void CheckBitTest1()
    {
      // Arrange
      const byte target = 1;
      const byte expectedValue = 1;
      const byte bitPositionToCheck = 0;
      
      // Act
      byte actualValue = target.CheckBit(bitPositionToCheck);

      // Assert
      Assert.AreEqual(expectedValue, actualValue);
    }

    [TestMethod]
    public void CheckBitTest2()
    {
      // Arrange
      const byte target = 1;
      const byte expectedValue = 0;
      const byte bitPositionToCheck = 1;

      // Act
      byte actualValue = target.CheckBit(bitPositionToCheck);

      // Assert
      Assert.AreEqual(expectedValue, actualValue);
    }
  }
}
