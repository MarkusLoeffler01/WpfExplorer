using CommandHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RelayCommand = CommandHelper.RelayCommand;
using System.Threading;
using Newtonsoft.Json;
using System.Threading.Tasks;
using WpfExplorer.Model;

namespace WpfExplorer.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public static string AUTH_KEY = "";
        public static string CONFIG_LOCATIONS = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WpfExplorer\\");
        public static DriveInfo[] allDrives;
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {

            //Hole die ID von der Datei
            var FILE = db.getConf<fs.C_IZ>("database");
            if(FILE.AUTH_KEY == null)
            {
                FILE.AUTH_KEY = main.RandomString(64);
                fs.writeFileSync(MainWindowViewModel.CONFIG_LOCATIONS+"\\database.json", JsonConvert.SerializeObject(FILE), true);
            }

            AUTH_KEY = FILE.AUTH_KEY;
            //Task.Run(db.sync);
            tb_Ping_Text = "Connecting to Database...";
            ButtonCommand = new RelayCommand(o => Debug_Click());
            tb_Search_Command = new RelayCommand(o => tb_Search_TextChanged());
            MouseDoubleClick = new RelayCommand(o => My(o));
            //MyCommand = new RelayCommand(o => My(o));
            fs.checkConfig();
            db.initDB();
            if (main.PingDB()) tb_Ping_Text = "Connected";
            else 
            {
                tb_Ping_Text = "Connection failed..."; 
                main.ReportError(new Exception("Ping not successfull")); 
                return; 
            }
            System.Windows.Threading.DispatcherTimer dT = new System.Windows.Threading.DispatcherTimer();
            dT.Tick += new EventHandler(SetPing);
            dT.Interval = new TimeSpan(0, 0, 1);
            dT.Start();
           

            List<string> query = db.myquery("SELECT version();");
            MessageBox.Show(query[0]);

            allDrives = DriveInfo.GetDrives();
        }

        private void SetPing(object sender, EventArgs e)
        {
            Console.WriteLine("PING");
            double PingTime = db.PingDB();
            tb_Ping_Text = $"{PingTime}ms";
        }

        public ICommand KeyInputCommand
        {
            get
            {
                if (_keyInputCommand == null) _keyInputCommand = new GalaSoft.MvvmLight.Command.RelayCommand(KeyDown);
                return _keyInputCommand;
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        private void KeyDown()
        {
            // Hier Logik
        }

        private ICommand _keyInputCommand;

        private string _name;

        private RelayCommand enterKeyCommand;

        public ICommand EnterKeyCommand
        {
            get
            {
                if (enterKeyCommand == null)
                {
                    enterKeyCommand = new RelayCommand(EnterKey);
                }

                return enterKeyCommand;
            }
        }

        private void EnterKey(object commandParameter)
        {
            MessageBox.Show(commandParameter.ToString());

            FileExceptionList.Add(commandParameter.ToString());
            tb_AddExceptionsText = "";
        }

        public void tb_Search_TextChanged()
        {
            FoundFiles.Clear();

            /** Nichts eingegeben = nichts anzeigen */
            if (tb_Search_Text.Length == 0) return;
            /**Es sollten zuerst die Dateinamen und DANN erst Dateien mit dem Inhalt durchsucht werden */

            var File = fs.searchFile(tb_Search_Text, false);
            if (File.Count != 0)
            {

                foreach (var v in File)
                {
                    string res = "";
                    res += v.Filename + "\n";
                    res += v.Path + "\n\n";

                    FoundFiles.Add(res);
                }
            }


            //fs.C_IZ _ = db.getConf<fs.C_IZ>("database");
            //for (int i = 0; i < _.Paths.Length; i++)
            //{

            //    System.Windows.Controls.TextBox txt = new System.Windows.Controls.TextBox();
            //    List<Model.FileStructure> oof = fs.searchFile(tb_Search.Text, false);
            //    oof.ForEach((p) =>
            //    {
            //        AddToGrid(p.Filename, p.Path);
            //        txt.Text += $"\n\n{p.Filename} in {p.Path}";
            //    });

            //}


        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, newValue)) return false;

            field = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        public List<string> getFileExceptions()
        {
            return new List<string>(FileExceptionList);
        }

        public ICommand ButtonCommand { get; set; }
        public ICommand tb_Search_Command { get; set; }
        public ICommand MouseDoubleClick { get; set; }

        //public ICommand Index_Click { get; set; }

        //public ICommand MyCommand { get; set; }

        public void MyCommand(object sender, SelectionChangedEventArgs e)
        {
            List<string> lul = (List<string>)e.AddedItems;
            MessageBox.Show(string.Join("\n", lul));
        }

        public void Debug_Click()
        {
            MessageBox.Show(string.Join("\n", FileExceptionList));
        }

        public string tb_AddExceptionsText { get; set; } = null;

        public string _tb_Search_Text = "";
        public string tb_Search_Text
        {
            get { return _tb_Search_Text; }
            set
            {
                if (!_tb_Search_Text.Equals(value))
                {
                    _tb_Search_Text = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("tb_Search_Text"));
                        tb_Search_TextChanged();
                    }
                }

            }
        }

        public string _tb_Ping_Text = "";
        public string tb_Ping_Text
        {
            get { return _tb_Ping_Text; }
            set
            {
                if (!_tb_Ping_Text.Equals(value))
                {
                    _tb_Ping_Text = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("tb_Ping_Text"));
                    }
                    //PropertyChanged(this, new PropertyChangedEventArgs("tb_Ping_Text"));
                }
            }
        }






        public ObservableCollection<string> FileExceptionList { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> FoundFiles { get; set; } = new ObservableCollection<string>();
        //public ObservableCollection<string> MyProperty { get; set; } = new ObservableCollection<string>();
        private object selectedFileException;

        public object SelectedFileException { get => selectedFileException; set => SetProperty(ref selectedFileException, value); }

        public Model.FileStructure selectedFile;

        public Model.FileStructure SelectedFile
        {
            get { return selectedFile; }
            set
            {
                //if (value == selectedFile) return;
                selectedFile = value;
                PropertyChanged(this, new PropertyChangedEventArgs("selectedFile"));
            }
        }

        public void FileSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            ListBoxItem lbi = ((sender as ListBox).SelectedItem as ListBoxItem);
            MessageBox.Show(lbi.Content.ToString());
            MessageBox.Show(selectedFile.Path);
            Process.Start(lbi.Content.ToString());
        }

        private void My(object o)
        {
            if (o == null) return;
            string p = o.ToString();
            string[] pp = p.Split('\n');

            Process cmd = new Process();
            cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.Arguments = "/c " + $"explorer.exe /select,\"{pp[1]}\\{pp[0]}\"";
            cmd.EnableRaisingEvents = true;
            cmd.Start();
        }

        private RelayCommand _bt_Help1;

        public ICommand _bt_Help
        {
            get
            {
                if (_bt_Help1 == null)
                {
                    _bt_Help1 = new RelayCommand(Perform_bt_Help);
                }

                return _bt_Help1;
            }
        }

        private void Perform_bt_Help(object commandParameter)
        {
            MessageBox.Show("Bedienung:\n*.jpg => Filter alle JPG Dateien\nHa* => Filter alle Dateien, welche mit Ha beginnen\n*2021* => Filter alle Dateien, welche 2021 im Namen haben\n\nFiles/ ignoriert jedes Verzeichnis mit dem Namen Files\n*les/ ignoriert jedes Verzeichnis mit les am Ende\nC:\\Users\\ ignoriert NUR diesen einen Ordner\n\nDoppelklicken Sie auf einen Eintrag, um diesen zu entfernen");
        }        //public object SelectedFile { get => selectedFile; set => SetProperty(ref selectedFile, value); }


        string _PATH = "";
        List<string> ExcList;

        public void Index_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            _PATH = main.getPathDialog();
            ExcList = GetExceptionList();
            if (_PATH == "") return;

            worker.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            worker.WorkerSupportsCancellation = true;
            //worker.WorkerReportsProgress = true;
            //worker.ProgressChanged += OnProgressChanged;
            worker.RunWorkerAsync();
            return;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string path = "C:\\Users\\LoefflerM\\OneDrive - Putzmeister Holding GmbH\\Desktop\\Berichtsheft";

            string[] files = fs.readDirSync(_PATH, true, true);

            //Check if the file type or files are in the ExceptionList
            MessageBox.Show(files.Length + "\n" + string.Join(",", files));
            files = checkForExcpetionlist(files);
            MessageBox.Show(files.Length + "\n" + string.Join(",", files));

            int TotalFiles = files.Length;
            ScannedFilesList ProcessedFiles = new ScannedFilesList();
            for (int i = 0; i < TotalFiles; i++)
            {
                //fs.AddToIndex(files[i]);
                Thread.Sleep(100);
                switch (fs.AddToIndex(files[i]))
                {

                    case -1: MessageBox.Show($"Die Datei {Path.GetFileName(files[i])} konnte nicht indiziert werden, da sie schon vorhanden ist"); ProcessedFiles.FilesErr.Add(new ScannedFile { FileName = Path.GetFileName(files[i]), Path = files[i] }); break; //Datei schon vorhanden
                    case -255: break; //Exception
                    case 0: break;

                }
                SetIndexProgress(files[i], i, TotalFiles);
            }
            MessageBox.Show(TotalFiles.ToString() + " Dateien erfolgreich hinzugefügt");
        }

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


        public List<string> GetExceptionList()
        {

            return new main().getMVVM().FileExceptionList.ToList();
            //return ListBox.Items.Cast<string>().ToList();
        }

        public static string IndexProgress { get; set; }

        /** Diese Methode sollte in einem neuen Thread ausgrführt werden, um die UI nicht zu blockieren*/
        static public void SetIndexProgress(string FileName, int current, int total)
        {
            current++;
            double p = 100 / Convert.ToDouble(total);
            double prozent = p * Convert.ToDouble(current);

            /** Gebe die Aufgabe zurück an den HauptThread. 
             * Nur dieser darf auf die UI zugreifen
             */
            //this.Dispatcher.Invoke(() =>
            //{
                IndexProgress = $"{FileName} | {current} von {total} ({Math.Round(prozent, 2)}%) ";
            //});

        }

        private RelayCommand settings_Click;

        public ICommand Settings_Click
        {
            get
            {
                if (settings_Click == null)
                {
                    settings_Click = new RelayCommand(PerformSettings_Click);
                }

                return settings_Click;
            }
        }


        //In der Window sollte es Tickboxen geben, welche beim Anklick Variablen ändern.
        //Z.B. ob rekursiv gesucht werden sollte. Unten im Eck sollte es einen "Schließen" Button geben
        private void PerformSettings_Click(object commandParameter)
        {
            //Einstellungen wie rekursiv indizieren, Cache leeren
            Window popup = new Window();
            popup.MaxHeight = popup.MinHeight = popup.Height = 300;
            popup.MaxWidth = popup.MinWidth = popup.Width = 400;
            popup.Title = "Einstellungen - WpfExplorer";
            var grid = new Grid();
      
            popup.ShowDialog();

        }
    }
}
