using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plc
{
    class Model
    {
        private static string assCode;
        private static string singCode;
        private static bool b_judge;

        public static string AssCode { get => assCode; set => assCode = value; }
        public static string SingCode { get => singCode; set => singCode = value; }
        public static bool B_judge { get => b_judge; set => b_judge = value; }
    }
}
