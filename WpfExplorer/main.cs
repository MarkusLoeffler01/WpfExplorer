using System;
using System.Collections.Generic;
using System.Windows;
using WPFFolderBrowser;

namespace WpfExplorer
{
    public class main : Window
    {
        public static bool isIndexerRunning = false;
        public static void ReportError(Exception e)
        {
            string msg;

            if(e.GetType().ToString() == "Npgsql.NpgsqlException") msg = "Es wurde ein Fehler festgestellt. Stellen Sie sicher, dass sie mit dem Internet verbunden sind. Bei weiteren Fragen wenden Sie sich an ihren System Administrator\n\n\nFehlertext: " + e.Message;
            else  msg = "Es ist ein unbekannter Fehler aufgetreten: \n\n" + e.Message +"\nWeitere Hilfe erhalten Sie hier: "+ e.HelpLink;
            
            MessageBox.Show(msg);
            Environment.Exit(1);
        }

        public static bool PingDB()
        {
            List<string> cmd = db.query("SELECT 1+1;");
            string msgS = "";
            for(int i = 0; i < cmd.ToArray().Length; i++)
            {
                msgS += cmd[i];
                msgS +="\n\n";
            }
            //MessageBox.Show(msgS);
            return cmd[0] == "2";
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
            if(dia.ShowDialog() == true)
            {
                MessageBox.Show(dia.FileName);
                dia.ShowHiddenItems = true;
                dia.Title = "Verzeichnis zum Indizieren wählen";
                return dia.FileName;
            }
            return "";

        }
    }
}
