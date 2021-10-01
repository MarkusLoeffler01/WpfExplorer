using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfExplorer.Model
{
    public class FileStructure
    {
        public int ID { get; set; }
        public string Path { get; set; }
        public string Filename { get; set; }
        public string Content { get; set; }
    }
}
