using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUSC
{
    class KuscSynth
    {
        #region Class verbs

        KuscCommon.REG_DATA regData = new KuscCommon.REG_DATA();

        Int32 synthReg04Tx = KuscCommon.SYNTH_REG04;
        Int32 synthReg04Rx = KuscCommon.SYNTH_REG04;
        #endregion

        #region Synthesizers output calculations

        #region Synthesizers make output registers list

        internal List<string> GetDataRegisters(KuscCommon.SYNTH_TYPE cSynthType, double fRF, double fIF)
        {
            List<string> regList = new List<string>();

            List<Int32> regListNum = new List<Int32>();
            CalcSynthParams(fRF, fIF);

            regList.Add(KuscCommon.SYNTH_REG10.ToString() + '@' + 0xa.ToString() + '#');     // R10
            regList.Add(KuscCommon.SYNTH_REG06.ToString() + '@' + 0x6.ToString() + '#');     // R6
            if (cSynthType == KuscCommon.SYNTH_TYPE.SYNTH_TX)
            {
                regList.Add(synthReg04Tx.ToString() + '@' + 0x4.ToString() + '#');          // R4 - TX
            }
            else if (cSynthType == KuscCommon.SYNTH_TYPE.SYNTH_RX)
            {
                regList.Add(synthReg04Rx.ToString() + '@' + 0x4.ToString() + '#');          // R4 - RX
            }
            regList.Add(CalcReg02().ToString() + '@' + 0x2.ToString() + '#');                // R2
            regList.Add(CalcReg01().ToString() + '@' + 0x1.ToString() + '#');                // R1
            regList.Add(CalcReg00().ToString() + '@' + 0x0.ToString() + '#');                // R0
            regList.Add(KuscCommon.SYNTH_REG04.ToString() + '@' + 0x4.ToString() + '#');     // R4
            regList.Add(CalcReg00().ToString() + '@' + 0x0.ToString() + '#');                // R0

            // Now send the F_RF frequancy to read latter time:
            Int32 fRfToSend = Convert.ToInt32(fRF * 100);
            regList.Add(fRfToSend.ToString() + '@' + 0xb.ToString() + '#');

            return regList;
        }

        internal List<string> GetStartRegisters(int fRF, int fIF)
        // This function is for future use.
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
        #endregion

        #region Calculation of output Synthesizers registers

        private void CalcSynthParams(double fRF, double fIF)
        {
            regData.fVco = Math.Abs(fRF - fIF) / 2.0;
            regData.fVco = Math.Round(regData.fVco, 3);
            regData.fPFD = KuscCommon.SYNTH_F_PFD;
            regData.INT = (int)(regData.fVco / regData.fPFD);
            regData.Mod1 = KuscCommon.SYNTH_MOD1;
            regData.Fraq = KuscUtil.GetFractionOfDouble(regData.fVco / regData.fPFD);
            regData.Fraq1 = (int)(regData.Fraq * KuscCommon.SYNTH_MOD1);
            regData.remFraq1 = KuscUtil.GetFractionOfDouble(regData.Fraq * KuscCommon.SYNTH_MOD1);
            regData.Mod2 = KuscCommon.SYNTH_MOD2; //(int)((regData.fPFD*10e6) / KuscUtil.GCD(regData.fPFD*10e6, KuscCommon.FREQ_STEP_KHZ * 10e3));
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

        internal void CalcReg04(KuscCommon.SYNTH_TYPE cSynthType, int cpCurrentValue)
        // According to data sheet: 0 -> 0.300, 15 -> 
        {
            // Create and change bit array
            BitArray bitArr = new BitArray(new int[] { KuscCommon.SYNTH_REG04 });
            BitArray bitArrCp = new BitArray(new int[] { cpCurrentValue });

            for (int idx = 0; idx < 4; idx++)
            {
                bitArr[10 + idx] = bitArrCp[idx];
            }


            // Convert result to int32 again
            int value = 0;

            for (int i = 0; i < bitArr.Count; i++)
            {
                if (bitArr[i])
                    value += Convert.ToInt32(Math.Pow(2, i));
            }

            if (cSynthType == KuscCommon.SYNTH_TYPE.SYNTH_TX)
            {
                synthReg04Tx = value;
            }
            else if (cSynthType == KuscCommon.SYNTH_TYPE.SYNTH_RX)
            {
                synthReg04Rx = value;
            }
        }
        #endregion

        #endregion

        #region Synthesizers calculation from income EUSART stream

        internal double calcFreqFromUartData(string data)
        {
            double rfSignal = 0.0;

            // Prepere stream data for read
            int[] regArr = new int[KuscCommon.SYNTH_NUM_UPDATE_REGISTERS];

            var chars = data.Replace("\x2C", string.Empty).ToCharArray();

            // Make numbers array of the incoming stream
            for (int regIdx = 0; regIdx < KuscCommon.SYNTH_NUM_UPDATE_REGISTERS; regIdx++)
            {
                for (int byteIdx = 0; byteIdx < KuscCommon.SYNTH_NUM_BYTE_UPDATE_REGISTER; byteIdx++)
                {
                    char t = chars[regIdx * KuscCommon.SYNTH_NUM_BYTE_UPDATE_REGISTER + byteIdx];
                    Int32 numBase = (Int32)Math.Pow(2, 8 * byteIdx);
                    regArr[regIdx] |= t * numBase;
                }
            }

            if(regArr[0] == 0x0)
            {
                return 0;
            }

            // Calculate synthesizer params
            regData.INT = (((regArr[0] >> 4) << 21) >> 21);
            regData.Fraq1 = regArr[1] >> 4;
            regData.Mod1 = KuscCommon.SYNTH_MOD1;
            regData.Mod2 = KuscCommon.SYNTH_MOD2;
            regData.Fraq2 = regArr[2] >> 18;

            // Calculate frequancy from synthesizer params
            rfSignal = (regData.INT + ((double)(regData.Fraq1 + regData.Fraq2 / regData.Mod2) / regData.Mod1)) * KuscCommon.SYNTH_F_PFD;

            // Return calculated rf value
            return rfSignal;
        }

        internal bool GetCeCondition(string data)
        {
            var chars = data.Replace("\x2C", string.Empty).ToCharArray();
            bool condition = chars[23] == 0x1 ? true : false;
            return condition;
        }

        internal Int32 GetCpIndxFromStream(string data)
        {
            var chars = data.Replace("\x2C", string.Empty).ToCharArray();
            var returnVal = (Convert.ToInt32(chars[13]) & 0x3C) >> 2;
            return returnVal;
        }

        internal double calcFrfFromUartData(string data)
        {
            var chars = data.Replace("\x2C", string.Empty).ToCharArray();
            return (((chars[22] << 16) | chars[21] << 8 | chars[20]) / 100.00);
        }
        #endregion

    }
}
