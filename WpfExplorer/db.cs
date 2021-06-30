using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Threading;

using static WpfExplorer.main;
using System.Windows;
using System.Net.NetworkInformation;
using static WpfExplorer.MainWindow;

namespace WpfExplorer
{
    class db
    {
        /**Method */
        public static Item getConf()
        {
            string dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            using (StreamReader r = new StreamReader(dir+"\\..\\..\\..\\config.json"))
            {
                string json = r.ReadToEnd();
                List<Item> items = JsonConvert.DeserializeObject<List<Item>>(json);
                return items[0];
            }
        }
        
        public static double PingDB()
        {
            Item conf = getConf();
            Ping p = new Ping();
            PingReply r = p.Send(conf.Host);
            if(r.Status == IPStatus.Success) return r.RoundtripTime;
            return -1;
            
        }
        
        public static List<string> query(string command)
        {
            Item item = getConf();
            string conStr = $"Host={item.Host};Username={item.Username};Password={item.Password};Database={item.Database}";
            //MessageBox.Show(conStr);
            var con = new NpgsqlConnection(conStr);
            try { con.Open(); }
            catch(Exception e) { ReportError(e); throw; }
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


        public class Item
        {
            public string Host;
            public string Username;
            public string Password;
            public string Database;
            public int Port;
        }
    }
}




