using Serilog.Templates;
using Serilog.Templates.Themes;

namespace BuildLib.Logging;

public static class LogUtil
{
    public static readonly ExpressionTemplate DefaultExpressionTemplate =
        new(
            template: "{ {@t, @l, SourceContext, @mt, @r, @x, @p} }\n",
            theme: TemplateTheme.Code
        );
}