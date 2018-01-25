using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUSC
{
    static class KuscCommon
    {
        #region Technician mode  

        public static string TECH_USER = "AB";
        public static string TECH_PASS = "1234";
        public static string TECH_LOGIN_OK_MSG = "Enter to technician mode";
        public static string TECH_LOGIN_FAIL_MSG = "Incorrect login parameters";
        #endregion

        #region Serial configuration

        public static int SERIAL_BAUD_RATE = 115200; 
        public static int SERIAL_READ_TIMEOUT_MSEC = 500;
        public static int SERIAL_WRITE_TIMEOUT_MSEC = 500;
        public static int RX_BUF_SIZE_BYTES = 20;
        #endregion
    }
}
