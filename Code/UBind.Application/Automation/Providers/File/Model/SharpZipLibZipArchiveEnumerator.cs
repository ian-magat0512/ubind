// <copyright file="SharpZipLibZipArchiveEnumerator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File.Model
{
    using System.Collections;
    using System.Collections.Generic;
    using ICSharpCode.SharpZipLib.Zip;

    public class SharpZipLibZipArchiveEnumerator : IEnumerator<IArchiveEntry>
    {
        private IEnumerator enumerator;
        private SharpZipLibZipArchive zipArchive;

        public SharpZipLibZipArchiveEnumerator(SharpZipLibZipArchive zipArchive)
        {
            this.zipArchive = zipArchive;
            this.enumerator = zipArchive.ZipFile.GetEnumerator();
        }

        public IArchiveEntry Current => new SharpZipLibZipArchiveEntry(this.zipArchive, (ZipEntry)this.enumerator.Current);

        object IEnumerator.Current => this.Current;

        public void Dispose()
        {
            // no-op
        }

        public bool MoveNext()
        {
            return this.enumerator.MoveNext();
        }

        public void Reset()
        {
            this.enumerator.Reset();
        }
    }
}
