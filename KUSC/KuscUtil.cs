using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUSC
{
    class KuscUtil
    {
        #region Class verbs:

        static KuscForm _KuscForm;

        #endregion

        #region CRC

        public char CalcCrc8(char[] input)
        {
            int crc = 0;
            for (int i = 0; i < input.Length; i++)
                crc += input[i];
            crc &= 0xff;
            //string crchex = crc.ToString("X2");
            return Convert.ToChar(crc);
        }

        public ushort CalcCrc16(char[] bytes)
        {
            const ushort poly = 4129;
            ushort[] table = new ushort[256];
            ushort initialValue = 0xffff;
            ushort temp, a;
            ushort crc = initialValue;
            for (int i = 0; i < table.Length; ++i)
            {
                temp = 0;
                a = (ushort)(i << 8);
                for (int j = 0; j < 8; ++j)
                {
                    if (((temp ^ a) & 0x8000) != 0)
                        temp = (ushort)((temp << 1) ^ poly);
                    else
                        temp <<= 1;
                    a <<= 1;
                }
                table[i] = temp;
            }
            for (int i = 0; i < bytes.Length; ++i)
            {
                crc = (ushort)((crc << 8) ^ table[((crc >> 8) ^ (0xff & bytes[i]))]);
            }
            return crc;
        }

        #endregion

        #region Update main form

        public void UpdateStatusObject(KuscForm Sender)
        {
            _KuscForm = Sender;
        }

        public void UpdateStatusOk(string msg)
        {
            _KuscForm.WriteStatusOk(msg);
        }

        public void UpdateStatusFail(string msg)
        {
            _KuscForm.WriteStatusFail(msg);
        }

        public void TestUartCom(char c)
        {
            _KuscForm.TestUart(c);
        }
        #endregion

    }
}
