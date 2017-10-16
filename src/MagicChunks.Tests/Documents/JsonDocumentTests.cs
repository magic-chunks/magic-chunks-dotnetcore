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
        public void ValidateEmptyPath()
        {
            // Assert
            JsonDocument document = new JsonDocument("{}");

            // Act
            ArgumentException result = Assert.Throws<ArgumentException>(() => document.ReplaceKey(new[] { "a", "", "b" }, ""));

            // Arrange
            Assert.True(result.Message?.StartsWith("There is empty items in the path."));
        }

        [Fact]
        public void ValidateWithespacePath()
        {
            // Assert
            JsonDocument document = new JsonDocument("{}");

            // Act
            ArgumentException result = Assert.Throws<ArgumentException>(() => document.ReplaceKey(new[] { "a", "   ", "b" }, ""));

            // Arrange
            Assert.True(result.Message?.StartsWith("There is empty items in the path."));
        }
    }
}