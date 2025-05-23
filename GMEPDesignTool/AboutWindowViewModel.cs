using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    public class AboutWindowViewModel
    {
        public string Version { get; set; }

        public AboutWindowViewModel()
        {
            using (var fileStream = File.OpenRead("version.txt"))
            {
                using (var reader = new StreamReader(fileStream))
                {
                    string line;
                    if ((line = reader.ReadLine()) != null)
                    {
                        Version = line;
                    }
                }
            }
        }
    }
}
