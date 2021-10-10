using CommandHelper;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Input;
using WpfExplorer.View;
using System.Drawing;

namespace WpfExplorer.ViewModel
{
    public class UserSettingsViewModel : INotifyPropertyChanged
    {
        public string windowTitle = "UserSettingsViewModel";
        public string WindowTitle { get => windowTitle; set => SetProperty(ref windowTitle, value); }

        

        public string messageTitle = "MessageTitle";
        public string MessageTitle { get => messageTitle; set => SetProperty(ref messageTitle, value); }


        public event PropertyChangedEventHandler PropertyChanged;
        public UserSettingsViewModel()
        {
            //windowTitle = title;
            this.CloseWindowCommand = new RelayCommand<Window>(this.CloseWindow);
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }

        private CommandHelper.RelayCommand oKButton;

        public ICommand OKButton
        {
            get
            {
                if (oKButton == null) oKButton = new CommandHelper.RelayCommand(PerformOKButton);
                return oKButton;
            }
        }

        private void PerformOKButton(object commandParameter)
        {
            fs.exportPassword = passwordText;
            CloseWindowCommand.Execute(commandParameter);
            
        }

        public RelayCommand<Window> CloseWindowCommand { get; private set; }

        private void CloseWindow(Window window)
        {
            if (window != null) window.Close();
        }


        private string passwordText;

        public string PasswordText { get => passwordText; set => SetProperty(ref passwordText, value); }

        private bool recursiveCheck1 = false;

        public bool RecursiveCheck { get => recursiveCheck1; set => SetProperty(ref recursiveCheck1, value); }

        private bool darkModeCheck;
        public bool DarkModeCheck { get => darkModeCheck; set
                {
                    SetProperty(ref darkModeCheck, value);
                    MainWindow win = main.getSession<MainWindow>();
                    //UserSettingsWindow win2 = main.getSession<UserSettingsWindow>();
                if (win != null)
                {
                    var converter = new System.Windows.Media.BrushConverter();

                    if (darkModeCheck) win.Background  = (System.Windows.Media.Brush)converter.ConvertFromString("#252525");
                    else win.Background  = (System.Windows.Media.Brush)converter.ConvertFromString("#FFFFFF");
                }
                fs.C_UC conf = db.getConf<fs.C_UC>("userconfig");
                conf.Recursive = darkModeCheck;
                db.setConf("usersettings", conf);
            }
        }

        private RelayCommand<Window> importButton;

        public ICommand ImportButton
        {
            get
            {
                if (importButton == null)
                {
                    importButton = new RelayCommand<Window>(PerformImportButton);
                }

                return importButton;
            }
        }

        private void PerformImportButton(object commandParameter)
        {
            var path = new OpenFileDialog();
            path.ShowDialog();
            fs.import(path.FileName);
        }

        private RelayCommand<Window> exportButton;

        public ICommand ExportButton
        {
            get
            {
                if (exportButton == null)
                {
                    exportButton = new RelayCommand<Window>(PerformExportButton);
                }

                return exportButton;
            }
        }

        private void PerformExportButton(object commandParameter)
        {
            fs.export();
        }
    }
}
