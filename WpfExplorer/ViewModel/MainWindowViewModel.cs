using CommandHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RelayCommand = CommandHelper.RelayCommand;

namespace WpfExplorer.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        string _PATH = "";
        public static string CONFIG_LOCATIONS = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WpfExplorer\\");
        public static DriveInfo[] allDrives;
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
            ButtonCommand = new RelayCommand(o => Debug_Click());
            tb_Search_Command = new RelayCommand(o => tb_Search_TextChanged());
            MouseDoubleClick = new RelayCommand(o => My(o));
            //MyCommand = new RelayCommand(o => My(o));
            fs.checkConfig();
            db.initDB();
            if (main.PingDB()) { tb_Ping_Text = "Connected"; }
            else { tb_Ping_Text = "Connection failed..."; main.ReportError(new Exception("Ping not successfull")); return; }
            System.Windows.Threading.DispatcherTimer dT = new System.Windows.Threading.DispatcherTimer();
            dT.Tick += new EventHandler(SetPing);
            dT.Interval = new TimeSpan(0, 0, 1);
            dT.Start();

            List<string> query = db.myquery("SELECT version();");
            MessageBox.Show("MySQL " + query[0]);

            allDrives = DriveInfo.GetDrives();
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

        private void ToExceptionList()
        {
            //List<string> _ = MainWindow.GetExceptionList();
            //foreach (var d in _) MainWindow.ListBox.Items.Add(d);
            return;
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
            //    List<main.FileStructure> oof = fs.searchFile(tb_Search.Text, false);
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

        public string _tb_Search_Text { get; set; } = null;
        public string tb_Search_Text
        {
            get { return _tb_Search_Text; }
            set
            {
                if (_tb_Search_Text != value)
                {
                    _tb_Search_Text = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("tb_Search_Text"));
                    PropertyChanged(this, new PropertyChangedEventArgs("_tb_Search_Text"));
                    tb_Search_TextChanged();
                }

            }
        }

        public string _tb_Ping_Text { get; set; } = null;
        public string tb_Ping_Text
        {
            get { return _tb_Ping_Text; }
            set
            {
                if (_tb_Ping_Text != value)
                {
                    _tb_Ping_Text = value;
                    //PropertyChanged(this, new PropertyChangedEventArgs("_tb_Ping_Text"));
                    //PropertyChanged(this, new PropertyChangedEventArgs("tb_Ping_Text"));
                }
            }
        }






        public ObservableCollection<string> FileExceptionList { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> FoundFiles { get; set; } = new ObservableCollection<string>();
        //public ObservableCollection<string> MyProperty { get; set; } = new ObservableCollection<string>();
        private object selectedFileException;

        public object SelectedFileException { get => selectedFileException; set => SetProperty(ref selectedFileException, value); }

        public main.FileStructure selectedFile;

        public main.FileStructure SelectedFile
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


    }
}
