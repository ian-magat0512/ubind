// <copyright file="SqlServerSysProcessViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModels.Dba
{
    /// <summary>
    /// Wrapper for the summarized version of master.sys.sysprocesses.
    /// </summary>
    public class SqlServerSysProcessViewModel
    {
        public int DbId { get; set; }

        public string DbName { get; set; }

        public int NumberOfConnections { get; set; }

        public string LoginName { get; set; }

        public string Status { get; set; }
    }
}
