namespace Db.PodcastMgt.DbModel
{
    using System;
    using System.Data.Entity;
    using System.Diagnostics;

    public partial class A0DbContext : DbContext
    {
        public static A0DbContext Create() { return new A0DbContext(); }

        A0DbContext()
          : base()
        {
            Database.Connection.ConnectionString = conStr; //tu: remove CREATE TABLE permissin neeed - Database.SetInitializer<A0DbContext>(null);
            Trace.WriteLine(string.Format("  SqlEnv: {0}. \r\n ConStr: {1}.", SqlEnv, Database.Connection.ConnectionString));
        }

        static string conStr
        {
            get
            {
                var dbg =
#if DEBUG______
          "Dbg";
#else
          "";
#endif
                switch (Environment.MachineName) // .Equals("Asus2", StringComparison.OrdinalIgnoreCase))
                {
                    default:
                    case "AP-201704-LW": 
                    case "Asus2":        /**/ return $@"data source=.\sqlexpress;               initial catalog=PodcastMgt{dbg}; integrated security=True;MultipleActiveResultSets=True;App=EntityFramework;Encrypt=False;Connect Timeout=180";
                    case "AP-201704-L~": /**/ return $@"data source={Environment.MachineName};  initial catalog=PodcastMgt{dbg}; integrated security=True;MultipleActiveResultSets=True;App=EntityFramework;Encrypt=False;Connect Timeout=180";
                }
            }
        }

        public static string SqlEnv
        {
            get
            {
#if DEBUG
        return "dbg";
#else
                return "RLS";
#endif
            }
        }
    }
    //	{
    //    public static A0DbContext Create() { return new A0DbContext(); }
    //    public static A0DbContext Create(string conStr) { return new A0DbContext(conStr); }

    //    A0DbContext()
    //      : base()
    //    {
    //      //tu: remove CREATE TABLE permissin neeed - Database.SetInitializer<A0DbContext>(null);
    //      Database.Connection.ConnectionString = initcs(); // Trace.WriteLine(string.Format("  SqlEnv: {0}. \r\n ConStr: {1}.", _sqlEnv, Database.Connection.ConnectionString));
    //    }
    //    A0DbContext(string conStr)
    //      : base()
    //    {
    //      //tu: remove CREATE TABLE permissin neeed - Database.SetInitializer<A0DbContext>(null);
    //      _sqlEnv = conStr;
    //      Database.Connection.ConnectionString = csd[_sqlEnv];
    //    }

    //    static string initcs()
    //    {
    //      try
    //      {
    //#if DEBUG
    //        _sqlEnv = "rls";
    //#elif dbg
    //        _sqlEnv = "dbg"; // Debug
    //#else
    //        _sqlEnv = "rls"; // PROD 
    //#endif
    //        return ConStr;
    //      }
    //      finally
    //      {
    //        if (System.Environment.Is64BitProcess)
    //          _sqlEnv = _sqlEnv.ToUpper();
    //      }
    //    }

    //    public static string ConStr
    //    {
    //      get
    //      {
    //        if (!csd.ContainsKey(_sqlEnv.ToLower()))
    //          _sqlEnv = "rls";

    //        return csd[_sqlEnv];
    //      }
    //    }

    //    public static IEnumerable Keys { get { return csd.Keys; } }

    //    static Dictionary<string, string> csd = new Dictionary<string, string> {
    //      { "rls", @"data source=.\sqlexpress;initial catalog=PodcastMgt;    integrated security=True;MultipleActiveResultSets=True;App=EntityFramework" },
    //      { "dbg", @"data source=.\sqlexpress;initial catalog=PodcastMgtDbg; integrated security=True;MultipleActiveResultSets=True;App=EntityFramework" },
    //      //{ "cfg", string.Format("data source={0}; initial catalog={1};          user id={2};    password={3};          MultipleActiveResultSets=True; App=EntityFramework; persist security info=True; Connect Timeout=3; ", Settings.Default.DbServer, Settings.Default.Database, Settings.Default.Username, Settings.Default.Password) }
    //    };

    //    static string _sqlEnv = "???";
    //    public static string SqlEnv
    //    {
    //      get
    //      {
    //        if (_sqlEnv == "???") initcs();
    //        return System.Environment.Is64BitProcess ? _sqlEnv.ToUpper() : _sqlEnv.ToLower();
    //      }
    //    }
    //  }
}
