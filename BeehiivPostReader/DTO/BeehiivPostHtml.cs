using System;
using System.Collections.Generic;
using System.Text;

namespace BeehiivPostReader.DTO
{
    public class BeehiivPostHtml
    {
        public BeehiivPostParentElement Document { get; internal set; }
        public IEnumerable<string> ImageDownloadUrls { get; internal set; }
    }
}
