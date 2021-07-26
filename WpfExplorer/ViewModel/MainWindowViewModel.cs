using CommandHelper;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using RelayCommand = CommandHelper.RelayCommand;

namespace WpfExplorer.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand KeyInputCommand
        {
            get
            {
                if (_keyInputCommand == null) _keyInputCommand = new GalaSoft.MvvmLight.Command.RelayCommand(KeyDown);
                return _keyInputCommand;
            }
        }

        public string KeineÄnderung { get; }

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
                string res = "";
                foreach (var v in File)
                {
                    res += v.Filename + "\n";
                    res += v.Path + "\n\n";
                }

                FoundFiles.Add(res+"\n");
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

        private RelayCommand tb_Search_Command1;

        //public ICommand tb_Search_Command
        //{
        //    get
        //    {
        //        if (tb_Search_Command1 == null)
        //        {
        //            tb_Search_Command1 = new RelayCommand(tb_Search_);
        //        }

        //        return tb_Search_Command1;
        //    }
        //}

        private void tb_Search_(object commandParameter)
        {
        }

        public ICommand ButtonCommand { get; set; }
        public ICommand tb_Search_Command { get; set; }

        public MainWindowViewModel()
        {
            ButtonCommand = new RelayCommand(o => Debug_Click());
            tb_Search_Command = new RelayCommand(o => tb_Search_TextChanged());
        }

        public void Debug_Click()
        {
            MessageBox.Show(string.Join("\n", FileExceptionList));
        }

        public string tb_AddExceptionsText { get; set; } = null;

        public string _tb_Search_Text;
        public string tb_Search_Text {
            get { return _tb_Search_Text; }
            set
            {
                if(_tb_Search_Text != value)
                {
                    _tb_Search_Text = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(tb_Search_Text));
                    PropertyChanged(this, new PropertyChangedEventArgs(_tb_Search_Text));
                    tb_Search_TextChanged();
                }
                
            }
        }


        public ObservableCollection<string> FileExceptionList { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> FoundFiles { get; set; } = new ObservableCollection<string>();
        //public ObservableCollection<string> MyProperty { get; set; } = new ObservableCollection<string>();
        private object selectedFileException;

        public object SelectedFileException { get => selectedFileException; set => SetProperty(ref selectedFileException, value); }

        
    }
}
