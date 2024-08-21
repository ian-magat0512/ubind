// <copyright file="PeriodicTrigger.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Providers;

    public class PeriodicTrigger : ConditionalTrigger
    {
        private Func<string, int> toMonthNumber = (monthName) =>
        {
            return Array.FindIndex(
                CultureInfo.CurrentCulture.DateTimeFormat.MonthNames,
                c => c.Equals(monthName, StringComparison.CurrentCultureIgnoreCase)) + 1;
        };

        private Func<string, int> toDayOfTheWeek = day =>
        {
            return Convert.ToInt32((DayOfWeek)Enum.Parse(
                            typeof(DayOfWeek), day, true));
        };

        private Func<string, int> toInt = str =>
        {
            return Convert.ToInt32(str);
        };

        public PeriodicTrigger(
            string name,
            string alias,
            string description,
            IProvider<Data<bool>> runConditionProvider,
            TimeZoneInfo timeZone,
            TimeConfigModel month,
            DayConfigModel day,
            TimeConfigModel hour,
            TimeConfigModel minute)
            : base(name, alias, description, runConditionProvider)
        {
            this.TimeZone = timeZone;
            this.Month = month;
            this.Day = day;
            this.Hour = hour;
            this.Minute = minute;
        }

        public TimeZoneInfo TimeZone { get; private set; }

        public TimeConfigModel Month { get; private set; }

        public DayConfigModel Day { get; private set; }

        public TimeConfigModel Hour { get; private set; }

        public TimeConfigModel Minute { get; private set; }

        public string GetCronSchedule()
        {
            var minutes = this.GetFieldValue(this.Minute, this.toInt);
            var hours = this.GetFieldValue(this.Hour, this.toInt);
            var days = "*";
            var dayOftheWeek = "*";
            if (this.Day.DayOfTheMonths != null)
            {
                days = this.GetFieldValue(this.Day.DayOfTheMonths, this.toInt);
            }

            if (this.Day.DayOfTheWeeks != null)
            {
                dayOftheWeek = this.GetFieldValue(this.Day.DayOfTheWeeks, this.toDayOfTheWeek);
            }

            if (this.Day.DayOfTheWeekOccurrenceWithinMonths != null)
            {
                var occurence = this.Day.DayOfTheWeekOccurrenceWithinMonths.OccurenceValue.Item2;
                days = this.GetEquivalentDaysBaseOnOccurence(occurence);
                dayOftheWeek = this.Day.DayOfTheWeekOccurrenceWithinMonths.OccurenceValue.Item1;
                dayOftheWeek = ((int)Enum.Parse(typeof(DayOfWeek), dayOftheWeek)).ToString();
            }

            var months = this.GetFieldValue(this.Month, this.toMonthNumber);
            return $"{minutes} {hours} {days} {months} {dayOftheWeek}";
        }

        public override Task<bool> DoesMatch(AutomationData automationData) => Task.FromResult(automationData.Trigger.Type == TriggerType.PeriodicTrigger);

        private string GetFieldValue(TimeConfigModel itemSet, Func<string, int> transform)
        {
            switch (itemSet.Type)
            {
                case DatePartDataType.List:
                    var list = itemSet.ListValue.Select(c => transform(c)).ToList();
                    return string.Join(",", list);
                case DatePartDataType.Range:
                    var from = transform(itemSet.RangeValue.Item1);
                    var to = transform(itemSet.RangeValue.Item2);
                    return $"{from}-{to}";
                case DatePartDataType.Every:
                    var value = itemSet.EveryValue;
                    return $"*/{value}";
                default:
                    return "0";
            }
        }

        private string GetEquivalentDaysBaseOnOccurence(int occurence)
        {
            if (occurence <= 0 || occurence > 5)
            {
                return "0";
            }

            var days = new Dictionary<int, string>()
            {
                { 1, "1-7" },
                { 2, "8-14" },
                { 3, "15-21" },
                { 4, "22-28" },
                { 5, "29-31" },
            };

            return days[occurence];
        }
    }
}
