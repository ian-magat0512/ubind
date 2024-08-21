// <copyright file="DomainNameHelperTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Helpers
{
    using System.Collections.Generic;
    using UBind.Domain.Helpers;
    using Xunit;

    public class DomainNameHelperTest
    {
        [Fact]
        public void IsDomainMatchesWithAnyApplicableDomain_ReturnsTrue_WhenDomainNameMatchesWithAnyApplicableDomainName()
        {
            // Prepare
            string domainName = "advancen.com";
            List<string> applicableDomainNames = new List<string> { "advancen.com" };

            // Act
            var isDomainMatchesWithAnyApplicableDomain = DomainNameHelper.DoesDomainMatchPartially(domainName, applicableDomainNames);

            // Assert
            Assert.True(isDomainMatchesWithAnyApplicableDomain);
        }

        [Fact]
        public void IsDomainMatchesWithAnyApplicableDomain_ReturnsFalse_WhenDomainNameDoesNotHaveAnyMatchesWithApplicableDomainName()
        {
            // Prepare
            string domainName = "advancen.com";
            List<string> applicableDomainNames = new List<string> { "advan.com" };

            // Act
            var isDomainMatchesWithAnyApplicableDomain = DomainNameHelper.DoesDomainMatchPartially(domainName, applicableDomainNames);

            // Assert
            Assert.False(isDomainMatchesWithAnyApplicableDomain);
        }

        [Fact]
        public void IsDomainMatchesWithAnyApplicableDomain_ReturnsTrue_WhenDomainNameEndsWithAnyApplicableDomain()
        {
            // Prepare
            string domainName = "advancen.com";
            List<string> applicableDomainNames = new List<string> { "anything.advancen.com", "abc.advancen.com" };

            // Act
            var isDomainMatchesWithAnyApplicableDomain = DomainNameHelper.DoesDomainMatchPartially(domainName, applicableDomainNames);

            // Assert
            Assert.True(isDomainMatchesWithAnyApplicableDomain);
        }

        [Fact]
        public void IsDomainMatchesWithAnyApplicableDomain_ReturnsFalse_WhenDomainNameDoesNotEndsWithAnyApplicableDomain()
        {
            // Prepare
            string domainName = "advancen.com";
            List<string> applicableDomainNames = new List<string> { "anything.advan.com", "abc.advancen.ph", "advancen.ph" };

            // Act
            var isDomainMatchesWithAnyApplicableDomain = DomainNameHelper.DoesDomainMatchPartially(domainName, applicableDomainNames);

            // Assert
            Assert.False(isDomainMatchesWithAnyApplicableDomain);
        }

        [Fact]
        public void IsDomainMatchesWithAnyApplicableDomain_ReturnsTrue_WhenDomainNameMatchesWithWildCardApplicableName()
        {
            // Prepare
            List<string> domainNames = new List<string> { "abc.advancen.com", "anything.advancen.com", "subdomain.advancen.com" };
            List<string> applicableDomainNames = new List<string> { "*.advancen.com" };

            foreach (string domainName in domainNames)
            {
                // Act
                var isDomainMatchesWithAnyApplicableDomain = DomainNameHelper.DoesDomainMatchPartially(domainName, applicableDomainNames);

                // Assert
                Assert.True(isDomainMatchesWithAnyApplicableDomain);
            }
        }

        [Fact]
        public void IsDomainMatchesWithAnyApplicableDomain_ReturnsFalse_WhenDomainNameDoesNotMatchWithAnyWildCardApplicableName()
        {
            // Prepare
            List<string> domainNames = new List<string> { "abc.anything.com", "anything.advan.au", "subdomain.advancen.ph" };
            List<string> applicableDomainNames = new List<string> { "*.advancen.com" };

            foreach (string domainName in domainNames)
            {
                // Act
                var isDomainMatchesWithAnyApplicableDomain = DomainNameHelper.DoesDomainMatchPartially(domainName, applicableDomainNames);

                // Assert
                Assert.False(isDomainMatchesWithAnyApplicableDomain);
            }
        }
    }
}
