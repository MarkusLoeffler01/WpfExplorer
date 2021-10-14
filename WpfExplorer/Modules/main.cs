using Microsoft.Win32;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WpfExplorer.ViewModel;
using WPFFolderBrowser;

namespace WpfExplorer
{
    public class main : Window
    {
        public static bool isIndexerRunning = false;
        public static void ReportError(Exception e, main.status status, string message = null)
        {
            string msg;
            if (e.GetType().ToString() == "Npgsql.NpgsqlException") msg = "Es wurde ein Fehler festgestellt. Stellen Sie sicher, dass sie mit dem Internet verbunden sind. Bei weiteren Fragen wenden Sie sich an ihren System Administrator\n\n\nFehlertext: " + e.Message;
            else msg = $"Es ist ein kritischer Fehler aufgetreten, das Programm muss beendet werden: \n\n{e.Message}Die Log Datei finden Sie unter {Path.Combine(MainWindowViewModel.TEMP_LOCATION)} oder Sie nutzen den internen Log-Viewer\nWeitere Hilfe erhalten Sie hier: " + e.HelpLink;

            if (status == main.status.warning)
            {
                var msgbox = message.Equals(null) ? MessageBox.Show($"Es ist eine unbekannte Warnung aufgetreten!\n\n{e.Message}", null, MessageBoxButton.OK, MessageBoxImage.Warning) : MessageBox.Show($"Es ist eine Warnung aufgetreten!\n\n{message} \n\n{e.Message}", null, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if(status == main.status.error)
            {
                MessageBox.Show(msg);
                Environment.Exit(1);
            }

            if (message != null) AddLog(message, status);

            
        }

        public static bool PingDB()
        {
            List<string> cmd = db.myquery("SELECT 1+1;");
            string msgS = "";
            for (int i = 0; i < cmd.ToArray().Length; i++)
            {
                msgS += cmd[i];
                msgS += "\n\n";
            }
            //MessageBox.Show(msgS);
            return cmd[0] == "2";
        }


        public static void AddToGrid(fs.C_File FileName, string FullPath)
        {

        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static Model.FileStructure[] FoundFiles;


        public static string getPathDialog(string path = null, string Title = "Verzeichnis zum Indizieren wählen")
        {
            WPFFolderBrowserDialog dia = new WPFFolderBrowserDialog();
            dia.InitialDirectory = path;
            if (dia.ShowDialog() == true)
            {
                MessageBox.Show(dia.FileName);
                dia.ShowHiddenItems = true;
                dia.Title = Title;
                return dia.FileName;
            }
            return null;
        }

        public static string getExportDialog(string path = null, bool encrypted = false)
        {
            SaveFileDialog dia = new SaveFileDialog();
            dia.InitialDirectory = path;
            dia.Title = "Verzeichnis zum Indizieren wählen";
            if (encrypted)
            {
                dia.DefaultExt = MainWindowViewModel.DBEXTENSION;
                UseDefaultExtAsFilterIndex(dia);
                dia.Filter = $"Verschlüsselte DB Datei| *{MainWindowViewModel.DB_ENC_EXTENSION}";
            }
            else dia.Filter = $"DB Datei| *{MainWindowViewModel.DBEXTENSION}";
            if (dia.ShowDialog() == true)
            {
                return dia.FileName;
            }
            return null;
        }

        public static string exportZipDialog(string path = null)
        {
            SaveFileDialog dia = new SaveFileDialog();
            dia.InitialDirectory = path;
            dia.Title = "Verzeichnis für die ZIP wählen";
                dia.DefaultExt = "zip";
                UseDefaultExtAsFilterIndex(dia);
                dia.Filter = $"ZIP|*.zip|7-zip|*.7z";
            if (dia.ShowDialog() == true) return dia.FileName;
            return null;
        }

        public enum status
        {
            log,
            warning,
            error
        }

        public static void AddLog(string message, status status)
        {
            //Create File if not exists
            try
            {
                if(!Directory.Exists(MainWindowViewModel.TEMP_LOCATION)) Directory.CreateDirectory(MainWindowViewModel.TEMP_LOCATION);
                string path = Path.Combine(MainWindowViewModel.TEMP_LOCATION, DateTime.Now.ToString("yyyy-MM-dd") + ".log");
                using (StreamWriter sw = (File.Exists(path)) ? File.AppendText(path) : File.CreateText(path))
                {
                    string now = DateTime.Now.ToString("g");
                    string text = $"[{now}] {status} | {message}";
                    sw.WriteLine(text);
                    sw.Close();
                }
            }
            catch (Exception e) { MessageBox.Show($"Log-Dateien konnten nicht geschrieben werden. Prüfen Sie die Berechtigungen für das Verzeichnis {MainWindowViewModel.TEMP_LOCATION}\n\n{e}"); }

        }


        public static void UseDefaultExtAsFilterIndex(FileDialog dialog)
        {
            var ext = "*." + dialog.DefaultExt;
            var filter = dialog.Filter;
            var filters = filter.Split('|');
            for (int i = 1; i < filters.Length; i += 2)
            {
                if (filters[i] == ext)
                {
                    dialog.FilterIndex = 1 + (i - 1) / 2;
                    return;
                }
            }
        }

        //Create a function which displays a messagebox with chechboxes inside


        public static MainWindow getSession<MainWindow>()
        {
            return Application.Current.Windows.Cast<MainWindow>().First();
        }

        

        //Gebe das Objekt der ViewModel zurück
        public MainWindow getMVVM()
        {
            var d = new MainWindow();
            this.Dispatcher.BeginInvoke((Action)delegate ()
            {
                d = Application.Current.Windows.Cast<MainWindow>().FirstOrDefault();
            });
            return d;
        }
    }
}
