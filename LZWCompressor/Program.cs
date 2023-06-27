using LZMCompressor;
using Mono.Options;

var encoderInputPath = string.Empty;
var decoderInputPath = string.Empty;
var dictionarySize = 1 << 16;
var help = false;
var options = new OptionSet
{
    {"e|encode=", "encode", input => { encoderInputPath = input;}},
    {"d|decode=", "decode", input => { decoderInputPath = input;}},
    {"ds|dictionarySize=", "max dictionary size(default = 2^16)", (int size) => { dictionarySize = size;}},
    {"h|help", "help", v => { help = true; }}
};
var unknown = options.Parse(args);
if (unknown.Count > 0 || help)
{
    if (unknown.Count > 0)
    {
        Console.WriteLine("Unknown parameters:");
        foreach (var p in unknown)
        {
            Console.WriteLine(p);
        }
    }

    options.WriteOptionDescriptions(Console.Out);
    return;
}

if (!string.IsNullOrEmpty(encoderInputPath))
{
    var folderPath = Path.GetDirectoryName(encoderInputPath);
    var encoderOutputPath = folderPath + @"\encoded" + $"_{DateTime.Now:yyyyMMddTHHmmss}";
    using var sr = new StreamReader(encoderInputPath);
    using var fs = new FileStream(encoderOutputPath, FileMode.Create);
    using var bw = new BinaryWriter(fs);
    var encoder = new LzwEncoder(dictionarySize);
    encoder.Encode(sr, bw);
    ShowCompressionResultInfo(encoderInputPath, encoderOutputPath);
}

if (!string.IsNullOrEmpty(decoderInputPath))
{
    var folderPath = Path.GetDirectoryName(encoderInputPath);
    var decoderOutputPath = folderPath + @"\decoded" + $"_{DateTime.Now:yyyyMMddTHHmmss}" + ".txt";
    using var sw = new StreamWriter(decoderOutputPath);
    var decoderInput = File.ReadAllBytes(decoderInputPath);
    var decoder = new LzwDecoder(dictionarySize);
    decoder.Decode(decoderInput, sw);
}

void ShowCompressionResultInfo(string initialFile, string resultFile)
{
    var initialLength = new FileInfo(initialFile).Length;
    var resultLength = new FileInfo(resultFile).Length;
    Console.WriteLine($"Compression ratio = [{initialLength / (double) resultLength}], " +
                      $"Initial size = [{initialLength}] MiB, Result size = [{resultLength}] MiB");
}