using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfExplorer
{
    public class main
    {
        public static void ReportError(Exception e)
        {
            if(e.GetType().ToString() == "Npgsql.NpgsqlException")
            {
                string msg = "Es wurde ein Fehler festgestellt. Stellen Sie sicher, dass sie mit dem Internet verbunden sind. Bei weiteren Fragen wenden Sie sich an ihren System Administrator\n\n\nFehlertext: " + e.Message;
                MessageBox.Show(msg);
                System.Environment.Exit(1);
            }
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
            MessageBox.Show(msgS);
            if (cmd[0] == "2") return true;
            return false;
        }
    }
}
