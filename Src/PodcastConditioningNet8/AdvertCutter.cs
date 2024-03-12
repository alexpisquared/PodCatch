using StandardLib.Extensions;//using AAV.Sys.Ext;
using PodcastClientTpl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AsLink;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;

namespace PodcastConditioning
{
  public class AdvertCutter
  {
    const int _defaultCut1minIn = 2; //aug2015: from1to2. may2013: from2to1 (or from 30 sec to 60 sec): to fix the problem of the player not paying all the shows (and reduce copying/deleting times).
    static readonly DateTime _last = DateTime.MinValue;

    public static void CreateSummaryAnons(double ttlDurationMin, string trgDir)
    {
      try
      {
        foreach (var file in Directory.GetFiles(trgDir, "0000*.wav.mp3", SearchOption.TopDirectoryOnly))
          File.Delete(file);

        var dir = trgDir.Split('\\').LastOrDefault()?.Replace("_NoAn", "")?.Replace("_", " ");
        var msg =
          ttlDurationMin < 120 ?
          $"Folder   {dir} contains {ttlDurationMin:N0} minutes." :
          $"Folder   {dir} contains over {ttlDurationMin / 60.0:N0} hours.";

        CreateWavConvertToMp3File($@"{trgDir}\0000 BeginningEndOfAllShows {DateTime.Now:MMdd-HHmm}  .wav.mp3", msg);
      }
      catch (Exception ex) { ex.Log(); }
    }

    public static void CreateOverwriteAnons(string feedName, DateTime pubDate, string castTitle, int castsLeft, double durationLeftMin, double durnMin, string srcFile) // used 
    {
      if (srcFile.EndsWith(MediaHelper.AnonsExt)) return;

      var anonsFile = CalcAnonsFilename(srcFile);

      if (File.Exists(anonsFile)) File.Delete(anonsFile);

      if (durnMin > 2)
        try
        {
          var dn = (durnMin == ConstHelper.Unknown4004Duration) ? "unknown " : $"{durnMin:N0}";
          var py = (DateTime.Today - pubDate).TotalDays > 732 ? $"{pubDate.Year}" : (DateTime.Today - pubDate).TotalDays > 150 ? $"{pubDate: MMMM d yyyy}" : $"{pubDate: MMMM d}";
          var ss = $"'{castTitle}', (duration: {dn} minutes).";
          if (castsLeft < 10)
            ss += $" Followed by {(castsLeft - 1)} others.";

          //var ss = $"{dn} minutes of '{castTitle}', followed by {(castsLeft - 1)} others."; // marathon mode
          //var ss = $"And now {dn} minutes of '{feedName}', titled '{castTitle}', followed by {(castsLeft - 1)} others."; 
          //var ss = $"{(durationLeftMin / 60.0):N1} hours left of {castsLeft} shows.        Starting {dn} minutes of '{feedName}', titled '{castTitle}', published {py}";
          CreateWavConvertToMp3File(anonsFile, ss);
        }
        catch (Exception ex) { ex.Log(); }
    }

    public static string CalcAnonsFilename(string anonsFile)
    {
      var nm = Path.GetFileNameWithoutExtension(anonsFile);
      var xt = Path.GetExtension(anonsFile);
      anonsFile = anonsFile.Replace(nm, nm + " ").Replace(xt, ".wav.mp3");
      return anonsFile;
    }

