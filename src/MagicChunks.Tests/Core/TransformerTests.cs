using System;
using MagicChunks.Core;
using MagicChunks.Documents;
using Xunit;

namespace MagicChunks.Tests.Core
{
    public class TransformerTests
    {
        [Fact]
        public void TransformJson()
        {
            // Arrange

            var transform = new TransformationCollection()
            {
                { "a/y", "2" },
                { "a/z/t/w", "3" },
                { "b", "5" },
                { "c/a", "1" },
                { "c/b", "2" },
                { "c/b/t", "3" },
                { "d", "4" },
                { "#e", "" },
                { "f/items[]`1", "1" },
                { "f/items[]`2", "2" }
            };


            // Act

            var transformer = new Transformer();
            string result = transformer.Transform(new JsonDocument(@"{ 
    'a': { 
        'x': '1'
    },
    'b': '2',
    'c': '3',
    'e': '4'
}"), transform);


            // Assert

            Assert.Equal(@"{
  ""a"": {
    ""x"": ""1"",
    ""y"": ""2"",
    ""z"": {
      ""t"": {
        ""w"": ""3""
      }
    }
  },
  ""b"": ""5"",
  ""c"": {
    ""a"": ""1"",
    ""b"": {
      ""t"": ""3""
    }
  },
  ""d"": ""4"",
  ""f"": {
    ""items"": [
      ""1"",
      ""2""
    ]
  }
}", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void TransformJsonImplicit()
        {
            // Arrange

            var transform = new TransformationCollection("e")
            {
                { "a/y", "2" },
                { "a/z/t/w", "3" },
                { "b", "5" },
                { "c/a", "1" },
                { "c/b", "2" },
                { "c/b/t", "3" },
                { "d", "4" },
            };


            // Act

            var transformer = new Transformer();
            string result = transformer.Transform(@"{ 
    'a': { 
        'x': '1'
    },
    'b': '2',
    'c': '3',
    'e': '4'
}", transform);


            // Assert

            Assert.Equal(@"{
  ""a"": {
    ""x"": ""1"",
    ""y"": ""2"",
    ""z"": {
      ""t"": {
        ""w"": ""3""
      }
    }
  },
  ""b"": ""5"",
  ""c"": {
    ""a"": ""1"",
    ""b"": {
      ""t"": ""3""
    }
  },
  ""d"": ""4""
}", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void TransformXml()
        {
            // Arrange

            var transform = new TransformationCollection()
            {
                { "xml/a/y", "2" },
                { "xml/a/@y", "3" },
                { "xml/a/x/@q", "9" },
                { "xml/a/z/t/w", "3" },
                { "xml/b", "5" },
                { "xml/c/a", "1" },
                { "xml/c/b", "2" },
                { "xml/c/b/t", "3" },
                { "xml/e/item[@key = 'item2']", "5" },
                { "xml/e/item[@key=\"item3\"]", "6" },
                { "xml/f/item[@key = 'item2']/val", "7" },
                { "xml/f/item[@key=\"item3\"]/val", "8" },
                { "xml/d", "4" },
                { "#xml/g", "" },
                { "xml/items[]`1", "<val>1</val>" },
                { "xml/items[]`2", "<val>2</val>" },
                { "xml/data[]`1", "<x>1</x>" },
                { "xml/data[]`2", "<y>2</y>" },
            };


            // Act

            var transformer = new Transformer();
            string result = transformer.Transform(new XmlDocument(@"<xml>
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
<g>
    <x></x>
</g>
<items />
</xml>"), transform);


            // Assert

            Assert.Equal(@"<xml>
  <a y=""3"">
    <x q=""9"">1</x>
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
  <items>
    <val>1</val>
    <val>2</val>
  </items>
  <d>4</d>
  <data>
    <x>1</x>
    <y>2</y>
  </data>
</xml>", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void TransformXmlImplicit()
        {
            // Arrange

            var transform = new TransformationCollection("xml/g")
            {
                { "xml/a/y", "2" },
                { "xml/a/@y", "3" },
                { "xml/a/z/t/w", "3" },
                { "xml/b", "5" },
                { "xml/c/a", "1" },
                { "xml/c/b", "2" },
                { "xml/c/b/t", "3" },
                { "xml/e/item[@key = 'item2']", "5" },
                { "xml/e/item[@key=\"item3\"]", "6" },
                { "xml/f/item[@key = 'item2']/val", "7" },
                { "xml/f/item[@key=\"item3\"]/val", "8" },
                { "xml/d", "4" },
            };


            // Act

            var transformer = new Transformer();
            string result = transformer.Transform(@"<xml>
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
<g>
    <x></x>
</g>
</xml>", transform);


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
        public void TransformXmlWithNamespace()
        {
            // Arrange

            var transform = new TransformationCollection()
            {
                { "manifest/@android:versionCode", "10001" }
            };


            // Act

            var transformer = new Transformer();
            string result = transformer.Transform(new XmlDocument(@"<manifest xmlns:android=""http://schemas.android.com/apk/res/android""
  package=""com.myapp.name.here""
  android:installLocation=""auto""
  android:versionCode=""10000""
  android:versionName=""1"">
	<uses-sdk android:minSdkVersion=""18"" android:targetSdkVersion=""23"" />
	<uses-permission android:name=""android.permission.CHANGE_WIFI_STATE"" />
	<uses-permission android:name=""android.permission.ACCESS_WIFI_STATE"" />
	<application android:label=""Scan Plan 2 Test"" android:icon="""" android:theme="""" />
</manifest>"), transform);


            // Assert

            Assert.Equal(@"<manifest xmlns:android=""http://schemas.android.com/apk/res/android"" package=""com.myapp.name.here"" android:installLocation=""auto"" android:versionCode=""10001"" android:versionName=""1"">
  <uses-sdk android:minSdkVersion=""18"" android:targetSdkVersion=""23"" />
  <uses-permission android:name=""android.permission.CHANGE_WIFI_STATE"" />
  <uses-permission android:name=""android.permission.ACCESS_WIFI_STATE"" />
  <application android:label=""Scan Plan 2 Test"" android:icon="""" android:theme="""" />
</manifest>", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void TransformYaml()
        {
            // Arrange

            var transform = new TransformationCollection("e")
            {
                { "a/y", "2" },
                { "a/z/t/w", "3" },
                { "b", "5" },
                { "c/a", "1" },
                { "c/b", "2" },
                { "c/b/t", "3" },
                { "d", "4" },
                { "f[]`1", "1" },
                { "f[]`2", "2" }
            };


            // Act

            var transformer = new Transformer();
            string result = transformer.Transform(new YamlDocument(@"a:
    x: 1
b: 2
c: 3
e:
    x: 5"), transform);


            // Assert

            Assert.Equal(@"a:
  x: 1
  y: 2
  z:
    t:
      w: 3
b: 5
c:
  a: 1
  b:
    t: 3
d: 4
f:
- 1
- 2
", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void TransformYamlImplicit()
        {
            // Arrange

            var transform = new TransformationCollection("e")
            {
                { "a/y", "2" },
                { "a/z/t/w", "3" },
                { "b", "5" },
                { "c/a", "1" },
                { "c/b", "2" },
                { "c/b/t", "3" },
                { "d", "4" },
            };


            // Act

            var transformer = new Transformer();
            string result = transformer.Transform(@"a:
    x: 1
b: 2
c: 3
e:
    x: 5", transform);


            // Assert

            Assert.Equal(@"a:
  x: 1
  y: 2
  z:
    t:
      w: 3
b: 5
c:
  a: 1
  b:
    t: 3
d: 4
", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void RemoveNode()
        {
            // Arrange

            var transform = new TransformationCollection("e")
            {
                { "a/y", "2" },
                { "a/z/t/w", "3" },
                { "#b", "5" },
                { "c/a", "1" },
                { "c/b", "2" },
                { "c/b/t", "3" },
                { "d", "4" },
            };


            // Act

            var transformer = new Transformer();
            string result = transformer.Transform(new JsonDocument(@"{ 
    'a': { 
        'x': '1'
    },
    'b': '2',
    'c': '3',
    'e': '4'
}"), transform);


            // Assert

            Assert.Equal(@"{
  ""a"": {
    ""x"": ""1"",
    ""y"": ""2"",
    ""z"": {
      ""t"": {
        ""w"": ""3""
      }
    }
  },
  ""c"": {
    ""a"": ""1"",
    ""b"": {
      ""t"": ""3""
    }
  },
  ""d"": ""4""
}", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void ValidateTransformationKey()
        {// Arrange

            var transform = new TransformationCollection()
            {
                { "", "1" }
            };


            // Act

            var transformer = new Transformer();
            ArgumentException result = Assert.Throws<ArgumentException>(() => transformer.Transform(new JsonDocument(@"{ }"), transform));


            // Assert
            Assert.True(result.Message?.StartsWith("Transformation key is empty."));
        }

        [Fact]
        public void ValidateWrongDocumentType()
        {// Arrange

            var transform = new TransformationCollection()
            {
                { "x", "1" }
            };


            // Act

            var transformer = new Transformer();
            ArgumentException result = Assert.Throws<ArgumentException>(() => transformer.Transform(@"{ <xml> x:y </xml> }", transform));


            // Assert
            Assert.True(result.Message?.StartsWith("Unknown document type."));
        }
    }
}