using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KUSC
{
    public partial class KuscForm : Form
    {
        #region Class c`tor and verbs

        KuscSerial _kuscSerial;
        KuscUtil _KuscUtil;
        KuscSynth _kuscSynth;
        KuscExtDac _kuscExtDac;
        KuscLogs _kuscLogs;

        // Synthesizers:
        List<string> dataList;
        private short _synthUpdateCnt = 0;

        // Adc samples
        List<RichTextBox> _adcTable;

        public KuscForm()
        {
            InitializeComponent();
            _kuscSerial = new KuscSerial();
            _KuscUtil = new KuscUtil();
            _kuscSynth = new KuscSynth();
            _kuscExtDac = new KuscExtDac();
            _kuscLogs = new KuscLogs();

            // Set Synthesizers init values
            dataList = new List<string>();
            cbxSynthTxSetCp.Text = "Choose";
            cbxSynthRxSetCp.Text = "Choose";

            // Set DAC init value
            rdbDacA.Checked = true;

            // Set UI Vref
            lblAdcVreTechUi.Text = string.Format("Vref= {0} [mVdc]", KuscCommon.DAC_VSOURCEPLUS_MILI.ToString());
            lblAdcVreUserUi.Text = string.Format("Vref= {0} [mVdc]", KuscCommon.DAC_VSOURCEPLUS_MILI.ToString());
            _KuscUtil.UpdateStatusObject(this);
            _kuscLogs.StoreFormArguments(rtbLogRunWindow, sfdLogFileSaver, fbdLoggerSearcherOpen, dgvLogsFilesList, rtbLogViewer);

            // Set ADC table
            _adcTable = new List<RichTextBox>();
            _adcTable.Add(rtbAdc_REV_PWR);
            _adcTable.Add(rtbAdc_FWD_PWR2);
            _adcTable.Add(rtbAdc_FWD_PWR1);
            _adcTable.Add(rtbAdc_FWD_IN_POWER);
            _adcTable.Add(rtbAdc_P7_SENSE);
            _adcTable.Add(rtbAdc_28V_SENSE);
            _adcTable.Add(rtbAdc_UP_TMP_SNS);
            _adcTable.Add(rtbAdc_DOWN_TMP_SNS);
            _adcTable.Add(rtbAdc_PA_TMP);
            _adcTable.Add(rtbAdc_LD_SYNTH_RX);
            _adcTable.Add(rtbAdc_LD_SYNTH_TX);
            _adcTable.Add(rtbAdc_Flash_Row_data);

        }
        #endregion

        #region Technician mode

        private void btnTechLogin_Click(object sender, EventArgs e)
        {
            if ((tbxTechUser.Text == KuscCommon.TECH_USER) && (tbxTechPass.Text == KuscCommon.TECH_PASS))
            {
                WriteStatusOk(KuscCommon.TECH_LOGIN_OK_MSG);
                gbxTechMode.Visible = true;
            }
            else
            {
                WriteStatusFail(KuscCommon.TECH_LOGIN_FAIL_MSG);
            }

        }
        #endregion

        #region Serial port

        #region Serial port init

        private void btnCheckLocalPortsNames_Click(object sender, EventArgs e)
        {

            cbxLocalPortsNames.Items.Clear();
            var selectedPorts = _kuscSerial.GetComPorts();
            if(selectedPorts.Count > 0)
            {
                foreach (var port in selectedPorts)
                {
                    cbxLocalPortsNames.Items.Add(port);
                }
                cbxLocalPortsNames.SelectedText = selectedPorts[0];
                cbxLocalPortsNames.SelectedItem = selectedPorts[0];
            }
            else
            {
                WriteStatusFail(KuscCommon.MSG_SERIAL_ERR_DONT_FOUND_ANY_COMPORT); 

            }
        }

        private void btninitCom(object sender, EventArgs e)
        {
            string err = string.Empty;
            if (cbxLocalPortsNames.SelectedItem == null)
            {
                WriteStatusFail("Please select port of COM");

            }
            else
            {
                err = _kuscSerial.InitSerialPort(cbxLocalPortsNames.SelectedItem.ToString());
                if (err != string.Empty)
                {
                    WriteStatusFail(err);
                }
                else
                {
                    WriteStatusOk("Comport open ok");
                }
            }
        }

        #endregion

        private void btnUartTestSend_Click(object sender, EventArgs e)
        {
            char c = Convert.ToChar(0xA1);
            _kuscSerial.SerialWriteChar(c);
        }

        #endregion

        #region Application logs

        #region Application interface

        public void WriteStatusOk(string statMessage)
        {
            _kuscLogs.WriteLogMsgOk(statMessage);
            lblStatus.ForeColor = Color.Black;
            lblStatus.Text = statMessage;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
        }

        public void WriteStatusFail(string statMessage)
        {
            _kuscLogs.WriteLogMsgFail(statMessage);
            lblStatus.ForeColor = Color.Red;
            lblStatus.Text = statMessage;
        }

        public void WriteCmdToLogWindow(string cmd)
        {
            rtbLogRunWindow.Text = cmd;
        }

        #endregion

        #endregion

        #region Application controller

        #region ADC component

        public void UpdateAdcTable(string dataSample)
        {
            var samples = dataSample.Split(',');
            for (int i = 0; i < samples.Length - 1; i+=2)
            {
                if(samples[i + 1] == string.Empty || samples[i] == string.Empty)
                {
                    continue;
                }
                int highRes = Convert.ToChar(samples[i + 1]) << 8;
                int lowRes = Convert.ToChar(samples[i]);
                int res = highRes + lowRes;
                UInt16 sampleData = (UInt16)(res & 0x0FFF);
                UInt16 channel = (UInt16)(res >> 12);

                // update channels tables:
                UpdateAdcChannelTable(sampleData, channel);

                // update row data:
                rtbAdc_Flash_Row_data.Text += res;
            }
        }

        private void UpdateAdcChannelTable(UInt16 dataSample, UInt16 channel)
        {
            switch(channel)
            {
                case 0x1:
                    rtbAdc_REV_PWR.AppendText(dataSample.ToString() + Environment.NewLine);
                    break;

                case 0x2:
                    rtbAdc_FWD_PWR2.AppendText(dataSample.ToString() + Environment.NewLine);
                    break;

                case 0x3:
                    rtbAdc_FWD_PWR1.AppendText(dataSample.ToString() + Environment.NewLine);
                    break;

                case 0x4:
                    rtbAdc_FWD_IN_POWER.AppendText(dataSample.ToString() + Environment.NewLine);
                    break;

                case 0x5:
                    rtbAdc_P7_SENSE.AppendText(dataSample.ToString() + Environment.NewLine);
                    break;

                case 0x6:
                    rtbAdc_28V_SENSE.AppendText(dataSample.ToString() + Environment.NewLine);
                    break;

                case 0x7:
                    rtbAdc_UP_TMP_SNS.AppendText(dataSample.ToString() + Environment.NewLine);
                    break;

                case 0x8:
                    rtbAdc_DOWN_TMP_SNS.AppendText(dataSample.ToString() + Environment.NewLine);
                    break;

                case 0x9:
                    rtbAdc_PA_TMP.AppendText(dataSample.ToString() + Environment.NewLine);
                    break;

                case 0xA:
                    rtbAdc_LD_SYNTH_RX.AppendText(_KuscUtil.WriteLockStateFromGivenData(dataSample));
                    break;

                case 0xB:
                    rtbAdc_LD_SYNTH_TX.AppendText(_KuscUtil.WriteLockStateFromGivenData(dataSample));
                    break;

            }
        }

        private void btnClearAdcTable_Click(object sender, EventArgs e)
        {
            foreach (var table in _adcTable)
            {
                table.Clear();
            }
            
        }

        private void btnSaveSamplesToFile_Click(object sender, EventArgs e)
        {
            sfdSaveSamplesFile.Filter = "Text file|*.txt";
            sfdSaveSamplesFile.Title = "Save an Samples File";
            string sampleStr = string.Empty;

            if (sfdSaveSamplesFile.ShowDialog() == DialogResult.OK)
            {

                StreamWriter writer = new StreamWriter(sfdSaveSamplesFile.OpenFile());

                foreach (var table in _adcTable)
                {
                    sampleStr = "Channel name: " + table.Name.Substring(7);
                    writer.WriteLine(sampleStr);
                    var lines = table.Lines;
                    foreach (var line in lines)
                    {
                        writer.Write(line + ",");
                    }
                    writer.WriteLine(Environment.NewLine + "*********************************************************");
                }

                //string text = textRange.Text;
                writer.Dispose();
                writer.Close();
            }
        }

        private void btnAdcUpdateLogger_Click(object sender, EventArgs e)
        {
            string sampleTitle = string.Empty;
            rtbLogRunWindow.AppendText("********************************************" + Environment.NewLine);
            foreach (var table in _adcTable)
            {
                sampleTitle = "Channel name: " + table.Name.Substring(7);
                var SampleResults = table.Lines;
                _kuscLogs.WriteLogMsgSamples(sampleTitle, SampleResults);
                
            }
            rtbLogRunWindow.AppendText("********************************************" + Environment.NewLine);
        }

        #endregion

        #region System common

        public void UpdateMcuFw(string data)
        {
            var chars = data.Replace("\x2C", string.Empty).ToCharArray();

            // Date parameters
            string compileDateYear = (chars[0] << 8 | chars[1]).ToString();
            string compileDateMonth = string.Format("{0}{1}{2}", chars[2], chars[3], chars[4]).ToUpper();
            string compileDateDay = (chars[5] << 8 | chars[6]).ToString();

            // Time parameters
            string compileTimeHour = (chars[7] << 8 | chars[8]).ToString();
            string compileTimeMin = (chars[9] << 8 | chars[10]).ToString();
            string compileTimeSec = (chars[11] << 8 | chars[12]).ToString();

            tbxMcuFwVerDate.Text = string.Format("{0}/{1}/{2}", compileDateDay, compileDateMonth, compileDateYear);
            tbxMcuFwVerTimeOfDay.Text = string.Format("{0}:{1}:{2}", compileTimeHour, compileTimeMin, compileTimeSec);
        }

        public void UpdateSystemRunTime(string data)
        {
            var chars = data.Replace("\x2C", string.Empty).ToCharArray();
            chars[chars.Length - 1] = '\0';

            Int32 regsiterVal = 0x0;

            for (int byteIdx = 0; byteIdx < 3; byteIdx++)
            {
                regsiterVal |= chars[byteIdx] * (Int16)Math.Pow(2, 8 * byteIdx);
            }
            tbxSysRunTime.Text = TimeSpan.FromSeconds(regsiterVal).ToString();

        }

        private void btnRunTimeLoggerClear_Click(object sender, EventArgs e)
        {
            rtbLogRunWindow.Clear();
        }
        #endregion

        #region FLASH

        public void UpdateFlashCondition(string dataSample)
        {
            var samples = dataSample.Split(',');
            var FlashSize = (Convert.ToInt16(Convert.ToChar(samples[0]) << 8) + Convert.ToInt16(Convert.ToChar(samples[1])));
            var writeAddress = (Convert.ToInt16(Convert.ToChar(samples[2]) << 8) + Convert.ToInt16(Convert.ToChar(samples[3])));
            var precentage = Convert.ToInt16(((double)(FlashSize - writeAddress) / FlashSize) * 100);
            var numPackets = (FlashSize - writeAddress) / 32;

            // Update checkbox fields

            tbxFlashCondTotal.Text = FlashSize.ToString();
            tbxFlashCondFree.Text = writeAddress.ToString();
            tbxFlashCondPrecentage.Text = precentage.ToString();
            tbxFlashCondNumPackets.Text = numPackets.ToString();

            // Update run time logger
            _kuscLogs.WriteLogMsgOk("Flash size [kBytes] \t Write Address \t Free precentage [%] \t Available packets");
            _kuscLogs.WriteLogMsgOk(string.Format("{0} \t\t {1} \t\t {2} \t\t\t {3} \t ", FlashSize, writeAddress, precentage, numPackets));
        }
        #endregion

        #region DAC

        internal void DacReadData(string data)
        {
            Int32 dacIndex = 0x0;
            double AnalogVal = 0x0;
            _kuscExtDac.GetDacValueFromInputStream(data, ref dacIndex, ref AnalogVal);
            switch (dacIndex)
            {
                case 0:
                    tbxDacReadValA.Text = AnalogVal.ToString("#.00");
                    break;

                case 1:
                    tbxDacReadValB.Text = AnalogVal.ToString("#.00");
                    break;

                case 2:
                    tbxDacReadValC.Text = AnalogVal.ToString("#.00");
                    break;

                case 3:
                    tbxDacReadValD.Text = AnalogVal.ToString("#.00");
                    break;

                default:
                    WriteStatusFail(KuscCommon.MSG_DAC_ERR_WRONG_INPUT_INDEX);
                    break;
            }
            WriteStatusOk(string.Format("MCU: read DAC {0} OK", dacIndex + 1));
            _kuscLogs.WriteLogMsgOk(string.Format("DAC {0} read value: {1} [mVdc]", dacIndex, AnalogVal.ToString("#.00")));
        }
        #endregion

        #region System logs

        private void btnRunTimeLoggerSave_Click(object sender, EventArgs e)
        {
            sfdSaveSamplesFile.Filter = "KUSC log file format|*" + KuscCommon.LOG_FILE_FORMAT_NAME;
            sfdSaveSamplesFile.Title = "Save an Samples File";
            string sampleStr = string.Empty;


            if (sfdSaveSamplesFile.ShowDialog() == DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(sfdSaveSamplesFile.OpenFile());

                // Insert file title
                writer.WriteLine("*******************************************************************");
                writer.WriteLine("File name: \t KUSC application host logger file");
                writer.WriteLine(string.Format("Date of file: \t {0}", DateTime.Now.ToString()));
                writer.WriteLine("*******************************************************************");

                var lines = rtbLogRunWindow.Lines;
                foreach (var line in lines)
                {
                    writer.WriteLine(line);
                }
                writer.Dispose();
                writer.Close();
            }
        }

        private void btnLoggerSearcherOpen_Click(object sender, EventArgs e)
        {
            _kuscLogs.UpdateLogsStoredTable();
        }

        private void btnLoggerSearcherLogOpen_Click(object sender, EventArgs e)
        {
            rtbLogViewer.Clear();
            Int32 ReqLogIndx = Convert.ToInt32(tbxLoggerSearcherLogReqIndex.Text);
            if (true == _kuscLogs.LoadLogInLogViewer(ReqLogIndx))
            {
                WriteStatusOk(KuscCommon.MSG_LOG_OK_OPEN_LOG);
            }
            else
            {
                WriteStatusFail(KuscCommon.MSG_LOG_ERR_CANT_OPEN_LOG);
            }
        }

        private void btnLogViewerClear_Click(object sender, EventArgs e)
        {
            rtbLogViewer.Clear();
        }
        #endregion

        #region Synthesizers

        private Int32 SetSynthCp(KuscCommon.SYNTH_TYPE cType)
        {
            Int32 cpVal = 0;
            if (cType == KuscCommon.SYNTH_TYPE.SYNTH_TX)
            {
                if (cbxSynthTxSetCp.Text == string.Empty || cbxSynthTxSetCp.Text == "choose")
                {
                    cpVal = KuscCommon.SYNTH_TX_CP_DEFAULT_INDEX;
                }
                else
                {
                    cpVal = cbxSynthTxSetCp.SelectedIndex;
                    _kuscSynth.CalcReg04(KuscCommon.SYNTH_TYPE.SYNTH_TX, cpVal);
                }
            }
            else if (cType == KuscCommon.SYNTH_TYPE.SYNTH_RX)
            {
                if (cbxSynthRxSetCp.Text == string.Empty || cbxSynthRxSetCp.Text == "choose")
                {
                    cpVal = KuscCommon.SYNTH_RX_CP_DEFAULT_INDEX;
                }
                else
                {
                    cpVal = cbxSynthRxSetCp.SelectedIndex;
                    _kuscSynth.CalcReg04(KuscCommon.SYNTH_TYPE.SYNTH_RX, cpVal);
                }
            }
            return cpVal;
        }

        internal void SendSynthRegisters(KuscCommon.SYNTH_TYPE cSynthType)
        {
            if (_synthUpdateCnt < KuscCommon.SYNTH_NUM_CYCLE_IN_UPDATE_REGISTERS)
            {
                if (cSynthType == KuscCommon.SYNTH_TYPE.SYNTH_TX)
                {
                    _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.SYNTH_MSG, KuscMessageParams.MESSAGE_REQUEST.SYNTH_DOWN_SET, dataList[_synthUpdateCnt]);
                }
                else if (cSynthType == KuscCommon.SYNTH_TYPE.SYNTH_RX)
                {
                    _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.SYNTH_MSG, KuscMessageParams.MESSAGE_REQUEST.SYNTH_UP_SET, dataList[_synthUpdateCnt]);
                }
                _synthUpdateCnt++;
            }
            _synthUpdateCnt %= KuscCommon.SYNTH_NUM_CYCLE_IN_UPDATE_REGISTERS;
        }

        internal void ReadSynthUp(string data)
        {
            double fVcoCalc = (2 * _kuscSynth.calcFreqFromUartData(data));
            if(fVcoCalc > 0)
            {
                double fRfCalc = _kuscSynth.calcFrfFromUartData(data);
                tbxSynthRxReadRf.Text = fRfCalc.ToString("#.00");
                tbxSynthRxReadVco.Text = fVcoCalc.ToString("#.00");
                tbxSynthRxReadIf.Text = (fRfCalc - fVcoCalc).ToString("#.00");

                if (true == _kuscSynth.GetCeCondition(data))
                {
                    tbxSynthRxReadCe.Text = KuscCommon.SYNTH_STATE_ON;
                    btnOperSyntUp.Text = KuscCommon.SYNTH_STATE_OFF;
                }
                else
                {
                    tbxSynthRxReadCe.Text = KuscCommon.SYNTH_STATE_OFF;
                    btnOperSyntUp.Text = KuscCommon.SYNTH_STATE_ON;
                }
                tbxSynthRxReadCp.Text = cbxSynthRxSetCp.Items[_kuscSynth.GetCpIndxFromStream(data)].ToString();

                // Store info at system log
                _kuscLogs.WriteLogMsgOk("System: get Synthesizer RX [Up] data");
                _kuscLogs.WriteLogMsgOk(string.Format("Freq: {0} [MHz] \t CP: {1} [mA] \t CE STATE: {2}", tbxSynthRxReadRf.Text, tbxSynthRxReadCp.Text, tbxSynthRxReadCe.Text));

                // Write status ok to screen
                WriteStatusOk(KuscCommon.MSG_SYNTH_OK_READ_STATUS_OK);
            }
            else
            {
                WriteStatusFail("MCU: Error in read synthesizer up (RX) please chech that this isn`t the first time program running");
                tbxSynthRxReadRf.Text = "ERR";
                tbxSynthRxReadVco.Text = "ERR";
                tbxSynthRxReadIf.Text = "ERR";
                tbxSynthRxReadCe.Text = "ERR";
                tbxSynthRxReadCp.Text = "ERR";
            }

        }

        internal void ReadSynthDown(string data)
        {
            double fVcoCalc = (2 * _kuscSynth.calcFreqFromUartData(data));
            if (fVcoCalc > 0)
            {
                double fRfCalc = _kuscSynth.calcFrfFromUartData(data);

                tbxSynthTxReadRf.Text = fRfCalc.ToString("#.00");
                tbxSynthTxReadVco.Text = fVcoCalc.ToString("#.00");
                tbxSynthTxReadIf.Text = (fRfCalc - fVcoCalc).ToString("#.00");

                if (true == _kuscSynth.GetCeCondition(data))
                {
                    tbxSynthTxReadCe.Text = KuscCommon.SYNTH_STATE_ON;
                    btnOperSyntDown.Text = KuscCommon.SYNTH_STATE_OFF;
                }
                else
                {
                    tbxSynthTxReadCe.Text = KuscCommon.SYNTH_STATE_OFF;
                    btnOperSyntDown.Text = KuscCommon.SYNTH_STATE_ON;
                }
                tbxSynthTxReadCp.Text = cbxSynthTxSetCp.Items[_kuscSynth.GetCpIndxFromStream(data)].ToString();

                // Store info at system log
                _kuscLogs.WriteLogMsgOk("System: get Synthesizer TX [down] data");
                _kuscLogs.WriteLogMsgOk(string.Format("Freq: {0} [MHz] \t CP: {1} [mA] \t CE STATE: {2}", tbxSynthTxReadRf.Text, tbxSynthTxReadCp.Text, tbxSynthTxReadCe.Text));

                // Write status ok to screen
                WriteStatusOk(KuscCommon.MSG_SYNTH_OK_READ_STATUS_OK);
            }
            else
            {
                WriteStatusFail("MCU: Error in read synthesizer down (TX) please chech that this isn`t the first time program running");
                tbxSynthTxReadRf.Text = "ERR";
                tbxSynthTxReadVco.Text = "ERR";
                tbxSynthTxReadIf.Text = "ERR";
                tbxSynthTxReadCe.Text = "ERR";
                tbxSynthTxReadCp.Text = "ERR";
            }
           
        }

        internal void UpdateSynthDownOper()
        {
            if (lblStatusSyntDown.Text == KuscCommon.SYNTH_STATE_ON)
            {
                lblStatusSyntDown.Text = KuscCommon.SYNTH_STATE_OFF;
                btnOperSyntDown.Text = KuscCommon.SYNTH_STATE_ON;
            }
            else if (lblStatusSyntDown.Text == KuscCommon.SYNTH_STATE_OFF)
            {
                lblStatusSyntDown.Text = KuscCommon.SYNTH_STATE_ON;
                btnOperSyntDown.Text = KuscCommon.SYNTH_STATE_OFF;
            }
        }

        internal void UpdateSynthUpOper()
        {
            if (lblStatusSyntUp.Text == "ON")
            {
                lblStatusSyntUp.Text = "OFF";
                btnOperSyntUp.Text = "ON";
            }
            else if (lblStatusSyntUp.Text == "OFF")
            {
                lblStatusSyntUp.Text = "ON";
                btnOperSyntUp.Text = "OFF";
            }
        }
        #endregion

        #endregion

        #region UART messages events

        #region MCU control message group

        private void btnControlLedTest_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.CONTROL_MSG, KuscMessageParams.MESSAGE_REQUEST.CONTROL_TEST_LEDS, string.Empty);
        }

        private void btnResetMcu_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.CONTROL_MSG, KuscMessageParams.MESSAGE_REQUEST.CONTROL_RESET_MCU, string.Empty);
        }

        private void btnPA1Set_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.CONTROL_MSG, KuscMessageParams.MESSAGE_REQUEST.CONTROL_PA1_SET, string.Empty);
        }

        private void btnPA2Set_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.CONTROL_MSG, KuscMessageParams.MESSAGE_REQUEST.CONTROL_PA2_SET, string.Empty);
        }

        private void btnBootFileSelect_Click(object sender, EventArgs e)
        {
            if (fdBootloaderOpenFile.ShowDialog() == DialogResult.OK) // Test result.
            {
                WriteStatusOk("Boot file: " + fdBootloaderOpenFile.FileName + "Load ok, start transferring to MCU unit");
            }

        }
        #endregion

        #region MCU status and version group

        private void btnReadMcuFwVer_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.MCU_STATUS_VERSION_MSG, KuscMessageParams.MESSAGE_REQUEST.STATUS_GET_MCU_FW_VERSION, string.Empty);
        }

        private void btnReadMcuTime_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.MCU_STATUS_VERSION_MSG, KuscMessageParams.MESSAGE_REQUEST.STATUS_MCU_RUN_TIME, string.Empty);
        }

        #endregion

        #region MCU ADC group

        private void btnAdcChMode_Click(object sender, EventArgs e)
        {
            string data = string.Empty;
            if(rdbAdcCircMode.Checked == true || rdbAdcSingleMode.Checked == true)
            {
                if (rdbAdcCircMode.Checked == true)
                {
                    data = 0x0.ToString();
                }
                else if (rdbAdcSingleMode.Checked == true)
                {
                    if(cbxAdcSingleCh.SelectedIndex == -1)
                    {
                        WriteStatusFail(KuscCommon.MSG_ADC_ERR_INPUT_WRONG_FORMAT);
                        return;
                    }
                    else
                    {
                        data = (1*10 + cbxAdcSingleCh.SelectedIndex).ToString(); 
                    }
                    
                }
                _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.ADC_MSG, KuscMessageParams.MESSAGE_REQUEST.ADC_CHANNEL_MODE, data);
            }
            else
            {
                WriteStatusFail(KuscCommon.MSG_ADC_ERR_INPUT_MISSING);
            }
            
        }
        #endregion

        #region MCU Synthesizers down / up

        private void btnSetSyntDown_Click(object sender, EventArgs e)
        // Set TX synth
        {
            if ((tbxSynthTxRf.Text != string.Empty) && (tbxSynthTxIf.Text != string.Empty) && (tbxSynthTxRf.Text.Length == 8) && (tbxSynthTxIf.Text.Length == 8) && (tbxSynthTxIf.Text[5] == '.') && (tbxSynthTxRf.Text[5] == '.'))
            {
                double fRf = KuscUtil.ParseDoubleFromString(tbxSynthTxRf.Text);
                double fIf = KuscUtil.ParseDoubleFromString(tbxSynthTxIf.Text);
                tbxSynthVcoOutTxPre.Text = Math.Abs(fRf - fIf).ToString();
                tbxSynthVcoOutTxAfter.Text = (Math.Abs(fRf - fIf) / 2).ToString("F2");
                WriteStatusOk(KuscCommon.MSG_SYNTH_OK_TX_FREQ_SENT);
                if (fRf >= KuscCommon.SYNTH_TX_FRF_MIN_VALUE_MHZ && fRf <= KuscCommon.SYNTH_TX_FRF_MAX_VALUE_MHZ)
                {
                    if (fIf >= KuscCommon.SYNTH_TX_FIF_MIN_VALUE_MHZ && fIf <= KuscCommon.SYNTH_TX_FIF_MAX_VALUE_MHZ)
                    {
                        // Calc CP value and config synth TX register
                        Int32 cpVal = SetSynthCp(KuscCommon.SYNTH_TYPE.SYNTH_TX);

                        // Add info to system log
                        _kuscLogs.WriteLogMsgOk("MCU set synthesizer down (TX) vales");
                        //_kuscLogs.WriteLogMsgOk(string.Format("F_IF: {0} [MHz] \tF_RF: {1} [MHz] \tCP {2} [mA]", fRf, fIf, cbxSynthTxSetCp.Items[cpVal]));

                        dataList = _kuscSynth.GetDataRegisters(KuscCommon.SYNTH_TYPE.SYNTH_TX, fRf, fIf);
                        SendSynthRegisters(KuscCommon.SYNTH_TYPE.SYNTH_TX);
                    }
                    else
                    {
                        WriteStatusFail(string.Format("Please insert TX synthesizer F-IF between {0} and {1}", KuscCommon.SYNTH_TX_FIF_MIN_VALUE_MHZ, KuscCommon.SYNTH_TX_FIF_MAX_VALUE_MHZ));
                    }
                }
                else
                {
                    WriteStatusFail(string.Format("Please insert TX synthesizer F-RF between {0} and {1}", KuscCommon.SYNTH_TX_FRF_MIN_VALUE_MHZ, KuscCommon.SYNTH_TX_FRF_MAX_VALUE_MHZ));
                }
            }
            else
            {
                WriteStatusFail(KuscCommon.MSG_SYNTH_ERR_TX_INPUT_WRONG_FORMAT);
            }
        }

        private void btnSetSyntUp_Click(object sender, EventArgs e)
        {
            if ((tbxSynthRxRf.Text != string.Empty) && (tbxSynthRxIf.Text != string.Empty) && (tbxSynthRxRf.Text.Length == 8) && (tbxSynthRxIf.Text.Length == 8) && (tbxSynthRxIf.Text[5] == '.') && (tbxSynthRxRf.Text[5] == '.'))
            {
                double fRf = KuscUtil.ParseDoubleFromString(tbxSynthRxRf.Text);
                double fIf = KuscUtil.ParseDoubleFromString(tbxSynthRxIf.Text);
                tbxSynthVcoOutRxPre.Text = Math.Abs(fRf - fIf).ToString();
                tbxSynthVcoOutTRxAfter.Text = (Math.Abs(fRf - fIf) / 2).ToString("F2");

                WriteStatusOk(KuscCommon.MSG_SYNTH_OK_RX_FREQ_SENT);
                if (fRf >= KuscCommon.SYNTH_RX_FRF_MIN_VALUE_MHZ && fRf <= KuscCommon.SYNTH_RX_FRF_MAX_VALUE_MHZ)
                {
                    if (fIf >= KuscCommon.SYNTH_RX_FIF_MIN_VALUE_MHZ && fIf <= KuscCommon.SYNTH_RX_FIF_MAX_VALUE_MHZ)
                    {
                        // Calc CP value and config synth TX register
                        Int32 cpVal = SetSynthCp(KuscCommon.SYNTH_TYPE.SYNTH_RX);

                        // Add info to system log
                        _kuscLogs.WriteLogMsgOk("MCU set synthesizer up (RX) vales");
                        if(cpVal > 0) _kuscLogs.WriteLogMsgOk(string.Format("F_IF: {0} [MHz] \tF_RF: {1} [MHz] \tCP {2} [mA]", fRf, fIf, cbxSynthRxSetCp.Items[cpVal]));

                        
                        dataList = _kuscSynth.GetDataRegisters(KuscCommon.SYNTH_TYPE.SYNTH_RX, fRf, fIf);
                        SendSynthRegisters(KuscCommon.SYNTH_TYPE.SYNTH_RX);
                    }
                    else
                    {
                        WriteStatusFail(string.Format("Please insert RX synthesizer F-IF between {0} and {1}", KuscCommon.SYNTH_RX_FIF_MIN_VALUE_MHZ, KuscCommon.SYNTH_RX_FIF_MAX_VALUE_MHZ));
                    }
                }
                else
                {
                    WriteStatusFail(string.Format("Please insert RX synthesizer F-RF between {0} and {1}", KuscCommon.SYNTH_RX_FRF_MIN_VALUE_MHZ, KuscCommon.SYNTH_RX_FRF_MAX_VALUE_MHZ));
                }
            }
            else
            {
                WriteStatusFail(KuscCommon.MSG_SYNTH_ERR_RX_INPUT_WRONG_FORMAT);
            }
        }

        private void btnOperSyntDown_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.SYNTH_MSG, KuscMessageParams.MESSAGE_REQUEST.SYNTH_DOWN_OPER, KuscCommon.TX_SYNTH_ID.ToString());
            if(btnOperSyntDown.Text == KuscCommon.SYNTH_STATE_ON)
            {
                btnOperSyntDown.Text = KuscCommon.SYNTH_STATE_OFF;
            }
            else
            {
                btnOperSyntDown.Text = KuscCommon.SYNTH_STATE_ON;
            }
        }

        private void btnOperSyntUp_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.SYNTH_MSG, KuscMessageParams.MESSAGE_REQUEST.SYNTH_UP_OPER, KuscCommon.RX_SYNTH_ID.ToString());
            if (btnOperSyntUp.Text == KuscCommon.SYNTH_STATE_ON)
            {
                btnOperSyntUp.Text = KuscCommon.SYNTH_STATE_OFF;
            }
            else
            {
                btnOperSyntUp.Text = KuscCommon.SYNTH_STATE_ON;
            }
        }

        private void btnReadSyntDown_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.SYNTH_MSG, KuscMessageParams.MESSAGE_REQUEST.SYNTH_DOWN_READ_DATA, 0x0.ToString());
        }

        private void btnReadSyntUp_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.SYNTH_MSG, KuscMessageParams.MESSAGE_REQUEST.SYNTH_UP_READ_DATA, 0x1.ToString());
        }

        #endregion

        #region MCU FLASH

        private void btnReadFlashData_Click(object sender, EventArgs e)
        {
            if(Convert.ToInt32(tbxFlashNumSampleRead.Text) > 0)
            {
                string data = tbxFlashNumSampleRead.Text.ToString() + '@' + '#';
                _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.FLASH, KuscMessageParams.MESSAGE_REQUEST.FLASH_REQUEST_RAW_DATA, data);
            }
            else
            {
                WriteStatusFail(KuscCommon.MSG_FLASH_ERR_DONT_VALID_NUM_SAMPLES_REQUEST);
            }
        }

        private void btnEmptyFlash_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.FLASH, KuscMessageParams.MESSAGE_REQUEST.FLASH_EREASE_MEMORY, string.Empty);
        }

        private void btnReadFlashStatus_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.FLASH, KuscMessageParams.MESSAGE_REQUEST.FLASH_READ_CONDITION, string.Empty);
        }

        #endregion

        #region MCU DAC

        private void btnSetDacTech_Click(object sender, EventArgs e)
        {
            int dacIndex = 0;
            int dacVal = 0;

            if (rdbDacC.Checked == false && rdbDacD.Checked == false)
            {
                WriteStatusFail(KuscCommon.MSG_DAC_ERR_DAC_NOT_SELECTED);
                return;
            }
            else
            {
                
                if (rdbDacC.Checked)
                {
                    if (tbxDacValC.Text == string.Empty || tbxDacValC.Text.Length != KuscCommon.DAC_MAX_UI_DIGITS)
                    {
                        WriteStatusFail(string.Format(KuscCommon.MSG_DAC_ERR_DAC_C_INPUT_WRONG_FORMAT, KuscCommon.DAC_MAX_UI_DIGITS));
                        return;
                    }
                    else
                    {
                        dacIndex = 2;
                        dacVal = Convert.ToInt32(tbxDacValC.Text);
                    }
                }
                else if (rdbDacD.Checked)
                {
                    if (tbxDacValD.Text == string.Empty || tbxDacValD.Text.Length != KuscCommon.DAC_MAX_UI_DIGITS)
                    {
                        WriteStatusFail(string.Format(KuscCommon.MSG_DAC_ERR_DAC_D_INPUT_WRONG_FORMAT, KuscCommon.DAC_MAX_UI_DIGITS));
                        return;
                    }
                    else
                    {
                        dacIndex = 3;
                        dacVal = Convert.ToInt32(tbxDacValD.Text);
                    }
                }
                if((dacVal > KuscCommon.DAC_VSOURCEPLUS_MILI) || (dacVal < KuscCommon.DAC_VSOURCEMINUS_MILI))
                {
                    WriteStatusFail(string.Format(KuscCommon.MSG_DAC_ERR_VALUE_NOT_IN_RANGE, KuscCommon.DAC_VSOURCEMINUS_MILI, KuscCommon.DAC_VSOURCEPLUS_MILI));
                    return;
                }
            }

            // Prepere configuration word:
            var data = _kuscExtDac.GetDacData(dacIndex, dacVal);

            // Send data to MCU:
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.DAC, KuscMessageParams.MESSAGE_REQUEST.DAC_SET_VALUE, data);
            _kuscLogs.WriteLogMsgOk(string.Format("DAC {0} set value {1} [mVdc]", dacIndex, dacVal));

        }

        private void btnSetDacUser_Click(object sender, EventArgs e)
        {
            int dacIndex = 0;
            int dacVal = 0;

            if (rdbDacA.Checked == false && rdbDacB.Checked == false)
            {
                WriteStatusFail(KuscCommon.MSG_DAC_ERR_DAC_NOT_SELECTED);
                return;
            }
            else
            {
                if (rdbDacA.Checked)
                {
                    if (tbxDacValA.Text == string.Empty || tbxDacValA.Text.Length != KuscCommon.DAC_MAX_UI_DIGITS)
                    {
                        WriteStatusFail(string.Format(KuscCommon.MSG_DAC_ERR_DAC_A_INPUT_WRONG_FORMAT, KuscCommon.DAC_MAX_UI_DIGITS));
                        return;
                    }
                    else
                    {
                        dacIndex = 0;
                        dacVal = Convert.ToInt32(tbxDacValA.Text);
                    }

                }
                else if (rdbDacB.Checked)
                {
                    if (tbxDacValB.Text == string.Empty || tbxDacValB.Text.Length != KuscCommon.DAC_MAX_UI_DIGITS)
                    {
                        WriteStatusFail(string.Format(KuscCommon.MSG_DAC_ERR_DAC_B_INPUT_WRONG_FORMAT, KuscCommon.DAC_MAX_UI_DIGITS));
                        return;
                    }
                    else
                    {
                        dacIndex = 1;
                        dacVal = Convert.ToInt32(tbxDacValB.Text);
                    }
                }

                if ((dacVal > KuscCommon.DAC_VSOURCEPLUS_MILI) || (dacVal < KuscCommon.DAC_VSOURCEMINUS_MILI))
                {
                    WriteStatusFail(string.Format(KuscCommon.MSG_DAC_ERR_VALUE_NOT_IN_RANGE, KuscCommon.DAC_VSOURCEMINUS_MILI, KuscCommon.DAC_VSOURCEPLUS_MILI));
                    return;
                }
            }

            // Prepere configuration word:
            var data = _kuscExtDac.GetDacData(dacIndex, dacVal);

            // Send data to MCU:
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.DAC, KuscMessageParams.MESSAGE_REQUEST.DAC_SET_VALUE, data);
            _kuscLogs.WriteLogMsgOk(string.Format("DAC {0} set value {1} [mVdc]", dacIndex, dacVal));
        }

        private void btnReadDacTech_Click(object sender, EventArgs e)
        {
            byte dacIndex = 0;
            if (rdbDacC.Checked == false && rdbDacD.Checked == false)
            {
                WriteStatusFail(KuscCommon.MSG_DAC_ERR_DAC_NOT_SELECTED);
                return;
            }
            else
            {
                if (rdbDacC.Checked)
                {
                    dacIndex = 2;
                }
                else if (rdbDacD.Checked)
                {
                    dacIndex = 3;
                }
                // Send data to MCU:
                _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.DAC, KuscMessageParams.MESSAGE_REQUEST.DAC_READ_VALUE, dacIndex.ToString());
            }
        }

        private void btnReadDacUser_Click(object sender, EventArgs e)
        {
            byte dacIndex = 0;
            if (rdbDacA.Checked == false && rdbDacB.Checked == false)
            {
                WriteStatusFail(KuscCommon.MSG_DAC_ERR_DAC_NOT_SELECTED);
                return;
            }
            else
            {
                if (rdbDacA.Checked)
                {
                    dacIndex = 0;
                }
                else if (rdbDacB.Checked)
                {
                    dacIndex = 1;
                }
 
                // Send data to MCU:
                _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.DAC, KuscMessageParams.MESSAGE_REQUEST.DAC_READ_VALUE, dacIndex.ToString());
            }
        }

        #endregion

        #endregion

        #region Update system at start

        internal void UpdateSystemAtStart()
        {
            InitSynthesizers();
            lblStatusSyntDown.Text = KuscCommon.SYNTH_STATE_ON;
            lblStatusSyntUp.Text = KuscCommon.SYNTH_STATE_ON;
        }

        internal void InitSynthesizers()
        {
            //var dataList = _kuscSynth.GetStartRegisters(Convert.ToInt32(tbxSynthTxFrfInit.Text), Convert.ToInt32(tbxSynthTxFifInit.Text));
            //foreach (var regData in dataList)
            //{
            //    _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.SYNTH_MSG, KuscMessageParams.MESSAGE_REQUEST.SYNTH_TX_INIT_SET, regData);
            //}
        }
        #endregion

    }
}
