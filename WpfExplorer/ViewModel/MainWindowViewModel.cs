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
using Newtonsoft.Json;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using WpfExplorer.Model;
using WpfExplorer.View;
using Microsoft.Win32;
using System.Threading.Tasks;

namespace WpfExplorer.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public static string AUTH_KEY = "";
        public static string TEMP_LOCATION = Path.Combine(Path.GetTempPath(), "WpfExplorer");
        public static fs.C_UC us = new fs.C_UC();
        public static string DBEXTENSION = ".wpfex";
        public static string DB_ENC_EXTENSION = ".enc.wpfex";
        public static string CONFIG_LOCATIONS = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WpfExplorer\\");
        public static DriveInfo[] allDrives;
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;
            fs.checkConfig();

            //fs.checkUserSettings();

            main.AddLog("initialized", main.status.log);
            
            tb_Ping_Text = "Connecting to Database...";
            ButtonCommand = new RelayCommand(o => Debug_Click());
            Index_Click = new RelayCommand(o => Indiziere());
            tb_Search_Command = new RelayCommand(o => tb_Search_TextChanged());
            MouseDoubleClick = new RelayCommand(o => OpenFileInExplorer(o));
            //MyCommand = new RelayCommand(o => My(o));
            MainWindowViewModel.instance = this;
            if (main.PingDB()) tb_Ping_Text = "Connected";
            else
            {
                tb_Ping_Text = "Connection failed...";
                main.ReportError(new Exception(), main.status.error, "Die Datenbank ist nicht erreichbar.Stellen Sie sicher, dass Sie den Port 3306 von ryukyun.de erreichen können");
                return;
            }
            DispatcherTimer dT = new DispatcherTimer();

            //DispatcherTimer ready = new DispatcherTimer(TimeSpan.Zero, DispatcherPriority.ApplicationIdle, ready_Tick, Application.Current.Dispatcher);
            dT.Tick += new EventHandler(SetPing);
            dT.Interval = new TimeSpan(0, 0, 0);
            dT.Start();


            //List<string> query = db.myquery("SELECT version();");
            //MessageBox.Show(query[0]);

            allDrives = DriveInfo.GetDrives();
        }

        public static MainWindowViewModel instance;

        public void OpenDialog()
        {
            DialogWindow window = new DialogWindow();
            window.Show();
        }
        public void ready_Tick()
        {
            var res = db.fetch();
            if(res == 1)
            {
                PPbtn = "↓";
            }
            else if(res == 0)
            {
                PPbtn = "↑";
            }
            else if(res == -2)
            {
                PPbtn = "✓";
            }
        }

        public void sync()
        {
            
            db.pull();
            db.push();
        }
        

        public static void CheckExtKey()
        {
            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.wpfex\\OpenWithList";
            const string subkey_enc = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.enc.wpfex\\OpenWithList";
            const string keyName = userRoot + "\\" + subkey;
            Registry.SetValue(keyName, "a", "WpfExplorer.exe", RegistryValueKind.String);
            Registry.SetValue(keyName, "b", "a", RegistryValueKind.String);
            Registry.SetValue(keyName, "MRUList", "ab", RegistryValueKind.String);
        }

        private void SetPing(object sender, EventArgs e)
        {
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

        private ICommand _deleteFileException;

        public ICommand DeleteFileException
        {
            get
            {
                if (_deleteFileException == null) _deleteFileException = new RelayCommand(DeleteFromExceptionList);
                return _deleteFileException;
            }
        }

        public void DeleteFromExceptionList(object commandParamter)
        {
            FileExceptionList.Remove(commandParamter.ToString());
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(FileExceptionList)));
        }

        public List<string> getFileExceptionList() => FileExceptionList.ToList<string>();

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

            List<FileStructure> File = fs.searchFile(tb_Search_Text, true);
            Task.Run(searchContext);
            if (File.Count != 0)
            {

                foreach (var v in File)
                {
                    double size = 0;
                    string end = "B";
                    if (v.Size > 1000) { end = "KB"; size = Convert.ToDouble(v.Size / 1000); }
                    else if (v.Size > 1000000) { end = "MB"; size = v.Size / 1000000; }
                    else if (v.Size > 1000000000) { end = "GB"; size = v.Size / 1000000000; }

                    v.Path = Path.GetDirectoryName(v.Path);

                    string res = "";
                    res += v.Filename + "\n";
                    res += v.Path + "\n";
                    res += size + end + "\n\n";

                    FoundFiles.Add(res);
                }
            }
        }

        public void searchContext()
        {

        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, newValue)) return false;

            field = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
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

        public static List<string> GetExceptionList(MainWindowViewModel model)
        {
            return model.FileExceptionList.ToList();
        }

        public void showFileExceptions()
        {
            List<string> ex = new List<string>();

            for (int i = 0; i < FileExceptionList.Count; i++) ex.Add(FileExceptionList[i]);

            MessageBox.Show(ex.ToString());
        }

        public ICommand ButtonCommand { get; set; }
        public ICommand tb_Search_Command { get; set; }
        public ICommand MouseDoubleClick { get; set; }

        public ICommand Index_Click { get; set; }

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
                    if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("tb_Ping_Text"));
                }
            }
        }






        public ObservableCollection<string> FileExceptionList { get; set; } = new ObservableCollection<string>();

        private ObservableCollection<string> foundFiles = new ObservableCollection<string>();
        public ObservableCollection<string> FoundFiles { get => foundFiles; set => SetProperty(ref foundFiles, value); }
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

        private void OpenFileInExplorer(object o)
        {
            if (o == null) return;
            string p = o.ToString();
            string[] pp = p.Split('\n');
            string path = Path.Combine(pp[1], pp[0]);

            string argument = "/select, \"" + path + "\"";
            Process.Start("explorer.exe", argument);
            //cmd.StartInfo.Arguments = "/c " + $"explorer.exe /select,\"{pp[1]}\\{pp[0]}\"";
            //cmd.EnableRaisingEvents = true;
            //cmd.Start();
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
            MessageBox.Show("Bedienung:\n*.jpg => Filter alle JPG Dateien\nHa* => Filter alle Dateien, welche mit Ha beginnen\n*2021* => Filter alle Dateien, welche 2021 im Namen haben\n\nFiles/ ignoriert jedes Verzeichnis mit dem Namen Files\n*les/ ignoriert jedes Verzeichnis mit les am Ende\nC:\\Users\\ ignoriert NUR diesen einen Ordner\n\nRechtsklicken Sie auf einen Eintrag, um diesen zu entfernen");
        }


        string _PATH = "";
        List<string> ExcList = new List<string>();

        public void Indiziere()
        {
            _PATH = main.getPathDialog();
            if (_PATH == "") return;

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.ProgressChanged += backgroundWorker1_ProgressChanged;
            worker.DoWork += backgroundWorker1_DoWork;
            worker.RunWorkerAsync();
            return;
        }

        public bool IsExceptedFile(string file)
        {
            //List<string> ExceptionList = getFileExceptionList();
            var valid = false;
            List<string> ExceptionList = new List<string>
            {
                @"C:\Temp\"
            };
            for (int i = 0; i < ExceptionList.Count; i++)
            {
                if (ExceptionList[i].EndsWith("/"))
                {
                    List<string> split = ExceptionList[i].Split('/').ToList();
                    if (file.Contains(split[0] + "\\")) return true;
                }
                if (ExceptionList[i].EndsWith("\\"))
                {
                    if (Path.GetDirectoryName(file) + "\\" == ExceptionList[i]) return true;
                }
                if (Regex.IsMatch(file, fs.WildCardToRegular(ExceptionList[i]))) { return true; }
            }

            return valid;
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BW_Files files = JsonConvert.DeserializeObject<BW_Files>(e.UserState.ToString());
            if (files.Message != null) SetIndexMessage(files.Message);
            if(files.Current != null) SetIndexProgress(files.Current.FileName, files.Current.FileCount, files.Total);

        }

        private class BW_Files
        {
            public class File
            {
                public string FileName;
                public int FileCount;
            };
            public int Total;
            public uint InDB;
            public File Current;
            public string Message;

            public BW_Files()
            {
                this.InDB = db.CountFiles();
                this.Current = new File();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            BW_Files _progress = new BW_Files();
            
            string[] files = fs.readDirSync(_PATH, true, UserSettingsViewModel.instance.RecursiveCheck);
            if (files == null) return;
            List<fs.C_File> _files = new List<fs.C_File>();

            for (int i = 0; i < files.Length; i++)
            {
                if (IsExceptedFile(files[i])) continue;
                _files.Add(fs.getFileInfo(files[i]));
                _progress.Message = "Dateien werden gesucht... " + i + " Dateien gefunden";
                worker.ReportProgress(0, JsonConvert.SerializeObject(_progress));
            }


            //Check if the file type or files are in the ExceptionList
            files = checkForExcpetionlist(files);

            int TotalFiles = files.Length;
            _progress.Total = TotalFiles;
            C_TFiles ProcessedFiles = new C_TFiles();
            for (int i = 0; i < TotalFiles; i++)
            {
                //fs.AddToIndex(files[i]);
                //Thread.Sleep(100);
                switch (fs.AddToIndex(files[i]))
                {

                    case -1: string msg = $"Die Datei {Path.GetFileName(files[i])} konnte nicht indiziert werden, da sie schon vorhanden ist"; SetIndexMessage(msg); main.AddLog(msg, main.status.warning); ProcessedFiles.FilesSkipped.Add(new C_Files { FileName = Path.GetFileName(files[i]), Path = files[i] }); break; //Datei schon vorhanden
                    case -255: ProcessedFiles.FilesErr.Add(new C_Files { FileName = Path.GetFileName(files[i]), Path = files[i] }); break; //Exception
                    case 0: ProcessedFiles.FilesOk.Add(new C_Files { FileName = Path.GetFileName(files[i]), Path = files[i] }); main.AddLog($"Die Datei {Path.GetFileName(files[i])} wurde zur Datenbank hinzugefügt", main.status.log); break;

                }
                _progress.Current.FileCount++;
                _progress.Current.FileName = _files[i].Name;
                worker.ReportProgress(0, JsonConvert.SerializeObject(_progress));
                //SetIndexProgress(_files[i], i, TotalFiles);
            }
            var temp_file = db.getConf<fs.C_IZ>("database");
            temp_file.last_sync = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            db.setConf("database", temp_file);
            string MsgText = "";
            if (ProcessedFiles.FilesSkipped.Count != 0) MsgText += $"{ProcessedFiles.FilesSkipped.Count} Dateien übersprungen\n";
            if (ProcessedFiles.FilesErr.Count != 0) MsgText += $"{ProcessedFiles.FilesErr.Count} Dateien fehlerhaft\n";
            if (ProcessedFiles.FilesOk.Count != 0) MsgText += $"{ProcessedFiles.FilesOk.Count} Dateien erfolgreich hinzugefügt\n";

            int total = ProcessedFiles.FilesOk.Count + ProcessedFiles.FilesSkipped.Count + ProcessedFiles.FilesErr.Count;
            MsgText += $"\n{total} von {TotalFiles} Dateien verarbeitet";
            main.AddLog(MsgText.Replace("\n", " | "), main.status.log);
            MessageBox.Show(MsgText);
        }

        public class C_TFiles
        {
            public List<C_Files> FilesOk = new List<C_Files>() { };
            public List<C_Files> FilesErr = new List<C_Files>() { };
            public List<C_Files> FilesSkipped = new List<C_Files>() { };
        }

        public class C_Files
        {
            public string FileName;
            public string Path;
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


        public void SetIndexProgress(string FileName, int current, int total)
        {
            double p = 100 / Convert.ToDouble(total);
            double prozent = p * Convert.ToDouble(current);
            if (Double.IsInfinity(prozent)) prozent = 0; 

            tb_IndizierteFiles = $"{current} von {total} ({Math.Round(prozent, 2)}%) | {FileName}";
            tb_DatenbankFiles = $"{current} Dateien in der Datenbank";

        }

        public void SetIndexMessage(string message)
        {
            tb_FoundFiles = message;
            //FileProgress = message;
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

        public ICommand Log_Click
        {
            get
            {
                if (settings_Click == null)
                {
                    settings_Click = new RelayCommand(PerformLogs_Click);
                }

                return settings_Click;
            }
        }


        //In der Window sollte es Tickboxen geben, welche beim Anklick Variablen ändern.
        //Z.B. ob rekursiv gesucht werden sollte. Unten im Eck sollte es einen "Schließen" Button geben
        private void PerformSettings_Click(object commandParameter)
        {
            //Einstellungen wie rekursiv indizieren, Cache leeren
            UserSettingsWindow window = new UserSettingsWindow();
            window.Title = "Einstellungen - WpfExplorer";
            window.ShowDialog();

        }

        private void PerformLogs_Click(object commandParameter)
        {
            //Einstellungen wie rekursiv indizieren, Cache leeren
            LogViewer window = new LogViewer();
            window.Title = "LogViewer - WpfExplorer";
            window.ShowDialog();

        }

        private string fileProgress;

        public string FileProgress
        {
            get => fileProgress;
            set
            {
                if (value == fileProgress) return;
                fileProgress = value;
                this?.PropertyChanged(this, new PropertyChangedEventArgs(nameof(FileProgress)));
            }
        }


        private System.Windows.Media.Brush color_ExceptionLabel;

        public System.Windows.Media.Brush Color_ExceptionLabel { get => color_ExceptionLabel; set
            {
                SetProperty(ref color_ExceptionLabel, value);
            }
        }

        private System.Windows.Media.Brush color_SuchFeldLabel;

        public System.Windows.Media.Brush Color_SuchFeldLabel { get => color_SuchFeldLabel; set => SetProperty(ref color_SuchFeldLabel, value); }

        private System.Windows.Media.Brush color_FileExceptionList;

        public System.Windows.Media.Brush Color_FileExceptionList { get => color_FileExceptionList; set => SetProperty(ref color_FileExceptionList, value); }

        private System.Windows.Media.Brush color_tb_Ping;

        public System.Windows.Media.Brush Color_tb_Ping { get => color_tb_Ping; set => SetProperty(ref color_tb_Ping, value); }

        private System.Windows.Media.Brush color_tb_Search;

        public System.Windows.Media.Brush Color_tb_Search { get => color_tb_Search; set => SetProperty(ref color_tb_Search, value); }

        private System.Windows.Media.Brush color_tb_AddExceptions;

        public System.Windows.Media.Brush Color_tb_AddExceptions { get => color_tb_AddExceptions; set => SetProperty(ref color_tb_AddExceptions, value); }

        private System.Windows.Media.Brush color_FoundFiles;

        public System.Windows.Media.Brush Color_FoundFiles { get => color_FoundFiles; set => SetProperty(ref color_FoundFiles, value); }

        private RelayCommand _bt_Log1;

        public ICommand _bt_Log
        {
            get
            {
                if (_bt_Log1 == null) _bt_Log1 = new RelayCommand(Perform_bt_Log);
                return _bt_Log1;
            }
        }

        private void Perform_bt_Log(object commandParameter)
        {
            LogViewer viewer = new LogViewer();
            viewer.ShowDialog();
        }

        private string tb_DatenbankFiles1;

        public string tb_DatenbankFiles { get => tb_DatenbankFiles1; set => SetProperty(ref tb_DatenbankFiles1, value); }

        private string tb_IndizierteFiles1;

        public string tb_IndizierteFiles { get => tb_IndizierteFiles1; set => SetProperty(ref tb_IndizierteFiles1, value); }

        private string tb_FoundFiles1;

        public string tb_FoundFiles { get => tb_FoundFiles1; set => SetProperty(ref tb_FoundFiles1, value); }

        private RelayCommand removeFromDB;
        public ICommand RemoveFromDB
        {
            get
            {
                if (removeFromDB == null) removeFromDB = new RelayCommand(Perform_removeFromDB);
                return removeFromDB;
            }
        }

        private void Perform_removeFromDB(object commandParameter)
        {

            string[] splitted = commandParameter.ToString().Split('\n') ?? new string[] { };
            fs.RemoveFromIndex(Path.Combine(splitted[1], splitted[0]));


            //BackgroundWorker worker = new BackgroundWorker();
            //worker.WorkerReportsProgress = true;
            //worker.WorkerSupportsCancellation = true;
            //worker.ProgressChanged += RemoveFromIndex_ProgressChanged;
            //worker.DoWork += RemoveFromIndex_DoWork;
            //worker.RunWorkerAsync();
        }
        private object pPbtn;

        public object PPbtn { get => pPbtn; set => SetProperty(ref pPbtn, value); }

        private double syncbtn_Rotate;

        public double Syncbtn_Rotate { get => syncbtn_Rotate; set => SetProperty(ref syncbtn_Rotate, value); }

        private RelayCommand rotate_button1;

        public void _rotate_button(object commandParameter)
        {
            MainWindow.instance.AnimationBoard.Begin();
            //MainWindow.instance.AnimationBoard.BeginAnimation()
            ready_Tick();
            //MainWindow.instance.AnimationBoard.Stop();
        }

        public ICommand rotate_button
        {
            get
            {
                if (rotate_button1 == null)
                {
                    rotate_button1 = new RelayCommand(_rotate_button);
                }

                return rotate_button1;
            }
        }

        private RelayCommand syncbtn_Sync;

        public ICommand Syncbtn_Sync
        {
            get
            {
                if (syncbtn_Sync == null)
                {
                    syncbtn_Sync = new RelayCommand(PerformSyncbtn_Sync);
                }

                return syncbtn_Sync;
            }
        }

        private void PerformSyncbtn_Sync(object commandParameter)
        {
            db.pull();
            db.push();
            ready_Tick();
        }
    }
}
