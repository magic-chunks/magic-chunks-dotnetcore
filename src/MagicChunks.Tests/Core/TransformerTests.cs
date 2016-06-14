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
            };


            // Act

            var transformer = new Transformer();
            string result = transformer.Transform(new JsonDocument(@"{ 
    'a': { 
        'x': '1'
    },
    'b': '2',
    'c': '3'
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
  ""d"": ""4""
}", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void TransformJsonImplicit()
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
            };


            // Act

            var transformer = new Transformer();
            string result = transformer.Transform(@"{ 
    'a': { 
        'x': '1'
    },
    'b': '2',
    'c': '3'
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
</xml>"), transform);


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
        public void TransformXmlImplicit()
        {
            // Arrange

            var transform = new TransformationCollection()
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
        public void TransformYaml()
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
            };


            // Act

            var transformer = new Transformer();
            string result = transformer.Transform(new YamlDocument(@"a:
    x: 1
b: 2
c: 3"), transform);


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
        public void TransformYamlImplicit()
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
            };


            // Act

            var transformer = new Transformer();
            string result = transformer.Transform(@"a:
    x: 1
b: 2
c: 3", transform);


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
            Assert.True(result.Message?.StartsWith("Wrong transformation key."));
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