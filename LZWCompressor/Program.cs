using LZMCompressor;
using Mono.Options;

var encoderInputPath = string.Empty;
var decoderInputPath = string.Empty;
var checkTextPath = string.Empty;
var dictionarySize = 1 << 16;
var help = false;
var options = new OptionSet
{
    {"e|encode=", "encode", input => { encoderInputPath = input;}},
    {"d|decode=", "decode", input => { decoderInputPath = input;}},
    {"ds|dictionarySize=", "max dictionary size(default = 2^16)", (int size) => { dictionarySize = size;}},
    {"c|check=", "check text", input => { checkTextPath = input;}},
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
    var folderPath = new FileInfo(encoderInputPath).Directory?.FullName;
    var encoderOutputPath = folderPath + @"\encoded" + $"_{DateTime.Now:yyyyMMddTHHmmss}";
    using var sr = new StreamReader(encoderInputPath);
    using var fs = new FileStream(encoderOutputPath, FileMode.Create);
    using var bw = new BinaryWriter(fs);
    var encoder = new LzwEncoder(dictionarySize);
    encoder.Encode(sr, bw);
    Console.WriteLine("Saved encoding result to " + encoderOutputPath);
    ShowCompressionResultInfo(encoderInputPath, encoderOutputPath);
}

if (!string.IsNullOrEmpty(decoderInputPath))
{
    var folderPath = new FileInfo(decoderInputPath).Directory?.FullName;
    var decoderOutputPath = folderPath + @"\decoded" + $"_{DateTime.Now:yyyyMMddTHHmmss}" + ".txt";
    using var sw = new StreamWriter(decoderOutputPath);
    var decoderInput = File.ReadAllBytes(decoderInputPath);
    var decoder = new LzwDecoder(dictionarySize);
    decoder.Decode(decoderInput, sw);
    Console.WriteLine("Saved decoding result to " + decoderOutputPath);
}

if (!string.IsNullOrEmpty(checkTextPath))
{
    CheckTextHasCorrectSymbols(checkTextPath);
}

void ShowCompressionResultInfo(string initialFile, string resultFile)
{
    var initialLength = new FileInfo(initialFile).Length;
    var resultLength = new FileInfo(resultFile).Length;
    Console.WriteLine($"Compression ratio = [{initialLength / (double) resultLength}], " +
                      $"Initial size = [{initialLength}] Byte, Result size = [{resultLength}] Byte");
}

void CheckTextHasCorrectSymbols(string textPath)
{
    var hasIncorrectSymbols = false;
    var checkDictionary = InitializeDictionary();
    var lines = File.ReadAllLines(textPath);
    var incorrectSymbolsMessages = new List<string>();
    for (var j = 0; j < lines.Length; j++)
    {
        var line = lines[j];
        for (var i = 0; i < line.Length; i++)
        {
            var symbol = line[i];
            if (checkDictionary.ContainsKey(symbol))
                continue;
            hasIncorrectSymbols = true;
            incorrectSymbolsMessages.Add($"[{symbol}] on line {j} column {i}");
        }
    }

    if (!hasIncorrectSymbols)
    {
        Console.WriteLine("Text is OK");
        return;
    }
    
    Console.WriteLine($"Text contains {incorrectSymbolsMessages.Count} incorrect symbols(positions start from 0):");
    Console.WriteLine(string.Join("\n", incorrectSymbolsMessages));
}

static Dictionary<char, int> InitializeDictionary()
{
    var d = new Dictionary<char, int>();
    for (var i = (char) byte.MinValue; i <= byte.MaxValue; i++)
    {
        d.Add(i, i);
    }

    return d;
}