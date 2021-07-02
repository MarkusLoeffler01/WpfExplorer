using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Management;


namespace WpfExplorer
{
    class fs
    {
        public static main.FileStructure[] Files = { };

        public static string[] readDirSync(string path, bool fullpath = false, bool recursive = false, string[] _dirs = null)
        {
            if(!recursive)
            {
                if (fullpath) return Directory.GetFiles(path).Select(p => Path.GetFullPath(p)).ToArray();
                return Directory.GetFiles(path).Select(p => Path.GetFileName(p)).ToArray();
            }
            if (fullpath) return Directory.GetFiles(path, "*", SearchOption.AllDirectories).Select(p => Path.GetFullPath(p)).ToArray();
            return Directory.GetFiles(path, "*", SearchOption.AllDirectories).Select(p => Path.GetFileName(p)).ToArray();


            string[] dirs = new string[] { };
            if (_dirs != null) dirs = _dirs;

            foreach(string d in Directory.GetDirectories(path))
            {
                foreach(string f in Directory.GetFiles(d))
                {
                    dirs.Append(f);
                }
                dirs.Append(string.Join("\n", readDirSync(d, fullpath, true, dirs)));
                //readDirSync(d, fullpath, true, dirs);
            }
            return dirs;
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

        public static void startIndexing()
        {
            CF_Ind conf = db.getConf<CF_Ind>("index");
            
            for (int i = 0; i < conf.Paths.Length; i++)
            {
                string[] FileNames = readDirSync(conf.Paths[i]);
                string[] FullPaths = readDirSync(conf.Paths[i], true);

                Files[i].Filename = FileNames[i];
                Files[i].Path = FullPaths[i];
                main.isIndexerRunning = false;
            }

        }
        public static main.FileStructure searchFile(string File, bool SearchFileContent)
        {
            /** Es wird noch indiziert. Kann nicht gesucht werden*/
            while (!main.isIndexerRunning) { }

            main.FileStructure file = Files.FirstOrDefault(_file => _file.Filename == File);
            return file;
        }
        



        /** Welche Pfade sollen indiziert werden*/
        public class CF_Ind
        {
            public string[] Paths;
        }

        public static int getFileCount(string path)
        {
            return Directory.GetFiles(path).Length;
        }
    }
}
