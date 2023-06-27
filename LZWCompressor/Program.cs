using System.Collections;
using LZMCompressor;

const string encoderInputPath = @"..\..\..\Test\Encoder\input.txt";
const string encoderOutputPath = @"..\..\..\Test\Encoder\output";
const string decoderInputPath = @"..\..\..\Test\Decoder\input";
const string decoderOutputPath = @"..\..\..\Test\Decoder\output.txt";

var encoder = new LzwEncoder(1 << 12);
using var sr = new StreamReader(encoderInputPath);
using var fs = new FileStream(encoderOutputPath, FileMode.Create);
using var bw = new BinaryWriter(fs);
using var sw = new StreamWriter(decoderOutputPath);
var decoderInput = File.ReadAllBytes(decoderInputPath);
encoder.Encode(sr, bw);
var decoder = new LzwDecoder(1 << 12);
decoder.Decode(decoderInput, sw);
