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

        // UART Read variables:
        private static string _recBuffer = string.Empty;
        private static List<char> _rxMsgBuffer;
        private static List<char> _rxDataArray;
        private static Semaphore _serialSem;

        // UART Write
        private static List<char> _txMessageBuffer;

        // define groups functions array:
        public delegate bool Delegatearray(KuscMessageParams.MESSAGE_REQUEST request, string data);
        static Delegatearray[] _groups =
        {
            new Delegatearray(KuscMessageFunctions.GroupControlMcu),
            new Delegatearray(KuscMessageFunctions.GroupStatusAndVersion),
            new Delegatearray(KuscMessageFunctions.GroupAdc),
            new Delegatearray(KuscMessageFunctions.GroupSynthesizers),
            new Delegatearray(KuscMessageFunctions.GroupFlashMemory),
            new Delegatearray(KuscMessageFunctions.GroupDAC),
        };

        // Serial RX enum:

        enum UART_READ_STATE
        {
            START_RX_MESSAGE_READ = 0,
            FIND_MAGIC,
            READ_GROUP,
            READ_REQUEST,
            READ_DATA_SIZE,
            READ_DATA,
            CHECK_CRC,
            JUMP_FUNCTION,
            FINISH_ROUND
        };

        static UART_READ_STATE cRxState;

        // System utils:
        KuscUtil _KuscUtil;
        #endregion

        #region Local com settings and C`tor

        public KuscSerial()
        {
            _rxMsgBuffer = new List<char>();
            _txMessageBuffer = new List<char>();
            _rxDataArray = new List<char>();
            _KuscUtil = new KuscUtil();
            cRxState = UART_READ_STATE.FIND_MAGIC;

            _serialSem = new Semaphore(1, 1);
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
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
                _serialPort.Encoding = System.Text.Encoding.GetEncoding(28591);
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

        private static void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int readBytes = 0;
            if (e.EventType == SerialData.Chars)
            {
                Thread.Sleep(KuscMessageParams.RX_SLEEP_TIME_MSEC);

                string str = _serialPort.ReadExisting();

                _recBuffer += str;

                while (_recBuffer != string.Empty)
                {
                    readBytes = 0;
                    if (_recBuffer.Length >= KuscMessageParams.MIN_RX_MSG_SIZE)
                    {
                        readBytes = ParseMessage(_recBuffer);
                    }


                    if (readBytes > 0)
                    {
                        _recBuffer = _recBuffer.Remove(0, readBytes);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }


        private static int ParseMessage(string msg)
        {
            int _rxReadByte = 0;
            cRxState = UART_READ_STATE.START_RX_MESSAGE_READ;

            while (true)
            {
                try
                {
                    switch (cRxState)
                    {

                        case UART_READ_STATE.START_RX_MESSAGE_READ:
                            _rxMsgBuffer.Clear();
                            _rxDataArray.Clear();
                            _rxReadByte = 0;
                            cRxState = UART_READ_STATE.FIND_MAGIC;
                            break;

                        case UART_READ_STATE.FIND_MAGIC:

                            _rxReadByte++;
                            if (msg[KuscMessageParams.MSG_MAGIC_LOCATION] == KuscMessageParams.MSG_MAGIC_A)
                            {
                                _rxMsgBuffer.Add(msg[KuscMessageParams.MSG_MAGIC_LOCATION]);
                                cRxState = UART_READ_STATE.READ_GROUP;
                            }
                            else
                            {
                                return _rxReadByte;
                            }

                            cRxState = UART_READ_STATE.READ_GROUP;
                            break;

                        case UART_READ_STATE.READ_GROUP:

                            _rxMsgBuffer.Add(msg[KuscMessageParams.MSG_GROUP_LOCATION]);
                            _rxReadByte++;
                            cRxState = UART_READ_STATE.READ_REQUEST;
                            break;

                        case UART_READ_STATE.READ_REQUEST:

                            _rxMsgBuffer.Add(msg[KuscMessageParams.MSG_REQUEST_LOCATION]);
                            cRxState = UART_READ_STATE.READ_DATA_SIZE;
                            _rxReadByte++;
                            break;

                        case UART_READ_STATE.READ_DATA_SIZE:

                            _rxMsgBuffer.Add(msg[KuscMessageParams.MSG_REQUEST_DATA_SIZE_LOCATION]);
                            cRxState = UART_READ_STATE.READ_DATA_SIZE;
                            _rxReadByte++;
                            if (msg[KuscMessageParams.MSG_REQUEST_DATA_SIZE_LOCATION] == 0x0)  // check if there is data to read.
                            {
                                cRxState = UART_READ_STATE.CHECK_CRC;
                            }
                            else
                            {
                                cRxState = UART_READ_STATE.READ_DATA;

                            }
                            break;

                        case UART_READ_STATE.READ_DATA:

                            int dataSize = Convert.ToInt32(msg[KuscMessageParams.MSG_REQUEST_DATA_SIZE_LOCATION]);
                            if (dataSize > msg.Length)
                            {
                                return 0;
                            }
                            for (int idx = 0; idx <= dataSize; idx++)
                            {
                                _rxDataArray.Add(msg[KuscMessageParams.MSG_REQUEST_DATA_LOCATION + idx]);
                            }
                            _rxReadByte += dataSize;
                            cRxState = UART_READ_STATE.CHECK_CRC;
                            break;

                        case UART_READ_STATE.CHECK_CRC:
                            int crcLocation = KuscMessageParams.MSG_REQUEST_DATA_LOCATION + Convert.ToInt32(msg[KuscMessageParams.MSG_REQUEST_DATA_SIZE_LOCATION]);
                            char crcGiven = msg[crcLocation];
                            char crcCalc = KuscUtil.CalcCrc8(msg.ToArray());
                            cRxState = UART_READ_STATE.JUMP_FUNCTION;
                            break;

                        case UART_READ_STATE.JUMP_FUNCTION:

                            _groups[msg[KuscMessageParams.MSG_GROUP_LOCATION] - 1]((KuscMessageParams.MESSAGE_REQUEST)msg[KuscMessageParams.MSG_REQUEST_LOCATION], string.Join(",", _rxDataArray.ToArray()));
                            cRxState = UART_READ_STATE.FINISH_ROUND;
                            break;

                        case UART_READ_STATE.FINISH_ROUND:
                            return _rxReadByte;
                    }
                }
                catch (Exception)
                {
                    
                }
                  
            }
            
        }

        #endregion

        #region Uart Write

        public void SerialWriteChar(char charData)
        {
            if (true == _serialPort.IsOpen)
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
            _serialSem.WaitOne();
            _txMessageBuffer.Clear(); // Prepere sending list to contain new frame

            _txMessageBuffer.Add(KuscMessageParams.MSG_MAGIC_A);    // First frame char contain magic.
            _txMessageBuffer.Add(Convert.ToChar(group));            // Second frame char contain group.
            _txMessageBuffer.Add(Convert.ToChar(request));          // Second frame char contain message request.
            _txMessageBuffer.Add(Convert.ToChar(data.Length));      // Third frame contain number of bytes of data.
            if (data != string.Empty)
            {
                foreach (char dataItem in data)
                {
                    if (dataItem >= 0x30) // Dont convert special sings.
                    {
                        char c = Convert.ToChar(Convert.ToInt32(dataItem) - 0x30);
                        _txMessageBuffer.Add(c);
                    }
                    else
                    {
                        _txMessageBuffer.Add(dataItem);
                    }
                }
            }

            // Calc CRC-8:
            char crc = KuscUtil.CalcCrc8(_txMessageBuffer.ToArray());
            _txMessageBuffer.Add(crc);

            // Now send the frame
            foreach (var item in _txMessageBuffer)
            {
                SerialWriteChar(item);
            }
            _serialSem.Release();
            return true;
        }
        #endregion

        #endregion
    }
}
