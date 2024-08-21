// <copyright file="ReleaseNumberModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    /// <summary>
    /// Represents Release Numbers of a Release.
    /// </summary>
    public class ReleaseNumberModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseNumberModel"/> class.
        /// Release number constructor.
        /// </summary>
        /// <param name="majorNumber">major release number.</param>
        /// <param name="minorNumber">minor release number.</param>
        public ReleaseNumberModel(int majorNumber, int minorNumber)
        {
            this.MajorReleaseNumber = majorNumber;
            this.MinorReleaseNumber = minorNumber;
        }

        /// <summary>
        /// Gets or sets gets major Release Number.
        /// </summary>
        public int MajorReleaseNumber { get; set; }

        /// <summary>
        /// Gets or sets gets minor Release Number.
        /// </summary>
        public int MinorReleaseNumber { get; set; }

        /// <summary>
        /// Gets combination of the two.
        /// </summary>
        public string ReleaseNumber
        {
            get
            {
                return this.MajorReleaseNumber + "." + this.MinorReleaseNumber;
            }
        }

        /// <summary>
        /// Increments the release number for a new release.
        /// </summary>
        /// <param name="type">The type of release.</param>
        public void IncrementForRelease(ReleaseType type)
        {
            if (type == ReleaseType.Major)
            {
                this.MajorReleaseNumber++;
                this.MinorReleaseNumber = 0;
            }
            else
            {
                this.MinorReleaseNumber++;
            }
        }
    }
}
