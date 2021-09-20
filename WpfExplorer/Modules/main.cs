using System;
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
        public static void ReportError(Exception e)
        {
            string msg;
            if (e.GetType().ToString() == "Npgsql.NpgsqlException") msg = "Es wurde ein Fehler festgestellt. Stellen Sie sicher, dass sie mit dem Internet verbunden sind. Bei weiteren Fragen wenden Sie sich an ihren System Administrator\n\n\nFehlertext: " + e.Message;
            else msg = "Es ist ein unbekannter Fehler aufgetreten: \n\n" + e.Message + "\nWeitere Hilfe erhalten Sie hier: " + e.HelpLink;

            MessageBox.Show(msg);
            Environment.Exit(1);
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

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static FileStructure[] FoundFiles;

        public class FileStructure
        {
            public string Path;
            public string Filename;
        }

        public static string getPathDialog(string path = null)
        {
            WPFFolderBrowserDialog dia = new WPFFolderBrowserDialog();
            dia.InitialDirectory = path;
            if (dia.ShowDialog() == true)
            {
                MessageBox.Show(dia.FileName);
                dia.ShowHiddenItems = true;
                dia.Title = "Verzeichnis zum Indizieren wählen";
                return dia.FileName;
            }
            return "";

        }

        //Create a function which displays a messagebox with chechboxes inside

        
        public static MainWindow getSession()
        {
            return Application.Current.Windows.Cast<MainWindow>().First();
        }

        //Gebe das Objekt der ViewModel zurück
        public MainWindowViewModel getMVVM()
        {
            var d = new MainWindowViewModel();
            this.Dispatcher.BeginInvoke((Action)delegate ()
            {
                d = Application.Current.Windows.Cast<MainWindowViewModel>().FirstOrDefault();
            });
            return d;
        }
    }
}
