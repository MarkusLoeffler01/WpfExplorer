using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfExplorer.Model
{
        class ScannedFilesList
        {
            public List<ScannedFile> FilesOk;
            public List<ScannedFile> FilesErr;
        }

        public class ScannedFile
        {
            public string FileName;
            public string Path;
        }
}
