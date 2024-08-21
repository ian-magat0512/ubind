// <copyright file="ExceptionHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers
{
    using System;
    using UBind.Domain.Extensions;

    public static class ExceptionHelper
    {
        public static string GetAllMessages(this Exception ex)
        {
            string messages = string.Empty;
            while (ex != null)
            {
                messages += ex.Message;
                if (ex.InnerException != null)
                {
                    messages = messages.WithDot().WithSpace();
                }

                ex = ex.InnerException;
            }

            return messages;
        }
    }
}
