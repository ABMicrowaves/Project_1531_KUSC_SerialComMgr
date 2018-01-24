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
        public KuscForm()
        {
            InitializeComponent();
            _kuscSerial = new KuscSerial();
            
            
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
            _kuscSerial.SerialWriteString(tbxWriteSerial.Text);
        }



        #endregion



        #region Application logs

        #region Application interface

        void WriteStatusOk(string statMessage)
        {
            lblStatus.ForeColor = Color.Black;
            lblStatus.Text = statMessage;
        }

        void WriteStatusFail(string statMessage)
        {
            lblStatus.ForeColor = Color.Red;
            lblStatus.Text = statMessage;
        }
        #endregion

        #endregion

        private void btnControlLedTest_Click(object sender, EventArgs e)
        {
            _kuscSerial.SerialWriteMessage(KuscMessage.MESSAGE_GROUP.CONTROL_MSG, KuscMessage.MESSAGE_REQUEST.CONTROL_TEST_LEDS, string.Empty);
        }
    }
}
