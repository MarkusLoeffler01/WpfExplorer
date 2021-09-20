using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfExplorer
{
    class fs
    {


        /** Prüft alle Dateien und erstellt diese, falls nicht vorhanden. Läuft im neuen Thread */
        public static void checkConfig()
        {
            Task.Run(() =>
            {
                string path = MainWindow.CONFIG_LOCATIONS;
                string[] files = { "config", "database", "WhichPaths" };

                Directory.CreateDirectory(path);
                for (int i = 0; i < files.Length; i++) { if (!File.Exists(path + files[i] + ".json")) File.WriteAllText(path + files[i] + ".json", "[{}]"); }
            });
        }

        public static string[] readDirSync(string path, bool fullpath = false, bool recursive = false, string[] _dirs = null)
        {
            if (path == null) return null;
            if (!recursive)
            {
                if (fullpath) return Directory.GetFiles(path).Select(p => Path.GetFullPath(p)).ToArray();
                return Directory.GetFiles(path).Select(p => Path.GetFileName(p)).ToArray();
            }
            if (fullpath) return Directory.GetFiles(path, "*", SearchOption.AllDirectories).Select(p => Path.GetFullPath(p)).ToArray();
            return Directory.GetFiles(path, "*", SearchOption.AllDirectories).Select(p => Path.GetFileName(p)).ToArray();


            //string[] dirs = new string[] { };
            //if (_dirs != null) dirs = _dirs;

            //foreach(string d in Directory.GetDirectories(path))
            //{
            //    foreach(string f in Directory.GetFiles(d))
            //    {
            //        dirs.Append(f);
            //    }
            //    dirs.Append(string.Join("\n", readDirSync(d, fullpath, true, dirs)));
            //    //readDirSync(d, fullpath, true, dirs);
            //}
            //return dirs;
        }

        public static string readFileSync(string path)
        {
            using (StreamReader r = new StreamReader(path, Encoding.UTF8)) return r.ReadToEnd();
        }

        public static void writeFileSync(string path, string context, bool overwrite = false)
        {
            if (!overwrite && File.Exists(path)) return;
            File.WriteAllText(path, context);
        }

        public static bool exists(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }


        /** Other section*/

        /** GIbt -1 zurück, sollte der Dateiname ungültige Zeichen beinhalten*/
        public static string[] getPath(string fileName)
        {
            /**Error handling. Ungültige Zeichen wie ", ' oder ` geben -1 zurück */
            if (fileName.Contains("'") || fileName.Contains('"') || fileName.Contains('`')) { main.ReportError(new Exception("Ungültiger Dateiname")); return new string[] { "-1" }; }
            return db.myquery("SELECT * FROM test WHERE fileName =" + fileName).ToArray();
        }

        public static string[] getContent(string content)
        {
            if (content.Contains("'") || content.Contains('"') || content.Contains('`')) { main.ReportError(new Exception("Ungültiger Dateiname")); return new string[] { "-1" }; }
            return db.myquery("SELECT * FROM test WHERE content =" + content).ToArray();
        }


        /** Liest alle Dateien aus dem Index */
        public static C_IZ readIndexedFiles()
        {
            /** Hole die Config, welche Directories gescannt werden sollten*/
            return db.getConf<C_IZ>("database");

        }
        /** Sucht nach einer Datei nach ihrem Namen und gibt die Datei mit dem Pfad zurück */
        public static List<main.FileStructure> searchFile(string Filename, bool SearchFileContent)
        {
            /** Es wird noch indiziert. Kann nicht gesucht werden*/
            int ___ = 0;
            while (main.isIndexerRunning) { System.Diagnostics.Debug.WriteLine("Still running" + ___); ___++; }
            C_IZ conf = db.getConf<C_IZ>("database");
            List<main.FileStructure> FoundFiles = new List<main.FileStructure>();
            for (int i = 0; i < conf.Paths.Count; i++)
            {
                for (int o = 0; o < conf.Paths[i].Files.Count; o++)
                {
                    if (conf.Paths[i].Files[o].Contains(Filename)) FoundFiles.Add(new main.FileStructure() { Filename = conf.Paths[i].Files[o], Path = conf.Paths[i].Path });
                }
            }
            return FoundFiles;
        }
        /** Holt sich die IndexDatei und fügt einen Eintrag hinzu */
        public static int AddToIndex(string _file)
        {
            try
            {
                string path = Path.GetDirectoryName(_file);
                string file = Path.GetFileName(_file);

                //Hole die DB-Datei
                C_IZ data = db.getConf<C_IZ>("database");
                //object db = JsonConvert.DeserializeObject<object>(jsonstring);

                //Ist Paths[i].Path schon vorhanden?
                bool found = false;
                for (int i = 0; i < data.Paths.Count; i++)
                {
                    if (data.Paths[i].Path == path)
                    {
                        //JA, füge Eintrag zu Paths[i].Files hinzu, wenn diese nicht schon vorhanden ist
                        if (!data.Paths[i].Files.Contains(file)) { data.Paths[i].Files.Add(file); } else { return -1; }
                        found = true;
                        break;
                    }
                }
                //NEIN, lege einen neuen Path Eintrag an und füge die File hinzu
                if (!found) { data.Paths.Add(new C_Path { Path = path, Files = new List<string> { file } }); }

                //Speicher die DB-Datei
                db.setConf("database", data);
                //fs.writeFileSync(MainWindow.CONFIG_LOCATIONS + "database.json", JsonConvert.SerializeObject(data), true);
                MainWindow.AddToGrid(file, path);
                return 0;
            }
            catch (Exception e)
            {
                main.ReportError(e); return -255;
            }
        }

        public static void RemoveFromIndex(string _file)
        {
            try
            {
                string path = Path.GetDirectoryName(_file);
                string file = Path.GetFileName(_file);

                //Hole die DB-Datei
                C_IZ data = db.getConf<C_IZ>("database");
                //object db = JsonConvert.DeserializeObject<object>(jsonstring);

                //Gibt es den Pfad zur Datei?
                bool found = false;
                for (int i = 0; i < data.Paths.Count; i++)
                {
                    if (data.Paths[i].Path == path)
                    {
                        //JA, füge Eintrag zu Paths[i].Files hinzu
                        if (data.Paths[i].Files.Contains(file))
                        {
                            data.Paths[i].Files.Remove(file);
                            found = true;
                            break;
                        }
                        //Der Directory ist nicht indiziert
                        else
                        {
                            MessageBox.Show($"Die Datei ${_file} in {path} konnte nicht entfernt werden, da das Verzeichnis nicht indiziert wurde");
                            return;
                        }
                    }
                }
                //NEIN, der Directory ist nicht im Index und die Datei wurde nicht gefunden
                if (!found) { MessageBox.Show($"Die Datei ${_file} in {path} konnte nicht entfernt werden, da diese nicht indiziert wurde"); }

                //Speicher die DB-Datei
                db.setConf("database", data);
                //fs.writeFileSync(MainWindow.CONFIG_LOCATIONS + "database.json", JsonConvert.SerializeObject(data), true);
                MainWindow.AddToGrid(file, path);
                return;
            }
            catch (Exception e)
            {
                main.ReportError(e);
            }
        }

        interface index
        {
            string path { get; set; }
        }

        public class i
        {
            public i(string path)
            {
                this.path = path;
            }
            public string path;
        }




        /** Welche Pfade sollen indiziert werden*/
        //public class CF_Ind
        //{
        //    public object[] Paths = {
        //        string FileName,
        //        string Path
        //    };
        //}


        public partial class C_IZ
        {
            [JsonProperty("Paths")]
            public List<C_Path> Paths { get; set; }

            [JsonProperty("AUTH_KEY")]
            public string AUTH_KEY {get; set;}
        }

        public partial class C_Path
        {
            [JsonProperty("Path")]
            public string Path { get; set; }

            [JsonProperty("Files")]
            public List<string> Files { get; set; }
        }


        public partial class C_Which
        {
            [JsonProperty("Paths")]
            public List<string> Paths { get; set; }
        }


        public static int getFileCount(string path)
        {
            return Directory.GetFiles(path).Length;
        }
    }
}
