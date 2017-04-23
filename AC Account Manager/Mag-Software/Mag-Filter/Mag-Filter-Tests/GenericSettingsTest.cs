using System;
using System.Collections.Generic;
using System.Text;


namespace Mag_Filter_Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class SettingsLineParserTest
    {
        [Test]
        public void SingleEmptyValue()
        {
            // Arrange
            var line = "Debug";

            // Act
            var sut = new GenericSettingsFile.SettingsLineParser();
            var actual = sut.ExtractLine(line);

            // Assert
            Assert.AreEqual("Debug", actual.Name);
            Assert.AreEqual(true, actual.HasSingleValue);
            Assert.AreEqual(false, actual.HasParameters);
            Assert.AreEqual(0, actual.Parameters.Count);
            Assert.AreEqual("", actual.SingleValue);
        }
        [Test]
        public void SingleValue()
        {
            // Arrange
            var line = "Color:brown";

            // Act
            var sut = new GenericSettingsFile.SettingsLineParser();
            var actual = sut.ExtractLine(line);

            // Assert
            Assert.AreEqual("Color", actual.Name);
            Assert.AreEqual(true, actual.HasSingleValue);
            Assert.AreEqual(false, actual.HasParameters);
            Assert.AreEqual(0, actual.Parameters.Count);
            Assert.AreEqual("brown", actual.SingleValue);
        }
        [Test]
        public void SingleParam()
        {
            // Arrange
            var line = "Colors=foreground:red";

            // Act
            var sut = new GenericSettingsFile.SettingsLineParser();
            var actual = sut.ExtractLine(line);

            // Assert
            Assert.AreEqual("Colors", actual.Name);
            Assert.AreEqual(false, actual.HasSingleValue);
            Assert.AreEqual(true, actual.HasParameters);
            Assert.AreEqual(1, actual.Parameters.Count);
        }
        [Test]
        public void SingleParamWithSingleQuotes()
        {
            // Arrange
            var line = "Colors=foreground:'red'";

            // Act
            var sut = new GenericSettingsFile.SettingsLineParser();
            var actual = sut.ExtractLine(line);

            // Assert
            Assert.AreEqual("Colors", actual.Name);
            Assert.AreEqual(false, actual.HasSingleValue);
            Assert.AreEqual(true, actual.HasParameters);
            Assert.AreEqual(1, actual.Parameters.Count);
        }
        [Test]
        public void SingleParamWithDoubleQuotes()
        {
            // Arrange
            var line = "Colors=foreground:\"red\"";

            // Act
            var sut = new GenericSettingsFile.SettingsLineParser();
            var actual = sut.ExtractLine(line);

            // Assert
            Assert.AreEqual("Colors", actual.Name);
            Assert.AreEqual(false, actual.HasSingleValue);
            Assert.AreEqual(true, actual.HasParameters);
            Assert.AreEqual(1, actual.Parameters.Count);
        }
        [Test]
        public void TwoParams()
        {
            // Arrange
            var line = "Colors=foreground:red background:blue";

            // Act
            var sut = new GenericSettingsFile.SettingsLineParser();
            var actual = sut.ExtractLine(line);

            // Assert
            Assert.AreEqual("Colors", actual.Name);
            Assert.AreEqual(false, actual.HasSingleValue);
            Assert.AreEqual(true, actual.HasParameters);
            Assert.AreEqual(2, actual.Parameters.Count);
            Assert.AreEqual(true, actual.Parameters.ContainsKey("foreground"));
            Assert.AreEqual("red", actual.GetStringParam("foreground"));
            Assert.AreEqual(true, actual.Parameters.ContainsKey("background"));
            Assert.AreEqual("blue", actual.GetStringParam("background"));
        }
        [Test]
        public void TwoParamsExtraSpaces()
        {
            // Arrange
            var line = "Colors=foreground:red     background:blue  ";

            // Act
            var sut = new GenericSettingsFile.SettingsLineParser();
            var actual = sut.ExtractLine(line);

            // Assert
            Assert.AreEqual("Colors", actual.Name);
            Assert.AreEqual(false, actual.HasSingleValue);
            Assert.AreEqual(true, actual.HasParameters);
            Assert.AreEqual(2, actual.Parameters.Count);
            Assert.AreEqual(true, actual.Parameters.ContainsKey("foreground"));
            Assert.AreEqual("red", actual.GetStringParam("foreground"));
            Assert.AreEqual(true, actual.Parameters.ContainsKey("background"));
            Assert.AreEqual("blue", actual.GetStringParam("background"));
        }
        [Test]
        public void LargeMultipleParams()
        {
            // Arrange
            var line = "G195=Out:LED0799,LED0814,Flags:L-N Desc:\"EAF-QCH-B1-01\" Invert:00 STO:35 SP:0 FStart: FStop: ";

            // Act
            var sut = new GenericSettingsFile.SettingsLineParser();
            GenericSettingsFile.Setting actual = sut.ExtractLine(line);

            // Assert
            Assert.AreEqual("G195", actual.Name);
            Assert.AreEqual(false, actual.HasSingleValue);
            Assert.AreEqual(true, actual.HasParameters);
            Assert.AreEqual("LED0799,LED0814,Flags:L-N", actual.GetStringParam("Out"));
            Assert.AreEqual("EAF-QCH-B1-01", actual.GetStringParam("Desc"));
            Assert.AreEqual("00", actual.GetStringParam("Invert"));
            Assert.AreEqual("35", actual.GetStringParam("STO"));
            Assert.AreEqual("0", actual.GetStringParam("SP"));
            Assert.AreEqual("", actual.GetStringParam("FStart"));
            Assert.AreEqual("", actual.GetStringParam("FStop"));
        }

    }
}
