// <copyright file="ValidationStub.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Validation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using UBind.Domain.Validation;

    public class ValidationStub
    {
        public class WithValidateObject
        {
            [Required]
            public string Property1 { get; set; }

            [ValidateObject]
            public WithRequiredProperties Object1 { get; set; }
        }

        public class WithRequiredProperties
        {
            [Required]
            public string Property2 { get; set; }

            [Required]
            public string Property3 { get; set; }
        }

        public class WithValidateItems
        {
            [Required]
            public string Property1 { get; set; }

            [ValidateItems]
            public List<WithRequiredProperties> List1 { get; set; }
        }
    }
}
