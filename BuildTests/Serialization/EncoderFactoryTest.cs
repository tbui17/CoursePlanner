using BuildLib.Serialization;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging.Abstractions;

namespace BuildTests.Serialization;

[TestSubject(typeof(EncoderFactory))]
public class EncoderFactoryTest
{
    [Fact]
    public void CreateBase64Encoder_NoOutputFileName_CreatesTxtFileWithDerivedNameInRelativeOutputFolder()
    {
        // no IO
        var factory = new EncoderFactory(new NullLogger<EncoderFactory>());
        var encoder = factory.CreateBase64Encoder("input.json");
        var outputFile = encoder.OutputFile;
        outputFile.Directory.Should().NotBeNull();
        outputFile.Directory.Name.Should().Be("output");
        outputFile.Name.Should().Be("input.txt");
    }

    [Fact]
    public async Task EncodeDecode_ProducesIdenticalString()
    {
        var factory = new EncoderFactory(new NullLogger<EncoderFactory>());
        const string inputLocation = "input.txt";
        const string outputLocation = "output.txt";
        const string decodeOutputLocation = "output2.txt";
        const string text = "hello\nworld";
        await File.WriteAllTextAsync(inputLocation, text);

        var encoder = factory.CreateBase64Encoder(inputLocation, outputLocation);
        await encoder.WriteAsync();
        var b64 = await File.ReadAllTextAsync(outputLocation);
        var decoder = factory.CreateBase64Decoder(b64, decodeOutputLocation);
        await decoder.WriteAsync();
        var decodedText = await File.ReadAllTextAsync(decodeOutputLocation);

        decodedText.Should().Be(text);
    }
}