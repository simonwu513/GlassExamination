using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlassExamination
{
    public partial class GlassMarker
    {
        public int ImageId { get; set; }

        public string MarkedPoints { get; set; }

        public string ThisUser { get; set; }

        public string TxtTimeStamp { get; set; }

        public string ImagePath {  get; set; }
    }

    public partial class GlassMarkerSelectedItem
    {
        public int ImageId { get; set; }

        public string ImagePath {  set; get; }

        public string FileName { get; set;}

        public string MarkedPoints { get; set; }
     
    }
}
