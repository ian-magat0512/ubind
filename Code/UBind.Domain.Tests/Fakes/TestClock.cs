// <copyright file="TestClock.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Tests.Fakes
{
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Fake clock for tests.
    /// </summary>
    public class TestClock : IClock
    {
        private bool trackSystemTime;
        private Instant timestamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestClock"/> class.
        /// </summary>
        /// <param name="trackSystemTime">A value indicating whether to track system time.</param>
        public TestClock(bool trackSystemTime = false)
        {
            this.trackSystemTime = trackSystemTime;
            this.timestamp = SystemClock.Instance.Now();
        }

        /// <summary>
        /// Gets the time the clock is set to return.
        /// </summary>
        public Instant Timestamp => this.timestamp;

        /// <summary>
        /// A static clock that will always start from the 1970.
        /// </summary>>
        public static TestClock StaticClock()
        {
            TestClock clock = new TestClock(false);
            clock.trackSystemTime = false;
            clock.timestamp = Instant.FromUnixTimeTicks(0);
            return clock;
        }

        /// <inheritdoc />
        public Instant GetCurrentInstant()
        {
            return this.trackSystemTime
                ? SystemClock.Instance.GetCurrentInstant()
                : this.timestamp;
        }

        /// <summary>
        /// Make the clock track the system time.
        /// </summary>
        public void StartTrackingSystemTime()
        {
            this.trackSystemTime = true;
        }

        /// <summary>
        /// Stop the clock tracking system time, freezing it at the current time if it was tracking system time.
        /// </summary>
        public void StopTrackingSystemTime()
        {
            // If the clock was tracking system time, then freeze it at the current system time.
            if (this.trackSystemTime)
            {
                this.timestamp = SystemClock.Instance.GetCurrentInstant();
            }

            this.trackSystemTime = false;
        }

        /// <summary>
        /// Sets the clock to a given AET time and date.
        /// </summary>
        /// <param name="time">.</param>
        public void SetAetTime(LocalDateTime time)
        {
            this.trackSystemTime = false;
            this.timestamp = time.InZoneStrictly(Timezones.AET).ToInstant();
        }

        /// <summary>
        /// Set the clock to the current system time.
        /// </summary>
        public void SetToCurrentTime()
        {
            this.trackSystemTime = false;
            this.timestamp = SystemClock.Instance.Now();
        }

        /// <summary>
        /// Se the clock to a given instant.
        /// </summary>
        /// <param name="instant">The instant.</param>
        public void SetToInstant(Instant instant)
        {
            this.trackSystemTime = false;
            this.timestamp = instant;
        }

        /// <summary>
        /// Increment the clock by a given duration.
        /// </summary>
        /// <param name="duration">duration.</param>
        public void Increment(Duration duration)
        {
            this.trackSystemTime = false;
            this.timestamp = this.Timestamp.Plus(duration);
        }
    }
}
