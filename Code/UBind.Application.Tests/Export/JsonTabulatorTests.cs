// <copyright file="JsonTabulatorTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Export
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Export;
    using Xunit;

    public class JsonTabulatorTests
    {
        [Fact]
        public void Tabulate_CorrectlyFormatsNestedObjects_WhenPropertyContainsWordsAndIntegers()
        {
            // Arrange
            var data = new List<object>
            {
                new
                {
                    A = "a",
                    B = (string)null,
                    C = new
                    {
                        X = 1,
                        Y = 2,
                    },
                    D = new
                    {
                        P = "DP",
                    },
                    X = 1,
                },
                new
                {
                    A = "a",
                    B = (string)null,
                    C = new
                    {
                        X = 11,
                        Z = 13,
                    },
                    E = new
                    {
                        P = "EP",
                    },
                    X = 11,
                },
            };

            // Act
            var table = JsonTabulator.TabulateCsv(data);

            // Assert
            var rows = table.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
            Assert.Equal("\"A\",\"B\",\"C.X\",\"C.Y\",\"C.Z\",\"D.P\",\"E.P\",\"X\"", rows[0]);
            Assert.Equal("\"a\",,\"1\",\"2\",,\"DP\",,\"1\"", rows[1]);
            Assert.Equal("\"a\",,\"11\",,\"13\",,\"EP\",\"11\"", rows[2]);
        }

        [Fact]
        public void Tabulate_CorrectlyFormatsNestedObjects_WhenPropertyContainsJson()
        {
            // Arrange
            var subObject = new { A = "a", X = 1 };
            var subObjectJson = JsonConvert.SerializeObject(subObject);
            var datum = new { P = "p", Q = subObjectJson };
            var data = new[] { datum };

            // Act
            var table = JsonTabulator.TabulateCsv(data);

            // Assert
            var rows = table.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
            Assert.Equal("\"P\",\"Q\"", rows[0]);
            Assert.Equal("\"p\",\"{\"\"A\"\":\"\"a\"\",\"\"X\"\":1}\"", rows[1]);
        }

        [Fact]
        public void Tabulate_CorrectlyFormatsNestedObjects_WhenPropertyBackslashesAndDoubldQuotes()
        {
            // Arrange
            var data = new List<object>
            {
                new { X = "\"" },  // Single double quote
                new { X = "\\" }, // Single backslash
                new { X = "\\\"" }, // Escaped double quote
                new { X = "\\\\" }, // Escaped backslash
                new { X = "\\\\\\\\\"\"\"\"" }, // 4 backslashes and 4 double quotes
            };

            // Act
            var table = JsonTabulator.TabulateCsv(data);

            // Assert
            var rows = table.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
            Assert.Equal("\"X\"", rows[0]);
            Assert.Equal("\"\"\"\"", rows[1]);
            Assert.Equal("\"\\\"", rows[2]);
            Assert.Equal("\"\\\"\"\"", rows[3]);
            Assert.Equal("\"\\\\\"", rows[4]);
            Assert.Equal("\"\\\\\\\\\"\"\"\"\"\"\"\"\"", rows[5]);
        }

        [Fact]
        public void TabulateTabDelimited_Correctly_Formats_A_List_Of_JArrays()
        {
            // Arrange
            var list = new List<JArray>();
            for (int i = 1; i < 5; i++)
            {
                JArray jArray = new JArray();
                jArray.Add($"a{i}");
                jArray.Add($"b{i}");
                jArray.Add($"c{i}");
                jArray.Add($"d{i}");
                list.Add(jArray);
            }

            // Act
            var table = JsonTabulator.TabulateTabDelimited(list);

            // Assert
            var rows = table.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
            Assert.Equal("a1\tb1\tc1\td1", rows[0]);
            Assert.Equal("a2\tb2\tc2\td2", rows[1]);
            Assert.Equal("a3\tb3\tc3\td3", rows[2]);
            Assert.Equal("a4\tb4\tc4\td4", rows[3]);
        }
    }
}
