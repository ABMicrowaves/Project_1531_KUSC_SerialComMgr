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

        #region Set Local serial paramaters
        private void btnCheckLocalPortsNames_Click(object sender, EventArgs e)
        {
            cbxLocalPortsNames.Items.Clear();
            foreach (var port in _kuscSerial.GetComPorts())
            {
                cbxLocalPortsNames.Items.Add(port);
            }
        }

        private void btnLocalBaudSelect_Click(object sender, EventArgs e)
        {
            if (cbxLocalBaud.SelectedItem == null || cbxLocalPortsNames.SelectedItem == null)
            {
                lblStatus.Text = "Please select both baud-rate speed and COM port name";
            }
            else
            {
                lblStatus.Text = "Local serial paramters saved (" +
                                cbxLocalPortsNames.SelectedItem.ToString() + "," +
                                cbxLocalBaud.SelectedItem.ToString() + ")";
                _kuscSerial.LocalComPort = cbxLocalPortsNames.SelectedItem.ToString();
                _kuscSerial.LocalBaudRate = int.Parse(cbxLocalBaud.SelectedItem.ToString());
                _kuscSerial.SetLocalComConfig();
            }
        }
        #endregion
    }
}
