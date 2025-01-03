using System;
using System.IO;
using System.Diagnostics;
using PodcastClientTpl;

namespace PodcastConditioning
{
    class Program
	{
		static void Main(string[] args)
		{
			Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
			Trace.Listeners.Add(new TextWriterTraceListener(@"C:\temp\logs\TraceListenerOutput." + AppDomain.CurrentDomain.FriendlyName.Replace(".exe", ".txt")));
			Trace.WriteLine(DateTime.Now.ToString("\n\ndd-MMM-yyyy HH:mm"));


			//foreach (DriveInfo driveInfo in DriveInfo.GetDrives())				Console.WriteLine(driveInfo.Name);


			run(args);

			//			Trace.WriteLine(string.Format("\nFiles found:\t{0} \nProcessed:\t{1}  \n\n\n           -= Press Enter key ... not required =-            ", totalFound, _processedAsRunnig));
			Trace.Flush();
			Trace.Close();

			Console.WriteLine("\n\n\t  All done.\t  Press any key to close.........");
			Console.ReadKey();
		}

		private static void run(string[] args)
		{
			string targetFolder = args.Length > 0 ? args[0] : @"C:\" + ConstHelper._AllSrc + "_0New";

			string[] targetSubFolders = Directory.GetDirectories(targetFolder, "*", SearchOption.AllDirectories);

			// Console.WriteLine("{0}      folder contents:", targetFolder);			foreach (string subfolder in targetSubFolders)				Console.WriteLine(subfolder);

			processFilesInTheFolder(targetFolder);

			Array.ForEach(targetSubFolders, processFilesInTheFolder); //TU: predicates & actions (http://msdn.microsoft.com/en-us/magazine/cc163550.aspx) ==    foreach (string subfolder in targetSubFolders) processFilesInTheFolder(subfolder);
		}

		private static void processFilesInTheFolder(string subfolder)
		{
			string[] mp3s = Directory.GetFiles(subfolder, "*.MP3");//WMA");//

			Console.WriteLine("\n{1,-44}   {0,3} files:", mp3s.Length, subfolder);

			foreach (string mp3 in mp3s)
			{
				AdvertCutter.ReplaceOrgWithAdRemoved_OLD(mp3);
			}
		}
	}
}