    public static void OffsetAndCutIntoPieces(string feedName, string mp3file, long oneMinuteSizeInBytes, long advOffset, string srcDir, string trgDir, DateTime pubDate, string castTitle, int castsLeft, double ttlDurationMin, double durationLeftMin, double partSizeMin) // old
    {
      if (!File.Exists(mp3file)) return;

      if (oneMinuteSizeInBytes < 1000) oneMinuteSizeInBytes = 3000001;

      try
      {
        using (var org = File.Open(mp3file, FileMode.Open, FileAccess.Read, FileShare.Read))//; // new FileStream(fi, FileMode.Open, FileAccess.Read);
        {
          var advFreeLen = (int)(org.Length - advOffset);
          var pieceCount = 1 + (int)(advFreeLen / (oneMinuteSizeInBytes * partSizeMin));
          var pieceLen = 1 + advFreeLen / pieceCount;
          if (pieceLen < 0)
            return;

          org.Position = advOffset;
          org.Seek((advOffset), SeekOrigin.Begin);

          Debug.Assert(org.Length - advOffset == advFreeLen, "AP: Casting LONG to INT resulted in loss of correct value....");

          var ext = Path.GetExtension(mp3file);

          var fileDir = mp3file.Replace(ext, "").Replace(srcDir, trgDir);
          if (!Directory.Exists(fileDir)) Directory.CreateDirectory(fileDir);

          if (ttlDurationMin > 2)
            CreateWavConvertToMp3File(mp3file.Replace(ext, "\\[000]---Intro---.wav").Replace(srcDir, trgDir),
              //$"Starting {(advFreeLen / oneMinuteSizeInBytes):N0} minutes of '{feedName}', titled '{castTitle}'"
              $"{(durationLeftMin / 60.0):N1} hours left of {castsLeft} shows. Starting {(advFreeLen / oneMinuteSizeInBytes):N0} minutes of '{feedName}', titled '{castTitle}', published {pubDate: MMMM d} {((DateTime.Today - pubDate).TotalDays > 150 ? pubDate.Year.ToString() : "")}"
              );

          var min5piece = (int)((partSizeMin <= 1 ? 5 : 10) / partSizeMin);
          for (var pieceNum = 0; pieceNum < pieceCount - 0; pieceNum++) // 
          {
            var trgPiece = mp3file.Replace(ext, $"\\[{pieceNum + 1:00#}]{ext}").Replace(srcDir, trgDir);

            //string oldseparatr = trgPiece.Replace(ext, "~" + ext); if (File.Exists(oldseparatr)) File.Delete(oldseparatr);

            if (//pieceNum > min5piece &&                       // skip first not exact 60 * partSizeMin seconds piece
                    (((pieceCount - pieceNum) % min5piece == 0) ||    // every fifth part
                    (pieceCount - pieceNum == 1 / partSizeMin)))            // last 1.
            {
              var wavNum = trgPiece.Replace(ext, ".--- " + (((pieceCount - pieceNum) * partSizeMin)).ToString() + " ---.wav");
              if (!File.Exists(wavNum))
                CreateWavConvertToMp3File(wavNum, pieceCount - pieceNum == 1 / partSizeMin ? "This is the very Last minute" :
                                $"Now, {(pieceCount - pieceNum) * partSizeMin} min left.");
            }

            var biteContentOfTheFile = new byte[pieceLen];
            if (!File.Exists(trgPiece))
            {
              var trg = File.OpenWrite(trgPiece);
              trg.Write(biteContentOfTheFile, 0, org.Read(biteContentOfTheFile, 0, pieceLen));
              trg.Close();
              //no real need to: setFileDates(trgPiece, pubDate);
            }
          }

          org.Close();    //jun2010, Too much to listen to: createWavFile(mp3file.Replace(ext, ".[ZZ].wav").Replace(srcDir, trgDir), string.Format("End of file {0}.", Path.GetFileNameWithoutExtension(mp3file).Substring(6)));
        }
      }
      catch (Exception ex) { ex.Log(); }
    }

