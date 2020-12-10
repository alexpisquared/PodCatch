﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NAudioTrimmer.Logic;

namespace NAudioTrimmer
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            new MainWindow(new NAudioHelper(), new BitmapHelper()).Show();
        }
    }
}
