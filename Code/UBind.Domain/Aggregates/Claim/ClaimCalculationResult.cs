// <copyright file="ClaimCalculationResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Domain.Json;
    using UBind.Domain.ReadWriteModel.CalculationTrigger;

    /// <summary>
    /// Representation of a Claim calculation result.
    /// </summary>
    public class ClaimCalculationResult : CachingJObjectWrapper
    {
        private const string ResultState = "state";
        private readonly IEnumerable<ICalculationTrigger> triggers = new List<ICalculationTrigger>
        {
            new ReviewCalculationTrigger(),
            new AssessmentCalculationTrigger(),
            new NotificationCalculationTrigger(),
            new DeclinedCalculationTrigger(),
            new ErrorCalculationTrigger(),
        };

        private ClaimCalculationResult(Guid formDataId, string json)
        {
            this.FormDataId = formDataId;
            this.Json = json;
        }

        [JsonConstructor]
        private ClaimCalculationResult()
        {
        }

        /// <summary>
        /// Gets a value indicating whether the calculation has <see cref="ReviewCalculationTrigger"/>.
        /// </summary>
        public bool HasReviewCalculationTriggers
        {
            get
            {
                return this.GetCalculationTriggers().Any(x => x.GetType().Name == typeof(ReviewCalculationTrigger).Name);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the calculation has <see cref="ReviewCalculationTrigger"/>.
        /// </summary>
        public bool HasAssessmentCalculationTriggers
        {
            get
            {
                return this.GetCalculationTriggers().Any(x => x.GetType().Name == typeof(AssessmentCalculationTrigger).Name);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the calculation has <see cref="ReviewCalculationTrigger"/>.
        /// </summary>
        public bool HasNotificationCalculationTriggers
        {
            get
            {
                return this.GetCalculationTriggers().Any(x => x.GetType().Name == typeof(NotificationCalculationTrigger).Name);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the calculation has <see cref="DeclinedCalculationTrigger"/>.
        /// </summary>
        public bool HasDeclinedCalculationTriggers
        {
            get
            {
                return this.GetCalculationTriggers().Any(x => x.GetType().Name == typeof(DeclinedCalculationTrigger).Name);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the calculation has <see cref="ErrorCalculationTrigger"/>.
        /// </summary>
        public bool HasErrorCalculationTriggers
        {
            get
            {
                return this.GetCalculationTriggers().Any(x => x.GetType().Name == typeof(ErrorCalculationTrigger).Name);
            }
        }

        /// <summary>
        /// Gets the ID of teh form data record used in the calculation.
        /// </summary>
        [JsonProperty]
        public Guid FormDataId { get; private set; }

        /// <summary>
        /// Gets a collection of all the triggers in the calculation result.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<ICalculationTrigger> Triggers
        {
            get
            {
                return this.GetCalculationTriggers();
            }
        }

        /// <summary>
        /// Gets the state of the calculation result.
        /// </summary>
        public string CalculationResultState
        {
            get
            {
                return this.JObject[ResultState].ToString();
            }
        }

        /// <summary>
        /// Create new Calculation Result.
        /// </summary>
        /// <param name="formDataId">The form data ID.</param>
        /// <param name="json">The json string.</param>
        /// <returns>Returns an object of ICalculationTrigger interface.</returns>
        public static ClaimCalculationResult Create(Guid formDataId, string json)
        {
            return new ClaimCalculationResult(formDataId, json);
        }

        /// <summary>
        /// Gets a calculation trigger value.
        /// </summary>
        /// <returns>Returns an object of ICalculationTrigger interface.</returns>
        private IEnumerable<ICalculationTrigger> GetCalculationTriggers()
        {
            List<ICalculationTrigger> calculationTrigger = new List<ICalculationTrigger>();

            var model = JsonConvert.DeserializeObject<CalculationDataTriggerModel>(this.Json) ?? null;
            if (model?.Triggers != null)
            {
                void GetCalculationTrigger()
                {
                    foreach (var key1 in model.Triggers.Keys)
                    {
                        var value = model.Triggers[key1];
                        foreach (var key2 in value.Keys)
                        {
                            if (value[key2])
                            {
                                var trigger = this.triggers.FirstOrDefault(t => t.Name == key1);
                                if (trigger != null)
                                {
                                    calculationTrigger.Add(trigger);
                                }
                            }
                        }
                    }
                }

                GetCalculationTrigger();
            }

            return calculationTrigger;
        }
    }
}
