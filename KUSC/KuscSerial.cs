using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace KUSC
{
    class KuscSerial
    {
        #region Class verbs

        private static SerialPort _serialPort = new SerialPort();
        private static bool _readMessageMutex;
        private static List<char> _rxBuffer;
        private static List<char> _txMessageBuffer;

        // define functions of delegates:
        delegate double MessagesGroups(string data);
        private KuscMessageFunctions _messageGroups;
        #endregion

        #region Local com settings and C`tor

        public KuscSerial()
        {
            _rxBuffer = new List<char>();
            _txMessageBuffer = new List<char>();
            _messageGroups = new KuscMessageFunctions();
            //MessagesGroups[] groups = new MessagesGroups
        }

        public List<string> GetComPorts()
        {
            List<string> portsNames = new List<string>();
            foreach (string s in SerialPort.GetPortNames())
            {
                portsNames.Add(s);
            }

            return portsNames;
        }

        public string InitSerialPort(string comport)
        {
            string errMessage = string.Empty;
            try
            {
                _serialPort.BaudRate = KuscCommon.SERIAL_BAUD_RATE;
                _serialPort.PortName = comport;

                // Set the read/write timeouts
                _serialPort.ReadTimeout = KuscCommon.SERIAL_READ_TIMEOUT_MSEC;
                _serialPort.WriteTimeout = KuscCommon.SERIAL_WRITE_TIMEOUT_MSEC;
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(UartRxInterrupt);

                // Open serial port channel:
                _serialPort.Open();

            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
            }
            return errMessage;
        }

        public string UartClose()
        {
            string err = string.Empty;
            try
            {
                _serialPort.Close();
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }
            return err;
        }

        #endregion

        #region UART interface settings

        #region Uart read

        private static void UartRxInterrupt(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            _rxBuffer.Add(Convert.ToChar(sp.ReadByte()));
        }

        public string OpenUartReadMessage()
        {
            string err = string.Empty;
            try
            {
                _readMessageMutex = true;
                var taskReadMessage = new Thread(() => ReadMessage());
                taskReadMessage.Start();
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }
            return err;

        }

        public void ReadMessage()
        {
            while (true == _readMessageMutex)
            {
                // Try to find start of normal frame by 2 magic bytes: 
                if (_rxBuffer.Count > KuscCommon.MIN_RX_MSG_SIZE)
                {
                    if (_rxBuffer[0] == KuscMessageParams.MSG_MAGIC_A)
                    {
                        
                    }
                }
                Task.Delay(1000);
            }
        }


        #endregion

        #region Uart Write

        public void SerialWriteChar(char charData)
        {
            _serialPort.Write(charData.ToString());
        }

        public void SerialWriteString(string stringData)
        {
            _serialPort.Write(stringData);
        }
        #endregion

        #endregion

        #region UART messages

        public bool SerialWriteMessage(KuscMessageParams.MESSAGE_GROUP group, KuscMessageParams.MESSAGE_REQUEST request, string data)
        {
            _txMessageBuffer.Clear(); // Prepere sending list to contain new frame

            _txMessageBuffer.Add(KuscMessageParams.MSG_MAGIC_A);    // First frame char contain magic.
            _txMessageBuffer.Add(Convert.ToChar(group));            // Second frame char contain group.
            _txMessageBuffer.Add(Convert.ToChar(request));          // Second frame char contain message request.
            _txMessageBuffer.Add(Convert.ToChar(data.Length));      // Third frame contain number of bytes of data.
            if (data.Length > 0)
            {
                foreach (char dataChar in data)
                {
                    _txMessageBuffer.Add(dataChar);
                }
                _txMessageBuffer.Add(Convert.ToChar(data.Length));
            }

            // Now send the frame

            foreach (var item in _txMessageBuffer)
            {
                SerialWriteChar(item);
            }

            return true;
        }

        delegate double ControlMessageGroup(string data);
        #endregion
    }
}
