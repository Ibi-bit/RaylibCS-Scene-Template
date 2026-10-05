using System.Buffers.Binary;
using Raylib_cs;

namespace My2DGame.Audio;

/// <summary>
/// A little retro "coin" blip, synthesized as a 16-bit PCM WAV in memory.
/// The template therefore makes sound without shipping a single asset file.
/// Swap this for real clips loaded from Content/ once you have your own audio.
/// </summary>
public sealed class CoinBlip : IDisposable
{
    private const int SampleRate = 22050;
    private const int BitsPerSample = 16;
    private const int Channels = 1;
    private const short Amplitude = 12000;

    private readonly Sound _sound;

    public CoinBlip()
    {
        Wave wave = Raylib.LoadWaveFromMemory(".wav", SynthSineSweep(0.25f, 900f, 1500f));
        _sound = Raylib.LoadSoundFromWave(wave);
        Raylib.UnloadWave(wave);
        Raylib.SetSoundVolume(_sound, 0.5f);
    }

    public void Play() => Raylib.PlaySound(_sound);

    public void Dispose() => Raylib.UnloadSound(_sound);

    /// <summary>Rising sine sweep with a fast decay envelope — the classic coin sound.</summary>
    private static byte[] SynthSineSweep(float seconds, float startFrequency, float endFrequency)
    {
        int sampleCount = (int)(seconds * SampleRate);
        int dataSize = sampleCount * (BitsPerSample / 8);
        byte[] wav = new byte[44 + dataSize];
        WriteWavHeader(wav, sampleCount, dataSize);

        Span<byte> samples = wav.AsSpan(44);
        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)SampleRate;
            float progress = i / (float)sampleCount;
            float frequency = startFrequency + (endFrequency - startFrequency) * (progress * progress);
            float envelope = MathF.Exp(-4.5f * progress);
            float sample = MathF.Sin(2f * MathF.PI * frequency * t) * envelope * Amplitude;
            short value = (short)Math.Clamp((int)sample, short.MinValue, short.MaxValue);
            BinaryPrimitives.WriteInt16LittleEndian(samples.Slice(i * 2, 2), value);
        }
        return wav;
    }

    /// <summary>Standard 44-byte RIFF/WAVE header for 1-channel 16-bit PCM.</summary>
    private static void WriteWavHeader(Span<byte> wav, int sampleCount, int dataSize)
    {
        int byteRate = SampleRate * Channels * (BitsPerSample / 8);
        int blockAlign = Channels * (BitsPerSample / 8);

        BinaryPrimitives.WriteUInt32LittleEndian(wav, 0x46464952); // "RIFF"
        BinaryPrimitives.WriteUInt32LittleEndian(wav.Slice(4), (uint)(36 + dataSize));
        BinaryPrimitives.WriteUInt32LittleEndian(wav.Slice(8), 0x45564157); // "WAVE"
        BinaryPrimitives.WriteUInt32LittleEndian(wav.Slice(12), 0x20746D66); // "fmt "
        BinaryPrimitives.WriteUInt32LittleEndian(wav.Slice(16), 16); // fmt chunk size
        BinaryPrimitives.WriteUInt16LittleEndian(wav.Slice(20), 1); // PCM
        BinaryPrimitives.WriteUInt16LittleEndian(wav.Slice(22), (ushort)Channels);
        BinaryPrimitives.WriteUInt32LittleEndian(wav.Slice(24), (uint)SampleRate);
        BinaryPrimitives.WriteUInt32LittleEndian(wav.Slice(28), (uint)byteRate);
        BinaryPrimitives.WriteUInt16LittleEndian(wav.Slice(32), (ushort)blockAlign);
        BinaryPrimitives.WriteUInt16LittleEndian(wav.Slice(34), BitsPerSample);
        BinaryPrimitives.WriteUInt32LittleEndian(wav.Slice(36), 0x64617461); // "data"
        BinaryPrimitives.WriteUInt32LittleEndian(wav.Slice(40), (uint)dataSize);
    }
}