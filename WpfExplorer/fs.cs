using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WpfExplorer
{
    class fs
    {
        public static string[] readDirSync(string path)
        {
            return Directory.GetFiles(path);
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
    }
}
