using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUSC
{
    class KuscSynth
    {

        KuscCommon.REG_DATA regData = new KuscCommon.REG_DATA();

        internal List<string> GetDataRegisters(int fRF, int fIF)
        {
            List<string> regList = new List<string>();

            CalcSynthParams(fRF, fIF);
            regList.Add(CalcReg02() + '@');
            regList.Add(CalcReg01() + '@');
            regList.Add(CalcReg00() + '@');

            return regList;
        }


        internal List<string> GetStartRegisters(int fRF, int fIF)
        {
            List<string> recvList = new List<string>();

            recvList.Add(KuscCommon.SYNTH_REG11 + '@');
            recvList.Add(KuscCommon.SYNTH_REG10 + '@');
            recvList.Add(KuscCommon.SYNTH_REG09 + '@');
            recvList.Add(KuscCommon.SYNTH_REG08 + '@');
            recvList.Add(KuscCommon.SYNTH_REG07 + '@');
            recvList.Add(KuscCommon.SYNTH_REG06 + '@');
            recvList.Add(KuscCommon.SYNTH_REG05 + '@');
            recvList.Add(KuscCommon.SYNTH_REG04 + '@');
            recvList.Add(KuscCommon.SYNTH_REG03 + '@');

            CalcSynthParams(fRF, fIF);
            recvList.Add(CalcReg02());
            recvList.Add(CalcReg01());
            recvList.Add(CalcReg00());

            return recvList;
        }

        private void CalcSynthParams(int fRF, int fIF)
        {
            regData.fVco = Math.Abs(fRF - fIF) / 2;
            regData.fPFD = (KuscCommon.SYNTH_F_REF_MHZ * (1 + KuscCommon.SYNTH_D)) / (KuscCommon.SYNTH_R * (1 + KuscCommon.SYNTH_T));
            regData.INT = (regData.fVco / regData.fPFD);
            regData.Mod1 = KuscCommon.SYNTH_MOD1;
            regData.Fraq = (regData.fVco % regData.fPFD) / (double)regData.fPFD;
            regData.Fraq1 = (int)(regData.Fraq * KuscCommon.SYNTH_MOD1);
            regData.remFraq1 = (regData.Fraq * KuscCommon.SYNTH_MOD1) % 1;
            regData.Mod2 = (regData.fPFD * 1000) / KuscUtil.GCD(regData.fPFD * 1000, KuscCommon.FREQ_STEP_KHZ);
            regData.Fraq2 = (int)(regData.remFraq1 * regData.Mod2);
        }

        private string CalcReg00()
        {
            int regConfig = (Convert.ToInt16(KuscCommon.SYNTH_AUTOCAL) << 21) + (regData.INT << 4) | 0x0;
            return regConfig.ToString("X");
        }

        private string CalcReg01()
        {
            int regConfig = (regData.Fraq1 << 4) | 0x1;
            return regConfig.ToString("X");
        }

        private string CalcReg02()
        {
            int regConfig = (regData.Fraq2 << 18) + (regData.Mod2 << 4) | 0x2;
            return regConfig.ToString("X");
        }


    }
}
