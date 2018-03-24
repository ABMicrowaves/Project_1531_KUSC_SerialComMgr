using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUSC
{
    class KuscUtil
    {
        #region Class verbs

        static KuscForm _KuscForm;

        #endregion

        #region CRC

        internal static char CalcCrc8(char[] input)
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

        internal static double GCD(double a, double b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }

            if(a == 0)
            {
                return b;
            }
            else
            {
                return a;
            }
        }

        internal static double GetFractionOfDouble(double num)
        {
            return num - (int)num;
        }

        internal static double ParseDoubleFromString(string data)
        {
            var fraction = data.Split('.');
            return Convert.ToDouble(fraction[0]) + (Convert.ToDouble(fraction[1]) / 100);
        }
        #endregion

        #region Update main form

        internal void UpdateStatusObject(KuscForm Sender)
        {
            _KuscForm = Sender;
        }

        internal static void UpdateStatusOk(string msg)
        {
            _KuscForm.WriteStatusOk(msg);
        }

        internal static void UpdateStatusFail(string msg)
        {
            _KuscForm.WriteStatusFail(msg);
        }

        internal static void UpdateAdcTable(string dataSample)
        {
            _KuscForm.UpdateAdcTable(dataSample);
        }

        internal static void UpdateMcuFwVersion(string fwVersionData)
        {
            _KuscForm.UpdateMcuFw(fwVersionData);
        }

        internal static void UpdateRunTime(string sysRunTime)
        {
            _KuscForm.UpdateSystemRunTime(sysRunTime);
        }

        internal static void UpdateFlashCondition(string flashCondData)
        {
            _KuscForm.UpdateFlashCondition(flashCondData);
        }

        internal static void UpdateSystemRegisters()
        {
            _KuscForm.UpdateSystemAtStart();
        }

        #endregion

        #region Util Functions

        internal static string HexToDecimalString(string val)
        {
            int value = Convert.ToInt32(val, 16);
            return Convert.ToInt32(val.ToString(), 16).ToString();
        }

        internal static void UpdateSynthUpOper()
        {
            _KuscForm.UpdateSynthUpOper();
        }

        internal static void UpdateSynthDownOper()
        {
            _KuscForm.UpdateSynthDownOper();
        }

        internal static void ReadSynthUp(string data)
        {
            _KuscForm.ReadSynthUp(data);
        }

        internal static void ReadSynthDown(string data)
        {
            _KuscForm.ReadSynthDown(data);
        }

        internal static void ReqAntherTxRegister()
        {
            _KuscForm.SendSynthRegisters(KuscCommon.SYNTH_TYPE.SYNTH_TX);
        }

        internal static void ReqAntherRxRegister()
        {
            _KuscForm.SendSynthRegisters(KuscCommon.SYNTH_TYPE.SYNTH_RX);
        }

        internal static void DacReadValue(string data)
        {
            _KuscForm.DacReadData(data);
        }

        internal string WriteLockStateFromGivenData(ushort dataSample)
        {
            string returnVal = string.Empty;
            BitArray bitArr = new BitArray(new int[] { dataSample });
            for (int idx = 0; idx < 12; idx++)
            {
                if (bitArr[idx] == true)
                {
                    returnVal += "LOCK" + Environment.NewLine;
                }
                else
                {
                    returnVal += "UNLOCK" + Environment.NewLine;
                }
            }
            return returnVal;

        }
        #endregion

    }
}
