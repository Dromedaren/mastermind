using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MastaMind.DataRepresentations {
    public class DataCluePin {
        public int InternalXPosition { get; set; }
        public int XPosition { get; set; }
        public int YPosition { get; set; }
        public bool IsPinSet { get; set; }
        public String PinColor { get; set; }
    }
}
