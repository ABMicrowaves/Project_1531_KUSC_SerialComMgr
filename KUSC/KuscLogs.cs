using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUSC
{
    class KuscLogs
    {
        static KuscForm _KuscForm;
        public static void LogPrintCommand (string cmd)
        {
            _KuscForm.WriteCmdToLogWindow(cmd);
        }
    }
}
