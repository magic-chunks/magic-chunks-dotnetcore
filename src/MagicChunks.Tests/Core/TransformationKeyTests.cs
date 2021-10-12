using System.Linq;
using MagicChunks.Core;
using Xunit;

namespace MagicChunks.Tests.Core
{
    public class TransformationKeyTests
    {
        [InlineData("xml", TransformationKeyType.Replace, new [] { "xml" })]
        [InlineData("xml/a/y", TransformationKeyType.Replace, new [] { "xml", "a", "y" })]
        [InlineData("xml/a/@y", TransformationKeyType.Replace, new [] { "xml", "a", "@y" })]
        [InlineData("xml/a[1]", TransformationKeyType.Replace, new [] { "xml", "a[1]" })]
        [InlineData("xml/b[@key=\"item\"]", TransformationKeyType.Replace, new[] { "xml", "b[@key=\"item\"]" })]
        [InlineData("xml/b[@key='item/subsetting']/val", TransformationKeyType.Replace, new[] { "xml", "b[@key='item/subsetting']", "val" })]
        [InlineData("items[@i='a/b']/data", TransformationKeyType.Replace, new[] { "items[@i='a/b']", "data" })]
        [InlineData("#xml/a", TransformationKeyType.Remove, new [] { "xml", "a" })]
        [InlineData("xml/items[]`1", TransformationKeyType.AddToArray, new[] { "xml", "items" })]
        [InlineData("xml/items[]`2", TransformationKeyType.AddToArray, new[] { "xml", "items" })]
        [Theory]
        public void Parse(string key, TransformationKeyType type, string[] path)
        {
            // Act

            var transformationKey = new TransformationKey(key);

            // Assert

            Assert.Equal(type, transformationKey.Type);
            Assert.Equal(path.AsEnumerable(), transformationKey.Path);
        }
    }
}