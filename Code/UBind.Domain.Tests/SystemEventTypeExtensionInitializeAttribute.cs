// <copyright file="SystemEventTypeExtensionInitializeAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Tests;
using System.Reflection;
using UBind.Domain.Events;
using Xunit.Sdk;

public class SystemEventTypeExtensionInitializeAttribute : BeforeAfterTestAttribute
{
    public override void Before(MethodInfo methodUnderTest)
    {
        // needed to instantiate the SystemEventTypePersistenceDurationRegistry used by SystemEventExtensions
        var registry = new SystemEventTypePersistenceDurationRegistry();
    }
}