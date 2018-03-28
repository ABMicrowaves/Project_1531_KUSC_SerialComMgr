using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUSC
{
    class KuscExtDac
    {
        internal string GetDacData(int dacIndex, int miliVolts)
        {
            string dacConfigWord = string.Empty;

            int dVal = (int)((miliVolts * (Math.Pow(2, KuscCommon.DAC_BITS) - 1)) / KuscCommon.DAC_VSOURCEPLUS_MILI);
            int powerMode = Convert.ToInt16(KuscCommon.DAC_NO_LOW_POWER_MODE);
            int ldac = Convert.ToInt16(KuscCommon.DAC_UPDATE_OUTPUTS);

            int dacVal = (dVal << 2) | (ldac << 12) | (powerMode << 13) | (dacIndex << 14);
            dacConfigWord = dacVal.ToString("X") + '@' + '#';
            int indx = (dacVal >> 14);
            return dacConfigWord;
        }

        internal void GetDacValueFromInputStream(string data, ref Int32 dacIndex, ref double AnalogVal)
        {
            var chars = data.Replace("\x2C", string.Empty).ToCharArray();

            Int32 regsiterVal   = 0x0, dacVal = 0x0;
            
            for (int byteIdx = 0; byteIdx < KuscCommon.DAC_NUM_BYTE_UPDATE_REGISTER; byteIdx++)
            {
                regsiterVal |= chars[byteIdx] * (Int16)Math.Pow(2, 8 * byteIdx);
            }

            dacVal = ((regsiterVal & 0x0ffc) >> 2) + 1;     // Add 1 for resolution
            dacIndex = regsiterVal >> 14;
            AnalogVal = ((KuscCommon.DAC_VSOURCEPLUS_MILI * dacVal)/ (Math.Pow(2, KuscCommon.DAC_BITS) - 1));

        }
    }
}
