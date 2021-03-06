﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace AsLink
{
  public static partial class Serializer
  {
    public static string SaveToString(object o)
    {
      var sb = new StringBuilder();
      using (var sw = new StringWriter(sb))
      {
        new XmlSerializer(o.GetType()).Serialize(sw, o);
#if WPF
				sw.Close();
#endif
      }
      return sb.ToString();
    }

    public static object LoadFromString<T>(string str)
    {
      try
      {
        using (var streamReader = new StringReader(str))
        {
          var o = (T)(new XmlSerializer(typeof(T)).Deserialize(streamReader)); //TU: DEserialise from a stream
#if WPF
					streamReader.Close();
#endif
          return o;
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine("\n::>{0}\n\n::>{1}\n", ex.Message,
          ex.InnerException == null ? "" :
          ex.InnerException.InnerException == null ? "\n::>" + ex.InnerException.Message :
          ex.InnerException.InnerException.InnerException == null ? "\n::>" + ex.InnerException.InnerException.Message : "\n::>" + ex.InnerException.InnerException.InnerException.Message);

        return Activator.CreateInstance(typeof(T)); //		???		throw;  Watch it !!!!!
      }
    }
    public static object LoadFromStringMin<T>(string str) => (T)new XmlSerializer(typeof(T)).Deserialize(new StringReader(str));

#if UWP
    public static object LoadFromStringWcf<T>(string str)
    {
      try
      {
        DataContractSerializer serializer = new DataContractSerializer(typeof(T));
        XmlReader xmlReader = XmlReader.Create(new System.IO.StringReader(str));
        return (T)serializer.ReadObject(xmlReader);
      }
      catch (Exception ex)
      {
        Debug.WriteLine("\n::>{0}\n\n::>{1}\n", ex.Message,
          ex.InnerException == null ? "" :
          ex.InnerException.InnerException == null ? "\n::>" + ex.InnerException.Message :
          ex.InnerException.InnerException.InnerException == null ? "\n::>" + ex.InnerException.InnerException.Message : "\n::>" + ex.InnerException.InnerException.InnerException.Message);

        return Activator.CreateInstance(typeof(T)); //		???		throw;  Watch it !!!!!
      }
    }
#endif
  }
}
