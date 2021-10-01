using System;
using System.Collections.Generic;
//using Npgsql;
using Newtonsoft.Json;
using System.IO;

using System.Net.NetworkInformation;
using System.Windows;
using MySql.Data.MySqlClient;
using WpfExplorer.ViewModel;

namespace WpfExplorer
{
    class db
    {
        /**Method */
        public static T getConf<T>(string name)
        {
            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            using (StreamReader r = new StreamReader(MainWindow.CONFIG_LOCATIONS + $"{name}.json"))
            {
                string json = r.ReadToEnd();
                try
                {
                    List<T> items = JsonConvert.DeserializeObject<List<T>>(json);
                    return items[0];
                }
                catch (Exception e) { main.ReportError(e); throw; }
            }
        }

        //Speichert eine Datei in Appdata\Roaming\WpfExplorer\. Nimmt den Dateinamen und den Text (in JSON) als Übergabewert
        public static void setConf(string name, object text)
        {
            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            try
            {
                string _ = JsonConvert.SerializeObject(text);
                fs.writeFileSync(MainWindow.CONFIG_LOCATIONS + $"{name}.json", $"[{_}]", true);
                return;
            }
            catch (Exception e) { main.ReportError(e); throw; }
        }

        //Pingt die Datenbank an und gibt den Ping in ms zurück. Falls fehlgeschlagen, gebe -1 zurück
        public static double PingDB()
        {
            DBConf conf = getConf<DBConf>("config");
            Ping p = new Ping();
            try
            {
                PingReply r = p.Send(conf.Host);
                if (r.Status == IPStatus.Success) return r.RoundtripTime;
                return -1;
            }
            catch (Exception e)
            {
                MessageBox.Show("Bei der Verbindung ist ein Fehler aufgetreten, prüfen Sie Ihre Verbindung\n\n" + e);
                Environment.Exit(1);
                throw;
                
            }
        }

        //Bitte in Task.Run ausführen
        public static void pull()
        {
            string _tmp = fs.readFileSync(MainWindow.CONFIG_LOCATIONS+"database.json");
            fs.C_IZ data = db.getConf<fs.C_IZ>("database");

            var dbc = myquery($"SELECT PATH FROM data WHERE ID = '{MainWindowViewModel.AUTH_KEY}'");
            
            for(int i = 0; i < dbc.Count; i++)
            {
                fs.AddToIndex(dbc[i]);
            }
        }


        public static void push()
        {
            fs.C_IZ data = db.getConf<fs.C_IZ>("database");
            var dbc = myquery($"SELECT PATH FROM data WHERE ID = '{MainWindowViewModel.AUTH_KEY}'");

            int totalFiles = data.Paths.Count;


            int cFile = 0;
            //string datar = JsonConvert.SerializeObject(data);
            for(int i = 0; i < data.Paths.Count; i++)
            {
                for(int o = 0; o < data.Paths[i].Files.Count; o++)
                {
                    cFile++;
                    MainWindowViewModel.SetIndexProgress(data.Paths[i].Files[o], cFile, totalFiles);
                    //Display(totalFiles, cFile.ToString(), data.Paths[i].Files[o]);
                }



                
            }
            myquery($"INSERT INTO data (ID, PATH, last_sync, CONTENT) VALUES ('{MainWindowViewModel.AUTH_KEY}', ) WHERE ID = '{MainWindowViewModel.AUTH_KEY}'");

        }

        public static void Display(string tFiles, string cFile, string cFileName)
        {


            //PropertyChanged-Event


        }

        //Create a function which returns a random number

        //baut eine Verbindung zur MariaDB auf und führt eine Query aus, gibt dann das Ergebnis zurück
        public static List<string> myquery(string command)
        {
            DBConf item = getConf<DBConf>("config");
            //MySqlConnection con = new MySqlConnection($"server={item.Host};database={item.Database};userid={item.Username};password={item.Password};");
            //MySqlConnection con = new MySqlConnection($"server=***REMOVED***;database=wpf;userid=wpf;password=***REMOVED***;");
            var con = new MySqlConnection(new MySqlConnectionStringBuilder
            {
                Server = "***REMOVED***",
                UserID = "wpf",
                Password = "***REMOVED***",
                Port = 3306,
                Database = "wpf"
            }.ConnectionString);
            try { con.Open(); }
            catch (Exception e) { main.ReportError(e); throw; }

            var cmd = new MySqlCommand(command, con);
            var reader = cmd.ExecuteReader();
            List<string> res = new List<string> { };
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++) res.Add(reader.GetValue(i).ToString());
            }
            reader.Close();
            return res;
        }


//        [Obsolete("switching to MySQL, please use myquery(command);")]
//        public static List<string> query(string command)
//        {
//#pragma warning disable 0649
//            DBConf item = getConf<DBConf>("config");
//            string conStr = $"Host={item.Host};Username={item.Username};Password={item.Password};Database={item.Database}";
//            //MessageBox.Show(conStr);
//            NpgsqlConnection con = new NpgsqlConnection(conStr);
//            try { con.Open(); }
//            catch (Exception e) { main.ReportError(e); throw; }
//            NpgsqlCommand cmd = new NpgsqlCommand(command, con);
//            //cmd.Prepare();
//            var reader = cmd.ExecuteReader();
//            //string[] x = new string[10000];
//            List<string> result = new List<string>();
//            while (reader.Read())
//            {
//                for (int i = 0; i < reader.FieldCount; i++) result.Add(reader.GetValue(i).ToString());
//            };
//            reader.Close();
//            return result;
//#pragma warning restore 0649


//            //MessageBox.Show(reader.GetString(0));
//        }

        public static void initDB()
        {
            var command = "CREATE TABLE IF NOT EXISTS data (id integer NOT NULL, fileName varchar(255) NOT NULL, fileContent text, PRIMARY KEY(id));";
            myquery(command);
        }

        public class DBConf
        {
            public string Host;
            public string Username;
            public string Password;
            public string Database;
            public int Port;
        }
    }
}
#pragma warning restore 649
