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

        internal List<string> GetDataRegisters(double fRF, double fIF)
        {
            List<string> regList = new List<string>();

            List<Int32> regListNum = new List<Int32>();
            CalcSynthParams(fRF, fIF);

            regList.Add(KuscCommon.SYNTH_REG10.ToString() + '@');   // R10
            regList.Add(KuscCommon.SYNTH_REG06.ToString() + '@');   // R6
            regList.Add(KuscCommon.SYNTH_REG04.ToString() + '@');   // R4
            regList.Add(CalcReg02().ToString() + '@');              // R2
            regList.Add(CalcReg01().ToString() + '@');              // R1
            regList.Add(CalcReg00().ToString() + '@');              // R0
            regList.Add(KuscCommon.SYNTH_REG04.ToString() + '@');   // R4
            regList.Add(CalcReg00().ToString() + '@');              // R0

            // Testing:

            regListNum.Add(KuscCommon.SYNTH_REG10);   // R10
            regListNum.Add(KuscCommon.SYNTH_REG06);   // R6
            regListNum.Add(KuscCommon.SYNTH_REG04);   // R4
            regListNum.Add(CalcReg02());              // R2
            regListNum.Add(CalcReg01());              // R1
            regListNum.Add(CalcReg00());              // R0
            regListNum.Add(KuscCommon.SYNTH_REG04);   // R4
            regListNum.Add(CalcReg00());              // R0

            return regList;
        }


        internal List<string> GetStartRegisters(int fRF, int fIF)
        {
            List<string> recvList = new List<string>();

            //recvList.Add(KuscCommon.SYNTH_REG11 + '@');
            //recvList.Add(KuscCommon.SYNTH_REG10 + '@');
            //recvList.Add(KuscCommon.SYNTH_REG09 + '@');
            //recvList.Add(KuscCommon.SYNTH_REG08 + '@');
            //recvList.Add(KuscCommon.SYNTH_REG07 + '@');
            //recvList.Add(KuscCommon.SYNTH_REG06 + '@');
            //recvList.Add(KuscCommon.SYNTH_REG05 + '@');
            //recvList.Add(KuscCommon.SYNTH_REG04 + '@');
            //recvList.Add(KuscCommon.SYNTH_REG03 + '@');

            CalcSynthParams(fRF, fIF);
            //recvList.Add(CalcReg02());
            //recvList.Add(CalcReg01());
            //recvList.Add(CalcReg00());

            return recvList;
        }

        private void CalcSynthParams(double fRF, double fIF)
        {
            regData.fVco = Math.Abs(fRF - fIF) / 2.0;
            regData.fVco = Math.Round(regData.fVco, 3);
            regData.fPFD = 40.0;
            regData.INT = (int)(regData.fVco / regData.fPFD);
            regData.Mod1 = KuscCommon.SYNTH_MOD1;
            regData.Fraq = KuscUtil.GetFractionOfDouble(regData.fVco / regData.fPFD);
            regData.Fraq1 = (int)(regData.Fraq * KuscCommon.SYNTH_MOD1);
            regData.remFraq1 = KuscUtil.GetFractionOfDouble(regData.Fraq * KuscCommon.SYNTH_MOD1);
            regData.Mod2 = 5461; //(int)((regData.fPFD*10e6) / KuscUtil.GCD(regData.fPFD*10e6, KuscCommon.FREQ_STEP_KHZ * 10e3));
            regData.Fraq2 = (int)(regData.remFraq1 * regData.Mod2);
        }

        private Int32 CalcReg00()
        {
            return ((Convert.ToInt16(KuscCommon.SYNTH_AUTOCAL) << 21) + (regData.INT << 4) | 0x0);
        }

        private Int32 CalcReg01()
        {
            return ((regData.Fraq1 << 4) | 0x1);
        }

        private Int32 CalcReg02()
        {
            return ((regData.Fraq2 << 18) + (regData.Mod2 << 4) | 0x2);
        }
    }
}
