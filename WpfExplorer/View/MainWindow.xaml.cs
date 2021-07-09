using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Windows.Interop;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.IO;
using System.Threading;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using CommandHelper;

namespace WpfExplorer 
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string _PATH = "";
        public static string CONFIG_LOCATIONS = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WpfExplorer\\");
        public static DriveInfo[] allDrives; 
        public MainWindow()
        {
            fs.checkFiles();
            InitializeComponent();

            if(main.PingDB()) { TB_Ping.Text = "Connected"; }
            else { TB_Ping.Text = "Connection failed..."; main.ReportError(new Exception("Ping not successfull")); return; }
            System.Windows.Threading.DispatcherTimer dT = new System.Windows.Threading.DispatcherTimer();
            dT.Tick += new EventHandler(SetPing);
            dT.Interval = new TimeSpan(0, 0, 1);
            dT.Start();

            allDrives = DriveInfo.GetDrives();
            //allDrives = DriveInfo.GetDrives();



            //MessageBox.Show(string.Join("\n", fs.readDirSync("..\\..\\..\\..")));
            //MessageBox.Show(string.Join("\n", fs.readDirSync("..\\..\\..\\..", true)));
            //DetectUSB.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            //DetectUSB.Click(new object(), new RoutedEventArgs());
            //Detect_Click(Window.GetWindow(this), new RoutedEventArgs());
            //DetectUSB.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));


            //var xx = fs.readIndexedFiles();
            //var yy = xx.ToArray<main.FileStructure>();
            //string res = "";
            //for(int i = 0; i < yy.Length; i++)
            //{
            //    res += yy[i].Filename+"\n";
            //    res += yy[i].Path+"\n\n";

            //}
            //MessageBox.Show(res);

            var File = fs.searchFile("bdhfszifzui1.txt", false);
            if (File.Count == 0) MessageBox.Show("Keine Dateien gefunden");
            else
            {
                string res = "";
                foreach (var v in File)
                {
                    res += v.Filename + "\n";
                    res += v.Path + "\n\n";
                }
                MessageBox.Show(res);
            }
        }

        private void SetPing(object sender, EventArgs e)
        {
            double PingTime = db.PingDB();
            TB_PingTime.Text = $"{PingTime}ms";
        }
        private void Index_Click(object sender, RoutedEventArgs e)
        { 
            BackgroundWorker worker = new BackgroundWorker();
            _PATH = main.getPathDialog();
            worker.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            worker.WorkerSupportsCancellation = true;
            //worker.WorkerReportsProgress = true;
            //worker.ProgressChanged += OnProgressChanged;
            worker.RunWorkerAsync();
            return;
            //List<string> x = db.query("SELECT 1+1;");
            //string res = "";
            //for (int i = 0; i < x.Count; i++)
            //{
            //    res += x[i];
            //    res += "\n\n";
            //}
            //MessageBox.Show(res);
        }

        /**Neuer Thread, um die UI nicht zu blockieren */
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string path = "C:\\Users\\LoefflerM\\OneDrive - Putzmeister Holding GmbH\\Desktop\\Berichtsheft";

            string[] files = fs.readDirSync(_PATH, true, true);
            int TotalFiles = files.Length;
            C_TFiles ProcessedFiles = new C_TFiles();
            for (int i = 0; i < TotalFiles; i++)
            {
                //fs.AddToIndex(files[i]);
                Thread.Sleep(100);
                switch (fs.AddToIndex(files[i]))
                {
                    case -1: MessageBox.Show($"Die Datei {Path.GetFileName(files[i])} konnte nicht indiziert werden, da sie schon vorhanden ist"); ProcessedFiles.FilesErr.Add(new C_Files { FileName = Path.GetFileName(files[i]), Path = files[i]}); break; //Datei schon vorhanden
                    case -255: break; //Exception
                    case 0: break;

                }
                SetIndexProgress(files[i], i, TotalFiles);
            }
            MessageBox.Show(TotalFiles.ToString() + " Dateien erfolgreich hinzugefügt");
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

        public static void AddToGrid(string FileName, string FullPath)
        {
            
        }

        ICommand _addToExceptList;

        public ICommand AddToExceptionList
        {
            get
            {
                if (_addToExceptList == null) _addToExceptList = new RelayCommand(c => ToExceptionList());
                return _addToExceptList;
            }
        }


        private void ToExceptionList()
        {
            List<string> _ = GetExceptionList();
        }


        private List<string> GetExceptionList()
        {
            return ListBox.Items.Cast<string>().ToList();
        }


        private void tb_Search_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            GD_Dateiausgabe.Children.Clear();

            /** Nichts eingegeben = nichts anzeigen */
            if (tb_Search.Text.Length == 0) return;
            /**Es sollten zuerst die Dateinamen und DANN erst Dateien mit dem Inhalt durchsucht werden */

            var File = fs.searchFile(tb_Search.Text, false);
            if(File.Count != 0)
            {
                string res = "";
                foreach (var v in File)
                {
                    res += v.Filename + "\n";
                    res += v.Path + "\n\n";
                }
                TextBlock tb = new TextBlock();
                tb.Text = res + "\n";

                tb.MouseLeftButtonUp += Tb_MouseLeftButtonUp;
                GD_Dateiausgabe.Children.Add(tb);
            }


            //fs.C_IZ _ = db.getConf<fs.C_IZ>("database");
            //for (int i = 0; i < _.Paths.Length; i++)
            //{

            //    System.Windows.Controls.TextBox txt = new System.Windows.Controls.TextBox();
            //    List<main.FileStructure> oof = fs.searchFile(tb_Search.Text, false);
            //    oof.ForEach((p) =>
            //    {
            //        AddToGrid(p.Filename, p.Path);
            //        txt.Text += $"\n\n{p.Filename} in {p.Path}";
            //    });

            //}


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
            foreach(var d in allDrives)
            {
                newD.Add(d.Name);
            }
            return oldD.Except(newD).Concat(newD.Except(oldD)).ToList();
        }

        /** Diese Methode sollte in einem neuen Thread ausgrführt werden, um die UI nicht zu blockieren*/
        public void SetIndexProgress(string FileName, int current, int total)
        {
            current++;
            double p = 100 / Convert.ToDouble(total);
            double prozent = p * Convert.ToDouble(current);

            /** Gebe die Aufgabe zurück an den HauptThread. 
             * Nur dieser darf auf die UI zugreifen
             */
            this.Dispatcher.Invoke(() =>
            { 
                IndexProgress.Text = $"{FileName} | {current} von {total} ({Math.Round(prozent, 2)}%) ";
            });
            
        }

        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Detect_Click(sender, e);
        }
    }
}