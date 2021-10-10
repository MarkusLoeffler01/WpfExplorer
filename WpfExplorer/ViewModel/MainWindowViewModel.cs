﻿using CommandHelper;
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
using System.Windows;
using System.Windows.Threading;
using iTextSharp;
using iTextSharp.text;
using System.Text.RegularExpressions;
using WpfExplorer.Model;
using WpfExplorer.View;
using Microsoft.Win32;

namespace WpfExplorer.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public static string AUTH_KEY = "";
        public static string DBEXTENSION = ".wpfex";
        public static string DB_ENC_EXTENSION = ".enc.wpfex";
        public static string CONFIG_LOCATIONS = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WpfExplorer\\");
        public static DriveInfo[] allDrives;
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
            
            fs.checkConfig();

            //fs.ExtractText("C:\\Temp\\owo");
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;
            //var path = new OpenFileDialog();
            //path.ShowDialog();
            //fs.import(path.FileName);
            fs.export();
            return;
            //fs.import(new FolderBrowserDialog().SelectedPath);

            tb_Ping_Text = "Connecting to Database...";
            ButtonCommand = new RelayCommand(o => Debug_Click());
            Index_Click = new RelayCommand(o => Indiziere());
            tb_Search_Command = new RelayCommand(o => tb_Search_TextChanged());
            MouseDoubleClick = new RelayCommand(o => OpenFileInExplorer(o));
            //MyCommand = new RelayCommand(o => My(o));
            
            if (main.PingDB()) tb_Ping_Text = "Connected";
            else 
            {
                tb_Ping_Text = "Connection failed..."; 
                main.ReportError(new Exception("Ping not successfull")); 
                return; 
            }
            DispatcherTimer dT = new DispatcherTimer();

            //DispatcherTimer ready = new DispatcherTimer(TimeSpan.Zero, DispatcherPriority.ApplicationIdle, ready_Tick, Application.Current.Dispatcher);
            dT.Tick += new EventHandler(SetPing);
            dT.Interval = new TimeSpan(0, 0, 1);
            dT.Start();
           

            //List<string> query = db.myquery("SELECT version();");
            //MessageBox.Show(query[0]);

            allDrives = DriveInfo.GetDrives();
        }

        public void OpenDialog()
        {
            DialogWindow window = new DialogWindow();
            window.Show();
        }
        public void ready_Tick()
        {
            db.push(this);
            db.pull();
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

            List<FileStructure> File = fs.searchFile(tb_Search_Text, false);
            if (File.Count != 0)
            {
                
                foreach (var v in File)
                {
                    double size = 0;
                    string end = "b";
                    if(v.Size > 1000) { end = "kB"; size = Convert.ToDouble(v.Size / 1000); }
                    else if (v.Size > 1000000) { end = "MB"; size = v.Size / 1000000; }
                    else if (v.Size > 1000000000) { end = "GB"; size = v.Size / 1000000000; }

                    string res = "";
                    res += v.Filename + "\n";
                    res += v.Path + "\n";
                    res += size+end + "\n\n";

                    FoundFiles.Add(res);
                }
            }
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, newValue)) return false;

            field = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        public void showFileExceptions()
        {
            List<string> ex = new List<string>();

            for(int i = 0; i < FileExceptionList.Count; i++) ex.Add(FileExceptionList[i]);

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
        public ObservableCollection<string> FoundFiles { get; set; } = new ObservableCollection<string>();
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
        }


        string _PATH = "";
        List<string> ExcList;

        public void Indiziere()
        {
            _PATH = main.getPathDialog();
            if (_PATH == "") return;

            Task.Run(backgroundWorker1_DoWork);
            return;
        }

        private void backgroundWorker1_DoWork()
        {
            string path = "C:\\Users\\LoefflerM\\OneDrive - Putzmeister Holding GmbH\\Desktop\\Berichtsheft";

            string[] files = fs.readDirSync(_PATH, true, true);
            List<fs.C_File> _files = new List<fs.C_File>();
            for(int  i = 0; i < files.Length; i++)
            {
                _files.Add(fs.getFileInfo(files[i]));
                SetIndexMessage("Dateien werden gesucht... "+i+" Dateien gefunden");
            }
            

            //Check if the file type or files are in the ExceptionList
            files = checkForExcpetionlist(files);

            int TotalFiles = files.Length;
            C_TFiles ProcessedFiles = new C_TFiles();
            for (int i = 0; i < TotalFiles; i++)
            {
                //fs.AddToIndex(files[i]);
                //Thread.Sleep(100);
                switch (fs.AddToIndex(files[i]))
                {

                    case -1: SetIndexMessage($"Die Datei {Path.GetFileName(files[i])} konnte nicht indiziert werden, da sie schon vorhanden ist"); ProcessedFiles.FilesSkipped.Add(new C_Files { FileName = Path.GetFileName(files[i]), Path = files[i] }); break; //Datei schon vorhanden
                    case -255: ProcessedFiles.FilesErr.Add(new C_Files { FileName = Path.GetFileName(files[i]), Path = files[i] }); break; //Exception
                    case 0: ProcessedFiles.FilesOk.Add(new C_Files {FileName = Path.GetFileName(files[i]), Path = files[i] });  break;

                }
                SetIndexProgress(_files[i], i, TotalFiles);
            }
            var temp_file = db.getConf<fs.C_IZ>("database");
            temp_file.last_sync = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            db.setConf("database", temp_file);
            string MsgText = "";
            if(ProcessedFiles.FilesSkipped.Count != 0) MsgText += $"{ProcessedFiles.FilesSkipped.Count} Dateien übersprungen\n";
            if(ProcessedFiles.FilesErr.Count != 0) MsgText += $"{ProcessedFiles.FilesErr.Count} Dateien fehlerhaft\n";
            if(ProcessedFiles.FilesOk.Count != 0) MsgText += $"{ProcessedFiles.FilesOk.Count} Dateien erfolgreich hinzugefügt\n";

            int total = ProcessedFiles.FilesOk.Count + ProcessedFiles.FilesSkipped.Count + ProcessedFiles.FilesOk.Count;
            MsgText += $"\n{total} von {TotalFiles} Dateien verarbeitet";
            MessageBox.Show(MsgText);
        }

        private string[] checkForExcpetionlist(string[] files)
        {
            /*
             * Wenn el[i][0] == *, el.Includes(el[i][el[i]-1]
             * 
             * prüfe ob Path.GetExtension(files[i]) != '' && ob in el, dann entferne
             */

            List<string> el = ExcList ?? new List<string>();
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


        /** Diese Methode sollte in einem neuen Thread ausgrführt werden, um die UI nicht zu blockieren*/
        public void SetIndexProgress(fs.C_File FileName, int current, int total)
        {
            current++;
            double p = 100 / Convert.ToDouble(total);
            double prozent = p * Convert.ToDouble(current);

            /** Gebe die Aufgabe zurück an den HauptThread. 
             * Nur dieser darf auf die UI zugreifen
             */
                FileProgress = $"{current} von {total} ({Math.Round(prozent, 2)}%) | {FileName.Name}";

        }

        public void SetIndexMessage(string message)
        {
            FileProgress = message;
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

        private string fileProgress;

        public string FileProgress { 
            get => fileProgress; 
            set
            {
                if (value == fileProgress) return;
                fileProgress = value;
                this?.PropertyChanged(this, new PropertyChangedEventArgs(nameof(FileProgress)));
            }
        }
    }
}
