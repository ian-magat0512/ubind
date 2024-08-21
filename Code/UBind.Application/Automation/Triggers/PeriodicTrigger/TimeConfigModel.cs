// <copyright file="TimeConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers
{
    using System.Collections.Generic;

    public class TimeConfigModel
    {
        public DatePartDataType Type { get; private set; }

        public IEnumerable<string> ListValue { get; private set; }

        public (string, string) RangeValue { get; private set; }

        public (int, int) IntRangeValue { get; private set; }

        public int EveryValue { get; private set; }

        public (string, int) OccurenceValue { get; private set; }

        public static TimeConfigModel FromList(IEnumerable<string> value)
        {
            var monthConfigModel = new TimeConfigModel();
            monthConfigModel.Type = DatePartDataType.List;
            monthConfigModel.ListValue = value;
            return monthConfigModel;
        }

        public static TimeConfigModel FromRange(string start, string end)
        {
            var monthConfigModel = new TimeConfigModel();
            monthConfigModel.Type = DatePartDataType.Range;
            monthConfigModel.RangeValue = (start, end);
            return monthConfigModel;
        }

        public static TimeConfigModel FromEvery(int value)
        {
            var monthConfigModel = new TimeConfigModel();
            monthConfigModel.Type = DatePartDataType.Every;
            monthConfigModel.EveryValue = value;
            return monthConfigModel;
        }

        public static TimeConfigModel FromOccurence(string dayOfWeek, int number)
        {
            var monthConfigModel = new TimeConfigModel();
            monthConfigModel.Type = DatePartDataType.Occurence;
            monthConfigModel.OccurenceValue = (dayOfWeek, number);
            return monthConfigModel;
        }
    }
}
