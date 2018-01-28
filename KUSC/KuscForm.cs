using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KUSC
{
    public partial class KuscForm : Form
    {
        KuscSerial _kuscSerial;
        KuscUtil _KuscUtil;
        public KuscForm()
        {
            InitializeComponent();
            _kuscSerial = new KuscSerial();
            _KuscUtil = new KuscUtil();
            _KuscUtil.UpdateStatusObject(this);
        }

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
            foreach (var port in _kuscSerial.GetComPorts())
            {
                cbxLocalPortsNames.Items.Add(port);
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
            //lblStatus.Text = statMessage;
        }

        public void WriteStatusFail(string statMessage)
        {
            lblStatus.ForeColor = Color.Red;
            lblStatus.Text = statMessage;
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
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.CONTROL_MSG, KuscMessageParams.MESSAGE_REQUEST.STATUS_MCU_FW_VERSION, string.Empty);
        }

        private void btnReadCpldFwVersion_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.CONTROL_MSG, KuscMessageParams.MESSAGE_REQUEST.STATUS_MCU_CPLD_VERSION, string.Empty);
        }

        private void btnReadMcuTime_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.CONTROL_MSG, KuscMessageParams.MESSAGE_REQUEST.STATUS_MCU_RUN_TIME, string.Empty);
        }
        #endregion

        #region MCU ADC group

        private void btnAdcNegativVoltage_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.CONTROL_MSG, KuscMessageParams.MESSAGE_REQUEST.ADC_ENABLE, string.Empty);
        }

        private void btnAdcChMode_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.CONTROL_MSG, KuscMessageParams.MESSAGE_REQUEST.ADC_CHANNEL_SELECT_MODE, string.Empty);
        }

        private void btnAdcPositveVoltage_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.CONTROL_MSG, KuscMessageParams.MESSAGE_REQUEST.ADC_ENABLE, string.Empty);
        }

        private void btnAdcConvMode_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.CONTROL_MSG, KuscMessageParams.MESSAGE_REQUEST.ADC_CONV_RESULT_FORMAT, string.Empty);
        }
        #endregion

        #region MCU Synthesizers down / up

        private void btnSetSyntDown_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.CONTROL_MSG, KuscMessageParams.MESSAGE_REQUEST.SYNTH_DOWN_SET, string.Empty);
        }

        private void btnSetSyntUp_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.CONTROL_MSG, KuscMessageParams.MESSAGE_REQUEST.SYNTH_UP_SET, string.Empty);
        }

        #endregion

        #region MCU FLASH

        private void btnReadFlashData_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.CONTROL_MSG, KuscMessageParams.MESSAGE_REQUEST.FLASH_READ_RAW_DATA, string.Empty);
        }

        private void btnEmptyFlash_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.CONTROL_MSG, KuscMessageParams.MESSAGE_REQUEST.FLASH_EREASE_MEMORY, string.Empty);
        }

        private void btnReadFlashStatus_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.CONTROL_MSG, KuscMessageParams.MESSAGE_REQUEST.FLASH_READ_CONDITION, string.Empty);
        }

        #endregion

        #region MCU DAC

        private void btnSetDac_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.CONTROL_MSG, KuscMessageParams.MESSAGE_REQUEST.DAC_SET_VALUE, string.Empty);
        }

        #endregion

        #endregion


    }
}
