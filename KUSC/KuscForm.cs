using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KUSC
{
    public partial class KuscForm : Form
    {
        #region Class c`tor and verbs

        KuscSerial _kuscSerial;
        KuscUtil _KuscUtil;
        public KuscForm()
        {
            InitializeComponent();
            _kuscSerial = new KuscSerial();
            _KuscUtil = new KuscUtil();
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
                    WriteStatusOk("Comport open OK");

                    err = _kuscSerial.OpenUartReadMessage();
                    if(err != string.Empty)
                    {
                        WriteStatusFail(err);
                    }
                }
            }
        }
        #endregion

        private void btnUartTestSend_Click(object sender, EventArgs e)
        {
            char c = Convert.ToChar(0xA1);
            _kuscSerial.SerialWriteChar(c);
        }

        public void TestUart(char c)
        {
            tbxTestUart.Text = c.ToString();
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

        public void UpdateAdcTable(string dataSample)
        {
            rtbAdcResults.Text = dataSample;
        }

        public void UpdateMcuFw(string dataSample)
        {
            tbxMcuFwVersion.Text = dataSample;
        }

        public void UpdateSystemRunTime(string dataSample)
        {
            tbxSysRunTime.Text = dataSample;
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

        private void btnResetCPLD_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.CONTROL_MSG, KuscMessageParams.MESSAGE_REQUEST.CONTROL_RESET_CPLD, string.Empty);
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

        private void btnReadCpldFwVersion_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.MCU_STATUS_VERSION_MSG, KuscMessageParams.MESSAGE_REQUEST.STATUS_GET_CPLD_VERSION, string.Empty);
        }

        private void btnReadMcuTime_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.MCU_STATUS_VERSION_MSG, KuscMessageParams.MESSAGE_REQUEST.STATUS_MCU_RUN_TIME, string.Empty);
        }

        private void btnSetMcuFw_Click(object sender, EventArgs e)
        {
            var fwVersion = tbxSetMcuFwValue.Text;
            if (fwVersion != string.Empty)
            {
                _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.MCU_STATUS_VERSION_MSG, KuscMessageParams.MESSAGE_REQUEST.STATUS_SET_MCU_FW_VERSION, fwVersion);
            }
            else
            {
                WriteStatusFail("Please Insert MCU FW version first");
            }
        }

        private void btnSetCpldFw_Click(object sender, EventArgs e)
        {
            var fwVersion = tbxSetCpldFwValue.Text;
            if (fwVersion != string.Empty)
            {
                _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.MCU_STATUS_VERSION_MSG, KuscMessageParams.MESSAGE_REQUEST.STATUS_SET_CPLD_VERSION, fwVersion);
            }
            else
            {
                WriteStatusFail("Please Insert CPLD FW version first");
            }
        }
        #endregion

        #region MCU ADC group

        private void btnAdcNegativVoltage_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.ADC_MSG, KuscMessageParams.MESSAGE_REQUEST.ADC_ENABLE, string.Empty);
        }

        private void btnAdcChMode_Click(object sender, EventArgs e)
        {
            string data = string.Empty;
            if(rdbAdcCircMode.Checked == true)
            {
                _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.ADC_MSG, KuscMessageParams.MESSAGE_REQUEST.ADC_CHANNEL_CIRC_MODE, data);
            }
            else if (rdbAdcSingleMode.Checked == true)
            {
                data = (cbxAdcSingleCh.SelectedIndex + 1).ToString();
                _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.ADC_MSG, KuscMessageParams.MESSAGE_REQUEST.ADC_CHANNEL_SINGLE_MODE, data);
            }
            
        }

        private void btnAdcPositveVoltage_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.ADC_MSG, KuscMessageParams.MESSAGE_REQUEST.ADC_ENABLE, string.Empty);
        }

        private void btnAdcConvMode_Click(object sender, EventArgs e)
        {
            if(rdbAdcConvFormatLeft.Checked == true)
            {
                _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.ADC_MSG, KuscMessageParams.MESSAGE_REQUEST.ADC_CONV_RESULT_LEFT, string.Empty);
            }
            else if(rdbAdcConvFormatRight.Checked == true)
            {
                _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.ADC_MSG, KuscMessageParams.MESSAGE_REQUEST.ADC_CONV_RESULT_RIGHT, string.Empty);
            }
            
        }
        #endregion

        #region MCU Synthesizers down / up

        private void btnSetSyntDown_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.SYNTH_MSG, KuscMessageParams.MESSAGE_REQUEST.SYNTH_DOWN_SET, string.Empty);
        }

        private void btnSetSyntUp_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.SYNTH_MSG, KuscMessageParams.MESSAGE_REQUEST.SYNTH_UP_SET, string.Empty);
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
            if( (tbxDacVal.Text != string.Empty) || 
                (tbxDacVal.Text.Length != 5) ||
                (tbxDacVal.Text[1] != 0x2e))    // 0x2e = "." 
            {
                _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.DAC, KuscMessageParams.MESSAGE_REQUEST.DAC_SET_VALUE, tbxDacVal.Text);
            }
            else
            {
                WriteStatusFail("Please insert dac value");
            }
            
        }

        #endregion

        #endregion

    }
}
