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
        private static bool _readMessageMutex;
        private static List<char> _rxBuffer;
        private List<char> _rxMsgBuffer;
        private static int _rxReadCount = 0;
        private static int _rxWriteCount = 0;
        private char cRxChar;
        private List<char> _rxDataArray;

        // UART Write
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
            JUMP_FUNCTION
        };

        static UART_READ_STATE cRxState;

        // System utils:
        KuscUtil _KuscUtil;
        #endregion

        #region Local com settings and C`tor

        public KuscSerial()
        {
            _rxBuffer = new List<char>();
            for (int i = 0; i < KuscMessageParams.MSG_RX_BUFFER_SIZE; i++)
            {
                _rxBuffer.Add('0');
            }
            _rxMsgBuffer = new List<char>();
            _txMessageBuffer = new List<char>();
            _rxDataArray = new List<char>();
            _KuscUtil = new KuscUtil();
            cRxState = UART_READ_STATE.START_RX_MESSAGE_READ;
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
            _rxBuffer[_rxWriteCount++] = rxRec;
            //_rxWriteCount %= KuscMessageParams.MSG_RX_BUFFER_SIZE;
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
                switch(cRxState)
                {
                    case UART_READ_STATE.START_RX_MESSAGE_READ:

                        if((_rxWriteCount - _rxReadCount) > KuscMessageParams.MIN_RX_MSG_SIZE)
                        {
                            cRxState = UART_READ_STATE.FIND_MAGIC;
                        }
                        break;

                    case UART_READ_STATE.FIND_MAGIC:

                        cRxChar = _rxBuffer[_rxReadCount++];
                        if (cRxChar == KuscMessageParams.MSG_MAGIC_A)
                        {
                            _rxMsgBuffer.Clear();
                            _rxDataArray.Clear();
                            _rxMsgBuffer.Insert(KuscMessageParams.MSG_MAGIC_LOCATION, cRxChar); 
                            cRxState = UART_READ_STATE.READ_GROUP;
                        }
                        break;

                    case UART_READ_STATE.READ_GROUP:

                        cRxChar = _rxBuffer[_rxReadCount++];
                        _rxMsgBuffer.Insert(KuscMessageParams.MSG_GROUP_LOCATION, cRxChar);
                        cRxState = UART_READ_STATE.READ_REQUEST;

                        break;

                    case UART_READ_STATE.READ_REQUEST:

                        cRxChar = _rxBuffer[_rxReadCount++];
                        _rxMsgBuffer.Insert(KuscMessageParams.MSG_REQUEST_LOCATION, cRxChar);
                        cRxState = UART_READ_STATE.READ_DATA_SIZE;
                        break;

                    case UART_READ_STATE.READ_DATA_SIZE:

                        cRxChar = _rxBuffer[_rxReadCount];
                        _rxMsgBuffer.Insert(KuscMessageParams.MSG_REQUEST_DATA_SIZE_LOCATION, cRxChar);
                        if (cRxChar == 0x0)  // check if there is data to read.
                        {
                            cRxState = UART_READ_STATE.CHECK_CRC;
                        }
                        else
                        {
                            cRxState = UART_READ_STATE.READ_DATA;

                        }
                        
                        break;

                    case UART_READ_STATE.READ_DATA:

                        int dataSize = Convert.ToInt32(_rxBuffer[_rxReadCount]);
                        for (int idx = 0; idx < dataSize; idx++)
                        {
                            char data = _rxBuffer[++_rxReadCount];
                            _rxMsgBuffer.Add(data);
                            _rxDataArray.Add(data);
                        }
                        cRxState = UART_READ_STATE.CHECK_CRC;
                        break;

                    case UART_READ_STATE.CHECK_CRC:
                        char crcGiven = _rxBuffer[_rxReadCount++];
                        char crcCalc = _KuscUtil.CalcCrc8(_rxMsgBuffer.ToArray());
                        cRxState = UART_READ_STATE.JUMP_FUNCTION;
                        break;

                    case UART_READ_STATE.JUMP_FUNCTION:
                        
                        _groups[_rxMsgBuffer[KuscMessageParams.MSG_GROUP_LOCATION] - 1]((KuscMessageParams.MESSAGE_REQUEST)_rxMsgBuffer[KuscMessageParams.MSG_REQUEST_LOCATION], string.Join(",", _rxDataArray.ToArray()));
                        cRxState = UART_READ_STATE.START_RX_MESSAGE_READ;
                        break;
                }
                _rxReadCount %= KuscMessageParams.MSG_RX_BUFFER_SIZE;
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
            if (data != string.Empty)
            {
                foreach (char dataItem in data)
                {
                    if(dataItem >= 0x30) // Dont convert special sings.
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
