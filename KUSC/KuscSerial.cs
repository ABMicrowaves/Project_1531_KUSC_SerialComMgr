using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace KUSC
{
    public class KuscSerial
    {
        #region Class verbs

        private int _baudrate = 0;
        private string _comPort;
        private static SerialPort _serialPort = new SerialPort();
        private static bool _readThreadMutex;
        #endregion

        #region Local com settings and C`tor

        public List<string> GetComPorts()
        {
            List<string> portsNames = new List<string>();
            foreach (string s in SerialPort.GetPortNames())
            {
                portsNames.Add(s);
            }

            return portsNames;
        }
        public int LocalBaudRate
        {
            get { return _baudrate; }
            set { _baudrate = value; }
        }
        public string LocalComPort
        {
            get { return _comPort; }
            set { _comPort = value; }
        }

        public bool SetLocalComConfig()
        {
            _serialPort.BaudRate = _baudrate;
            _serialPort.PortName = _comPort;

            // Set the read/write timeouts
            _serialPort.ReadTimeout = 500;  // TBD: need to take from common lib.
            _serialPort.WriteTimeout = 500; // TBD: need to take from common lib.

            // Open serial port channel:
            _serialPort.Open();

            // Start read thread
            Thread readThread = new Thread(Read);
            _readThreadMutex = true;
            readThread.Start();

            // If read stop close the serial port.
            readThread.Join();
            _serialPort.Close();
            return true;
        }

        #endregion

        #region Read tread

        public static void Read()
        {
            while (_readThreadMutex)
            {
                try
                {
                    int charIn = _serialPort.ReadChar();
                    Console.WriteLine(charIn);
                }
                catch (TimeoutException) { }
            }
        }

        #endregion



    }
}
