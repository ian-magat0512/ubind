// <copyright file="ReleaseNumber.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ValueTypes;

using UBind.Domain.Exceptions;

public class ReleaseNumber : ValueObject
{
    private readonly string releaseNumber;
    private int major;
    private int? minor;
    private int? patch;

    public ReleaseNumber(string releaseNumber)
    {
        if (string.IsNullOrEmpty(releaseNumber))
        {
            throw new ErrorException(Errors.Release.NumberInvalid(releaseNumber));
        }

        this.releaseNumber = releaseNumber;
        var parts = this.releaseNumber.Split('.');
        if (parts.Length > 3)
        {
            throw new ErrorException(Errors.Release.NumberInvalid(releaseNumber));
        }

        if (!int.TryParse(parts[0], out this.major))
        {
            throw new ErrorException(Errors.Release.NumberInvalid(releaseNumber));
        }

        if (parts.Length == 1)
        {
            return;
        }

        int minor;
        if (!int.TryParse(parts[1], out minor))
        {
            throw new ErrorException(Errors.Release.NumberInvalid(releaseNumber));
        }

        this.minor = minor;
        if (parts.Length == 2)
        {
            return;
        }

        int patch;
        if (!int.TryParse(parts[2], out patch))
        {
            throw new ErrorException(Errors.Release.NumberInvalid(releaseNumber));
        }

        this.patch = patch;
    }

    public int Major => int.Parse(this.releaseNumber.Split('.')[0]);

    public int? Minor => int.Parse(this.releaseNumber.Split('.')[1]);

    public int? Patch => int.Parse(this.releaseNumber.Split('.')[2]);

    public static implicit operator ReleaseNumber(string releaseNumber)
    {
        return new ReleaseNumber(releaseNumber);
    }

    public static explicit operator string(ReleaseNumber releaseNumber)
    {
        return releaseNumber.ToString();
    }

    public override string ToString()
    {
        return this.releaseNumber;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return this.releaseNumber;
    }
}
