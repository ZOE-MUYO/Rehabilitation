using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    const int HEADER_SIZE = 44;

    public static byte[] FromAudioClip(AudioClip clip)
    {
        if (clip.samples == 0)
            return null;

        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        byte[] wav = ConvertToWav(samples, clip.channels, clip.frequency);
        return wav;
    }

    private static byte[] ConvertToWav(float[] samples, int channels, int sampleRate)
    {
        MemoryStream stream = new MemoryStream();
        int sampleCount = samples.Length;

        int byteRate = sampleRate * channels * 2;
        int fileSize = HEADER_SIZE + sampleCount * 2 - 8;

        // WAV Header
        stream.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"), 0, 4);
        stream.Write(BitConverter.GetBytes(fileSize), 0, 4);
        stream.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"), 0, 4);
        stream.Write(System.Text.Encoding.ASCII.GetBytes("fmt "), 0, 4);
        stream.Write(BitConverter.GetBytes(16), 0, 4);
        stream.Write(BitConverter.GetBytes((short)1), 0, 2);
        stream.Write(BitConverter.GetBytes((short)channels), 0, 2);
        stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);
        stream.Write(BitConverter.GetBytes(byteRate), 0, 4);
        stream.Write(BitConverter.GetBytes((short)(channels * 2)), 0, 2);
        stream.Write(BitConverter.GetBytes((short)16), 0, 2);
        stream.Write(System.Text.Encoding.ASCII.GetBytes("data"), 0, 4);
        stream.Write(BitConverter.GetBytes(sampleCount * 2), 0, 4);

        // Audio Data
        foreach (var sample in samples)
        {
            short intData = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
            byte[] data = BitConverter.GetBytes(intData);
            stream.Write(data, 0, 2);
        }

        return stream.ToArray();
    }

    public static AudioClip ToAudioClip(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("WAV file not found: " + filePath);
            return null;
        }

        byte[] fileBytes = File.ReadAllBytes(filePath);

        int channels = BitConverter.ToInt16(fileBytes, 22);
        int sampleRate = BitConverter.ToInt32(fileBytes, 24);
        int dataOffset = 44;
        int sampleCount = (fileBytes.Length - dataOffset) / 2;

        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            short value = BitConverter.ToInt16(fileBytes, dataOffset + i * 2);
            samples[i] = value / 32768f;
        }

        AudioClip clip = AudioClip.Create("WavClip", sampleCount, channels, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
