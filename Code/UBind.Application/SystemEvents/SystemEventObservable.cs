// <copyright file="SystemEventObservable.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.SystemEvents
{
    using System;
    using System.Reactive;
    using System.Reactive.Disposables;
    using UBind.Domain.Events;

    /// <inheritdoc/>
    public class SystemEventObservable : ObservableBase<SystemEvent>, ISystemEventObservable
    {
        /// <summary>
        /// This will notify multiple observers the value they expect.
        /// </summary>
        private event Action<SystemEvent> Notify;

        /// <summary>
        /// This will notify multiple observers that it will receive the last value it will get from this observable.
        /// </summary>
        private event Action Closed;

        /// <summary>
        /// This will notify multiple observers that it will get an error for some reason.
        /// </summary>
        private event Action<Exception> Error;

        /// <inheritdoc/>
        public void Trigger(SystemEvent systemEvent)
        {
            try
            {
                this.Notify?.Invoke(systemEvent);
            }
            catch (Exception ex)
            {
                this.Error?.Invoke(ex);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Closed?.Invoke();
        }

        /// <inheritdoc/>
        protected override IDisposable SubscribeCore(IObserver<SystemEvent> observer)
        {
            Action<SystemEvent> notify = message => observer.OnNext(message);

            Action closed = () => observer.OnCompleted();

            Action<Exception> error = err => observer.OnError(err);

            this.Notify += notify;
            this.Error += error;
            this.Closed += closed;

            return Disposable.Create(() =>
            {
                this.Notify -= notify;
                this.Error -= error;
                this.Closed -= closed;
            });
        }
    }
}
