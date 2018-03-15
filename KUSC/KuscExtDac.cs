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
            dacConfigWord = dacVal.ToString("X") + '@';
            int indx = (dacVal >> 14);
            return dacConfigWord;
        }
    }
}
