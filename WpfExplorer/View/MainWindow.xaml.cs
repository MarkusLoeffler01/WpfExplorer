
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
#pragma warning disable 0649

namespace WpfExplorer
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string _PATH = "";
        public static int ID = -1;
        public static string CONFIG_LOCATIONS = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WpfExplorer\\");
        public static DriveInfo[] allDrives;

        public MainWindow()
        {
            this.Loaded += InitVM;
        }

        private void InitVM(object sender, EventArgs e)
        {
            ((MainWindowViewModel)DataContext).ready_Tick();
        }

        private void SetPing(object sender, EventArgs e)
        {
            double PingTime = db.PingDB();
            TB_PingTime.Text = $"{PingTime}ms";
        }

        List<string> ExcList;

        private string[] checkForExcpetionlist(string[] files)
        {
            /*
             * Wenn el[i][0] == *, el.Includes(el[i][el[i]-1]
             * 
             * prüfe ob Path.GetExtension(files[i]) != '' && ob in el, dann entferne
             */

            List<string> el = ExcList;
            List<string> filesList = files.ToList();

            //Entferne alle Dateitypen in der Liste
            for (int i = 0; i < filesList.Count; i++)
            {
                string ext = Path.GetExtension(filesList[i]);
                if (ext.Length > 0 && el.Contains(ext)) filesList.RemoveAt(i);
            }

            string[] _el = el.ToArray();
            //Enterne alle Verzeichnisse 
            for (int i = 0; i < _el.Length; i++)
            {
                for (int o = 0; o < filesList.Count; o++)
                {
                    //Prüfe, ob der Eintrag ein "Verzeichnis/" ist und prüfe anschließend, ob die Datei in solch einem Verzeichnis ist
                    if (_el[i].EndsWith("/") && filesList[o].Replace("\\", "/").Contains(_el[i])) filesList.RemoveAt(o);
                }
            }

            return filesList.ToArray();
        }

        public static void AddToGrid(fs.C_File FileName, string FullPath)
        {
          
        }

        ICommand _addToExceptList;

        public ICommand AddToExceptionList
        {
            get
            {
                if (_addToExceptList == null) _addToExceptList = new RelayCommand(e => ToExceptionList());
                return _addToExceptList;
            }
        }

        

        private void ToExceptionList()
        {
            return;
        }

        private void Tb_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Hier ein Event einfügen, welches den Pfad des angeklickten TextBlocks öffnet
        }

        public void tb_AddExceptions_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Detect_Click(object sender, RoutedEventArgs e)
        {
            HwndSource hwndSource = HwndSource.FromHwnd(Process.GetCurrentProcess().MainWindowHandle);
            if (hwndSource != null)
            {
                IntPtr windowHandle = hwndSource.Handle;
                hwndSource.AddHook(UsbNotificationHandler);
                USBDetector.RegisterUsbDeviceNotification(windowHandle);
            }
        }
        private IntPtr UsbNotificationHandler(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == USBDetector.UsbDevicechange)
            {
                switch ((int)wparam)
                {
                    case USBDetector.UsbDeviceRemoved:
                        MessageBox.Show("USB Removed");
                        break;
                    case USBDetector.NewUsbDeviceConnected:

                        MessageBoxResult res = MessageBox.Show("Neuer USB erkannt. Möchten Sie ihn indizieren?", "Neues USB Gerät", MessageBoxButton.YesNo);
                        if (res == MessageBoxResult.Yes) { NewIndex(); }
                        break;
                }
            }
            else
            {
              
            }

            handled = false;
            return IntPtr.Zero;
        }

        public static void NewIndex()
        {
            List<string> drive = ScanUSB();

            if (drive == null || drive.Count == 0) { MessageBox.Show("USB Gerät wurde nicht erkannt\nBitte erneut probieren"); return; }
            MessageBox.Show("Wählen Sie den Ordner aus, den Sie indizieren möchten");
            main.getPathDialog(drive[0]);
        }

        public static List<string> ScanUSB()
        {
            DriveInfo[] currentDrives = DriveInfo.GetDrives();

            List<string> oldD = new List<string> { };
            List<string> newD = new List<string> { };
            foreach (var curr in currentDrives)
            {
                oldD.Add(curr.Name);
            }
            foreach (var d in allDrives)
            {
                newD.Add(d.Name);
            }
            return oldD.Except(newD).Concat(newD.Except(oldD)).ToList();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Detect_Click(sender, e);
        }

        private void lb_Exceptions_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
    }
}