// <copyright file="ResourcePoolStatsCollector.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ResourcePool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Timers;
    using NodaTime;

    /// <summary>
    /// Collects usage statistics of a resource pool.
    /// </summary>
    public class ResourcePoolStatsCollector
    {
        private const int RetentionDays = 7;
        private readonly IClock clock;
        private IResourcePool pool;
        private Timer timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourcePoolStatsCollector"/> class.
        /// </summary>
        /// <param name="pool">The resource pool to collect stats about.</param>
        /// <param name="clock">The clock.</param>
        public ResourcePoolStatsCollector(IResourcePool pool, IClock clock)
        {
            this.pool = pool;
            this.clock = clock;
            this.Collect();
            this.TrimEventsPeriodically();
        }

        /// <summary>
        /// Gets or sets the events collected.
        /// </summary>
        public List<ResourcePoolEvent> Events { get; set; } = new List<ResourcePoolEvent>();

        /// <summary>
        /// Returns the number of events of a given type in the last X where X is a duration, e.g. 24 hours.
        /// </summary>
        /// <param name="type">The type of event.</param>
        /// <param name="duration">The duration.</param>
        /// <returns>The count or total number of the events in the period.</returns>
        public int GetNumberOfEventsOfTypeInLastDuration(ResourcePoolEvent.EventType type, Duration duration)
        {
            var cutoff = this.clock.GetCurrentInstant() - duration;
            return this.Events.Where(ev => ev.Type == type && ev.Timestamp > cutoff).Count();
        }

        /// <summary>
        /// Stops the collector and releases all resources for the timer and event handlers.
        /// </summary>
        public void Stop()
        {
            this.timer.Elapsed -= this.OnTimedEvent;
            this.Events.Clear();
            this.timer.Stop();
            this.timer.Close();
        }

        private void Collect()
        {
            this.pool.ResourceAquired += (object sender, EventArgs e)
                => this.Events.Add(
                    new ResourcePoolEvent(ResourcePoolEvent.EventType.ResourceAquired, this.clock.GetCurrentInstant()));

            this.pool.ResourceReleased += (object sender, EventArgs e)
                => this.Events.Add(
                    new ResourcePoolEvent(ResourcePoolEvent.EventType.ResourceReleased, this.clock.GetCurrentInstant()));

            this.pool.PoolExhausted += (object sender, EventArgs e)
                => this.Events.Add(
                    new ResourcePoolEvent(ResourcePoolEvent.EventType.PoolExhausted, this.clock.GetCurrentInstant()));

            this.pool.ResourceAdded += (object sender, EventArgs e)
                => this.Events.Add(
                    new ResourcePoolEvent(ResourcePoolEvent.EventType.ResourceAdded, this.clock.GetCurrentInstant()));

            this.pool.ResourceRemoved += (object sender, EventArgs e)
                => this.Events.Add(
                    new ResourcePoolEvent(ResourcePoolEvent.EventType.ResourceRemoved, this.clock.GetCurrentInstant()));

            if (this.pool.PoolSizeManager != null)
            {
                this.pool.PoolSizeManager.GrowPoolCompleted += (object sender, EventArgs e)
                    => this.Events.Add(
                        new ResourcePoolEvent(ResourcePoolEvent.EventType.PoolGrown, this.clock.GetCurrentInstant()));

                this.pool.PoolSizeManager.ResourcesWasted += (object sender, EventArgs e)
                    => this.Events.Add(
                        new ResourcePoolEvent(ResourcePoolEvent.EventType.ResourceWasted, this.clock.GetCurrentInstant()));
            }
        }

        private void TrimEventsPeriodically()
        {
            this.timer = new Timer(Duration.FromHours(1).TotalMilliseconds);
            this.timer.Elapsed += this.OnTimedEvent;
            this.timer.AutoReset = true;
            this.timer.Enabled = true;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            var cutoff = this.clock.GetCurrentInstant() - Duration.FromDays(RetentionDays);
            this.Events.RemoveAll(ev => ev.Timestamp < cutoff);
        }
    }
}
