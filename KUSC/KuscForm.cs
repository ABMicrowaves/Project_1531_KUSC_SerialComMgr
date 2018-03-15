using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

        // Synthesizers:
        List<string> dataList;
        private short _synthUpdateCnt = 0;

        public KuscForm()
        {
            InitializeComponent();
            _kuscSerial = new KuscSerial();
            _KuscUtil = new KuscUtil();
            _kuscSynth = new KuscSynth();
            _kuscExtDac = new KuscExtDac();

            // Set Synthesizers init value
            dataList = new List<string>();
            tbxSynthTxFifInit.Text = KuscCommon.SYNTH_TX_F_RF_INIT_VALUE;
            tbxSynthTxFifInit.Text = KuscCommon.SYNTH_TX_F_IF_INIT_VALUE;
            tbxSynthRxFrfInit.Text = KuscCommon.SYNTH_RX_F_RF_INIT_VALUE;
            tbxSynthRxFifInit.Text = KuscCommon.SYNTH_RX_F_IF_INIT_VALUE;


            // Set UI Vref
            lblAdcVrefUi.Text = string.Format("Vref= {0} [mVdc]", KuscCommon.DAC_VSOURCEPLUS_MILI.ToString());
            _KuscUtil.UpdateStatusObject(this);
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
                WriteStatusFail("Dont found any comport available");
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

        internal void ReadSynthUp(string data)
        {

        }

        internal void SendSynthRegisters()
        {
            if(_synthUpdateCnt < KuscCommon.SYNTH_NUM_UPDATE_REGISTERS)
            {
                _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.SYNTH_MSG, KuscMessageParams.MESSAGE_REQUEST.SYNTH_DOWN_SET, dataList[_synthUpdateCnt]);
                _synthUpdateCnt++;
            }
            _synthUpdateCnt %= KuscCommon.SYNTH_NUM_UPDATE_REGISTERS;
        }

        internal void ReadSynthDown(string data)
        {

        }

        internal void UpdateSynthDownOper()
        {
            if(lblStatusSyntDown.Text == "ON")
            {
                lblStatusSyntDown.Text = "OFF";
                btnOperSyntDown.Text = "ON";
            }
            else if (lblStatusSyntDown.Text == "OFF")
            {
                lblStatusSyntDown.Text = "ON";
                btnOperSyntDown.Text = "OFF";
            }
        }

        internal void UpdateSynthUpOper()
        {
            throw new NotImplementedException();
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
            lblStatus.ForeColor = Color.Black;
            lblStatus.Text = statMessage;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
        }

        public void WriteStatusFail(string statMessage)
        {
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
                rtbAdcResults.Text += res;
            }
        }

        private void UpdateAdcChannelTable(UInt16 dataSample, UInt16 channel)
        {
            switch(channel)
            {
                case 0x1:
                    rtbAdcRE2.AppendText(dataSample.ToString() + Environment.NewLine);
                    break;

                case 0x2:
                    rtbAdcRE1.AppendText(dataSample.ToString() + Environment.NewLine);
                    break;

                case 0x3:
                    rtbAdcRE0.AppendText(dataSample.ToString() + Environment.NewLine);
                    break;

                case 0x4:
                    rtbAdcRA5.AppendText(dataSample.ToString() + Environment.NewLine);
                    break;

                case 0x5:
                    rtbAdcRD5.AppendText(dataSample.ToString() + Environment.NewLine);
                    break;

                case 0x6:
                    rtbAdcRB1.AppendText(dataSample.ToString() + Environment.NewLine);
                    break;

                case 0x7:
                    rtbAdcRB5.AppendText(dataSample.ToString() + Environment.NewLine);
                    break;

            }
        }

        private void btnClearAdcTable_Click(object sender, EventArgs e)
        {
            rtbAdcResults.Clear();
            rtbAdcRE2.Clear();
            rtbAdcRE1.Clear();
            rtbAdcRE0.Clear();
            rtbAdcRA5.Clear();
            rtbAdcRD5.Clear();
            rtbAdcRB1.Clear();
            rtbAdcRB5.Clear();
        }

        #endregion


        public void UpdateMcuFw(string data)
        {
            data = data.Replace(",", "");
            var mcuCompileDate = data[0] | data[1];
            tbxMcuFwVerDate.Text = data;
        }

        public void UpdateSystemRunTime(string dataSample)
        {
            dataSample = dataSample.Replace(",", "");
            int seconds = Convert.ToInt32(new string(dataSample.ToCharArray().Reverse().ToArray()));
            tbxSysRunTime.Text = TimeSpan.FromSeconds(seconds).ToString();
             
        }

        public void UpdateFlashCondition(string dataSample)
        {
            var samples = dataSample.Split(',');
            var FlashSize = (Convert.ToInt16(Convert.ToChar(samples[0]) << 8) + Convert.ToInt16(Convert.ToChar(samples[1])));
            var writeAddress = (Convert.ToInt16(Convert.ToChar(samples[2]) << 8) + Convert.ToInt16(Convert.ToChar(samples[3])));
            var precentage = Convert.ToInt16(((double)(FlashSize - writeAddress) / FlashSize) * 100);
            var numPackets = (FlashSize - writeAddress) / 32;

            // Update checkbox fields:

            tbxFlashCondTotal.Text = FlashSize.ToString();
            tbxFlashCondFree.Text = writeAddress.ToString();
            tbxFlashCondPrecentage.Text = precentage.ToString();
            tbxFlashCondNumPackets.Text = numPackets.ToString();
        }
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

        private void btnAdcNegativVoltage_Click(object sender, EventArgs e)
        {
            //_kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.ADC_MSG, KuscMessageParams.MESSAGE_REQUEST.ADC_ENABLE, string.Empty);
        }

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
                        WriteStatusFail("In ADC single mode please choose channel to sample");
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
                WriteStatusFail("Please choose desire ADC sampling mode");
            }
            
        }

        private void btnAdcPositveVoltage_Click(object sender, EventArgs e)
        {
            //_kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.ADC_MSG, KuscMessageParams.MESSAGE_REQUEST.ADC_ENABLE, string.Empty);
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
                WriteStatusOk("Host: Freqancy send to unit");
                if (fRf >= KuscCommon.SYNTH_TX_FRF_MIN_VALUE_MHZ && fRf <= KuscCommon.SYNTH_TX_FRF_MAX_VALUE_MHZ)
                {
                    if (fIf >= KuscCommon.SYNTH_TX_FIF_MIN_VALUE_MHZ && fIf <= KuscCommon.SYNTH_TX_FIF_MAX_VALUE_MHZ)
                    {
                        dataList = _kuscSynth.GetDataRegisters(fRf, fIf);
                        SendSynthRegisters();
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
                WriteStatusFail("Please insert correct TX synthesizer Frf [UVWX.YZ] and Fif values [UVWX.YZ]");
            }
        }

        private void btnSetSyntUp_Click(object sender, EventArgs e)
        {
            string data = string.Empty;
            if ((tbxSynthRxRf.Text.Length == 5) && (tbxSynthRxIf.Text.Length == 5))
            {
                if ((Convert.ToInt32(tbxSynthRxRf.Text) * 1000 % KuscCommon.FREQ_STEP_KHZ == 0) && (Convert.ToInt32(tbxSynthRxIf.Text) * 1000 % KuscCommon.FREQ_STEP_KHZ == 0))
                {
                    if ((Convert.ToInt32(tbxSynthRxRf.Text) >= KuscCommon.SYNTH_RX_FRF_MIN_VALUE_MHZ) &&
                        (Convert.ToInt32(tbxSynthRxRf.Text) <= KuscCommon.SYNTH_RX_FRF_MAX_VALUE_MHZ))
                    {
                        if ((Convert.ToInt32(tbxSynthRxIf.Text) >= KuscCommon.SYNTH_RX_FIF_MIN_VALUE_MHZ) &&
                        (Convert.ToInt32(tbxSynthRxIf.Text) <= KuscCommon.SYNTH_RX_FIF_MAX_VALUE_MHZ))
                        {
                            
                            data = tbxSynthRxRf.Text;
                            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.SYNTH_MSG, KuscMessageParams.MESSAGE_REQUEST.SYNTH_UP_SET, data);
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
                    WriteStatusFail(string.Format("Allowed steps in RX F-RF and F-IF is {0} KHz", KuscCommon.FREQ_STEP_KHZ));
                }
                
            }
            else
            {
                WriteStatusFail("Please insert correct RX synthesizer Frf [5 digits length] and Fif values [5 digits length]");
            }
        }

        #endregion

        #region MCU FLASH

        private void btnReadFlashData_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.FLASH, KuscMessageParams.MESSAGE_REQUEST.FLASH_REQUEST_RAW_DATA, tbxFlashNumSampleRead.Text.ToString());
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

        private void btnSetDac_Click(object sender, EventArgs e)
        {
            int dacIndex = 0;
            int dacVal = 0;

            if (rdbDacA.Checked == false && rdbDacB.Checked == false && rdbDacC.Checked == false && rdbDacD.Checked == false)
            {
                WriteStatusFail("None of the DAC unit had been selected");
                return;
            }
            else
            {
                if(rdbDacA.Checked)
                {
                    if(tbxDacValA.Text == string.Empty || tbxDacValA.Text.Length != KuscCommon.DAC_MAX_UI_DIGITS)
                    {
                        WriteStatusFail(string.Format("Please insert dac A value [{0} digits]", KuscCommon.DAC_MAX_UI_DIGITS));
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
                        WriteStatusFail(string.Format("Please insert dac B value [{0} digits]", KuscCommon.DAC_MAX_UI_DIGITS));
                        return;
                    }
                    else
                    {
                        dacIndex = 1;
                        dacVal = Convert.ToInt32(tbxDacValB.Text);
                    }
                }
                else if (rdbDacC.Checked)
                {
                    if (tbxDacValC.Text == string.Empty || tbxDacValC.Text.Length != KuscCommon.DAC_MAX_UI_DIGITS)
                    {
                        WriteStatusFail(string.Format("Please insert dac C value [{0} digits]", KuscCommon.DAC_MAX_UI_DIGITS));
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
                        WriteStatusFail(string.Format("Please insert dac D value [{0} digits]", KuscCommon.DAC_MAX_UI_DIGITS));
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
                    WriteStatusFail(string.Format("Dac value can`t be lower then Vsource_minus [{0} [mVdc]] or higher then Vref = [{1} [mVdc]]", KuscCommon.DAC_VSOURCEMINUS_MILI, KuscCommon.DAC_VSOURCEPLUS_MILI));
                    return;
                }
            }

            // Prepere configuration word:
            var data = _kuscExtDac.GetDacData(dacIndex, dacVal);

            // Send data to MCU:
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.DAC, KuscMessageParams.MESSAGE_REQUEST.DAC_SET_VALUE, data);

        }


        #endregion

        #endregion

        #region Update system at start

        internal void UpdateSystemAtStart()
        {
            InitSynthesizers();
            lblStatusSyntDown.Text = "ON";
            lblStatusSyntUp.Text = "ON";
        }

        internal void InitSynthesizers()
        {
            var dataList = _kuscSynth.GetStartRegisters(Convert.ToInt32(tbxSynthTxFrfInit.Text), Convert.ToInt32(tbxSynthTxFifInit.Text));
            foreach (var regData in dataList)
            {
                _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.SYNTH_MSG, KuscMessageParams.MESSAGE_REQUEST.SYNTH_TX_INIT_SET, regData);
            }
        }
        #endregion

        private void tmrSysEvents_Tick(object sender, EventArgs e)
        {
            
        }

        private void btnBootFileSelect_Click(object sender, EventArgs e)
        {
            if (fdBootloaderOpenFile.ShowDialog() == DialogResult.OK) // Test result.
            {
                WriteStatusOk("Boot file: " + fdBootloaderOpenFile.FileName + "Load ok, start transferring to MCU unit");
            }
            
        }

        private void btnSetSynthInit_Click(object sender, EventArgs e)
        {

        }

        private void btnOperSyntDown_Click(object sender, EventArgs e)
        {
            if(lblStatusSyntDown.Text != string.Empty)
            {
                _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.SYNTH_MSG, KuscMessageParams.MESSAGE_REQUEST.SYNTH_DOWN_OPER, KuscCommon.TX_SYNTH_ID.ToString());
            }
            else
            {
                WriteStatusFail("Please read Synth TX values prioer to set operation state");
            }
        }

        private void btnOperSyntUp_Click(object sender, EventArgs e)
        {
            if (lblStatusSyntUp.Text != string.Empty)
            { 
                _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.SYNTH_MSG, KuscMessageParams.MESSAGE_REQUEST.SYNTH_UP_OPER, KuscCommon.RX_SYNTH_ID.ToString());
            }
            else
            {
                WriteStatusFail("Please read Synth RX values prioer to set operation state");
            }
        }

        private void btnReadSyntDown_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.SYNTH_MSG, KuscMessageParams.MESSAGE_REQUEST.SYNTH_DOWN_READ_DATA, 0x1.ToString());
        }

        private void btnReadSyntUp_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.SYNTH_MSG, KuscMessageParams.MESSAGE_REQUEST.SYNTH_UP_READ_DATA, 0x0.ToString());
        }

    }
}
