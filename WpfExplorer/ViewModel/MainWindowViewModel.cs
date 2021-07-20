using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfExplorer.ViewModel
{
    class MainWindowViewModel : INotifyPropertyChanged 
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand KeyInputCommand
        {
            get 
            {
                if (_keyInputCommand == null) _keyInputCommand = new RelayCommand(KeyDown);
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

    }
}
