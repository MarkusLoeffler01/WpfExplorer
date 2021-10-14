using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfExplorer.ViewModel
{
    public class LogViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public LogViewModel()
        {
            instance = this;
            
        }
        public static LogViewModel instance;

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
        int i = 0;

        public void fillLogs(object sender, RoutedEventArgs e)
        {
            string[] files = fs.readDirSync(Path.Combine(MainWindowViewModel.TEMP_LOCATION));
            logText.Text = "Hallo";
            foreach (string file in files)
            {
                
                DateTime now = DateTime.Parse(Path.GetFileNameWithoutExtension(file));
                LogList.Add(now.ToString("D"));
            }
        }

        public void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control) return;

            e.Handled = true;
            if (e.Delta > 0) ++TBFontSize.FontSize;
            else --TBFontSize.FontSize;
        }

        private object logSelected;

        public object LogSelected { get => logSelected; set => SetProperty(ref logSelected, value); }

        private ObservableCollection<string> logList = new ObservableCollection<string>();
        public ObservableCollection<string> LogList { get => logList; set => SetProperty(ref logList, value); }

        //public ListBox LogList { get => logList; set => SetProperty(ref logList, value); }

        private TextBlock logText = new TextBlock();

        public TextBlock LogText { get => logText; set => SetProperty(ref logText, value); }
        public TextBox tBFontSize = new TextBox();

        public TextBox TBFontSize { get => tBFontSize; set => SetProperty(ref tBFontSize, value);  }
    }
}