    [Obsolete]
    public static List<string> CutIntoPieces_OLD(string mp3file, long pieceCount)
    {
      if (pieceCount <= 0)
        return null;

      var ls = new List<string>();
      var fi = new FileInfo(mp3file);
      var piecLen = (int)(1 + fi.Length / pieceCount);

      var org = File.Open(mp3file, FileMode.Open, FileAccess.Read, FileShare.Read); // new FileStream(fi, FileMode.Open, FileAccess.Read);
                                                                                    //org.Position = offset;
                                                                                    //org.Seek((offset), SeekOrigin.Begin);

      //Debug.Assert(org.Length - offset == (long)len, "AP: Casting LONG to INT resulted in loss of correct value....");


      for (var i = 0; i < pieceCount; i++)
      {
        var biteContentOfTheFile = new byte[piecLen];
        var bytesReadIn = org.Read(biteContentOfTheFile, 0, piecLen);

        var ext = Path.GetExtension(mp3file);
        var newFile = mp3file.Replace(ext, $".[{i:0#}]{ext}");
        var trg = File.OpenWrite(newFile);
        trg.Write(biteContentOfTheFile, 0, bytesReadIn);
        trg.Close();
        ls.Add(newFile);
      }

      org.Close();

      var bakOrgFolder = moveOrgToBak(mp3file);

      return ls;
    }
    public static string ReplaceOrgWithAdRemovedNew_OLD(string mp3file, long offset)
    {
      if (offset <= 0)
        return null;

      if (mp3file.ToLower().Contains(".CUT."))
        return null;

      if (        //?? Directory.GetFiles(@"Computer\WALKMAN NWZ-W202\Storage Media\MUSIC", Path.GetFileName(mp3file) + ".cut.WMA").Length > 0 ||
                      Directory.GetFiles(Path.GetDirectoryName(mp3file), Path.GetFileName(mp3file) + ".org.WMA").Length > 0 ||
                      Directory.GetFiles(Path.GetDirectoryName(mp3file), Path.GetFileName(mp3file) + ".cut.WMA").Length > 0 ||
                      Directory.GetFiles(Path.GetDirectoryName(mp3file), Path.GetFileName(mp3file) + ".CUT.MP3").Length > 0 ||
                      Directory.GetFiles(Path.GetDirectoryName(mp3file), Path.GetFileName(mp3file) + "*.org.WMA").Length > 0 ||
                      Directory.GetFiles(Path.GetDirectoryName(mp3file), Path.GetFileName(mp3file) + "*.BAK").Length > 0
                      )
      {
        //Console.WriteLine("Already there: {0}.", mp3file);
        return null;
      }

      if (File.Exists(@"C:\" + ConstHelper._AllSrc + @"_Mirror\" + Path.GetFileName(mp3file) + ".cut.WMA"))
      {
        //Console.WriteLine("Already there: {0}.", mp3file);
        return null;
      }

      if (File.Exists(@"C:\" + ConstHelper._AllSrc + @"_Mirror\" + Path.GetFileName(mp3file) + ".CUT.MP3"))
      {
        //Console.WriteLine("Already there: {0}.", mp3file);
        return null;
      }

      return cutOffFirstBytes(mp3file, offset);
    }
    public static string ReplaceOrgWithAdRemoved_OLD(string mp3file)
    {
      if (mp3file.ToLower().Contains(".CUT."))
        return null;

      long offset = 0;

      if (mp3file.Contains("dotnetrocks"))
        offset = 1700000;
      else if (mp3file.ToLower().Contains("hanselminutes"))
        offset = 700000;
      else if (mp3file.ToLower().Contains("ideas"))
        offset = 300000;
      else if (mp3file.ToLower().Contains("quarks") || mp3file.ToLower().Contains("quirks"))
        offset = 377770;
      else if (mp3file.ToLower().Contains("digitalp"))
        offset = 240000;
      else if (mp3file.ToLower().Contains("runasradio"))
        offset = 820000;
      else
      {
        //Console.WriteLine("no offset for: {0}.", mp3file);

        return null;
      }

      if (        //?? Directory.GetFiles(@"Computer\WALKMAN NWZ-W202\Storage Media\MUSIC", Path.GetFileName(mp3file) + ".cut.WMA").Length > 0 ||
                      Directory.GetFiles(Path.GetDirectoryName(mp3file), Path.GetFileName(mp3file) + ".org.WMA").Length > 0 ||
                      Directory.GetFiles(Path.GetDirectoryName(mp3file), Path.GetFileName(mp3file) + ".cut.WMA").Length > 0 ||
                      Directory.GetFiles(Path.GetDirectoryName(mp3file), Path.GetFileName(mp3file) + ".CUT.MP3").Length > 0 ||
                      Directory.GetFiles(Path.GetDirectoryName(mp3file), Path.GetFileName(mp3file) + "*.org.WMA").Length > 0 ||
                      Directory.GetFiles(Path.GetDirectoryName(mp3file), Path.GetFileName(mp3file) + "*.BAK").Length > 0
                      )
      {
        //Console.WriteLine("Already there: {0}.", mp3file);
        return null;
      }

      if (File.Exists(@"C:\" + ConstHelper._AllSrc + @"_Mirror\" + Path.GetFileName(mp3file) + ".cut.WMA"))
      {
        //Console.WriteLine("Already there: {0}.", mp3file);
        return null;
      }

      if (File.Exists(@"C:\" + ConstHelper._AllSrc + @"_Mirror\" + Path.GetFileName(mp3file) + ".CUT.MP3"))
      {
        //Console.WriteLine("Already there: {0}.", mp3file);
        return null;
      }

      if (offset > 0)
        return cutOffFirstBytes(mp3file, offset);
      else
        return null;
    }

    public static bool CreateWavConvertToMp3File(string trgWavFile, string text)
    {
      var ps = new PromptStyle
      {
        Emphasis = PromptEmphasis.Strong,
        Volume = PromptVolume.ExtraLoud
      };
      //pStyle.Rate = PromptRate.Fast; 

      var pb = new PromptBuilder();
      pb.StartStyle(ps);
      pb.StartParagraph();
      pb.StartVoice(VoiceGender.Female, VoiceAge.Child);
      pb.StartSentence();
      pb.AppendBreak(TimeSpan.FromSeconds(.3)); // removed on Sep 26, 2011 // reesotred Jun 2013
      pb.AppendText(text, PromptRate.Medium);
      pb.AppendBreak(TimeSpan.FromSeconds(.3)); // removed on Sep 26, 2011 // reesotred Jun 2013
      pb.EndSentence();
      pb.EndVoice();
      pb.EndParagraph();
      pb.EndStyle();

      var tempWav = trgWavFile + ".WAV";

      using (var speaker = new SpeechSynthesizer())
      {
        //speaker.Speak(pbuilder);
        //	if (File.Exists(file))					File.Delete(file); // added Delete() on Sep 26, 2011.


        using (var fileStream = new FileStream(tempWav, FileMode.Create))
        {
          try
          {
            speaker.Volume = 100;
            speaker.SetOutputToWaveStream(fileStream); // 

            // speaker.SetOutputToWaveFile(tempWav);

            speaker.Speak(pb); //nogo: this makes annoying beep and place it instead of the TTS message : 						using (var writer = new BinaryWriter(fileStream)) { new WaveFun.WaveGenerator(WaveFun.WaveExampleType.ExampleSineWave).Save_NotClose(writer); writer.Close(); 
          }
          catch (Exception ex) { ex.Log(); }
          finally
          {
            speaker.SetOutputToDefaultAudioDevice();
            fileStream.Close();
          }
        }
      }

      NAudioHelper.ConvertWavToMp3(tempWav, trgWavFile);

      try { File.Delete(tempWav); }
      catch (Exception ex) { ex.Log(); }

      return true;
    }


    static string cutOffFirstBytes(string mp3file, long offset)
    {
      Console.Write("  cutting off {0:N1} kbytes ... ", offset / 1024);
      var org = File.Open(mp3file, FileMode.Open, FileAccess.Read, FileShare.Read); // new FileStream(fi, FileMode.Open, FileAccess.Read);
      org.Position = offset;
      org.Seek((offset), SeekOrigin.Begin);

      var len = (int)(org.Length - offset);

      Debug.Assert(org.Length - offset == len, "AP: Casting LONG to INT resulted in loss of correct value....");

      var biteContentOfTheFile = new byte[len];
      org.Read(biteContentOfTheFile, 0, biteContentOfTheFile.Length);
      org.Close();


      var newFile = mp3file;//!!! +".CUT.MP3";// +".cut.WMA";
      var trg = File.OpenWrite(newFile);
      trg.Write(biteContentOfTheFile, 0, biteContentOfTheFile.Length);
      trg.Close();

      Console.WriteLine("Done");

      var bakOrgFolder = moveOrgToBak(mp3file);
      return bakOrgFolder;
    }
    static string moveOrgToBak(string mp3file)
    {
      var bakOrgFolder = mp3file.Replace("_0New", "_zOrg.Bak");
      if (!File.Exists(bakOrgFolder))//might be there already - moved by prev smthg
        File.Move(mp3file, bakOrgFolder);
      return bakOrgFolder;
    }
    static void setFileDates(string file, DateTime date)
    {
      File.SetCreationTime(file, date);
      File.SetCreationTimeUtc(file, date);
      File.SetLastAccessTime(file, date);
      File.SetLastAccessTimeUtc(file, date);
      File.SetLastWriteTime(file, date); //TU: the only one which changes ...everything
      File.SetLastWriteTimeUtc(file, date);
    }



    public static void WavDevDbgPoc(string file = @"C:\temp\wav\test")
    {

      wavDevDbgPoc(16000, AudioBitsPerSample.Eight, AudioChannel.Mono, file);
      wavDevDbgPoc(16000, AudioBitsPerSample.Eight, AudioChannel.Stereo, file);
      wavDevDbgPoc(16000, AudioBitsPerSample.Sixteen, AudioChannel.Mono, file);
      wavDevDbgPoc(16000, AudioBitsPerSample.Sixteen, AudioChannel.Stereo, file);

      wavDevDbgPoc(32000, AudioBitsPerSample.Eight, AudioChannel.Mono, file);
      wavDevDbgPoc(32000, AudioBitsPerSample.Eight, AudioChannel.Stereo, file);
      wavDevDbgPoc(32000, AudioBitsPerSample.Sixteen, AudioChannel.Mono, file);
      wavDevDbgPoc(32000, AudioBitsPerSample.Sixteen, AudioChannel.Stereo, file);

      wavDevDbgPoc(64000, AudioBitsPerSample.Eight, AudioChannel.Mono, file);
      wavDevDbgPoc(64000, AudioBitsPerSample.Eight, AudioChannel.Stereo, file);
      wavDevDbgPoc(64000, AudioBitsPerSample.Sixteen, AudioChannel.Mono, file);
      wavDevDbgPoc(64000, AudioBitsPerSample.Sixteen, AudioChannel.Stereo, file);

    }
    public static void wavDevDbgPoc(int samplesPerSecond, AudioBitsPerSample bitsPerSample, AudioChannel channel, string file)
    {
      file = $"{file}.{bitsPerSample}.{channel}.{samplesPerSecond}.mp3";

      using (var synth = new SpeechSynthesizer())
      {
        synth.SetOutputToWaveFile(file, new SpeechAudioFormatInfo(32000, AudioBitsPerSample.Sixteen, AudioChannel.Mono));

        var m_SoundPlayer = new System.Media.SoundPlayer(file);

        var builder = new PromptBuilder();

        builder.AppendText($"{bitsPerSample}.{channel}.{samplesPerSecond}");

        synth.Speak(builder);
        m_SoundPlayer.Play();
      }
    }
  }
}

///todo: Sep 2016
/// http://stackoverflow.com/questions/19058530/change-format-from-wav-to-mp3-in-memory-stream-in-naudio
///		see Install-Package NAudio.Lame
///		D:\Users\alex\Videos\0Pod\sync.bat
///			robocopy D:\Users\alex\Videos\0Pod\_Player\  E:\MUSIC\0pod\  /MIR   /XF *.DB  *.BAT
///		