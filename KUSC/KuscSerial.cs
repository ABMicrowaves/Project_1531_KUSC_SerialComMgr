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
        private static int _rxReadCount = 0;
        private static List<char> _txMessageBuffer;

        // define groups functions array:
        public delegate bool Delegatearray(KuscMessageParams.MESSAGE_REQUEST request, string data);
        Delegatearray[] _groups =
        {
            new Delegatearray(KuscMessageFunctions.GroupControlMcu),
            new Delegatearray(KuscMessageFunctions.GroupStatusAndVersion),
            new Delegatearray(KuscMessageFunctions.GroupAdc),
            new Delegatearray(KuscMessageFunctions.GroupSynthesizers),
            new Delegatearray(KuscMessageFunctions.GroupFlashMemory),
            new Delegatearray(KuscMessageFunctions.GroupDAC),
        };

        // System utils:
        KuscUtil _KuscUtil;
        #endregion

        #region Local com settings and C`tor

        public KuscSerial()
        {
            _rxBuffer = new List<char>();
            _txMessageBuffer = new List<char>();
            _KuscUtil = new KuscUtil();
            



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
            char rxRec = Convert.ToChar(sp.ReadChar());
            _rxBuffer.Add(rxRec);
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
                if (_rxBuffer.Count > KuscMessageParams.MIN_RX_MSG_SIZE)
                {
                    if (_rxBuffer[KuscMessageParams.MSG_MAGIC_LOCATION] == KuscMessageParams.MSG_MAGIC_A)
                    {
                        // Store CRC-8 Rx input:
                        char crcIn = _rxBuffer[_rxBuffer.Count - 1];

                        // Remove CRC char:
                        _rxBuffer.RemoveAt(_rxBuffer.Count - 1);
                        char crcCalc = _KuscUtil.CalcCrc8(_rxBuffer.ToArray());
                        if (crcIn == crcCalc) // It is valid frame and recieve ok.
                        {
                            KuscMessageParams.MESSAGE_GROUP group = (KuscMessageParams.MESSAGE_GROUP)_rxBuffer[KuscMessageParams.MSG_GROUP_LOCATION];
                            KuscMessageParams.MESSAGE_REQUEST request = (KuscMessageParams.MESSAGE_REQUEST)_rxBuffer[KuscMessageParams.MSG_REQUEST_LOCATION];
                            string requestData = string.Empty;
                            if (_rxBuffer[KuscMessageParams.MSG_REQUEST_DATA_SIZE] > 0)
                            {
                                 
                                for (int dataIdx = KuscMessageParams.MSG_REQUEST_DATA_SIZE; dataIdx < _rxBuffer.Count - 2; dataIdx++)
                                {
                                    requestData += _rxBuffer[dataIdx] + ",";
                                }
                            }
                            _groups[(int)group - 1](request, requestData);
                        }

                        // Empty rx buffer after read message
                        _rxReadCount = _rxBuffer.Count;
                        _rxBuffer.RemoveRange(0, _rxReadCount);
                    }
                }
                Thread.Sleep(KuscMessageParams.DELAY_BETWEEN_MESSAGE_READ);
            }
        }

        #endregion

        #region Uart Write

        public void SerialWriteChar(char charData)
        {
            if(true == _serialPort.IsOpen)
            {
                _serialPort.Write(charData.ToString());
            }
            else
            {
                KuscUtil.UpdateStatusFail("Port is not open, please open it");
            }
            
        }

        public void SerialWriteString(string stringData)
        {
            _serialPort.Write(stringData);
        }

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
                    char c = Convert.ToChar(Convert.ToInt32(dataChar) - '0');
                    _txMessageBuffer.Add(c);
                }
            }

            // Calc CRC-8:
            char crc = _KuscUtil.CalcCrc8(_txMessageBuffer.ToArray());
            _txMessageBuffer.Add(crc);

            // Now send the frame
            foreach (var item in _txMessageBuffer)
            {
                SerialWriteChar(item);
            }

            return true;
        }
        #endregion

        #endregion
    }
}
