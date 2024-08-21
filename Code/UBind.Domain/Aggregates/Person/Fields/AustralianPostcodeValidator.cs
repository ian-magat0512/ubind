// <copyright file="AustralianPostcodeValidator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person.Fields
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The validator to validate australian post codes.
    /// </summary>
    public class AustralianPostcodeValidator : IPostcodeValidator
    {
        private Dictionary<string, List<Tuple<int, int>>> statePostCodes = new Dictionary<string, List<Tuple<int, int>>>
        {
            {
                ValueTypes.State.NSW.ToString().ToUpper(), new List<Tuple<int, int>>
                {
                    new Tuple<int, int>(2000, 2599),
                    new Tuple<int, int>(2620, 2899),
                    new Tuple<int, int>(2921, 2999),
                    new Tuple<int, int>(1000, 1999), // PO Box.
                }
            },
            {
                ValueTypes.State.ACT.ToString().ToUpper(), new List<Tuple<int, int>>
                {
                    new Tuple<int, int>(2600, 2619),
                    new Tuple<int, int>(2900, 2920),
                    new Tuple<int, int>(200, 299), // PO Box.
                }
            },
            {
                ValueTypes.State.VIC.ToString().ToUpper(), new List<Tuple<int, int>>
                {
                    new Tuple<int, int>(3000, 3999),
                    new Tuple<int, int>(8000, 8999), // PO Box.
                }
            },
            {
                ValueTypes.State.QLD.ToString().ToUpper(), new List<Tuple<int, int>>
                {
                    new Tuple<int, int>(4000, 4999),
                    new Tuple<int, int>(9000, 9999), // PO Box.
                }
            },
            {
                ValueTypes.State.SA.ToString().ToUpper(), new List<Tuple<int, int>>
                {
                    new Tuple<int, int>(5000, 5799),
                    new Tuple<int, int>(5800, 5999), // PO Box.
                }
            },
            {
                ValueTypes.State.WA.ToString().ToUpper(), new List<Tuple<int, int>>
                {
                    new Tuple<int, int>(6000, 6797),
                    new Tuple<int, int>(6800, 6999), // PO Box.
                }
            },
            {
                ValueTypes.State.TAS.ToString().ToUpper(), new List<Tuple<int, int>>
                {
                    new Tuple<int, int>(7000, 7799),
                    new Tuple<int, int>(7800, 7999), // PO Box.
                }
            },
            {
                ValueTypes.State.NT.ToString().ToUpper(), new List<Tuple<int, int>>
                {
                    new Tuple<int, int>(800, 899),
                    new Tuple<int, int>(900, 999), // PO Box.
                }
            },
        };

        public bool IsValidState(string state)
        {
            return this.statePostCodes.Any(x => x.Key.ToLower() == state.ToLower().Trim());
        }

        public bool IsValidPostCode(string postCode)
        {
            return int.TryParse(postCode, out int code);
        }

        public bool IsValidPostCodeForTheState(string state, string postCode)
        {
            int postCodeInt = int.Parse(postCode);
            if (this.IsValidState(state))
            {
                foreach (var stateItem in this.statePostCodes.Where(x => x.Key.ToLower() == state.ToLower().Trim()))
                {
                    foreach (var postCodeItems in stateItem.Value)
                    {
                        if (postCodeInt >= postCodeItems.Item1 && postCodeInt <= postCodeItems.Item2)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public List<string> GetValidStates()
        {
            return this.statePostCodes.Select(x => x.Key).ToList();
        }
    }
}
