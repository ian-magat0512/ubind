// <copyright file="ApplicationLifetimeManager.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Infrastructure
{
    public static class ApplicationLifetimeManager
    {
        private static readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        static ApplicationLifetimeManager()
        {
            // Register application exit events
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            Console.CancelKeyPress += OnCancelKeyPress;
        }

        public static CancellationToken ApplicationShutdownToken => _cancellationTokenSource.Token;

        private static void OnProcessExit(object sender, EventArgs e)
        {
            _cancellationTokenSource.Cancel();
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _cancellationTokenSource.Cancel();
        }
    }
}
