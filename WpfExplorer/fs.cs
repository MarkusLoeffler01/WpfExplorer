
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Management;
using Newtonsoft.Json;
using System.Windows;

namespace WpfExplorer
{
    class fs
    {


        /** Prüft alle Dateien und erstellt diese, falls nicht vorhanden. Läuft im neuen Thread */
        public static void checkFiles()
        {
            Task.Run(() =>
            {
                string path = MainWindow.CONFIG_LOCATIONS;

                Directory.CreateDirectory(path);
                if (!File.Exists(path + "index.json")) File.WriteAllText(path + "index.json", "[{}]");
                if (!File.Exists(path + "config.json")) File.WriteAllText(path + "config.json", "[{}]");

            });
        }

        public static string[] readDirSync(string path, bool fullpath = false, bool recursive = false, string[] _dirs = null)
        {
            if (path == null) return null;
            if(!recursive)
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
            return db.query("SELECT * FROM test WHERE fileName ="+fileName).ToArray();
        }

        public static string[] getContent(string content)
        {
            if (content.Contains("'") || content.Contains('"') || content.Contains('`')) { main.ReportError(new Exception("Ungültiger Dateiname")); return new string[] { "-1" }; }
            return db.query("SELECT * FROM test WHERE content =" + content).ToArray();
        }


        /** Liest alle Dateien aus dem Index */
        public static List<main.FileStructure> readIndexedFiles()
        {
            /** Hole die Config, welche Directories gescannt werden sollten*/
            CF_Ind conf = db.getConf<CF_Ind>("WhichPaths");
            
            List<main.FileStructure> Files = new List<main.FileStructure>();
            for (int i = 0; i < conf.Paths.Length; i++)
            {
                string[] FileNames = readDirSync(conf.Paths[i].Path);
                string[] FullPaths = readDirSync(conf.Paths[i].Path, true); 
                for(int o = 0; o < FileNames.Length; o++)
                {
                    /** Entferne die Dateinamen aus dem Path */
                        FullPaths[o] = Path.GetDirectoryName(FullPaths[o]);
                        Files.Add(new main.FileStructure() {Filename = FileNames[o], Path = FullPaths[o]});
                }
            }
            main.isIndexerRunning = false;
            return Files;

        }
        /** Sucht nach einer Datei nach ihrem Namen und gibt die Datei zurück */
        public static main.FileStructure searchFile(string Filename, bool SearchFileContent)
        {
            /** Es wird noch indiziert. Kann nicht gesucht werden*/
            while (!main.isIndexerRunning) { }
            var Files = readIndexedFiles();
            main.FileStructure file = Files.FirstOrDefault(_file => _file.Filename == Filename);
            return file;
        }
        /** Holt sich die IndexDatei und fügt einen Eintrag hinzu */
        public static void AddToIndex(string path)
        {
            
            try
            {
                List<i> obj = JsonConvert.DeserializeObject<List<i>>(fs.readFileSync(path));
                //TODO: Mach eine neue Variable mit dem Interface von index und setze das Attribut Path auf path
                i d = new i(path);
                obj.Add(d);
                fs.writeFileSync(path, JsonConvert.SerializeObject(obj));
                string[] filename = path.Split('\\');
                MainWindow.AddToGrid(filename[filename.Length-1], path);

            }
            catch(Exception e)
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

        public partial class CF_Ind
        {
            [JsonProperty("Paths")]
            public C_Path[] Paths { get; set; }
        }

        public partial class C_Path
        {
            [JsonProperty("FileName")]
            public string FileName { get; set; }

            [JsonProperty("Path")]
            public string Path { get; set; }
        }

        public static int getFileCount(string path)
        {
            return Directory.GetFiles(path).Length;
        }
    }
}
