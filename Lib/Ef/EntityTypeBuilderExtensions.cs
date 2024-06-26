using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lib.Ef;

public static class EntityTypeBuilderExtensions
{
    public static PropertyBuilder<T> IsEnum<T>(this PropertyBuilder<T> builder) where T : Enum =>
        builder
           .HasConversion
            (
                x => x.ToString(),
                x => (T)Enum.Parse(typeof(T), x)
            );
}