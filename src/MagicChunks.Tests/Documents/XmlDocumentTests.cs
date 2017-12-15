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

            document.ReplaceKey(new[] { "xml", "a", "y" }, "2");
            document.ReplaceKey(new[] { "xml", "a", "@y" }, "3");
            document.ReplaceKey(new[] { "xml", "a", "z", "t", "w" }, "3");
            document.ReplaceKey(new[] { "xml", "b" }, "5");
            document.ReplaceKey(new[] { "xml", "c", "a" }, "1");
            document.ReplaceKey(new[] { "xml", "c", "b" }, "2");
            document.ReplaceKey(new[] { "xml", "c", "b", "t" }, "3");
            document.ReplaceKey(new[] { "xml", "e", "item[@key = 'item2']" }, "5");
            document.ReplaceKey(new[] { "xml", "e", "item[@key=\"item3\"]" }, "6");
            document.ReplaceKey(new[] { "xml", "f", "item[@key = 'item2']", "val" }, "7");
            document.ReplaceKey(new[] { "xml", "f", "item[@key=\"item3\"]", "val" }, "8");
            document.ReplaceKey(new[] { "xml", "d" }, "4");

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
        public void TransformByIndex()
        {
            // Arrange

            var document = new XmlDocument(@"<info>
    <param>
        <option>AA</option>
    </param>
    <param>
        <option>BB</option>
        <argument>CC</argument>
    </param>
    <param>
        <option>DD</option>
    </param>
</info>");


            // Act

            document.ReplaceKey(new[] { "info", "param[1]", "option" }, "DD");

            var result = document.ToString();


            // Assert

            Assert.Equal(@"<info>
    <param>
        <option>AA</option>
    </param>
    <param>
        <option>DD</option>
        <argument>CC</argument>
    </param>
    <param>
        <option>DD</option>
    </param>
</info>", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void TransformByIndex2()
        {
            // Arrange

            var document = new XmlDocument(@"<info>
    <val x=""1"">
        <param>
            <option>AA</option>
        </param>
        <param>
            <option>BB</option>
            <argument>CC</argument>
        </param>
        <param>
            <option>DD</option>
        </param>
    </val>
    <val x=""2"">
        <param>
            <option>AA</option>
        </param>
        <param>
            <option>BB</option>
            <argument>CC</argument>
        </param>
        <param>
            <option>DD</option>
        </param>
    </val>
    <val x=""1"">
        <param>
            <option>AA</option>
        </param>
        <param>
            <option>BB</option>
            <argument>CC</argument>
        </param>
        <param>
            <option>DD</option>
        </param>
    </val>
</info>");


            // Act

            document.ReplaceKey(new[] { "info", "val[@x=\"2\"]", "param[1]", "option" }, "DD");

            var result = document.ToString();


            // Assert

            Assert.Equal(@"<info>
    <val x=""1"">
        <param>
            <option>AA</option>
        </param>
        <param>
            <option>BB</option>
            <argument>CC</argument>
        </param>
        <param>
            <option>DD</option>
        </param>
    </val>
    <val x=""2"">
        <param>
            <option>AA</option>
        </param>
        <param>
            <option>DD</option>
            <argument>CC</argument>
        </param>
        <param>
            <option>DD</option>
        </param>
    </val>
    <val x=""1"">
        <param>
            <option>AA</option>
        </param>
        <param>
            <option>BB</option>
            <argument>CC</argument>
        </param>
        <param>
            <option>DD</option>
        </param>
    </val>
</info>", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void TransformWithXmlHeader()
        {
            // Arrange

            var document = new XmlDocument(@"<?xml version=""1.0"" encoding=""utf-8""?>
<foo>
</foo>");


            // Act

//            document.ReplaceKey(new[] { "foo/@value" }, "foo");

            var result = document.ToString();


            // Assert

            Assert.Equal(@"<?xml version=""1.0"" encoding=""utf-8""?>
<foo></foo>", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
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
</nlog>".Replace("\r", String.Empty).Replace("\n", String.Empty), result.Replace("\r", String.Empty).Replace("\n", String.Empty), ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void TransformWithNamesapce3()
        {
            // Arrange

            var document = new XmlDocument(@"<nlog xmlns=""http://www.nlog-project.org/schemas/NLog.xsd""
      xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <targets>
    <target name=""file"" xsi:type=""File"" xsi:type2=""12345"" />
    <xsi:element></xsi:element>
  </targets>
</nlog>");


            // Act

            document.ReplaceKey(new[] { "nlog", "targets", "target[@name='file']", "@xsi:type2" }, "val1");
            document.ReplaceKey(new[] { "nlog", "targets", "target[@xsi:type='File']" }, "val2");
            document.ReplaceKey(new[] { "nlog", "targets", "xsi:element" }, "val3");
            document.ReplaceKey(new[] { "nlog", "targets", "xsi:element", "@prop" }, "val4");

            var result = document.ToString();

            // Assert

            Assert.Equal(@"<nlog xmlns=""http://www.nlog-project.org/schemas/NLog.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <targets>
    <target name=""file"" xsi:type=""File"" xsi:type2=""val1"">val2</target>
    <xsi:element prop=""val4"">val3</xsi:element>
  </targets>
</nlog>", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void TransformWithNamesapce4()
        {
            // Arrange

            var document = new XmlDocument(@"<manifest xmlns:android=""http://schemas.android.com/apk/res/android""
  package=""com.myapp.name.here""
  android:installLocation=""auto""
  android:versionCode=""10000""
  android:versionName=""1"">
	<uses-sdk android:minSdkVersion=""18"" android:targetSdkVersion=""23"" />
	<uses-permission android:name=""android.permission.CHANGE_WIFI_STATE"" />
	<uses-permission android:name=""android.permission.ACCESS_WIFI_STATE"" />
	<application android:label=""Scan Plan 2 Test"" android:icon="""" android:theme="""" />
</manifest>");

            // Act

            document.ReplaceKey(new[] { "manifest", "@android:versionCode" }, "10001");

            var result = document.ToString();

            // Assert

            Assert.Equal(@"<manifest xmlns:android=""http://schemas.android.com/apk/res/android"" package=""com.myapp.name.here"" android:installLocation=""auto"" android:versionCode=""10001"" android:versionName=""1"">
  <uses-sdk android:minSdkVersion=""18"" android:targetSdkVersion=""23"" />
  <uses-permission android:name=""android.permission.CHANGE_WIFI_STATE"" />
  <uses-permission android:name=""android.permission.ACCESS_WIFI_STATE"" />
  <application android:label=""Scan Plan 2 Test"" android:icon="""" android:theme="""" />
</manifest>", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void AddElementToArray()
        {
            // Arrange

            var document = new XmlDocument(@"<xml>
<a>
  <x>1</x>
</a>
<b>2</b>
<c>3</c>
<d />
</xml>");


            // Act

            document.AddElementToArray(new[] { "xml", "d" }, "<val>1</val>");
            document.AddElementToArray(new[] { "xml", "d" }, "<val>2</val>");
            document.AddElementToArray(new[] { "xml", "f" }, "<x>1</x>");

            var result = document.ToString();


            // Assert

            Assert.Equal(@"<xml>
  <a>
    <x>1</x>
  </a>
  <b>2</b>
  <c>3</c>
  <d>
    <val>1</val>
    <val>2</val>
  </d>
  <f>
    <x>1</x>
  </f>
</xml>", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void Remove()
        {
            // Arrange

            var document = new XmlDocument(@"<xml>
    <a>
      <x>1</x>
    </a>
    <b>
      <x>1</x>
    </b>
    <c key=""item1"" foo=""bar"">3</c>
</xml>");


            // Act

            document.RemoveKey(new[] { "xml", "a" });
            document.RemoveKey(new[] { "xml", "b", "x" });
            document.RemoveKey(new[] { "xml", "c", "@key" });

            var result = document.ToString();


            // Assert

            Assert.Equal(@"<xml>
    <b />
    <c foo=""bar"">3</c>
</xml>", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void RemoveByIndex()
        {
            // Arrange

            var document = new XmlDocument(@"<info>
    <val x=""1"">
        <param>
            <option>AA</option>
        </param>
        <param>
            <option>BB</option>
            <argument>CC</argument>
        </param>
        <param>
            <option>DD</option>
        </param>
    </val>
    <val x=""2"">
        <param>
            <option>AA</option>
        </param>
        <param>
            <option>BB</option>
            <argument>CC</argument>
        </param>
        <param>
            <option>DD</option>
        </param>
    </val>
    <val x=""1"">
        <param>
            <option>AA</option>
        </param>
        <param>
            <option>BB</option>
            <argument>CC</argument>
        </param>
        <param>
            <option>DD</option>
        </param>
    </val>
</info>");


            // Act

            document.RemoveKey(new[] { "info", "val[@x=\"2\"]", "param[1]", "option" });
            document.RemoveKey(new[] { "info", "val[@x=\"1\"]", "param[2]" });

            var result = document.ToString();


            // Assert

            Assert.Equal(@"<info>
    <val x=""1"">
        <param>
            <option>AA</option>
        </param>
        <param>
            <option>BB</option>
            <argument>CC</argument>
        </param>
    </val>
    <val x=""2"">
        <param>
            <option>AA</option>
        </param>
        <param>
            <argument>CC</argument>
        </param>
        <param>
            <option>DD</option>
        </param>
    </val>
    <val x=""1"">
        <param>
            <option>AA</option>
        </param>
        <param>
            <option>BB</option>
            <argument>CC</argument>
        </param>
        <param>
            <option>DD</option>
        </param>
    </val>
</info>", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void ValidateEmptyPath()
        {
            // Arrange
            XmlDocument document = new XmlDocument("<xml/>");

            // Act
            ArgumentException result = Assert.Throws<ArgumentException>(() => document.ReplaceKey(new[] { "a", "", "b" }, ""));

            // Assert
            Assert.True(result.Message?.StartsWith("There is empty items in the path."));
        }

        [Fact]
        public void ValidateWhiteSpacePath()
        {
            // Arrange
            XmlDocument document = new XmlDocument("<xml/>");

            // Act
            ArgumentException result = Assert.Throws<ArgumentException>(() => document.ReplaceKey(new[] { "a", "   ", "b" }, ""));

            // Assert
            Assert.True(result.Message?.StartsWith("There is empty items in the path."));
        }

        [Fact]
        public void ValidateAmpersandsEscaping()
        {
            // Arrange
            XmlDocument document = new XmlDocument(@"<configuration>
  <connectionStrings>
    <add name=""Connection"" connectionString="""" />
  </connectionStrings>
</configuration>");

            // Act
            document.ReplaceKey(new [] { "configuration", "connectionStrings", "add[@name=\"Connection\"]", "@connectionString" }, @"metadata=res://*/Model.csdl|res://*/Model.ssdl|res://*/Model.msl;provider=System.Data.SqlClient;provider connection string=&quot;data source=other-server\instance;initial catalog=database;integrated security=True;multipleactiveresultsets=True;&quot;");

            var result = document.ToString();

            // Assert
            Assert.Equal(@"<configuration>
  <connectionStrings>
    <add name=""Connection"" connectionString=""metadata=res://*/Model.csdl|res://*/Model.ssdl|res://*/Model.msl;provider=System.Data.SqlClient;provider connection string=&quot;data source=other-server\instance;initial catalog=database;integrated security=True;multipleactiveresultsets=True;&quot;"" />
  </connectionStrings>
</configuration>", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void ValidateAmpersandsEscaping2()
        {
            // Arrange
            XmlDocument document = new XmlDocument(@"<configuration>
  <connectionStrings>
    <add name=""Connection"" connectionString="""" />
  </connectionStrings>
</configuration>");

            // Act
            document.ReplaceKey(new[] { "configuration", "connectionStrings", "add[@name=\"Connection\"]", "@connectionString" }, @"metadata=res://*/Model.csdl|res://*/Model.ssdl|res://*/Model.msl;provider=System.Data.SqlClient;provider connection string=""data source=other-server\instance;initial catalog=database;integrated security=True;multipleactiveresultsets=True;""");

            var result = document.ToString();

            // Assert1
            Assert.Equal(@"<configuration>
  <connectionStrings>
    <add name=""Connection"" connectionString=""metadata=res://*/Model.csdl|res://*/Model.ssdl|res://*/Model.msl;provider=System.Data.SqlClient;provider connection string=&quot;data source=other-server\instance;initial catalog=database;integrated security=True;multipleactiveresultsets=True;&quot;"" />
  </connectionStrings>
</configuration>", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void TransformWithColonInAttributeName()
        {
            // Arrange
            var document = new XmlDocument(@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <appSettings>
    <add key=""Auth:Environment"" value=""Testing"" />
    <add key=""Auth:Credential"" value=""abcefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"" />
    <add key=""Auth:Type"" value=""Basic"" />
  </appSettings>
</configuration>");
            
            document.ReplaceKey(new[] { "configuration", "appSettings", "add[@key = 'Auth:Environment']", "@value" }, "Production");
            document.ReplaceKey(new[] { "configuration", "appSettings", "add[@key=\"Auth:Credential\"]", "@value" }, "XqoL6MXqOPeGTxXrJsQA7gK5cXLvnlSYJUtAwFWbRo7GDkbsSu");
            document.ReplaceKey(new[] { "configuration", "appSettings", "add[@key=\"Auth:Type\"]", "@value" }, "Bearer");

            var result = document.ToString();


            // Assert
            Assert.Equal(@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <appSettings>
    <add key=""Auth:Environment"" value=""Production"" />
    <add key=""Auth:Credential"" value=""XqoL6MXqOPeGTxXrJsQA7gK5cXLvnlSYJUtAwFWbRo7GDkbsSu"" />
    <add key=""Auth:Type"" value=""Bearer"" />
  </appSettings>
</configuration>", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

    }
}