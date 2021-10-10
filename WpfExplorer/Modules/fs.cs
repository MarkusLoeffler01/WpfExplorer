using Newtonsoft.Json;
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using iTextSharp;
using Visualis.Extractor;
using iTextSharp.text;
using System.Text.RegularExpressions;
using WpfExplorer.ViewModel;
using WpfExplorer.Modules;
using System.Diagnostics;
using WpfExplorer.View;

namespace WpfExplorer
{
    public class fs
    {


        /** Prüft alle Dateien und erstellt diese, falls nicht vorhanden. Läuft im neuen Thread */
        public static void checkConfig()
        {
            string path = MainWindowViewModel.CONFIG_LOCATIONS;
            string[] files = { "config", "database", "usersettings" };

            Directory.CreateDirectory(path);
            for (int i = 0; i < files.Length; i++) { if (!File.Exists(path + files[i] + ".json")) File.WriteAllText(path + files[i] + ".json", "{}"); }

            fs.C_IZ data = db.getConf<fs.C_IZ>("database");
            if (data.AUTH_KEY == null || data.AUTH_KEY.Length == 0) { data.AUTH_KEY = main.RandomString(64); }
            MainWindowViewModel.AUTH_KEY = data.AUTH_KEY;
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
        }

        public static string readFileSync(string path)
        {
            using (StreamReader r = new StreamReader(path, Encoding.UTF8)) try { return r.ReadToEnd(); } catch(Exception e) {  MessageBox.Show("Datenbank konnte nicht gefunden werden"); return null; }
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


        public static void import(string path)
        {
            if (!exists(path)) return;

            try
            {
                string file = fs.readFileSync(path);
                string content = file;





                //Check if File is encrypted
                if (!path.EndsWith(".wpfex") && !path.EndsWith(".enc.wpfex")) { MessageBox.Show("Die Datei ist keine Datenbank", "", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                if (!file.StartsWith("{"))
                {
                    DialogWindow dialogWindow = new DialogWindow();
                    dialogWindow.Title = "Entschlüsseln der Datenbank";
                    dialogWindow.txt_Info.Text = "Die Datei ist geschützt und benötigt ein Passwort";
                    dialogWindow.ShowDialog();
                    if (exportPassword == null) MessageBox.Show("Fehler");
                    content = StringCipher.Decrypt(content, exportPassword);
                }
                
                C_IZ data = JsonConvert.DeserializeObject<C_IZ>(content);
                db.setConf("database", data);
                MessageBox.Show("Datei erfolgreich importiert");
            }
            catch (Exception e) { 
                
                if(e.GetType().ToString() == "")
                
                MessageBox.Show($"Konnte Datei '{path}' nicht importieren: \n\n{e.Message}"); return; }
        }


        public static string exportPassword;
        public static void export()
        {
            try
            {
                bool enc = false;
                string path = null;
                var res = MessageBox.Show("Möchten Sie Ihre Datenbank verschlüsseln?", "Datenbank-Verschlüsselung", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if(res == MessageBoxResult.Cancel) { MessageBox.Show("Import abgebrochen", "Datenbank-Verschlüsselung", MessageBoxButton.OK, MessageBoxImage.Exclamation); return; }
                if ( res == MessageBoxResult.Yes)
                {
                    path = main.getSaveDialog(null, true);
                    if(path == null) { MessageBox.Show("Import abgebrochen", "Datenbank-Verschlüsselung", MessageBoxButton.OK, MessageBoxImage.Exclamation); return; }
                    DialogWindow dialogWindow = new DialogWindow();
                    dialogWindow.Title = "Verschlüsseln der Datenbank";
                    dialogWindow.ShowDialog();
                    if (exportPassword == null) { MessageBox.Show("Fehler"); return; }
                    fs.writeFileSync(path, StringCipher.Encrypt(JsonConvert.SerializeObject(db.getConf<C_IZ>("database")), exportPassword));

                }
                else
                {
                    path = main.getSaveDialog(null, false);
                    if (path == null) { MessageBox.Show("Import abgebrochen", "Datenbank-Verschlüsselung", MessageBoxButton.OK, MessageBoxImage.Exclamation); return; }
                    fs.writeFileSync(path, JsonConvert.SerializeObject(db.getConf<C_IZ>("database")));
                }

                if (MessageBox.Show($"Datenbank erfolgreich unter {path} exportiert. Möchten Sie den Pfad öffnen?", "Datenbank-Verschlüsselung", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
                Process.Start(path);

            }
            catch(Exception e) { throw e; }
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
        public static List<Model.FileStructure> searchFile(string Filename, bool SearchFileContent)
        {
            /** Es wird noch indiziert. Kann nicht gesucht werden*/
            int ___ = 0;
            while (main.isIndexerRunning) { System.Diagnostics.Debug.WriteLine("Still running" + ___); ___++; }
            C_IZ conf = db.getConf<C_IZ>("database");
            List<Model.FileStructure> FoundFiles = new List<Model.FileStructure>();

            Regex.IsMatch(Filename, WildCardToRegular(Filename));

            for (int i = 0; i < conf.Paths.Count; i++)
            {
                List<C_File> _ = new List<C_File>();
                if (Filename.Contains("*")) _ = conf.Paths[i].Files.Where(p => Regex.IsMatch(p.Name, WildCardToRegular(Filename))).ToList<C_File>();
                else _ = conf.Paths[i].Files.Where(p => p.Name.Contains(Filename)).ToList();
                for(int j = 0; j < _.Count(); j++)
                {
                    FoundFiles.Add(new Model.FileStructure() { Filename = _[j].Name, Path = _[j].FullPath, Size = _[j].Size });
                }
            }
            return FoundFiles;
        }
        /** Holt sich die IndexDatei und fügt einen Eintrag hinzu */
        public static int AddToIndex(string _file)
        {
            try
            {
                main.isIndexerRunning = true;
                string path = Path.GetDirectoryName(_file);
                C_File file = getFileInfo(_file);

                //Hole die DB-Datei
                C_IZ data = db.getConf<C_IZ>("database");

                //Ist Paths[i].Path schon vorhanden?
                bool found = false;
                for (int i = 0; i < data.Paths.Count; i++)
                {
                    if (data.Paths[i].Path == path)
                    {
                        //JA, füge Eintrag zu Paths[i].Files hinzu, wenn diese nicht schon vorhanden ist
                        if (data.Paths[i].Files.Where(p => p.FullPath == file.FullPath).Count() == 0) data.Paths[i].Files.Add(file);
                        else { main.isIndexerRunning = false; return -1; }
                        if (!data.Paths[i].Files.Contains(file)) { data.Paths[i].Files.Add(file); } else { main.isIndexerRunning = false; return -1; }
                        found = true;
                        break;
                    }
                }
                //NEIN, lege einen neuen Path Eintrag an und füge die File hinzu
                if (!found) { data.Paths.Add(new C_Path { Path = path, Files = new List<C_File> { file } }); }

                //Speicher die DB-Datei
                db.setConf("database", data);
                main.AddToGrid(file, path);
                main.isIndexerRunning = false;
                return 0;
            }
            catch (Exception e)
            {
                main.ReportError(e); return -255;
            }
        }

        public static List<string> validFileTypes = new List<string> {
            ".txt",
            ".pdf",
            ".doc",
            ".docx"
        };

        public static string WildCardToRegular(string value)
        {
            return "^" + Regex.Escape(value).Replace("\\*", ".*") + "$";
        }

        public static void RemoveFromIndex(string _file)
        {
            try
            {
                main.isIndexerRunning = true;
                string path = Path.GetDirectoryName(_file);
                var file = getFileInfo(_file);

                //Hole die DB-Datei
                C_IZ data = db.getConf<C_IZ>("database");

                //Gibt es den Pfad zur Datei?
                bool found = false;
                for (int i = 0; i < data.Paths.Count; i++)
                {
                    if (data.Paths[i].Path == path)
                    {
                        //JA, füge Eintrag zu Paths[i].Files hinzu
                        var f = data.Paths[i].Files.Where(p => p.Name == file.Name);
                        if (f.Count() == 1)
                        {
                            data.Paths[i].Files.Remove(f.First());
                            found = true;
                            break;
                        }
                        //Der Directory ist nicht indiziert
                        else
                        {
                            MessageBox.Show($"Die Datei ${_file} in {path} konnte nicht entfernt werden, da das Verzeichnis nicht indiziert wurde");
                            main.isIndexerRunning = false;
                            return;
                        }
                    }
                }
                //NEIN, der Directory ist nicht im Index und die Datei wurde nicht gefunden
                if (!found) { MessageBox.Show($"Die Datei ${_file} in {path} konnte nicht entfernt werden, da diese nicht indiziert wurde"); }

                //Speicher die DB-Datei
                db.setConf("database", data);
                main.AddToGrid(file, path);
                main.isIndexerRunning = false;
                return;
            }
            catch (Exception e)
            {
                main.ReportError(e);
            }
        }

        public static string ExtractText(C_File file)
        {
            FileAttributes attr = File.GetAttributes(file.FullPath);

            if (attr.HasFlag(FileAttributes.Directory)) return "";

            TextExtractorD extractor = new TextExtractorD();
            FileInfo sd = new FileInfo(file.FullPath);
            return extractor.Extract(sd.FullName);
        }

        public static C_File getFileInfo(string _f)
        {
            C_File file = new C_File();
            file.Name = Path.GetFileName(_f);
            file.FullPath = Path.GetFullPath(_f);
            file.Content = null;
            file.Size = Convert.ToUInt64(new FileInfo(file.FullPath).Length);
            if (fs.validFileTypes.Contains(Path.GetExtension(file.Name).ToLower()))  file.Content = ExtractText(file);
            return file;
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


        public class C_UC
        {
            [JsonProperty("DarkMode")]
            public bool DarkMode { get; set; }

            [JsonProperty("Recursive")]
            public bool Recursive { get; set; }
        };
        public class C_IZ
        {
            [JsonProperty("Paths")]
            public List<C_Path> Paths { get; set; }

            [JsonProperty("AUTH_KEY")]
            public string AUTH_KEY {get; set;}

            [JsonProperty("last_sync")]
            public string last_sync {get; set;}
        }

        public class C_Path
        {
            [JsonProperty("Path")]
            public string Path { get; set; }

            [JsonProperty("Files")]
            public List<C_File> Files { get; set; }
        }

        public class C_File
        {
            public string Name { get; set; }

            public string FullPath { get; set; }

            //<summary>
            //Die Länge der Datei in Bytes
            //</summary>
            public ulong Size { get; set; }
            public string Date { get; set; }
            public string Content { get; set; }
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
