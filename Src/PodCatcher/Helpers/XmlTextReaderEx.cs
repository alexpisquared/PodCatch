using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Syndication;
using System.Xml;

namespace PodCatcher
{
	class MyXmlReader : XmlTextReader
	{
		bool readingDate = false;

		public MyXmlReader(Stream s) : base(s) { }
		public MyXmlReader(string inputUri) : base(inputUri) { }

		public override void ReadStartElement()
		{
			if (string.Equals(base.NamespaceURI, string.Empty, StringComparison.InvariantCultureIgnoreCase) && (
				string.Equals(base.LocalName, "updated", StringComparison.InvariantCultureIgnoreCase) ||
				string.Equals(base.LocalName, "published", StringComparison.InvariantCultureIgnoreCase) ||
				string.Equals(base.LocalName, "lastBuildDate", StringComparison.InvariantCultureIgnoreCase) ||
				string.Equals(base.LocalName, "pubDate", StringComparison.InvariantCultureIgnoreCase)))
			{
				readingDate = true;
			}

			base.ReadStartElement();
		}
		public override void ReadEndElement()
		{
			if (readingDate)
			{
				readingDate = false;
			}

			base.ReadEndElement();
		}
		public override string ReadString()
		{
			if (readingDate)
			{
				string dateString = base.ReadString();
				var dt = MiscHelper.GetDateFromAnyFormatString(dateString);
				return dt.ToUniversalTime().ToString("R", CultureInfo.InvariantCulture);
			}
			else
			{
				return base.ReadString();
			}
		}
	}

	public class SyndicationFeedXmlReader : XmlTextReader
	{
		readonly string[] Rss20DateTimeHints = { "pubDate" };
		readonly string[] Atom10DateTimeHints = { "updated", "published", "lastBuildDate" };
		private bool isRss2DateTime = false;
		private bool isAtomDateTime = false;

		public SyndicationFeedXmlReader(Stream stream) : base(stream) { }
		public override bool IsStartElement(string localname, string ns)
		{
			isRss2DateTime = false;
			isAtomDateTime = false;

			if (Rss20DateTimeHints.Contains(localname)) isRss2DateTime = true;
			if (Atom10DateTimeHints.Contains(localname)) isAtomDateTime = true;

			return base.IsStartElement(localname, ns);
		}
		public override string ReadString()
		{
			string dateVal = base.ReadString();

			try
			{
				if (isRss2DateTime)
				{
					MethodInfo objMethod = typeof(Rss20FeedFormatter).GetMethod("DateFromString", BindingFlags.NonPublic | BindingFlags.Static);
					Debug.Assert(objMethod != null);
					objMethod.Invoke(null, new object[] { dateVal, this });
				}
				if (isAtomDateTime)
				{
					MethodInfo objMethod = typeof(Atom10FeedFormatter).GetMethod("DateFromString", BindingFlags.NonPublic | BindingFlags.Instance);
					Debug.Assert(objMethod != null);
					objMethod.Invoke(new Atom10FeedFormatter(), new object[] { dateVal, this });
				}
			}
			catch (TargetInvocationException tiex)
			{
				Debug.WriteLine(tiex);
				DateTimeFormatInfo dtfi = CultureInfo.CurrentCulture.DateTimeFormat;
				try
				{
					return DateTime.Parse(dateVal).ToString(dtfi.RFC1123Pattern);
				}
				catch (FormatException fex)
				{
					Debug.WriteLine(fex);

					return DateTimeOffset.UtcNow.ToString(dtfi.RFC1123Pattern);
				}
			}

			return dateVal;
		}
	}
}
