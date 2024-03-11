using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Linq;
using AAV.Sys.Ext;
using AsLink;
using Db.PodcastMgt.DbModel;

namespace PodCatcher
{
	public partial class RssHelper
	{
		public static RssHelper FindNewCasts(Feed feed)
		{
			try
			{
				Debug.WriteLine("~> {0,-32}   {1,5} casts,    LatestRssText.Length={2,5:N0} kb", feed.Name, feed.DnLds?.Count, .001 * feed.LatestRssText?.Length);

				var inst = new RssHelper();
				inst.ParseRssToGetAllCasts(feed);
				return inst;
			}
			catch (Exception ex) { ex.Log(); throw; }
		}
		public void ParseRssToGetAllCasts(Feed feed)
		{
      if (string.IsNullOrEmpty(feed.LatestRssText)) return;
			try
			{
				using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(feed?.LatestRssText)))
				{
					using (var reader = new MyXmlReader(stream))
					{
						if (!p_New(feed, reader))
							p_Old(feed, XDocument.Load(reader));
					}
				}
			}
			catch (Exception ex) { ex.Log(); throw; }
		}


		//tu: try if UTF8 does not pan out.
		//USAGE: Don't forget to use Using:			using (Stream s = GenerateStreamFromString("a,b \n c,d"))			{			... Do stuff to stream			}
		public Stream GenerateStreamFromString(string s)
		{
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(s);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}
	}
}
