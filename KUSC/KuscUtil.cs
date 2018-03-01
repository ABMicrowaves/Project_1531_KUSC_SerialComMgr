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

        public static char CalcCrc8(char[] input)
        {
            int crc = 0;
            for (int i = 0; i < input.Length; i++)
                crc += input[i];
            crc &= 0xff;
            return Convert.ToChar(crc);
        }

        private static string ConvertDataToString(string data)
        {
            string result = string.Empty;
            foreach (var num in data)
            {
                if(num == 0x2c)
                {
                    result += num.ToString();
                }
                else
                {
                    result += ((int)num).ToString();
                } 
            }
            return result;
        }

        public static int GCD(int a, int b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }

            if (a == 0)
                return b;
            else
                return a;
        }

        #endregion

        #region Update main form

        public void UpdateStatusObject(KuscForm Sender)
        {
            _KuscForm = Sender;
        }

        public static void UpdateStatusOk(string msg)
        {
            _KuscForm.WriteStatusOk(msg);
        }

        public static void UpdateStatusFail(string msg)
        {
            _KuscForm.WriteStatusFail(msg);
        }

        public static void UpdateAdcTable(string dataSample)
        {
            _KuscForm.UpdateAdcTable(dataSample);
        }

        public static void UpdateMcuFwVersion(string fwVersionData)
        {
            _KuscForm.UpdateMcuFw(ConvertDataToString(fwVersionData));
        }

        public static void UpdateCpldFwVersion(string fwVersionData)
        {
            _KuscForm.UpdateCpldFw(ConvertDataToString(fwVersionData));
        }

        public static void UpdateRunTime(string sysRunTime)
        {
            _KuscForm.UpdateSystemRunTime(ConvertDataToString(sysRunTime));
        }

        public static void UpdateFlashCondition(string flashCondData)
        {
            _KuscForm.UpdateFlashCondition(flashCondData);
        }

        public static void UpdateSystemRegisters()
        {
            _KuscForm.UpdateSystemAtStart();
        }

        #endregion

    }
}
