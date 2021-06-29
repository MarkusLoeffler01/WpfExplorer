using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows;

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

        async public static void connect()
        {
            Item item = getConf();
            string conStr = $"Host={item.Host};Username={item.Username};Password={item.Password};Database={item.Database}";
            MessageBox.Show(conStr);
            var con = new NpgsqlConnection(conStr);
            await con.OpenAsync();
            NpgsqlCommand cmd = new NpgsqlCommand("SHOW DATABASES;", con);
            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) { } 
            MessageBox.Show(reader.GetString(0));
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




