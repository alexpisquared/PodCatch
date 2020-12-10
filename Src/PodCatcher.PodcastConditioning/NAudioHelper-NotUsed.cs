using NAudio.MediaFoundation;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PodcastConditioning
{
  public static class NAudioHelper
  {
    const string
      tw = @"C:\Temp\352.wav",
      tm = @"C:\Temp\Sample.mp3";

    static string InputFile, InputFormat;
    static WaveFormat inputWaveFormat;
    public static async void Test()
    {
      try
      {

        var mt = new MediaType
        {
          ChannelCount = 2,
          //AverageBytesPerSecond = 32000,
          SampleRate = 48000
        };



        using (var reader = new MediaFoundationReader(tw))
        {
          string outputUrl = tm;           if (outputUrl == null) return;


          using (var encoder = new MediaFoundationEncoder(mt))
          {
            encoder.Encode(outputUrl, reader);
          }
        }






        foreach (var f in Directory.GetFiles(@"C:\temp", "*.WAV"))
        {
          ConvertWavToMp3(f, f + ".mp3");
        }

        SpeechSynthesizer synth = new SpeechSynthesizer();

        var fi = new SpeechAudioFormatInfo(4000, AudioBitsPerSample.Eight, AudioChannel.Stereo);
        /// 48000x8 = 384
        /// 48000x16 = 768
        synth.SetOutputToWaveFile(tw, fi);
        synth.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(synth_SpeakCompleted);

        var builder = new PromptBuilder();
        builder.AppendText("This sample asynchronously speaks a prompt to a WAVE file.");

        synth.SpeakAsync(builder);




        var wav = @"D:\Users\alex\Videos\0Pod\Cuts\BPr-6659,`Hacking Addiction with Dr. Mark – #351\[002].--- 70 ---.wav";
        var mp3 = @"D:\Users\alex\Videos\0Pod\Cuts\BPr-6659,`Hacking Addiction with Dr. Mark – #351\[002].--- 70 ---.mp3";
        //NAudioHelper.ConvertWavToMp3(wav, mp3);

        await Task.Delay(999);
        Thread.Sleep(1500);
      }
      catch (Exception ex) { System.Diagnostics.Trace.WriteLine(ex.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name); if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break(); throw; }
    }


    // Handle the SpeakCompleted event.
    static void synth_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
    {
      System.Media.SoundPlayer m_SoundPlayer = new System.Media.SoundPlayer(tw);

      m_SoundPlayer.PlaySync();

      ConvertWavToMp3(tw, tm);
    }


    public static void DoFolder(string dir)
    {
      foreach (var mp3 in Directory.GetFiles(dir, "*.mp3"))
      {
        var wav = mp3.Replace(".mp3", ".wav");

        if (!File.Exists(wav))
          ConvertMp3ToWav(mp3, wav);
      }
    }
    public static void ConvertWavToMp3_POC(string wav, string mp3) // NOT TESTED  see more at     http://mark-dot-net.blogspot.ca/2015/02/how-to-encode-mp3s-with-naudio.html
    {
      var wav2 = @"D:\Users\alex\Videos\0Pod\Cuts\BPr-6659,`Hacking Addiction with Dr. Mark – #351\[012].--- 65 ---.wav";
      try
      {
        var mediaType = MediaFoundationEncoder.SelectMediaType(AudioSubtypes.MFAudioFormat_WMAudioV8, new WaveFormat(16000, 1), 16000); if (mediaType != null) Debug.WriteLine(" we can encode");

        mediaType = MediaFoundationEncoder.SelectMediaType(AudioSubtypes.MFAudioFormat_MP3, new WaveFormat(44100, 1), 0); if (mediaType != null) Debug.WriteLine(" we can encode");
        mediaType = MediaFoundationEncoder.SelectMediaType(AudioSubtypes.MFAudioFormat_MP3, new WaveFormat(352000, 1), 0); if (mediaType != null) Debug.WriteLine(" we can encode");

        var incoming = new WaveFileReader(wav);
        var outgoing = new WaveFileReader(wav2);

        var mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(16000, 1));
        Debug.WriteLine(mixer.WaveFormat.ToString());

        // add the inputs - they will automatically be turned into ISampleProviders 
        mixer.AddMixerInput(incoming);
        mixer.AddMixerInput(outgoing);

        //var truncateAudio = true;        // optionally truncate to 30 second for unlicensed users 
        var truncated = //truncateAudio ? new OffsetSampleProvider(mixer) { Take = TimeSpan.FromSeconds(30) } : 
          (ISampleProvider)mixer;

        // go back down to 16 bit PCM 
        var converted16Bit = new SampleToWaveProvider16(truncated);

        // now for MP3, we need to upsample to 44.1kHz. Use MediaFoundationResampler 
        using (var resampled = new MediaFoundationResampler(converted16Bit, new WaveFormat(44100, 1)))
        {
          var desiredBitRate = 0; // ask for lowest available bitrate 
          MediaFoundationEncoder.EncodeToMp3(resampled, "mixed.mp3", desiredBitRate);
        }










        var myWaveProvider = (IWaveProvider)new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(16000, 1));

        using (var enc = new MediaFoundationEncoder(mediaType))
        {
          enc.Encode("output.wma", myWaveProvider);
        }

        using (var reader = new WaveFileReader(wav))
        {
          MediaFoundationEncoder.EncodeToMp3(reader, mp3, 48000);
        }

      }
      catch (Exception ex) { System.Diagnostics.Trace.WriteLine(ex.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name); if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break(); throw; }
    }
    public static void ConvertWavToMp3(string wav, string mp3) // NOT TESTED  see more at     http://mark-dot-net.blogspot.ca/2015/02/how-to-encode-mp3s-with-naudio.html
    {
      try
      {
        using (var reader = new WaveFileReader(wav))
        {
          MediaFoundationEncoder.EncodeToMp3(reader, mp3, 48000);
        }
      }
      catch (Exception ex) { System.Diagnostics.Trace.WriteLine(ex.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name); if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break(); throw; }
    }
    public static void ConvertMp3ToWav(string _inPath_, string _outPath_)
    {
      using (var mp3 = new Mp3FileReader(_inPath_))
      {
        using (WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(mp3))
        {
          WaveFileWriter.CreateWaveFile(_outPath_, pcm);
        }
      }
    }
  }
}
