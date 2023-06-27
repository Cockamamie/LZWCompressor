using System.Collections;
using LZMCompressor;

const string encoderInputPath = @"..\..\..\Test\Encoder\input.txt";
const string encoderOutputPath = @"..\..\..\Test\Encoder\output";
const string decoderInputPath = @"..\..\..\Test\Decoder\input";
const string decoderOutputPath = @"..\..\..\Test\Decoder\output.txt";
const int maxDictionarySize = 1 << 16;

var encoder = new LzwEncoder(maxDictionarySize);
using var sr = new StreamReader(encoderInputPath);
using var fs = new FileStream(encoderOutputPath, FileMode.Create);
using var bw = new BinaryWriter(fs);
using var sw = new StreamWriter(decoderOutputPath);
var decoderInput = File.ReadAllBytes(decoderInputPath);
encoder.Encode(sr, bw);
var decoder = new LzwDecoder(maxDictionarySize);
decoder.Decode(decoderInput, sw);
