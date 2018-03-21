using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KUSC
{
    class KuscLogs
    {
        #region Class verbs and c`tor

        RichTextBox _runTimeLoggerTable = null;
        SaveFileDialog _loggerFileSaver = null;
        #endregion

        internal void StoreFormArguments(RichTextBox rtbLogRunWindow, SaveFileDialog sfdLogFileSaver)
        {
            _runTimeLoggerTable = rtbLogRunWindow;
            _loggerFileSaver = sfdLogFileSaver;
        }

        internal void WriteLogMsgOk(string statMessage)
        {
            _runTimeLoggerTable.AppendText("[K] " + statMessage + System.Environment.NewLine);
        }

        internal void WriteLogMsgFail(string statMessage)
        {
            _runTimeLoggerTable.AppendText("[F] " + statMessage + System.Environment.NewLine);
        }
    }
}
