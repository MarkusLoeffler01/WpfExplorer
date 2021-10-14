using CommandHelper;
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
            foreach (string file in files)
            {
                
                DateTime now = DateTime.Parse(Path.GetFileNameWithoutExtension(file));
                LogList.Add(now.ToString("D"));
            }
        }

        private void fillLogsManually()
        {
            string[] files = fs.readDirSync(Path.Combine(MainWindowViewModel.TEMP_LOCATION));
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
            if (e.Delta > 0) ++TBFontSize;
            else --TBFontSize;
        }

        private object logSelected;

        public object LogSelected { get => logSelected; set => SetProperty(ref logSelected, value); }

        private ObservableCollection<string> logList = new ObservableCollection<string>();
        public ObservableCollection<string> LogList { get => logList; set => SetProperty(ref logList, value); }

        //public ListBox LogList { get => logList; set => SetProperty(ref logList, value); }

        private string logText = "";

        public string LogText { get => logText; set => SetProperty(ref logText, value); }
        public int tBFontSize = 12;

        public int TBFontSize { get => tBFontSize; set => SetProperty(ref tBFontSize, value);  }

        private RelayCommand selectLog;

        public ICommand SelectLog
        {
            get
            {
                if (selectLog == null)
                {
                    selectLog = new RelayCommand(PerformSelectLog);
                }

                return selectLog;
            }
        }

        private void PerformSelectLog(object commandParameter)
        {
            string command = commandParameter.ToString();

            DateTime time;
            string timeS = "";
            DateTime.TryParse(command, out time);
            if (time != null) timeS = time.ToString("yyyy-MM-dd")+".log";
            try
            {
                string content = fs.readFileSync(Path.Combine(MainWindowViewModel.TEMP_LOCATION, timeS));
                LogText = content;
            }
            catch(Exception e)
            {
                main.ReportError(e, main.status.error, $"Die LogDatei '{timeS}' konnte nicht gelesen werden. Prüfen Sie den Dateinamen");
            }
            //PropertyChanged(this, new PropertyChangedEventArgs(nameof(LogList)));
        }

        private RelayCommand deleteLog;

        public ICommand DeleteLog
        {
            get
            {
                if (deleteLog == null)
                {
                    deleteLog = new RelayCommand(PerformDeleteLog);
                }

                return deleteLog;
            }
        }

        private void PerformDeleteLog(object commandParameter)
        {
            string command = commandParameter.ToString();

            DateTime time;
            string timeS = "";
            DateTime.TryParse(command, out time);
            if (time != null) timeS = time.ToString("yyyy-MM-dd") + ".log";
            try
            {
                File.Delete(Path.Combine(MainWindowViewModel.TEMP_LOCATION, timeS));
            }
            catch (Exception e)
            {
                main.ReportError(e, main.status.error, $"Die LogDatei '{timeS}' konnte nicht gelesen werden. Prüfen Sie den Dateinamen");
            }
        }

        private RelayCommand wipeLogs;

        public ICommand WipeLogs
        {
            get
            {
                if (wipeLogs == null)
                {
                    wipeLogs = new RelayCommand(PerformWipeLogs);
                }

                return wipeLogs;
            }
        }

        private void PerformWipeLogs(object commandParameter)
        {
            string[] fileList = fs.readDirSync(Path.Combine(MainWindowViewModel.TEMP_LOCATION), true);
            for (int i = 0; i < fileList.Length; i++) File.Delete(fileList[i]);
            LogText = "";
            LogList.Clear();
            fillLogsManually();
        }
    }
}
