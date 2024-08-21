// <copyright file="TestMicrosoftGraphConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.MicrosoftGraph
{
    using UBind.Application.MicrosoftGraph;

    public class TestMicrosoftGraphConfiguration : IMicrosoftGraphConfiguration
    {
        public string ApplicationId => "ed60f657-25d6-446f-b9c0-fd3335ebd807";

        public string ClientId => "836f07a2-66ba-480b-b17f-4fd5ba550707";

        public string Username => "ubind.test@aptiture.com";

        public string Password => "W9]trades613gardensuch";

        public string UBindFolderName => "UBindTest";

        public int MaxRetryAttempts => 2;

        public int AccessTokenLifeTimeInMinutes => 60;
    }
}
