
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
            //fs.checkConfig();
            //InitializeComponent();


            //db.initDB();
            //if (main.PingDB()) { TB_Ping.Text = "Connected"; }
            //else { TB_Ping.Text = "Connection failed..."; main.ReportError(new Exception("Ping not successfull")); return; }
            //System.Windows.Threading.DispatcherTimer dT = new System.Windows.Threading.DispatcherTimer();
            //dT.Tick += new EventHandler(SetPing);
            //dT.Interval = new TimeSpan(0, 0, 1);
            //dT.Start();

            //List<string> query = db.myquery("SELECT version();");
            //MessageBox.Show("MySQL " + query[0]);

            //allDrives = DriveInfo.GetDrives();
        }

        private void SetPing(object sender, EventArgs e)
        {
            double PingTime = db.PingDB();
            TB_PingTime.Text = $"{PingTime}ms";
        }

        List<string> ExcList;
        //private void Index_Click(object sender, RoutedEventArgs e)
        //{
        //    BackgroundWorker worker = new BackgroundWorker();
        //    _PATH = main.getPathDialog();
        //    ExcList = GetExceptionList();
        //    if (_PATH == "") return;

        //    worker.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
        //    worker.WorkerSupportsCancellation = true;
        //    //worker.WorkerReportsProgress = true;
        //    //worker.ProgressChanged += OnProgressChanged;
        //    worker.RunWorkerAsync();
        //    return;
        //}

        /**Neuer Thread, um die UI nicht zu blockieren */
        //private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    string path = "C:\\Users\\LoefflerM\\OneDrive - Putzmeister Holding GmbH\\Desktop\\Berichtsheft";

        //    string[] files = fs.readDirSync(_PATH, true, true);

        //    //Check if the file type or files are in the ExceptionList
        //    MessageBox.Show(files.Length + "\n" + string.Join(",", files));
        //    files = checkForExcpetionlist(files);
        //    MessageBox.Show(files.Length + "\n" + string.Join(",", files));

        //    int TotalFiles = files.Length;
        //    C_TFiles ProcessedFiles = new C_TFiles();
        //    for (int i = 0; i < TotalFiles; i++)
        //    {
        //        //fs.AddToIndex(files[i]);
        //        Thread.Sleep(100);
        //        switch (fs.AddToIndex(files[i]))
        //        {

        //            case -1: MessageBox.Show($"Die Datei {Path.GetFileName(files[i])} konnte nicht indiziert werden, da sie schon vorhanden ist"); ProcessedFiles.FilesErr.Add(new C_Files { FileName = Path.GetFileName(files[i]), Path = files[i] }); break; //Datei schon vorhanden
        //            case -255: break; //Exception
        //            case 0: break;

        //        }
        //        SetIndexProgress(files[i], i, TotalFiles);
        //    }
        //    MessageBox.Show(TotalFiles.ToString() + " Dateien erfolgreich hinzugefügt");
        //}

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

        class C_TFiles
        {
            public List<C_Files> FilesOk;
            public List<C_Files> FilesErr;
        }

        public class C_Files
        {
            public string FileName;
            public string Path;
        }

        //private void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        //{
        //    SetIndexProgress()
        //}

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
            //List<string> _ = MainWindowViewModel.GetExceptionList();
            //foreach(var d in _) ListBox.Items.Add(d);
            return;
        }


        public static List<string> GetExceptionList(MainWindowViewModel model)
        {
            return model.FileExceptionList.ToList();
        }


        //private void tb_Search_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        //{
        //    GD_Dateiausgabe.Children.Clear();

        //    /** Nichts eingegeben = nichts anzeigen */
        //    if (tb_Search.Text.Length == 0) return;
        //    /**Es sollten zuerst die Dateinamen und DANN erst Dateien mit dem Inhalt durchsucht werden */

        //    var File = fs.searchFile(tb_Search.Text, false);
        //    if (File.Count != 0)
        //    {
        //        string res = "";
        //        foreach (var v in File)
        //        {
        //            res += v.Filename + "\n";
        //            res += v.Path + "\n\n";
        //        }
        //        TextBlock tb = new TextBlock();
        //        //ObservableCollection tb = new System.Collections.ObjectModel.ObservableCollection();
        //        tb.Text = res + "\n";

        //        tb.MouseLeftButtonUp += Tb_MouseLeftButtonUp;
        //        GD_Dateiausgabe.Children.Add(tb);
        //    }


        //    //fs.C_IZ _ = db.getConf<fs.C_IZ>("database");
        //    //for (int i = 0; i < _.Paths.Length; i++)
        //    //{

        //    //    System.Windows.Controls.TextBox txt = new System.Windows.Controls.TextBox();
        //    //    List<main.FileStructure> oof = fs.searchFile(tb_Search.Text, false);
        //    //    oof.ForEach((p) =>
        //    //    {
        //    //        AddToGrid(p.Filename, p.Path);
        //    //        txt.Text += $"\n\n{p.Filename} in {p.Path}";
        //    //    });

        //    //}


        //}

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
            //DetectUSB.IsEnabled = false;
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
            //foreach (var drive in currentDrives)
            //{
            //    if (!allDrives.Contains(drive)) { MessageBox.Show(drive.Name); }
            //}

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
    }
}