using System;
using MagicChunks.Documents;
using Xunit;

namespace MagicChunks.Tests.Documents
{
    public class JsonDocumentTests
    {
        [Fact]
        public void Transform()
        {
            // Arrange

            var document = new JsonDocument(@"{ 
    'a': { 
        'x': '1'
    },
    'b': '2',
    'c': '3'
}");


            // Act

            document.ReplaceKey(new[] {"A", "y"}, "2");
            document.ReplaceKey(new[] {"a", "z", "t", "w"}, "3");
            document.ReplaceKey(new[] {"B"}, "5");
            document.ReplaceKey(new[] {"c", "a"}, "1");
            document.ReplaceKey(new[] {"c", "b"}, "2");
            document.ReplaceKey(new[] {"c", "b", "t"}, "3");
            document.ReplaceKey(new[] {"d"}, "4");

            var result = document.ToString();


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
        public void Transform2()
        {
            // Arrange

            var document = new JsonDocument(@"{
  ""Data"": {
    ""DefaultConnection"": {
      ""ConnectionString"": ""mongodb://"",
      ""DatabaseName"": ""test1""
    }
  },

  ""Logging"": {
    ""IncludeScopes"": false,
    ""LogLevel"": {
      ""Default"": ""Verbose"",
      ""System"": ""Information"",
      ""Microsoft"": ""Information""
    }
  },

  ""Smtp"": {
    ""Method"": ""SpecifiedPickupDirectory"",

    ""From"": {
      ""Address"": ""test1@gmail.com"",
      ""DisplayName"": ""test""
    },

    ""SpecifiedPickupDirectory"": {
      ""PickupDirectoryLocation"": ""test\\maildrop\\"",
      ""AbsolutePath"": false
    },

    ""Network"": {
      ""Host"": ""smtp.gmail.com"",
      ""Port"": 587,
      ""Timeout"": 3000,
      ""EnableSsl"": true,
      ""Credentials"": {
        ""Username"": ""test@gmail.com"",
        ""Password"": ""asdasdasd""
      }
    }

  }
}");


            // Act

            document.ReplaceKey(new[] { "Data", "DefaultConnection", "ConnectionString" }, "mongodb://server1.local.db1.test");
            document.ReplaceKey(new[] { "Smtp", "Network", "Host" }, "test.ru");

            var result = document.ToString();


            // Assert

            Assert.Equal(@"{
  ""Data"": {
    ""DefaultConnection"": {
      ""ConnectionString"": ""mongodb://server1.local.db1.test"",
      ""DatabaseName"": ""test1""
    }
  },
  ""Logging"": {
    ""IncludeScopes"": false,
    ""LogLevel"": {
      ""Default"": ""Verbose"",
      ""System"": ""Information"",
      ""Microsoft"": ""Information""
    }
  },
  ""Smtp"": {
    ""Method"": ""SpecifiedPickupDirectory"",
    ""From"": {
      ""Address"": ""test1@gmail.com"",
      ""DisplayName"": ""test""
    },
    ""SpecifiedPickupDirectory"": {
      ""PickupDirectoryLocation"": ""test\\maildrop\\"",
      ""AbsolutePath"": false
    },
    ""Network"": {
      ""Host"": ""test.ru"",
      ""Port"": 587,
      ""Timeout"": 3000,
      ""EnableSsl"": true,
      ""Credentials"": {
        ""Username"": ""test@gmail.com"",
        ""Password"": ""asdasdasd""
      }
    }
  }
}", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void Transform3()
        {
            // Arrange

            var document = new JsonDocument(@"{
  ""bindings"": [
    {
      ""type"": ""httpTrigger"",
      ""direction"": ""in"",
      ""webHookType"": ""genericJson"",
      ""name"": ""req""
    },
    {
      ""type"": ""http"",
      ""direction"": ""out"",
      ""name"": ""res""
    },
    {
      ""type"": ""blob"",
      ""name"": ""name2"",
      ""path"": ""path2"",
      ""connection"": ""connection_storage"",
      ""direction"": ""in""
    },
    {
      ""type"": ""blob"",
      ""name"": ""name2"",
      ""path"": ""path2"",
      ""connection"": ""connection_storage"",
      ""direction"": ""in""
    }
  ],
  ""disabled"": false
}");


            // Act

            document.ReplaceKey(new[] { "bindings[]", "connection" }, "test");

            var result = document.ToString();


            // Assert

            Assert.Equal(@"{
  ""bindings"": [
    {
      ""type"": ""httpTrigger"",
      ""direction"": ""in"",
      ""webHookType"": ""genericJson"",
      ""name"": ""req"",
      ""connection"": ""test""
    },
    {
      ""type"": ""http"",
      ""direction"": ""out"",
      ""name"": ""res"",
      ""connection"": ""test""
    },
    {
      ""type"": ""blob"",
      ""name"": ""name2"",
      ""path"": ""path2"",
      ""connection"": ""test"",
      ""direction"": ""in""
    },
    {
      ""type"": ""blob"",
      ""name"": ""name2"",
      ""path"": ""path2"",
      ""connection"": ""test"",
      ""direction"": ""in""
    }
  ],
  ""disabled"": false
}", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void TransformByIndex()
        {
            // Arrange

            var document = new JsonDocument(@"{ 
    'items': [
        { 'i': 0, data: 'AA' },
        { 'i': 1, data: 'BB' },
        { 'i': 2, data: 'CC' },
        { 'i': 3, data: 'DD' },
    ],
    'b': '2',
    'c': '3'
}");


            // Act

            document.ReplaceKey(new[] { "items[2]", "data" }, "AA");

            var result = document.ToString();


            // Assert

            Assert.Equal(@"{
  ""items"": [
    {
      ""i"": 0,
      ""data"": ""AA""
    },
    {
      ""i"": 1,
      ""data"": ""BB""
    },
    {
      ""i"": 2,
      ""data"": ""AA""
    },
    {
      ""i"": 3,
      ""data"": ""DD""
    }
  ],
  ""b"": ""2"",
  ""c"": ""3""
}", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void TransformByNodeValue()
        {
            // Arrange

            var document = new JsonDocument(@"{ 
    'items': [
        { 'i': 0, data: 'AA' },
        { 'i': 1, data: 'BB' },
        { 'i': 2, data: 'CC' },
        { 'i': 3, data: 'DD' },
    ],
    'b': '2',
    'c': '3'
}");


            // Act

            document.ReplaceKey(new[] { "items[@i=1]", "data" }, "EE");

            var result = document.ToString();


            // Assert

            Assert.Equal(@"{
  ""items"": [
    {
      ""i"": 0,
      ""data"": ""AA""
    },
    {
      ""i"": 1,
      ""data"": ""EE""
    },
    {
      ""i"": 2,
      ""data"": ""CC""
    },
    {
      ""i"": 3,
      ""data"": ""DD""
    }
  ],
  ""b"": ""2"",
  ""c"": ""3""
}", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void AddStringElementToArray()
        {
            // Arrange

            var document = new JsonDocument(@"{ 
    'a': { 
        'x': '1'
    },
    'b': '2',
    'c': '3',
    'd': []
}");


            // Act

            document.AddElementToArray(new[] { "d" }, "1");

            var result = document.ToString();


            // Assert

            Assert.Equal(@"{
  ""a"": {
    ""x"": ""1""
  },
  ""b"": ""2"",
  ""c"": ""3"",
  ""d"": [
    ""1""
  ]
}", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void AddObjectElementToArray()
        {
            // Arrange

            var document = new JsonDocument(@"{ 
    'a': { 
        'x': '1'
    },
    'b': '2',
    'c': '3',
    'd': []
}");


            // Act

            document.AddElementToArray(new[] { "d" }, "{ 'val1': '1', 'val2': '2' }");

            var result = document.ToString();


            // Assert

            Assert.Equal(@"{
  ""a"": {
    ""x"": ""1""
  },
  ""b"": ""2"",
  ""c"": ""3"",
  ""d"": [
    {
      ""val1"": ""1"",
      ""val2"": ""2""
    }
  ]
}", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void Remove()
        {
            // Arrange

            var document = new JsonDocument(@"{ 
    'a': { 
        'x': '1'
    },
    'b': { 
        'x': '1'
    },
    'c': '3'
}");


            // Act

            document.RemoveKey(new[] { "a"});
            document.RemoveKey(new[] { "b", "x" });

            var result = document.ToString();


            // Assert

            Assert.Equal(@"{
  ""b"": {},
  ""c"": ""3""
}", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void RemoveArrayItem()
        {
            // Arrange

            var document = new JsonDocument(@"{ 
    'items': [
        { 'i': 0, data: 'AA' },
        { 'i': 1, data: 'BB' },
        { 'i': 2, data: 'CC' },
        { 'i': 3, data: 'DD' },
    ],
    'b': '2',
    'c': '3'
}");


            // Act

            document.RemoveKey(new[] { "items[2]" });
            document.RemoveKey(new[] { "items[@i=1]", "data" });

            var result = document.ToString();


            // Assert

            Assert.Equal(@"{
  ""items"": [
    {
      ""i"": 0,
      ""data"": ""AA""
    },
    {
      ""i"": 1
    },
    {
      ""i"": 3,
      ""data"": ""DD""
    }
  ],
  ""b"": ""2"",
  ""c"": ""3""
}", result, ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void ValidateEmptyPath()
        {
            // Assert
            JsonDocument document = new JsonDocument("{}");

            // Act
            ArgumentException result = Assert.Throws<ArgumentException>(() => document.ReplaceKey(new[] { "a", "", "b" }, ""));

            // Arrange
            Assert.True(result.Message?.StartsWith("There is at least one empty segment in the path."));
        }

        [Fact]
        public void ValidateWithespacePath()
        {
            // Assert
            JsonDocument document = new JsonDocument("{}");

            // Act
            ArgumentException result = Assert.Throws<ArgumentException>(() => document.ReplaceKey(new[] { "a", "   ", "b" }, ""));

            // Arrange
            Assert.True(result.Message?.StartsWith("There is at least one empty segment in the path."));
        }
    }
}