using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows;


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

        public static void setConf(string name, object text)
        {
            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            try
            {
                JsonConvert.SerializeObject(text);
                string _ = JsonConvert.SerializeObject(text);
                fs.writeFileSync(MainWindow.CONFIG_LOCATIONS + $"{name}.json", $"[{_}]", true);
                return;
            }
            catch (Exception e) { main.ReportError(e); throw; }
        }

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

        public static List<string> myquery(string command)
        {
            DBConf item = getConf<DBConf>("config");
            MySqlConnection con = new MySqlConnection($"server={item.Host};database={item.Database};uid={item.Username};password={item.Password};");
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
            //Array.ForEach(x, System.Diagnostics.Debug.WriteLine);
#pragma warning restore 0649

            //MessageBox.Show(reader.GetString(0));
        }

        public static void initDB()
        {
            var command = "CREATE TABLE IF NOT EXISTS data (id integer NOT NULL, fileName varchar(255) NOT NULL, fileContent text, PRIMARY KEY(id));";
            DBConf item = getConf<DBConf>("config");
            string conStr = $"Host={item.Host};Username={item.Username};Password={item.Password};Database={item.Database}";
            NpgsqlConnection con = new NpgsqlConnection(conStr);
            try { con.Open(); }
            catch (Exception e) { main.ReportError(e); throw; }
            NpgsqlCommand cmd = new NpgsqlCommand(command, con);
            cmd.ExecuteNonQuery();
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




