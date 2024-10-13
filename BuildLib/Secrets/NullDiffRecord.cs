using BuildLib.Utils;

namespace BuildLib.Secrets;

public record NullDiffRecord
{
    public required IList<ObjectNode> Nulls { get; init; }
    public required IList<ObjectNode> NotNulls { get; init; }
}