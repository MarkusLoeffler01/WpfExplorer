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

            var xx = fs.readIndexedFiles();
            var yy = xx.ToArray<main.FileStructure>();
            string res = "";
            for(int i = 0; i < yy.Length; i++)
            {
                res += yy[i].Filename+"\n";
                res += yy[i].Path+"\n\n";

            }
            MessageBox.Show(res);
            allDrives = DriveInfo.GetDrives();
            //allDrives = DriveInfo.GetDrives();



            //MessageBox.Show(string.Join("\n", fs.readDirSync("..\\..\\..\\..")));
            //MessageBox.Show(string.Join("\n", fs.readDirSync("..\\..\\..\\..", true)));
            //DetectUSB.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            //DetectUSB.Click(new object(), new RoutedEventArgs());
            //Detect_Click(Window.GetWindow(this), new RoutedEventArgs());
            //DetectUSB.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        private void SetPing(object sender, EventArgs e)
        
        {
            double PingTime = db.PingDB();
            TB_PingTime.Text = $"{PingTime}ms";
        }
        private void Button_Click(object sender, RoutedEventArgs e)
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
            for (int i = 0; i < TotalFiles; i++)
            {
                //Thread.Sleep(100);
                fs.AddToIndex(files[i]);
                SetIndexProgress(files[i], i, TotalFiles);
            }
        }

        //private void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        //{
        //    SetIndexProgress()
        //}

        public static void AddToGrid(string FileName, string FullPath)
        {
            index _ = new index(main.getPathDialog());
            _.start();
        }

        private void tb_Search_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            /**Es sollten zuerst die Dateinamen und DANN erst Dateien mit dem Inhalt durchsucht werden */

            main.isIndexerRunning = true;
            fs.CF_Ind _ = db.getConf<fs.CF_Ind>("index");
            for(int i = 0; i < _.Paths.Length; i++)
            {

                System.Windows.Controls.TextBox txt = new System.Windows.Controls.TextBox();
                main.FileStructure oof = fs.searchFile(_.Paths[i].FileName, false);
                txt.Text += $"\n\n{oof.Filename} in {oof.Path}";
            }
            

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
            DetectUSB.IsEnabled = false;
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

    }
}