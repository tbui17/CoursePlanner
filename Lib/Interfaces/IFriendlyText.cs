using Humanizer;
using Lib.Utils;

namespace Lib.Interfaces;

public interface IFriendlyText;

public static class FriendlyTextExtensions
{
    public static string ToFriendlyText(this IFriendlyText self) =>
        self.GetType()
            .GetProperties()
            .Select(prop =>
                {
                    var key = prop.Name.Humanize(LetterCasing.Title);
                    var value = prop.GetValue(self) switch
                    {
                        DateTime date => date.ToShortDateString(),
                        var x => x
                    };

                    var msg = $"{key}: {value}";

                    return msg;
                }
            )
            .StringJoin("\n");
}