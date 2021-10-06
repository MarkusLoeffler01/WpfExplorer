using System;
using System.Collections.Generic;
//using Npgsql;
using Newtonsoft.Json;
using System.IO;

using System.Net.NetworkInformation;
using System.Windows;
using MySql.Data.MySqlClient;
using WpfExplorer.ViewModel;
using System.Collections.ObjectModel;
using Npgsql;

namespace WpfExplorer
{
    class db
    {
        /**Method */
        public static T getConf<T>(string name)
        {
            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string json = "";

            try
            {
                using (StreamReader r = new StreamReader(MainWindow.CONFIG_LOCATIONS + $"{name}.json"))
                {
                    json = r.ReadToEnd(); r.Close();
                }
                return JsonConvert.DeserializeObject<T>(json);

            }
            catch (Exception e) { main.ReportError(e); throw; }
        }

        //Speichert eine Datei in Appdata\Roaming\WpfExplorer\. Nimmt den Dateinamen und den Text (in JSON) als Übergabewert
        public static void setConf(string name, object text)
        {
            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            try
            {
                string _ = JsonConvert.SerializeObject(text);
                fs.writeFileSync(MainWindow.CONFIG_LOCATIONS + $"{name}.json", _, true);
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
                if (conf.Host == null) MessageBox.Show("Die config.json ist leer, es wurde kein Host eingetragen"); 
                else MessageBox.Show("Bei der Verbindung ist ein Fehler aufgetreten, prüfen Sie Ihre Verbindung\n\n" + e);
                Environment.Exit(1);
                throw;
                
            }
        }

        //Bitte in Task.Run ausführen
        //Wenn false, dann pull nicht nötig
        //Wenn true, dann erfolgreich gepullt
        public static bool pull()
        {
            while (main.isIndexerRunning) ;
            fs.C_IZ data = db.getConf<fs.C_IZ>("database");

            if (data.Paths == null) data.Paths = new List<fs.C_Path>();
            if (data.AUTH_KEY == null) data.AUTH_KEY = main.RandomString(64);
            if (data.last_sync == null) data.last_sync = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            fs.writeFileSync(MainWindowViewModel.CONFIG_LOCATIONS + "\\database.json", JsonConvert.SerializeObject(data), true);
            MainWindowViewModel.AUTH_KEY = data.AUTH_KEY;
            var last_sync = myquery($"SELECT last_sync from users WHERE ID = '{MainWindowViewModel.AUTH_KEY}'");
            if (last_sync.Count == 0)
            {
                string dt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                myquery($"INSERT INTO users(ID, last_sync) VALUES('{MainWindowViewModel.AUTH_KEY}', '{dt}')");
                var db = getConf<fs.C_IZ>("database");
                db.last_sync = dt;
                setConf("database", db);
                return false;
            }
            DateTime dbTime = Convert.ToDateTime(last_sync[0]);
            DateTime lcTime = Convert.ToDateTime(data.last_sync);
            //Vergleiche dbs, wenn kleiner gleich 0, dann ist die DB später
            if (DateTime.Compare(dbTime, lcTime)<=0) return false;

            var dbc = myquery($"SELECT PATH FROM data WHERE ID = '{MainWindowViewModel.AUTH_KEY}'");
            
            for(int i = 0; i < dbc.Count; i++) fs.AddToIndex(dbc[i]);
            return true;
        }

        //Bitte in Task.Run ausführen
        //Wenn false, dann push nicht nötig
        //Wenn true, dann erfolgreich gepushed
        public static bool push(MainWindowViewModel window)
        {
            while (main.isIndexerRunning) ;
            if (MainWindowViewModel.AUTH_KEY.Length == 0) return false;
            string dt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            fs.C_IZ data = db.getConf<fs.C_IZ>("database");
            var last_sync = myquery($"SELECT last_sync FROM users WHERE ID = '{MainWindowViewModel.AUTH_KEY}'");
            if (last_sync.Count == 0)
            {
                
                myquery($"INSERT INTO users(ID, last_sync) VALUES('{MainWindowViewModel.AUTH_KEY}', '{dt}')");
                return false;
            }
            DateTime dbTime = Convert.ToDateTime(last_sync[0]);
            DateTime lcTime = Convert.ToDateTime(data.last_sync);
            //Vergleiche dbs, wenn größer gleich 0, dann ist die DB vor
            if (DateTime.Compare(dbTime, lcTime) >= 0) return false;
            int totalFiles = data.Paths.Count;


            int cFile = 0;
            //string datar = JsonConvert.SerializeObject(data);
            myquery($"DELETE FROM data WHERE ID = '{MainWindowViewModel.AUTH_KEY}'");
            for(int i = 0; i < data.Paths.Count; i++)
            {
                for(int o = 0; o < data.Paths[i].Files.Count; o++)
                {
                    cFile++;
                    //4 Backslashes, damit die in der DB nicht verloren gehen
                    data.Paths[i].Files[o].FullPath = data.Paths[i].Files[o].FullPath.Replace("\\", "\\\\");
                    window.SetIndexProgress(data.Paths[i].Files[o], cFile, totalFiles);
                    myquery($"INSERT INTO data (ID, PATH, CONTENT) VALUES ('{MainWindowViewModel.AUTH_KEY}', '{data.Paths[i].Files[o].FullPath}', '{data.Paths[i].Files[o].Content ?? ""}')");
                    //Display(totalFiles, cFile.ToString(), data.Paths[i].Files[o]);
                }
            }

            //Query die abfragt, ob der Pfad existiert
            myquery($"UPDATE users SET last_sync = '{dt}' WHERE ID = '{MainWindowViewModel.AUTH_KEY}'");
            var tmp_db = getConf<fs.C_IZ>("database");
            tmp_db.last_sync = dt;
            setConf("database", tmp_db);
            //myquery($"INSERT INTO data (ID, PATH, CONTENT) VALUES ('{MainWindowViewModel.AUTH_KEY}', ) WHERE ID = '{MainWindowViewModel.AUTH_KEY}'");
            return true;
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
            var con = new MySqlConnection(new MySqlConnectionStringBuilder
            {
                Server = item.Host,
                UserID = item.Username,
                Password = item.Password,
                Port = Convert.ToUInt16(item.Port),
                Database = item.Database
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


        [Obsolete("switching to MySQL, please use myquery(command);")]
        public static List<string> query(string command)
        {
#pragma warning disable 0649
            DBConf item = getConf<DBConf>("config");
            string conStr = $"Host={item.Host};Username={item.Username};Password={item.Password};Database={item.Database}";
            //MessageBox.Show(conStr);
            NpgsqlConnection con = new NpgsqlConnection(conStr);
            try { con.Open(); }
            catch (Exception e) { main.ReportError(e); throw; }
            NpgsqlCommand cmd = new NpgsqlCommand(command, con);
            //cmd.Prepare();
            var reader = cmd.ExecuteReader();
            //string[] x = new string[10000];
            List<string> result = new List<string>();
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++) result.Add(reader.GetValue(i).ToString());
            };
            reader.Close();
            return result;
#pragma warning restore 0649


            //MessageBox.Show(reader.GetString(0));
        }

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

        public class Properties
        {
            public ObservableCollection<string> Paths { get; set; }
            public string AuthKey { get; set; }
            public string LastSync { get; set; }
        }
    }
}
#pragma warning restore 649
