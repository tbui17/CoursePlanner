using BuildLib.Serialization;
using BuildLib.Utils;
using FluentValidation;

namespace BuildLib.FileSystem;

[Inject]
public class FileNameValidator : AbstractValidator<string>
{
    private static readonly HashSet<char> InvalidFileNameChars = Path.GetInvalidFileNameChars().ToHashSet();


    public FileNameValidator(DefaultSerializer serializer)
    {
        RuleFor(x => x)
            .NotNull()
            .NotEmpty()
            .MaximumLength(255)
            .Custom((str, ctx) =>
                {
                    var invalids = str.Intersect(InvalidFileNameChars).ToList();
                    if (invalids.Count != 0)
                    {
                        ctx.AddFailure($"Invalid characters in file name: {serializer.Serialize(invalids)}");
                    }
                }
            );
    }
}

[Inject]
public class SolutionFileValidator : AbstractValidator<string>
{
    public SolutionFileValidator(FileNameValidator fileNameValidator)
    {
        RuleFor(x => x)
            .SetValidator(fileNameValidator)
            .Must(x => x.EndsWith(".sln"))
            .WithMessage(x => x + " must end with .sln");
    }
}

public class ProjectFileValidator : AbstractValidator<string>
{
    public ProjectFileValidator(FileNameValidator fileNameValidator)
    {
        RuleFor(x => x)
            .SetValidator(fileNameValidator)
            .Must(x => x.EndsWith(".csproj"))
            .WithMessage(x => x + " must end with .sln");
    }
}