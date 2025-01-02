using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using NAudio.Wave;

namespace NAudioTrimmer6
{
  public interface INAudioHelper
  {
    (Bitmap, TimeSpan) LoadWaveFormToBitmap(string audioFile);

    void TrimMp3Both(string inputPath, string outputPath, TimeSpan begin, TimeSpan endin);
    void TrimMp3Left(string inputPath, string outputPath, TimeSpan begin);
    void TrimMp3Rght(string inputPath, string outputPath, TimeSpan endin);
  }

  public class NAudioHelper : INAudioHelper
  {
    public (Bitmap, TimeSpan) LoadWaveFormToBitmap(string file)
    {
      var sw = Stopwatch.StartNew();
      var widthInPixels = 40000;
      var heightInPixels = 100;
      var y0 = heightInPixels / 2;
      var yScale = 50;
      var bitmap = new Bitmap(widthInPixels, heightInPixels);

      using (var reader = new AudioFileReader(file))
      {
        var batchLen = (int)Math.Max(40, reader.Length / (widthInPixels * reader.WaveFormat.BitsPerSample / 8));
        var buffer = new float[batchLen];

        var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);
        var pen = new Pen(Color.DodgerBlue);

        Debug.WriteLine($"Len:{reader.Length:N0}  batchLen:{batchLen:N0} = {reader.Length / batchLen:N0}"); // Debug.WriteLine($"Len:{reader.Length:N0}  Channels {reader.WaveFormat.Channels} BitsPerSample:{reader.WaveFormat.BitsPerSample}  SampleRate:{reader.WaveFormat.SampleRate}  batchLen:{batchLen}");
        {
          int xPos; for (xPos = 0; reader.Read(buffer, 0, batchLen) == batchLen; xPos++) // Read is the bottleneck of performance.
          {
#if OneSideOnly
                        var max = Math.Abs(buffer.Max()) * yScale;
                        graphics.DrawLine(pen, xPos, y0 + max, xPos, y0 - max);
#else
            graphics.DrawLine(pen, xPos, y0 + yScale * buffer.Max(), xPos, y0 + yScale * buffer.Min());
#endif
          }

          Debug.WriteLine($"Len:{reader.Length:N0} / batchLen:{batchLen:N0} = {reader.Length / batchLen:N0} ?? {xPos:N0}");
        }

        //Parallel.For(0, widthInPixels, xPos => // <= works ..but produces randomised collection of lines ..and does not reduce the time!!!
        //{
        //    reader.Read(buffer, 0, batchLen);
        //    var max = Math.Abs(buffer.Max()) * yScale;                      
        //    graphics.DrawLine(pen, xPos, y0 + max, xPos, y0 - max);         
        //});
      }

      return (bitmap, sw.Elapsed); //tu:             bitmap.Save(@"C:\temp\waveform.png");
    }

    public void TrimMp3Both(string inputPath, string outputPath, TimeSpan begin, TimeSpan endin) => trim(inputPath, outputPath, begin, endin);
    public void TrimMp3Left(string inputPath, string outputPath, TimeSpan begin) => trim(inputPath, outputPath, begin);
    public void TrimMp3Rght(string inputPath, string outputPath, TimeSpan endin) => trim(inputPath, outputPath, TimeSpan.Zero, endin);

    void trim(string inputPath, string outputPath, TimeSpan? begin, TimeSpan? end = null) // https://stackoverflow.com/questions/7932951/trimming-mp3-files-using-naudio
    {
      if (begin.HasValue && end.HasValue && begin > end)
        throw new ArgumentOutOfRangeException("end", "end should be greater than begin");

      var directory = Path.GetDirectoryName(outputPath);
      if (directory is not null && Directory.Exists(directory) != true)
        Directory.CreateDirectory(directory);

      if (File.Exists(outputPath))
        File.Delete(outputPath);

      using (var reader = new Mp3FileReader(inputPath))
      using (var writer = File.Create(outputPath))
      {
        Mp3Frame frame;
        while ((frame = reader.ReadNextFrame()) != null)
        {
          if (reader.CurrentTime >= begin || !begin.HasValue)
          {
            if (reader.CurrentTime <= end || !end.HasValue)
              writer.Write(frame.RawData, 0, frame.RawData.Length);
            else break;
          }
        }
      }
    }
  }
}
