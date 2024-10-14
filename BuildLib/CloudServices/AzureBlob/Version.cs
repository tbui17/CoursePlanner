namespace BuildLib.CloudServices.AzureBlob;

public record Version : IComparable<Version>
{
    public int Major { get; init; }
    public int Minor { get; init; }
    public int Patch { get; init; }


    public int CompareTo(Version? other)
    {
        if (other is null) return 1;
        if (this == other) return 0;

        var items = new[] { this, other }
            .OrderBy(x => x.Major)
            .ThenBy(x => x.Minor)
            .ThenBy(x => x.Patch);

        return items.First() == this ? -1 : 1;
    }


    public Version UpdatePatch() => this with
    {
        Patch = Patch + 1
    };

    public Version UpdateMinor() => this with
    {
        Minor = Minor + 1,
        Patch = 0
    };

    public Version UpdateMajor() => new() { Major = Major + 1, Minor = 0, Patch = 0 };

    public static bool TryParse(string versionStr, out Version version)
    {
        try
        {
            version = Parse(versionStr);
            return true;
        }
        catch (FormatException)
        {
            version = null!;
            return false;
        }
    }

    public static Version Parse(string versionStr)
    {
        var parts = versionStr.Split(".");
        if (parts.Length != 3)
        {
            throw new FormatException($"Version string must have 3 parts. Found {parts.Length} parts in {versionStr}");
        }

        try
        {
            var res = parts.Select(int.Parse).ToList();
            return new Version
            {
                Major = res[0],
                Minor = res[1],
                Patch = res[2]
            };
        }
        catch (FormatException e)
        {
            throw new FormatException($"Failed to parse integers from parts in ${versionStr}", e);
        }
    }

    public static implicit operator string(Version version) => version.ToString();

    public override string ToString() => $"{Major}.{Minor}.{Patch}";
}