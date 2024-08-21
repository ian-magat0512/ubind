// <copyright file="ErrorTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UBind.Domain;
    using Xunit;

    public sealed class ErrorTests
    {
        [Fact]
        public void Error_codes_must_be_unique()
        {
            List<MethodInfo> methods = typeof(Error)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(x => x.ReturnType == typeof(Error))
                .ToList();

            int numberOfUniqueCodes = methods.Select(x => this.GetErrorCode(x))
                .Distinct()
                .Count();

            Assert.True(numberOfUniqueCodes == methods.Count);
        }

        private string GetErrorCode(MethodInfo method)
        {
            object[] parameters = method.GetParameters()
                .Select<ParameterInfo, object>(x =>
                {
                    if (x.ParameterType == typeof(string))
                    {
                        return string.Empty;
                    }

                    if (x.ParameterType == typeof(long))
                    {
                        return 0;
                    }

                    throw new Exception();
                })
                .ToArray();

            var error = (Error)method.Invoke(null, parameters);
            return error.Code;
        }
    }
}
