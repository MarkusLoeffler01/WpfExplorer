using CommandHelper;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RelayCommand = CommandHelper.RelayCommand;

namespace WpfExplorer.ViewModel
{
    class MainWindowViewModel : INotifyPropertyChanged 
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

            SerhatList.Add(commandParameter.ToString());

            FileExceptionCollection += commandParameter.ToString();
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }

        public ObservableCollection<string> SerhatList { get; set; } = new ObservableCollection<string>();
        //public ObservableCollection<string> MyProperty { get; set; } = new ObservableCollection<string>();

        private System.Collections.IEnumerable fileExceptionCollection;

        public System.Collections.IEnumerable FileExceptionCollection { get => fileExceptionCollection; set => SetProperty(ref fileExceptionCollection, value); }

        private object selectedFileException;

        public object SelectedFileException { get => selectedFileException; set => SetProperty(ref selectedFileException, value); }
    }
}
