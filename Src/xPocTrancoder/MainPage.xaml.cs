using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Audio;
using Windows.Media.MediaProperties;
using Windows.Media.Render;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace xPocTrancoder
{
    public sealed partial class MainPage : Page
	{
		const double _PlaybackSpeedFactor = 7.5;
		public MainPage() { this.InitializeComponent(); }

		AudioGraph _graph;
		AudioDeviceOutputNode _deviceOutput;
		List<AudioFileInputNode> _fileInputs = new List<AudioFileInputNode>();

		protected override async void OnNavigatedTo(NavigationEventArgs e) { await CreateAudioGraph_1(); }
		protected override void OnNavigatedFrom(NavigationEventArgs e) { _graph.Stop(); _graph?.Dispose(); }

		async Task CreateAudioGraph_1()
		{
			try
			{
				var result = await AudioGraph.CreateAsync(new AudioGraphSettings(AudioRenderCategory.Media)); if (result.Status != AudioGraphCreationStatus.Success) { notifyUser(String.Format("AudioGraph Creation Error because {0}", result.Status.ToString())); return; }

				_graph = result.Graph;

				var deviceOutputNodeResult = await _graph.CreateDeviceOutputNodeAsync(); if (deviceOutputNodeResult.Status != AudioDeviceNodeCreationStatus.Success) { notifyUser(String.Format("Device Output unavailable because {0}", deviceOutputNodeResult.Status.ToString())); return; }

				_deviceOutput = deviceOutputNodeResult.DeviceOutputNode; notifyUser("Device Output Node successfully created");
				_deviceOutput.OutgoingGain = .5;

			}
			catch (Exception ex) { Debug.WriteLine(ex); throw; }
		}

		async void onLoaded(object sender, RoutedEventArgs e) { await NewMethod(KnownFolders.MusicLibrary); }
		async void onClickM(object sender, RoutedEventArgs e) { await NewMethod(KnownFolders.MusicLibrary); }
		async void onClickV(object sender, RoutedEventArgs e) { await NewMethod(KnownFolders.VideosLibrary); } //todo: Access is denied!!!

		async Task NewMethod(StorageFolder sf)
		{
			try
			{
				foreach (StorageFolder item in await sf.GetFoldersAsync())					Debug.WriteLine(item.Name);

				//var dbgFoldr0 = await sf.CreateFolderAsync("Dbg");
				var dbgFoldr = await sf.GetFolderAsync("Dbg");
				var inpFiles = await dbgFoldr.GetFilesAsync(CommonFileQuery.OrderByName);

				foreach (var inpFile in inpFiles.Where(r => r.Name.StartsWith("[") && r.Name.EndsWith("3"))) //inpFiles.ForEach(inpFile =>{});
				{
					var outFile = await dbgFoldr.CreateFileAsync($"{_PlaybackSpeedFactor:N1}-{inpFile.Name}", CreationCollisionOption.ReplaceExisting);

					var fileInputResult = await _graph.CreateFileInputNodeAsync(inpFile); if (AudioFileNodeCreationStatus.Success != fileInputResult.Status) { notifyUser(String.Format("Cannot read input file because {0}", fileInputResult.Status.ToString())); return; }

					var fileInput = fileInputResult.FileInputNode;        //_fileInput.StartTime = TimeSpan.FromSeconds(10);				//_fileInput.EndTime = TimeSpan.FromSeconds(20);
					//fileInput.PlaybackSpeedFactor = _PlaybackSpeedFactor;

					var fileOutNodeResult = await _graph.CreateFileOutputNodeAsync(outFile, CreateMediaEncodingProfile(outFile)); // Operate node at the graph format, but save file at the specified format
					if (fileOutNodeResult.Status != AudioFileNodeCreationStatus.Success) { notifyUser(string.Format("Cannot create output file because {0}", fileOutNodeResult.Status.ToString())); return; }

					fileInput.AddOutgoingConnection(fileOutNodeResult.FileOutputNode);
					//fileInput.AddOutgoingConnection(_deviceOutput);
					fileInput.FileCompleted += fileInput_FileCompleted;

					//nogo{
					fileInput.EncodingProperties.Bitrate *= 2;
					fileInput.EncodingProperties.SampleRate *= 2;
					fileInput.EncodingProperties.BitsPerSample *= 2;
					fileOutNodeResult.FileOutputNode.EncodingProperties.Bitrate /= 2;
					fileOutNodeResult.FileOutputNode.EncodingProperties.SampleRate /= 2;
					fileOutNodeResult.FileOutputNode.EncodingProperties.BitsPerSample /= 2;
					//}nogo

					_fileInputs.Add(fileInput);
				}

				_graph.Start();             //await Task.Delay(12000);            _graph.Stop();
				notifyUser("Started...");
			}
			catch (Exception ex) { Debug.WriteLine(ex); throw; }
		}

		async void fileInput_FileCompleted(AudioFileInputNode fileInput, object args)
		{
			try
			{
				await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
				{
					try
					{
						fileInput.Reset(); // Reset the file input node so starting the graph will resume playback from beginning of the file
						_fileInputs.Remove(fileInput);
						notifyUser($"Disposing: {fileInput.SourceFile.Name} ");
						fileInput.Dispose();           /**/

						if (_fileInputs.Count() == 0)
						{
							_graph.Stop();
							notifyUser("All Done!!! ");
						}
					}
					catch (Exception ex) { notifyUser(ex.Message); }
				});

				await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { notifyUser("End of file reached"); });
			}
			catch (Exception ex) { notifyUser(ex.Message); }
		}

		MediaEncodingProfile CreateMediaEncodingProfile(StorageFile file)
		{
			switch (file.FileType.ToString().ToLowerInvariant())
			{
				case ".wma": return MediaEncodingProfile.CreateWma(AudioEncodingQuality.High);
				case ".mp3": return MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High);
				case ".wav": return MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);
				default: throw new ArgumentException();
			}
		}

		void notifyUser(string v, object k = null) { Debug.WriteLine(v); t1.Text += (v + "\r\n"); }

	}
}
///todo:
/// create audiograph from media player set to speed of 2.0!!!