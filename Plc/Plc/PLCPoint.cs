using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plc
{
    class PLCPoint
    {
        private string addressType;
        private int address;
        private string type;
        private int length;

       
        public int Address { get => address; set => address = value; }
        public string Type { get => type; set => type = value; }
        public int Length { get => length; set => length = value; }
        public string AddressType { get => addressType; set => addressType = value; }
    }
}
