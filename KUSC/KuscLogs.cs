using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KUSC
{
    class KuscLogs
    {
        #region Class verbs and c`tor

        RichTextBox _runTimeLoggerTable             = null;
        SaveFileDialog _loggerFileSaver             = null;
        FolderBrowserDialog _fbdLoggerSearcherOpen  = null;
        DataGridView _dgvLogsFilesList              = null;
        DataTable logsFilesTable                    = null;
        RichTextBox _rtbLogViewer                   = null;
        List<string> logsFilesList                  = null;
        #endregion

        public KuscLogs()
        {
            logsFilesTable = new DataTable();
            logsFilesList = new List<string>();
        }
        internal void StoreFormArguments(RichTextBox rtbLogRunWindow, SaveFileDialog sfdLogFileSaver, FolderBrowserDialog fbdLoggerSearcherOpen, DataGridView dgvLogsFilesList)
        {


        }

        internal void StoreFormArguments(RichTextBox rtbLogRunWindow, SaveFileDialog sfdLogFileSaver, FolderBrowserDialog fbdLoggerSearcherOpen, DataGridView dgvLogsFilesList, RichTextBox rtbLogViewer)
        {
            _runTimeLoggerTable = rtbLogRunWindow;
            _loggerFileSaver = sfdLogFileSaver;
            _fbdLoggerSearcherOpen = fbdLoggerSearcherOpen;
            _dgvLogsFilesList = dgvLogsFilesList;
            _rtbLogViewer = rtbLogViewer;

            // update logs table
            logsFilesTable.Columns.Add("File Index", typeof(Int32));
            logsFilesTable.Columns.Add("File name", typeof(string));
            logsFilesTable.Columns.Add("File modification date", typeof(string));
            logsFilesTable.Columns.Add("File size [kBytes]", typeof(string));
            _dgvLogsFilesList.DataSource = logsFilesTable;
        }

        internal void WriteLogMsgOk(string statMessage)
        {
            _runTimeLoggerTable.SelectionColor = System.Drawing.Color.Black;
            _runTimeLoggerTable.AppendText("[K] " + statMessage + System.Environment.NewLine);
        }

        internal void WriteLogMsgFail(string statMessage)
        {
            _runTimeLoggerTable.SelectionColor = System.Drawing.Color.Red;
            _runTimeLoggerTable.AppendText("[F] " + statMessage + System.Environment.NewLine);
        }

        internal void WriteLogMsgSamples(string sampleTitle, string[] sampleResults)
        {

            _runTimeLoggerTable.SelectionColor = System.Drawing.Color.Blue;

            _runTimeLoggerTable.AppendText("[D] " + sampleTitle);
            _runTimeLoggerTable.AppendText(Environment.NewLine);

            _runTimeLoggerTable.SelectionColor = System.Drawing.Color.SlateGray;
            foreach (var result in sampleResults)
            {
                _runTimeLoggerTable.AppendText(String.Format("{0}\t", result));
            }

            _runTimeLoggerTable.AppendText(Environment.NewLine);
        }

        internal void UpdateLogsStoredTable()
        {
            logsFilesList.Clear();
            FileInfo fileSize = null;

            if (_fbdLoggerSearcherOpen.ShowDialog() == DialogResult.OK)
            {
                // Remove past rows and insert new rows data
                logsFilesTable.AcceptChanges();
                foreach (DataRow row in logsFilesTable.Rows)
                {
                    // If this row is offensive then
                    row.Delete();
                }
                logsFilesTable.AcceptChanges();

                string foldername = this._fbdLoggerSearcherOpen.SelectedPath;
                foreach (string f in Directory.GetFiles(foldername))
                {
                    if (f.Contains(KuscCommon.LOG_FILE_FORMAT_NAME))
                    {
                        logsFilesList.Add(f);
                    }
                }

                for (int idx = 0; idx < logsFilesList.Count; idx++)
                {
                    fileSize = new FileInfo(logsFilesList[idx]);

                    logsFilesTable.Rows.Add( idx, 
                                            Path.GetFileName(logsFilesList[idx]),
                                            File.GetLastWriteTime(logsFilesList[idx]),
                                            (fileSize.Length / 1024) + 1);
                }

                // Update logs table
                _dgvLogsFilesList.DataSource = logsFilesTable;
            }

        }

        internal bool LoadLogInLogViewer(int reqLogIndx)
        {
            // Check if this is valid index
            if (reqLogIndx > logsFilesTable.Rows.Count)
            {
                return false;
            }
            else
            {
                string filePath = logsFilesList[reqLogIndx];
                _rtbLogViewer.LoadFile(filePath,
                  RichTextBoxStreamType.PlainText);
            }
            return true;

        }
    }
}
