using NAudio.MediaFoundation;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AsLink
{
  public static class NAudioHelper
  {
    public static TimeSpan GetDuration(string file) => new Mp3FileReader(file).TotalTime;
    public static void ConvertWavToMp3(string wav, string mp3 = null)
    {
      try
      {
        if (mp3 == null)
          mp3 = $"{wav}.mp3";

        var wavFrmt = getWaveFormat(wav);

        var mt1 = MediaFoundationEncoder.GetOutputMediaTypes(AudioSubtypes.MFAudioFormat_MP3).FirstOrDefault(m => m != null && m.SampleRate == wavFrmt.SampleRate && m.ChannelCount == wavFrmt.Channels);
        if (mt1 != null)
        {
          encode(wav, mp3, mt1);
        }
      }
      catch (Exception ex) { System.Diagnostics.Trace.WriteLine(ex.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name); if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break(); throw; }
    }
    public static void ConvertMp3ToWav(string mp3, string wav)
    {
      using (var mp3_ = new Mp3FileReader(mp3))
      {
        using (var pcm = WaveFormatConversionStream.CreatePcmStream(mp3_))
        {
          WaveFileWriter.CreateWaveFile(wav, pcm);
        }
      }
    }
    public static void DevDbgPoc__________(string wav)
    {
      try
      {
        var waveFormat = getWaveFormat(wav);
        var i = 0;
        foreach (var mt in MediaFoundationEncoder.GetOutputMediaTypes(AudioSubtypes.MFAudioFormat_MP3).Where(mt => mt != null && mt.SampleRate == waveFormat.SampleRate && mt.ChannelCount == waveFormat.Channels))
        {
          encode(wav, $@"{wav} - {++i} - {ShortDescription(mt)}.mp3", mt);
        }
      }
      catch (Exception e) { Debug.WriteLine($@"Not a supported input file ({e.Message})"); }
    }
    public static string ShortDescription(MediaType mediaType)
    {
      var subType = mediaType.SubType;
      var sampleRate = mediaType.SampleRate;
      var bytesPerSecond = mediaType.AverageBytesPerSecond;
      var channels = mediaType.ChannelCount;
      var bitsPerSample = mediaType.TryGetUInt32(MediaFoundationAttributes.MF_MT_AUDIO_BITS_PER_SAMPLE);

      //int bitsPerSample;
      //mediaType.GetUINT32(MediaFoundationAttributes.MF_MT_AUDIO_BITS_PER_SAMPLE, out bitsPerSample);
      var shortDescription = new StringBuilder();
      shortDescription.AppendFormat("{0:0.#}kbps, ", (8 * bytesPerSecond) / 1000M);
      shortDescription.AppendFormat("{0:0.#}kHz, ", sampleRate / 1000M);
      if (bitsPerSample != -1)
        shortDescription.AppendFormat("{0} bit, ", bitsPerSample);

      shortDescription.AppendFormat("{0}, ", channels == 1 ? "mono" : channels == 2 ? "stereo" : $"{channels} channels");

      if (subType == AudioSubtypes.MFAudioFormat_AAC)
      {
        var payloadType = mediaType.TryGetUInt32(MediaFoundationAttributes.MF_MT_AAC_PAYLOAD_TYPE);
        //if (payloadType != -1)
        //	shortDescription.AppendFormat("Payload Type: {0}, ", (AacPayloadType)payloadType);
      }
      shortDescription.Length -= 2;
      return shortDescription.ToString();
    }
    public static void DoFolder_Mp3ToWav(string dir)
    {
      foreach (var mp3 in Directory.GetFiles(dir, "*.mp3"))
      {
        var wav = mp3.Replace(".mp3", ".wav");
        if (!File.Exists(wav))
          ConvertMp3ToWav(mp3, wav);
      }
    }

    static WaveFormat getWaveFormat(string srcWavFile)
    {
      WaveFormat wavFrmt;
      using (var reader = new MediaFoundationReader(srcWavFile)) { wavFrmt = reader.WaveFormat; }
      return wavFrmt;
    }
    static void encode(string srcFile, string trgFile, MediaType trgMediaType)
    {
      if (string.IsNullOrEmpty(srcFile) || !File.Exists(srcFile)) { Debug.WriteLine("Please select a valid input file to convert"); return; }
      if (!string.IsNullOrEmpty(trgFile) && File.Exists(trgFile)) { File.Delete(trgFile); }

      using (var reader = new MediaFoundationReader(srcFile))
      {
        using (var encoder = new MediaFoundationEncoder(trgMediaType))
        {
          encoder.Encode(trgFile, reader);
        }
      }
    }
  }
}
