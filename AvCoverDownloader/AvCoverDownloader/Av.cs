using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvCoverDownloader
{
    class Av
    {
        public string Id { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string Html { get; set; }

        public bool CD1 { get; set; }

        public List<String> stars = new List<string>();
        public List<String> tags = new List<string>();

        public string date { get; set; }
        public string title { get; set; }
    }
}
