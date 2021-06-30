using System;
using System.Collections.Generic;
using Npgsql;
using Newtonsoft.Json;
using System.IO;

using System.Net.NetworkInformation;

namespace WpfExplorer
{
    class db
    {
        /**Method */
        public static T getConf<T>(string name)
        {
            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            using (StreamReader r = new StreamReader(dir+$"\\..\\..\\..\\{name}.json"))
            {
                string json = r.ReadToEnd();
                try
                {
                    List<T> items = JsonConvert.DeserializeObject<List<T>>(json);
                    return items[0];
                }
                catch(Exception e) { main.ReportError(e); throw; }
            }
        }
        
        public static double PingDB()
        {
            DBConf conf = getConf<DBConf>("config");
            Ping p = new Ping();
            PingReply r = p.Send(conf.Host);
            if(r.Status == IPStatus.Success) return r.RoundtripTime;
            return -1;
            
        }
        
        public static List<string> query(string command)
        {
            DBConf item = getConf<DBConf>("config");
            string conStr = $"Host={item.Host};Username={item.Username};Password={item.Password};Database={item.Database}";
            //MessageBox.Show(conStr);
            NpgsqlConnection con = new NpgsqlConnection(conStr);
            try { con.Open(); }
            catch(Exception e) { main.ReportError(e); throw; }
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
            //Array.ForEach(x, Console.WriteLine);


            //MessageBox.Show(reader.GetString(0));
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




