using System;
using System.IO;

namespace WaveFun
{
    /// <summary>
    /// Possible example waves to generate
    /// </summary>
    public enum WaveExampleType
	{
		ExampleSineWave = 0
	}

	/// <summary>
	/// Wraps a WAV file struture and auto-generates some canned waveforms.
	/// </summary>
	public class WaveGenerator
	{
		// Header, Format, Data chunks
		WaveHeader header;
		WaveFormatChunk format;
		WaveDataChunk data;

		/// <summary>
		/// Initializes the object and generates a wave.
		/// </summary>
		/// <param name="type">The type of wave to generate</param>
		public WaveGenerator(WaveExampleType type)
		{
			// Init chunks
			header = new WaveHeader();
			format = new WaveFormatChunk();
			data = new WaveDataChunk();

			// Fill the data array with sample data
			switch (type)
			{
				case WaveExampleType.ExampleSineWave:

					// Number of samples = sample rate * channels * bytes per sample
					uint numSamples = format.dwSamplesPerSec * format.wChannels;

					// Initialize the 16-bit array
					data.shortArray = new short[numSamples];

					int amplitude = 32760;  // Max amplitude for 16-bit audio
					double freq = 4400.0f;   // Concert A: 440Hz

					// The "angle" used in the function, adjusted for the number of channels and sample rate.
					// This value is like the period of the wave.
					double t = (Math.PI * 2 * freq) / (format.dwSamplesPerSec * format.wChannels);

					for (uint i = 0; i < numSamples - 1; i++)
					{
						// Fill with a simple sine wave at max amplitude
						for (int channel = 0; channel < format.wChannels; channel++)
						{
							data.shortArray[i + channel] = Convert.ToInt16(amplitude * Math.Sin(t * i));
						}
					}

					// Calculate data chunk size in bytes
					data.dwChunkSize = (uint)(data.shortArray.Length * (format.wBitsPerSample / 8));

					break;
			}
		}

		/// <summary>
		/// Saves the current wave data to the specified file.
		/// </summary>
		/// <param name="file"></param>
		public void Save_NotClose(BinaryWriter writer)
		{
			//// Use BinaryWriter to write the bytes to the file
			//BinaryWriter writer = new BinaryWriter(fileStream);

			// Write the header
			writer.Write(header.sGroupID.ToCharArray());
			writer.Write(header.dwFileLength);
			writer.Write(header.sRiffType.ToCharArray());

			// Write the format chunk
			writer.Write(format.sChunkID.ToCharArray());
			writer.Write(format.dwChunkSize);
			writer.Write(format.wFormatTag);
			writer.Write(format.wChannels);
			writer.Write(format.dwSamplesPerSec);
			writer.Write(format.dwAvgBytesPerSec);
			writer.Write(format.wBlockAlign);
			writer.Write(format.wBitsPerSample);

			// Write the data chunk
			writer.Write(data.sChunkID.ToCharArray());
			writer.Write(data.dwChunkSize);
			foreach (short dataPoint in data.shortArray)
			{
				writer.Write(dataPoint);
			}

			writer.Seek(4, SeekOrigin.Begin);
			uint filesize = (uint)writer.BaseStream.Length;
			writer.Write(filesize - 8);

			//// Clean up
			//writer.Close();
			//fileStream.Close();
		}
	}



	/// <summary>
	/// Wraps the header portion of a WAVE file.
	/// </summary>
	public class WaveHeader
	{
		public string sGroupID; // RIFF
		public uint dwFileLength; // total file length minus 8, which is taken up by RIFF
		public string sRiffType; // always WAVE

		/// <summary>
		/// Initializes a WaveHeader object with the default values.
		/// </summary>
		public WaveHeader()
		{
			dwFileLength = 0;
			sGroupID = "RIFF";
			sRiffType = "WAVE";
		}
	}

	/// <summary>
	/// Wraps the Format chunk of a wave file.
	/// </summary>
	public class WaveFormatChunk
	{
		public string sChunkID;         // Four bytes: "fmt "
		public uint dwChunkSize;        // Length of header in bytes
		public ushort wFormatTag;       // 1 (MS PCM)
		public ushort wChannels;        // Number of channels
		public uint dwSamplesPerSec;    // Frequency of the audio in Hz... 44100
		public uint dwAvgBytesPerSec;   // for estimating RAM allocation
		public ushort wBlockAlign;      // sample frame size, in bytes
		public ushort wBitsPerSample;    // bits per sample

		/// <summary>
		/// Initializes a format chunk with the following properties:
		/// Sample rate: 44100 Hz
		/// Channels: Stereo
		/// Bit depth: 16-bit
		/// </summary>
		public WaveFormatChunk()
		{
			sChunkID = "fmt ";
			dwChunkSize = 16;
			wFormatTag = 1;
			wChannels = 2;
			dwSamplesPerSec = 44100;
			wBitsPerSample = 16;
			wBlockAlign = (ushort)(wChannels * (wBitsPerSample / 8));
			dwAvgBytesPerSec = dwSamplesPerSec * wBlockAlign;
		}
	}

	/// <summary>
	/// Wraps the Data chunk of a wave file.
	/// </summary>
	public class WaveDataChunk
	{
		public string sChunkID;     // "data"
		public uint dwChunkSize;    // Length of header in bytes
		public short[] shortArray;  // 8-bit audio

		/// <summary>
		/// Initializes a new data chunk with default values.
		/// </summary>
		public WaveDataChunk()
		{
			shortArray = new short[0];
			dwChunkSize = 0;
			sChunkID = "data";
		}
	}
}
