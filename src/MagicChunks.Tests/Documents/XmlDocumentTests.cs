using System;
using System.Text.RegularExpressions;
using MagicChunks.Documents;
using Xunit;

namespace MagicChunks.Tests.Documents
{
    public class XmlDocumentTests
    {
        [Fact]
        public void Transform()
        {
            // Arrange

            var document = new XmlDocument(@"<xml>
<a>
  <x>1</x>
</a>
<b>2</b>
<c>3</c>
<e>
  <item key=""item1"">1</item>
  <item key=""item2"">2</item>
  <item key=""item3"">3</item>
</e>
<f>
  <item key=""item1"">
    <val>1</val>
  </item>
  <item key=""item2"">
    <val>2</val>
  </item>
  <item key=""item3"">
    <val>3</val>
  </item>
</f>
</xml>");


            // Act

            document.ReplaceKey(new[] {"xml", "a", "y"}, "2");
            document.ReplaceKey(new[] {"xml", "a", "@y"}, "3");
            document.ReplaceKey(new[] {"xml", "a", "z", "t", "w"}, "3");
            document.ReplaceKey(new[] {"xml", "b"}, "5");
            document.ReplaceKey(new[] {"xml", "c", "a"}, "1");
            document.ReplaceKey(new[] {"xml", "c", "b"}, "2");
            document.ReplaceKey(new[] {"xml", "c", "b", "t"}, "3");
            document.ReplaceKey(new[] {"xml", "e", "item[@key = 'item2']" }, "5");
            document.ReplaceKey(new[] {"xml", "e", "item[@key=\"item3\"]" }, "6");
            document.ReplaceKey(new[] {"xml", "f", "item[@key = 'item2']", "val"}, "7");
            document.ReplaceKey(new[] {"xml", "f", "item[@key=\"item3\"]", "val"}, "8");
            document.ReplaceKey(new[] {"xml", "d"}, "4");

            var result = document.ToString();


            // Assert

            Assert.Equal(@"<xml>
  <a y=""3"">
    <x>1</x>
    <y>2</y>
    <z>
      <t>
        <w>3</w>
      </t>
    </z>
  </a>
  <b>5</b>
  <c>
    <a>1</a>
    <b>
      <t>3</t>
    </b>
  </c>
  <e>
    <item key=""item1"">1</item>
    <item key=""item2"">5</item>
    <item key=""item3"">6</item>
  </e>
  <f>
    <item key=""item1"">
      <val>1</val>
    </item>
    <item key=""item2"">
      <val>7</val>
    </item>
    <item key=""item3"">
      <val>8</val>
    </item>
  </f>
  <d>4</d>
</xml>", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void TransformWithNamesapce()
        {
            // Arrange

            var document = new XmlDocument(@"<html xmlns=""http://www.w3.org/1999/xhtml"">
  <head></head>
  <BODY>
    <p></p>
    <DIV id=""d1""></DIV>
    <div id=""d2""><P></P></div>
  </BODY>
</html>");


            // Act

            document.ReplaceKey(new[] {"html", "body", "p"}, "Text");
            document.ReplaceKey(new[] {"html", "body", "div[@id='d1']"}, "Div");
            document.ReplaceKey(new[] {"html", "body", "div[@id='d2']", "p"}, "Text2");

            var result = document.ToString();


            // Assert

            Assert.Equal(@"<html xmlns=""http://www.w3.org/1999/xhtml"">
  <head></head>
  <BODY>
    <p>Text</p>
    <DIV id=""d1"">Div</DIV>
    <div id=""d2"">
      <P>Text2</P>
    </div>
  </BODY>
</html>", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void TransformWithNamesapce2()
        {
            // Arrange

            var document = new XmlDocument(@"<nlog xmlns=""http://www.nlog-project.org/schemas/NLog.xsd""
      xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
      autoReload=""true"">
  <targets>
    <target name=""file"" xsi:type=""File""
        layout=""${longdate} ${message} ${exception:format=tostring}""
        filename=""${basedir}\logs\${date:format=yyyy-MM-dd}.txt""
        archiveFileName=""${basedir}\logs\${date:format=yyyy-MM-dd}.{#}.txt""
        archiveEvery=""Day""
        archiveAboveSize=""512000""
        archiveNumbering=""Rolling""
        maxArchiveFiles=""15""
        keepFileOpen=""false"" />
  </targets>
  <rules>
    <logger name=""*"" minlevel=""Info"" writeTo=""file"" />
  </rules>
</nlog>");


            // Act

            document.ReplaceKey(new[] { "nlog", "targets", "target[@name='file']", "@filename"}, "$(logspath)\\${date:format=yyyy-MM-dd}.txt");
            document.ReplaceKey(new[] { "nlog", "targets", "target[@name='file']", "@archiveFileName"}, "$(logspath)\\${date:format=yyyy-MM-dd}.{#}.txt");
            document.ReplaceKey(new[] { "nlog", "rules", "logger[@name='*']", "@minlevel"}, "$(loglevel)");

            var result = document.ToString();

            // Assert

            Assert.Equal(@"<nlog xmlns=""http://www.nlog-project.org/schemas/NLog.xsd""
      xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
      autoReload=""true"">
  <targets>
    <target name=""file"" xsi:type=""File""
        layout=""${longdate} ${message} ${exception:format=tostring}""
        filename=""$(logspath)\${date:format=yyyy-MM-dd}.txt""
        archiveFileName=""$(logspath)\${date:format=yyyy-MM-dd}.{#}.txt""
        archiveEvery=""Day""
        archiveAboveSize=""512000""
        archiveNumbering=""Rolling""
        maxArchiveFiles=""15""
        keepFileOpen=""false"" />
  </targets>
  <rules>
    <logger name=""*"" minlevel=""$(loglevel)"" writeTo=""file"" />
  </rules>
</nlog>".Replace(Environment.NewLine, String.Empty), result.Replace(Environment.NewLine, String.Empty), ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void ValidateEmptyPath()
        {
            // Assert
            XmlDocument document = new XmlDocument("<xml/>");

            // Act
            ArgumentException result = Assert.Throws<ArgumentException>(() => document.ReplaceKey(new[] { "a", "", "b" }, ""));

            // Arrange
            Assert.True(result.Message?.StartsWith("There is empty items in the path."));
        }

        [Fact]
        public void ValidateWithespacePath()
        {
            // Assert
            XmlDocument document = new XmlDocument("<xml/>");

            // Act
            ArgumentException result = Assert.Throws<ArgumentException>(() => document.ReplaceKey(new[] { "a", "   ", "b" }, ""));

            // Arrange
            Assert.True(result.Message?.StartsWith("There is empty items in the path."));
        }
    }
}