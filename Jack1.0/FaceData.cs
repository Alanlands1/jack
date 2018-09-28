using Emgu.CV;
using Emgu.CV.Structure;
using System;

namespace Jack1._0
{
    class FaceData
    {
        public string PersonName { get; set; }
        public Image<Gray, byte> FaceImage { get; set; }
        public DateTime CreateDate { get; set; }

    }
}
