// <copyright file="ControllerActionMethodsTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Tests.Controllers
{
    using System.Linq;
    using System.Reflection;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Xunit;

    public class ControllerActionMethodsTests
    {
        /// <summary>
        /// Get all action methods that will be included in Swagger documentation.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> ActionMethods()
        {
            var returnTypes = new List<Type> {
                typeof(IActionResult),
                typeof(Task<IActionResult>),
            };
            var excludedMethods = new List<string>
            {
                "Index",
                "Error",
            };
            var methods = Assembly.GetAssembly(typeof(Startup)) // Assuming Startup class is in the same assembly as your controllers
                .GetTypes()
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract)
                .Select(p => p.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(method => method.IsPublic
                        && !method.IsDefined(typeof(NonActionAttribute))
                        && returnTypes.Contains(method.ReturnType)
                        && !excludedMethods.Contains(method.Name)))
                .SelectMany(p => p);

            foreach (var actionMethod in methods)
            {
                var apiExplorerSettingsAttribute = actionMethod.GetCustomAttributes()
                    .FirstOrDefault(attr => attr is ApiExplorerSettingsAttribute) as ApiExplorerSettingsAttribute;
                var isMethodVisibleInApi = apiExplorerSettingsAttribute != null ? !apiExplorerSettingsAttribute.IgnoreApi : true;
                if (isMethodVisibleInApi)
                {
                    yield return new object[] { (MethodInfo)actionMethod };
                }
            }
        }

        [Theory]
        [MemberData(nameof(ActionMethods))]
        public void AllControllerActions_ShouldHaveHttpMethodAttribute(MethodInfo method)
        {
            // Arrange
            var hasHttpMethodAttribute = method.GetCustomAttributes()
                .Any(attr => attr is HttpMethodAttribute);

            // Assert
            hasHttpMethodAttribute.Should().BeTrue($"because {method.Name} in {method.DeclaringType.Name} should have HTTP method attribute");
        }
    }
}
