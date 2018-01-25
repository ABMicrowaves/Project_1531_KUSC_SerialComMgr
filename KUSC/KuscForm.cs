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
            lblStatus.Text = statMessage;
        }

        public void WriteStatusFail(string statMessage)
        {
            lblStatus.ForeColor = Color.Red;
            lblStatus.Text = statMessage;
        }
        #endregion

        #endregion

        private void btnControlLedTest_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.CONTROL_MSG, KuscMessageParams.MESSAGE_REQUEST.CONTROL_TEST_LEDS, string.Empty);
        }

        private void btnReadMcuFwVer_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP.CONTROL_MSG, KuscMessageParams.MESSAGE_REQUEST.STATUS_MCU_FW_VERSION, string.Empty);
        }
    }
}
