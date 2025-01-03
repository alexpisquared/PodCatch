using System;

namespace AAV.Sys.Core3Ext
{
  public static class Core3SubstExtr // jsut to compile Core 3 code
  {
    public static bool Contains(this string str, string value, StringComparison stringComparison) => str.Contains(value);
    public static string Replace(this string str, string value1, string value2, StringComparison stringComparison) => str.Replace(value1, value2);
  }
}
