// <copyright file="SequentialClock.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Fakes
{
    using NodaTime;

    /// <summary>
    /// Fake clock that returns a sequence of times advancing by a fixed duration each time.
    /// </summary>
    public class SequentialClock : IClock
    {
        private Instant time;
        private Duration step;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequentialClock"/> class.
        /// </summary>
        /// <param name="startTime">The initial time to return.</param>
        /// <param name="step">The amount to increment the time by each time it is obtained.</param>
        public SequentialClock(Instant startTime, Duration step)
        {
            this.time = startTime;
            this.step = step;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequentialClock"/> class that starts from the current system
        /// time and increments by 1 second each time it is used.
        /// </summary>
        public SequentialClock()
            : this(SystemClock.Instance.GetCurrentInstant(), Duration.FromSeconds(1))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequentialClock"/> class that starts from a given time and
        /// increments by 1 second each time it is used.
        /// </summary>
        /// <param name="startTime">The initial time to return.</param>
        public SequentialClock(Instant startTime)
            : this(startTime, Duration.FromSeconds(1))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequentialClock"/> class that starts from the current system
        /// time and increments by a given amount each time it is used.
        /// </summary>
        /// <param name="step">The amount to increment the time by each time it is obtained.</param>
        public SequentialClock(Duration step)
            : this(SystemClock.Instance.GetCurrentInstant(), step)
        {
        }

        /// <summary>
        /// Get the time from the clock.
        /// </summary>
        /// <returns>The next time in the sequence.</returns>
        public Instant GetCurrentInstant()
        {
            var result = this.time;
            this.time = this.time.Plus(this.step);
            return result;
        }
    }
}
