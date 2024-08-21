// <copyright file="PeriodicTriggerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Triggers
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Triggers;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class PeriodicTriggerTests
    {
        [Fact]
        public void PeriodicTriggerConfigModel_Should_Parse_Range_Values()
        {
            // Arrange
            var json = @"{
                        ""name"": ""Periodic Trigger Test 3"",
                        ""alias"": ""periodicTriggerTest3"",
                        ""description"": ""A test of the periodic trigger with UTC time"",
                        ""month"": {
                            ""range"": {
                                ""from"": ""August"",
                                        ""to"": ""October""
                                      }
                        },
                                    ""day"": {
                            ""dayOfTheMonth"": {
                                ""range"": {
                                    ""from"": 1,
                                                ""to"": 15
                                            }
                            },
                                        ""dayOfTheWeek"": {
                                ""range"": {
                                    ""from"": ""Monday"",
                                                ""to"": ""Sunday""
                                            }
                            }
                        },
                                    ""hour"": {
                            ""range"": {
                                ""from"": 3,
                                            ""to"": 18
                                        }
                        },
                                    ""minute"": {
                            ""range"": {
                                ""from"": 1,
                                            ""to"": 55
                                        }
                        }
                    }";

            var periodicTriggerConfigModel = JsonConvert.DeserializeObject<PeriodicTriggerConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockServiceProvider = new Mock<IServiceProvider>();

            // Act
            var periodicTrigger = (PeriodicTrigger)periodicTriggerConfigModel.Build(mockServiceProvider.Object);

            // Assert
            periodicTrigger.Should().NotBeNull();
            periodicTrigger.TimeZone.Should().Be(TimeZoneInfo.Utc);
            periodicTrigger.Month.RangeValue.Item1.Should().Be("August");
            periodicTrigger.Month.RangeValue.Item2.Should().Be("October");

            periodicTrigger.Day.DayOfTheMonths.RangeValue.Item1.Should().Be("1");
            periodicTrigger.Day.DayOfTheMonths.RangeValue.Item2.Should().Be("15");

            periodicTrigger.Day.DayOfTheWeeks.RangeValue.Item1.Should().Be("Monday");
            periodicTrigger.Day.DayOfTheWeeks.RangeValue.Item2.Should().Be("Sunday");

            periodicTrigger.Hour.RangeValue.Item1.Should().Be("3");
            periodicTrigger.Hour.RangeValue.Item2.Should().Be("18");

            periodicTrigger.Minute.RangeValue.Item1.Should().Be("1");
            periodicTrigger.Minute.RangeValue.Item2.Should().Be("55");

            var schedule = periodicTrigger.GetCronSchedule();
            schedule.Should().Be("1-55 3-18 1-15 8-10 1-0");
        }

        [Fact]
        public void PeriodicTriggerConfigModel_Should_Parse_List_Values()
        {
            // Arrange
            var json = @"{
						""name"": ""Periodic Trigger Test 1"",
                        ""alias"": ""periodicTriggerTest1"",
                        ""description"": ""A test of the periodic trigger with UTC time zone offset"",
                        ""timeZoneOffset"": ""+11:00"",
                        ""month"": {
                            ""list"": [
                                ""March"",
                                ""June"",
                                ""October""
                            ]
                        },
                        ""day"": {
                            ""dayOfTheMonth"": {
                                ""list"": [ 1, 4, 5, 8, 11, 30 ]
                            },
                            ""dayOfTheWeek"": {
                              ""list"": [ ""Monday"", ""Tuesday"", ""Wednesday"", ""Friday"" ]
                            }
                        },
                        ""hour"": {
                            ""list"": [ 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 ]
                        },
                        ""minute"": {
                            ""list"": [ 0, 3, 6, 9, 12, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 40, 45, 50, 55, 56,57,58 ]
                        }
					}";

            var periodicTriggerConfigModel = JsonConvert.DeserializeObject<PeriodicTriggerConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockServiceProvider = new Mock<IServiceProvider>();

            // Act
            var periodicTrigger = (PeriodicTrigger)periodicTriggerConfigModel.Build(mockServiceProvider.Object);

            // Assert
            periodicTrigger.Should().NotBeNull();
            periodicTrigger.TimeZone.StandardName.Should().Be("GMT+11:00");
            periodicTrigger.Month.ListValue.Should().HaveCount(3);
            periodicTrigger.Month.ListValue.First().Should().Be("March");
            periodicTrigger.Month.ListValue.Last().Should().Be("October");

            periodicTrigger.Day.DayOfTheMonths.ListValue.Should().HaveCount(6);
            periodicTrigger.Day.DayOfTheMonths.ListValue.First().Should().Be("1");
            periodicTrigger.Day.DayOfTheMonths.ListValue.Last().Should().Be("30");

            periodicTrigger.Day.DayOfTheWeeks.ListValue.Should().HaveCount(4);
            periodicTrigger.Day.DayOfTheWeeks.ListValue.First().Should().Be("Monday");
            periodicTrigger.Day.DayOfTheWeeks.ListValue.Last().Should().Be("Friday");

            periodicTrigger.Hour.ListValue.Should().HaveCount(13);
            periodicTrigger.Hour.ListValue.First().Should().Be("6");
            periodicTrigger.Hour.ListValue.Last().Should().Be("18");

            periodicTrigger.Minute.ListValue.Should().HaveCount(22);
            periodicTrigger.Minute.ListValue.First().Should().Be("0");
            periodicTrigger.Minute.ListValue.Last().Should().Be("58");

            var schedule = periodicTrigger.GetCronSchedule();
            schedule.Should().Be("0,3,6,9,12,20,22,24,26,28,30,32,34,36,38,40,45,50,55,56,57,58 6,7,8,9,10,11,12,13,14,15,16,17,18 1,4,5,8,11,30 3,6,10 1,2,3,5");
        }

        [Fact]
        public void PeriodicTriggerConfigModel_Should_Parse_Every_Values()
        {
            // Arrange
            var json = @"{
                        ""name"": ""Periodic Trigger Test 2"",
                        ""alias"": ""periodicTriggerTest2"",
                        ""description"": ""A test of the periodic trigger with time zone name/ID"",
                        ""timeZoneId"": ""Australia/Sydney"",
                        ""month"": {
                            ""every"": 1
                        },
                        ""day"": {
                            ""dayOfTheWeekOccurrenceWithinMonth"": {
                                ""dayOfTheWeek"": ""Monday"",
                                ""occurrence"": 2
                            }
                        },
                        ""hour"": {
                            ""every"": 1
                        },
                        ""minute"": {
                            ""every"": 3
                        }
                    }";

            var periodicTriggerConfigModel = JsonConvert.DeserializeObject<PeriodicTriggerConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockServiceProvider = new Mock<IServiceProvider>();

            // Act
            var periodicTrigger = (PeriodicTrigger)periodicTriggerConfigModel.Build(mockServiceProvider.Object);

            // Assert
            periodicTrigger.Should().NotBeNull();
            periodicTrigger.TimeZone.StandardName.Should().Be("AUS Eastern Standard Time");
            periodicTrigger.Month.EveryValue.Should().Be(1);

            periodicTrigger.Day.DayOfTheWeekOccurrenceWithinMonths.OccurenceValue.Item1.Should().Be("Monday");
            periodicTrigger.Day.DayOfTheWeekOccurrenceWithinMonths.OccurenceValue.Item2.Should().Be(2);

            periodicTrigger.Hour.EveryValue.Should().Be(1);

            periodicTrigger.Minute.EveryValue.Should().Be(3);

            var schedule = periodicTrigger.GetCronSchedule();
            schedule.Should().Be("*/3 */1 8-14 */1 1");
        }

        [Fact]
        public void PeriodicTriggerConfigModel_Should_Throw_When_TimeZone_Is_InValid()
        {
            // Arrange
            var json = @"{
                        ""name"": ""Periodic Trigger Test 2"",
                        ""alias"": ""periodicTriggerTest2"",
                        ""description"": ""A test of the periodic trigger with time zone name/ID"",
                        ""timeZoneId"": ""Australia/InValid"",
                        ""month"": {
                            ""every"": 1
                        },
                        ""day"": {
                            ""dayOfTheWeekOccurrenceWithinMonth"": {
                                ""dayOfTheWeek"": ""Monday"",
                                ""occurrence"": 2
                            }
                        },
                        ""hour"": {
                            ""every"": 1
                        },
                        ""minute"": {
                            ""every"": 3
                        }
                    }";

            var periodicTriggerConfigModel = JsonConvert.DeserializeObject<PeriodicTriggerConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockServiceProvider = new Mock<IServiceProvider>();

            // Act
            Action act = () => periodicTriggerConfigModel.Build(mockServiceProvider.Object);

            // Assert
            var exception = act.Should().Throw<ErrorException>().And;
            exception.Error.Title.Should().Be("One of the properties for periodicTriggerTest2 is not valid");
            exception.Error.Message.Should().StartWith("The value of the property TimeZoneId for periodicTriggerTest2 is invalid.");
        }

        [Fact]
        public void PeriodicTriggerConfigModel_Should_Throw_When_TimeZoneOffset_Is_InValid()
        {
            // Arrange
            var json = @"{
                        ""name"": ""Periodic Trigger Test 2"",
                        ""alias"": ""periodicTriggerTest2"",
                        ""description"": ""A test of the periodic trigger with time zone name/ID"",
                        ""timeZoneOffset"": ""+25:00"",
                        ""month"": {
                            ""every"": 1
                        },
                        ""day"": {
                            ""dayOfTheWeekOccurrenceWithinMonth"": {
                                ""dayOfTheWeek"": ""Monday"",
                                ""occurrence"": 2
                            }
                        },
                        ""hour"": {
                            ""every"": 1
                        },
                        ""minute"": {
                            ""every"": 3
                        }
                    }";

            var periodicTriggerConfigModel = JsonConvert.DeserializeObject<PeriodicTriggerConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockServiceProvider = new Mock<IServiceProvider>();

            // Act
            Action act = () => periodicTriggerConfigModel.Build(mockServiceProvider.Object);

            // Assert
            var exception = act.Should().Throw<ErrorException>().And;
            exception.Error.Title.Should().Be("One of the properties for periodicTriggerTest2 is not valid");
            exception.Error.Message.Should().StartWith("The value of the property TimeZoneOffset for periodicTriggerTest2 is invalid.");
        }

        [Fact]
        public void PeriodicTriggerConfigModel_Should_Throw_When_TimeZoneOffset_AndTimeZoneId_Both_Present()
        {
            // Arrange
            var json = @"{
                        ""name"": ""Periodic Trigger Test 2"",
                        ""alias"": ""periodicTriggerTest2"",
                        ""description"": ""A test of the periodic trigger with time zone name/ID"",
                        ""timeZoneOffset"": ""+25:00"",
                        ""timeZoneId"": ""Australia/InValid"",
                        ""month"": {
                            ""every"": 1
                        },
                        ""day"": {
                            ""dayOfTheWeekOccurrenceWithinMonth"": {
                                ""dayOfTheWeek"": ""Monday"",
                                ""occurrence"": 2
                            }
                        },
                        ""hour"": {
                            ""every"": 1
                        },
                        ""minute"": {
                            ""every"": 3
                        }
                    }";

            var periodicTriggerConfigModel = JsonConvert.DeserializeObject<PeriodicTriggerConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockServiceProvider = new Mock<IServiceProvider>();

            // Act
            Action act = () => periodicTriggerConfigModel.Build(mockServiceProvider.Object);

            // Assert
            var exception = act.Should().Throw<ErrorException>().And;
            exception.Error.Title.Should().Be("Unsupported configuration.");
            exception.Error.Message.Should().StartWith("Only one property is expected. Either TimeZoneId or TimeZoneOffset should be present in the configuration");
        }

        [Fact]
        public void PeriodicTriggerConfigModel_Should_Parse_List_Values_Monday_7am()
        {
            // Arrange
            var json = @"{
                        ""name"": ""Periodic Trigger Test 2"",
                        ""alias"": ""periodicTriggerTest2"",
                        ""description"": ""A test of the periodic trigger with time zone name/ID"",
                        ""timeZoneId"": ""Australia/Sydney"",
                        ""month"": {
                            ""every"": 1
                        },
                        ""day"": {
                            ""dayOfTheWeek"": {
                                ""list"": [ ""Monday"" ]
                            }
                        },
                        ""hour"": {
                            ""list"": [ 7 ]
                        },
                        ""minute"": {
                            ""list"": [ 0 ]
                        }
                    }";

            var periodicTriggerConfigModel = JsonConvert.DeserializeObject<PeriodicTriggerConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockServiceProvider = new Mock<IServiceProvider>();

            // Act
            var periodicTrigger = (PeriodicTrigger)periodicTriggerConfigModel.Build(mockServiceProvider.Object);

            // Assert
            periodicTrigger.Should().NotBeNull();
            periodicTrigger.TimeZone.StandardName.Should().Be("AUS Eastern Standard Time");
            periodicTrigger.Month.EveryValue.Should().Be(1);

            periodicTrigger.Day.DayOfTheWeeks.ListValue.First().Should().Be("Monday");

            periodicTrigger.Hour.ListValue.First().Should().Be("7");

            periodicTrigger.Minute.ListValue.First().Should().Be("0");

            var schedule = periodicTrigger.GetCronSchedule();
            schedule.Should().Be("0 7 * */1 1");
        }

        [Fact]
        public void PeriodicTriggerConfigModel_Should_Parse_List_Values_Day_of_the_Month_Not_Starting_In_1()
        {
            // Arrange
            var json = @"{
                        ""name"": ""Periodic Trigger Test 2"",
                        ""alias"": ""periodicTriggerTest2"",
                        ""description"": ""A test of the periodic trigger with time zone name/ID"",
                        ""timeZoneId"": ""Australia/Sydney"",
                        ""month"": {
                            ""every"": 1
                        },
                        ""day"": {
                            ""dayOfTheMonth"": {
                                ""list"": [ 15, 21, 23, 25, 30 ]
                            }
                        },
                        ""hour"": {
                            ""list"": [ 7 ]
                        },
                        ""minute"": {
                            ""list"": [ 0 ]
                        }
                    }";

            var periodicTriggerConfigModel = JsonConvert.DeserializeObject<PeriodicTriggerConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockServiceProvider = new Mock<IServiceProvider>();

            // Act
            var periodicTrigger = (PeriodicTrigger)periodicTriggerConfigModel.Build(mockServiceProvider.Object);

            // Assert
            periodicTrigger.Should().NotBeNull();
            periodicTrigger.TimeZone.StandardName.Should().Be("AUS Eastern Standard Time");
            periodicTrigger.Month.EveryValue.Should().Be(1);

            periodicTrigger.Day.DayOfTheMonths.ListValue.Should().HaveCount(5);
            periodicTrigger.Day.DayOfTheMonths.ListValue.First().Should().Be("15");
            periodicTrigger.Day.DayOfTheMonths.ListValue.Last().Should().Be("30");

            periodicTrigger.Hour.ListValue.First().Should().Be("7");

            periodicTrigger.Minute.ListValue.First().Should().Be("0");

            var schedule = periodicTrigger.GetCronSchedule();
            schedule.Should().Be("0 7 15,21,23,25,30 */1 *");
        }

        [Fact]
        public void PeriodicTriggerConfigModel_Should_Parse_Range_Values_Of_Days_Not_Starting_In_1()
        {
            // Arrange
            var json = @"{
                        ""name"": ""Periodic Trigger Test 3"",
                        ""alias"": ""periodicTriggerTest3"",
                        ""description"": ""A test of the periodic trigger with UTC time"",
                        ""month"": {
                            ""range"": {
                                ""from"": ""August"",
                                        ""to"": ""October""
                                      }
                        },
                                    ""day"": {
                            ""dayOfTheMonth"": {
                                ""range"": {
                                    ""from"": 5,
                                                ""to"": 15
                                            }
                            },
                                        ""dayOfTheWeek"": {
                                ""range"": {
                                    ""from"": ""Wednesday"",
                                                ""to"": ""Sunday""
                                            }
                            }
                        },
                                    ""hour"": {
                            ""range"": {
                                ""from"": 3,
                                            ""to"": 18
                                        }
                        },
                                    ""minute"": {
                            ""range"": {
                                ""from"": 1,
                                            ""to"": 55
                                        }
                        }
                    }";

            var periodicTriggerConfigModel = JsonConvert.DeserializeObject<PeriodicTriggerConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockServiceProvider = new Mock<IServiceProvider>();

            // Act
            var periodicTrigger = (PeriodicTrigger)periodicTriggerConfigModel.Build(mockServiceProvider.Object);

            // Assert
            periodicTrigger.Should().NotBeNull();
            periodicTrigger.TimeZone.Should().Be(TimeZoneInfo.Utc);
            periodicTrigger.Month.RangeValue.Item1.Should().Be("August");
            periodicTrigger.Month.RangeValue.Item2.Should().Be("October");

            periodicTrigger.Day.DayOfTheMonths.RangeValue.Item1.Should().Be("5");
            periodicTrigger.Day.DayOfTheMonths.RangeValue.Item2.Should().Be("15");

            periodicTrigger.Day.DayOfTheWeeks.RangeValue.Item1.Should().Be("Wednesday");
            periodicTrigger.Day.DayOfTheWeeks.RangeValue.Item2.Should().Be("Sunday");

            periodicTrigger.Hour.RangeValue.Item1.Should().Be("3");
            periodicTrigger.Hour.RangeValue.Item2.Should().Be("18");

            periodicTrigger.Minute.RangeValue.Item1.Should().Be("1");
            periodicTrigger.Minute.RangeValue.Item2.Should().Be("55");

            var schedule = periodicTrigger.GetCronSchedule();
            schedule.Should().Be("1-55 3-18 5-15 8-10 3-0");
        }
    }
}
