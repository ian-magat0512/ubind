// <copyright file="GenericConverterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Export
{
    using Newtonsoft.Json;
    using UBind.Application.Export;
    using Xunit;

    public class GenericConverterTests
    {
        [Fact]
        public void GenericConverterWorks()
        {
            // Arrange
            var eventExporterActionModelTypeMap = new TypeMap
            {
                { "email", typeof(EmailEventExporterActionModel) },
            };

            var textProvderModelTypeMap = new TypeMap
            {
                { "fixed", typeof(FixedTextProviderModel) },
                { "formField", typeof(FormFieldTextProviderModel) },
            };

            var json = @"{
    ""type"" : ""email"",
    ""from"" : ""from@example.com"",
    ""to"" : ""to@example.com"",
    ""cc"" : ""cc@example.com"",
    ""bcc"" : ""bcc@example.com"",
    ""subject"" : ""Test message"",
    ""plainTextBody"" : ""Hello World"",
    ""htmlBody"" : ""<span>Hello World</span>""
}";

            // Act
            var model = JsonConvert.DeserializeObject<IExporterModel<EventExporterAction>>(
                json,
                new GenericConverter<IExporterModel<EventExporterAction>>(eventExporterActionModelTypeMap),
                new TextProviderModelConverter(textProvderModelTypeMap))
                    as EmailEventExporterActionModel;

            // Assert
            Assert.NotNull(model);
            Assert.Equal("from@example.com", ((FixedTextProviderModel)model.From).Text);
        }
    }
}
