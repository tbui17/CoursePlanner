using BuildLib.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace BuildLib.Serialization;

[Inject(Lifetime = ServiceLifetime.Singleton)]
public class EncoderFactory(ILogger<EncoderFactory> log)
{
    public static EncoderFactory Create()
    {
        var logger = LoggerFactory.Create(x => x.AddSerilog()).CreateLogger<EncoderFactory>();
        return new EncoderFactory(logger);
    }

    public FileBase64Encoder CreateBase64Encoder(string inputPath, string? outputPath = null)
    {
        var file = new FileInfo(inputPath);
        var outputFile = outputPath is not null
            ? new FileInfo(outputPath)
            : file
                .FullName
                .Thru(Path.GetFileNameWithoutExtension)
                .Thru(x => $"output/{x}.txt")
                .Thru(x => new FileInfo(x));

        log.LogInformation("Encoding file {InputPath} and writing to {OutputPath}",
            file.FullName,
            outputFile.FullName
        );
        return new FileBase64Encoder { InputFile = file, OutputFile = outputFile };
    }


    public FileBase64Decoder CreateBase64Decoder(string base64, string outputPath, bool useOutputFolder = false)
    {
        var path = useOutputFolder ? "output/" + outputPath : outputPath;
        var file = new FileInfo(path);
        log.LogInformation(
            "Decoding base64 string of length {Length} to file {OutputPath}",
            base64.Length,
            file.FullName
        );
        return new FileBase64Decoder
        {
            Base64Text = base64,
            OutputFile = file
        };
    }
}

public class FileBase64Encoder
{
    public required FileInfo InputFile { get; init; }
    public required FileInfo OutputFile { get; init; }

    public async Task<string> GetBase64Text() =>
        await File
            .ReadAllBytesAsync(InputFile.FullName)
            .ContinueWith(x => Convert.ToBase64String(x.Result));

    public async Task WriteAsync()
    {
        var b64 = await GetBase64Text();
        await File.WriteAllTextAsync(OutputFile.FullName, b64);
    }
}

public class FileBase64Decoder
{
    public required string Base64Text { get; init; }
    public required FileInfo OutputFile { get; init; }

    public byte[] Decode() => Convert.FromBase64String(Base64Text);

    public async Task WriteAsync()
    {
        await File.WriteAllBytesAsync(OutputFile.FullName, Decode());
    }
}