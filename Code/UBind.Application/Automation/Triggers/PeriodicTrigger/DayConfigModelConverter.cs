// <copyright file="DayConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class DayConfigModelConverter : DeserializationConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(DayConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var configModel = new DayConfigModel();
            if (reader.TokenType == JsonToken.StartObject)
            {
                var dayObject = JObject.Load(reader);
                var dayOfTheMonth = dayObject.Properties().FirstOrDefault(c => c.Name.Equals("dayOfTheMonth"));
                if (dayOfTheMonth != null)
                {
                    configModel.DayOfTheMonths = this.GetItemSet(dayOfTheMonth);
                }

                var dayOfTheWeek = dayObject.Properties().FirstOrDefault(c => c.Name.Equals("dayOfTheWeek"));
                if (dayOfTheWeek != null)
                {
                    configModel.DayOfTheWeeks = this.GetItemSet(dayOfTheWeek);
                }

                var dayOfTheWeekOccurrenceWithinMonth = dayObject.Properties().FirstOrDefault(c => c.Name.Equals("dayOfTheWeekOccurrenceWithinMonth"));
                if (dayOfTheWeekOccurrenceWithinMonth != null)
                {
                    var obj = JObject.Load(dayOfTheWeekOccurrenceWithinMonth.Value.CreateReader());
                    var dayOfTheWeekValue = obj.Properties().FirstOrDefault(c => c.Name.Equals("dayOfTheWeek")).Value;
                    var occurrence = obj.Properties().FirstOrDefault(c => c.Name.Equals("occurrence")).Value.ToObject<int>();
                    configModel.DayOfTheWeekOccurrenceWithinMonths = TimeConfigModel.FromOccurence(dayOfTheWeekValue.ToString(), occurrence);
                }
            }

            return configModel;
        }

        private TimeConfigModel GetItemSet(JProperty property)
        {
            var obj = JObject.Load(property.Value.CreateReader());
            var listProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("list"));
            if (listProperty != null)
            {
                JArray listValue = JArray.Load(listProperty.Value.CreateReader());
                return TimeConfigModel.FromList(listValue.ToObject<List<string>>());
            }

            var rangeProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("range"));
            if (rangeProperty != null)
            {
                var range = JObject.Load(rangeProperty.Value.CreateReader());
                var from = range.Properties().FirstOrDefault(c => c.Name.Equals("from")).Value.ToString();
                var to = range.Properties().FirstOrDefault(c => c.Name.Equals("to")).Value.ToString();
                return TimeConfigModel.FromRange(from, to);
            }

            var everyProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("every"));
            if (everyProperty != null)
            {
                return TimeConfigModel.FromEvery(everyProperty.ToObject<int>());
            }

            return null;
        }
    }
}
