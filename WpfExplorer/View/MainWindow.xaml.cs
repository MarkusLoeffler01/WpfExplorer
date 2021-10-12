
﻿using CommandHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using WpfExplorer.ViewModel;
using WpfExplorer.Modules;
#pragma warning disable 0649

namespace WpfExplorer
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            MainWindow.instance = this;
            this.Loaded += InitVM;
            
        }
        public static MainWindow instance;
        private void InitVM(object sender, RoutedEventArgs e)
        {
            ((MainWindowViewModel)DataContext).ready_Tick();
            USBDetector.Detect_Click(sender, e);
            fs.checkUserSettings(false);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}