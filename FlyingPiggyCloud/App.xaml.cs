﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace FlyingPiggyCloud
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        Controllers.TaskBarButton TaskBarButton { get; set; }

        public App()
        {
            TaskBarButton = new Controllers.TaskBarButton();
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }
    }
}
