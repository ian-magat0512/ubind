// <copyright file="TestDeftConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Fakes
{
    using UBind.Application.Payment.Deft;

    public class TestDeftConfiguration : DeftConfiguration
    {
        public TestDeftConfiguration()
            : base(
                "https://api.sandbox.deft.com.au/v2/auth/accessToken",
                "AAs7HeaG8zDCTKJOcijnp4Sg3z7q7D4v",
                "crZQSrBXYRTkVk9x",
                "https://api.sandbox.deft.com.au/v3/payment",
                "https://api.sandbox.deft.com.au/v2/surcharge",
                "https://api.sandbox.deft.com.au/v2/drn",
                "406389",
                false,
                CrnGenerationMethod.Unique10DigitNumber,
                "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAw+saE49OO2bHddoAAURn8FLIKsJCHL714B+6cqns2KKaLoygJc1363r7XZFuW0Z3acGHB9toEv98AHbaeu1cPnMitEAH1qHD6wTl27nTSAYSUEUq+vhAl23hxQc+DmBXYQCeEcWEa2mmS8dgwwn4CB6ku+4+GwZS5k0SWWEb+z7gvf9fVIX2/lDgG3eKMPp0afO48K4tMhDAR40lMfgaCGiMWFTFtYAmHpuxGsDB+/Y7I1GrqABQpLd1ZqphYW7zOTzBLOC004cIPKeR1HbBBtYZe2tqITSDgyshBi2ZEgMJ3eKQhOAsHolb7mhhwGeMNNAxWnhpHl2nMBY6OL0PbwIDAQAB",
                "8391")
        {
        }

        public TestDeftConfiguration(string paymentUrl)
            : base(
                "https://api.sandbox.deft.com.au/v2/auth/accessToken",
                "AAs7HeaG8zDCTKJOcijnp4Sg3z7q7D4v",
                "crZQSrBXYRTkVk9x",
                paymentUrl,
                "https://api.sandbox.deft.com.au/v2/surcharge",
                "https://api.sandbox.deft.com.au/v2/drn",
                "406389",
                false,
                CrnGenerationMethod.Unique10DigitNumber,
                "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAw+saE49OO2bHddoAAURn8FLIKsJCHL714B+6cqns2KKaLoygJc1363r7XZFuW0Z3acGHB9toEv98AHbaeu1cPnMitEAH1qHD6wTl27nTSAYSUEUq+vhAl23hxQc+DmBXYQCeEcWEa2mmS8dgwwn4CB6ku+4+GwZS5k0SWWEb+z7gvf9fVIX2/lDgG3eKMPp0afO48K4tMhDAR40lMfgaCGiMWFTFtYAmHpuxGsDB+/Y7I1GrqABQpLd1ZqphYW7zOTzBLOC004cIPKeR1HbBBtYZe2tqITSDgyshBi2ZEgMJ3eKQhOAsHolb7mhhwGeMNNAxWnhpHl2nMBY6OL0PbwIDAQAB",
                "8391")
        {
        }
    }
}
