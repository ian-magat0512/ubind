﻿// <copyright file="IDocumentFileRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Repository for storing document files.
    /// </summary>
    public interface IDocumentFileRepository
    {
        /// <summary>
        /// Retrieve the document file based on its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier for quote email model.</param>
        /// <returns>the filecontent in readmodelform.</returns>
        IFileContentReadModel GetFileContent(Guid id);
    }
}
