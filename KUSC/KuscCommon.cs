using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUSC
{
    static class KuscCommon
    {
        #region Technician mode  

        public static string TECH_USER              = "AB";
        public static string TECH_PASS              = "1234";
        public static string TECH_LOGIN_OK_MSG      = "Enter to technician mode";
        public static string TECH_LOGIN_FAIL_MSG    = "Incorrect login parameters";
        #endregion

        #region Serial configuration

        public static int SERIAL_BAUD_RATE          = 115200; 
        public static int SERIAL_READ_TIMEOUT_MSEC  = 500;
        public static int SERIAL_WRITE_TIMEOUT_MSEC = 500;
        public static int RX_BUF_SIZE_BYTES         = 20;
        #endregion

        #region Logic configuration

        #region SYNTH calculations params:

        public static double FREQ_STEP_KHZ = 10;

        #region SYNTH TX

        // F_IF allowed values: 
        public static int SYNTH_TX_FIF_MIN_VALUE_MHZ = 950;
        public static int SYNTH_TX_FIF_MAX_VALUE_MHZ = 2620;

        // F_RF
        public static int SYNTH_TX_FRF_MIN_VALUE_MHZ = 10950;
        public static int SYNTH_TX_FRF_MAX_VALUE_MHZ = 11700;
        #endregion

        #region SYNTH RX

        // F_IF
        public static int SYNTH_RX_FIF_MIN_VALUE_MHZ = 950;
        public static int SYNTH_RX_FIF_MAX_VALUE_MHZ = 2620;

        // F_RF allowed values: 
        public static int SYNTH_RX_FRF_MIN_VALUE_MHZ = 13750;
        public static int SYNTH_RX_FRF_MAX_VALUE_MHZ = 14500;

        #endregion

        #region SYNTH constant params

        // Synth ID:
        public static Int16 TX_SYNTH_ID = 0x1;
        public static Int16 RX_SYNTH_ID = 0x1;
        public static Int16 SYNTH_NUM_UPDATE_REGISTERS = 7;

        // Save Synth registers:
        public static Int32 SYNTH_REG04 = 0x30008384;
        public static Int32 SYNTH_REG06 = 0x35006076;
        public static Int32 SYNTH_REG10 = 0xC0193A;

        public static int SYNTH_F_REF_MHZ = 40;
        public static int SYNTH_D = 1;
        public static int SYNTH_R = 1;
        public static int SYNTH_T = 0;
        public static int SYNTH_MOD1 = 16777216;
        public static int SNYTH_F_CHSP_KHZ = 100;

        public static string SYNTH_TX_F_RF_INIT_VALUE = "11000";
        public static string SYNTH_TX_F_IF_INIT_VALUE = "02100";
        public static string SYNTH_RX_F_RF_INIT_VALUE = "00950";
        public static string SYNTH_RX_F_IF_INIT_VALUE = "13750";

        #endregion

        #region SYNTH registers caluclation verbs

        public static bool SYNTH_AUTOCAL = true;


        public struct REG_DATA
        {
            public double   fVco;
            public double   fPFD;
            public Int32    INT;
            public Int32    Mod1;
            public double   Fraq;
            public Int32    Fraq1;
            public double   remFraq1;
            public Int32    Mod2;
            public int  Fraq2;
        }
        #endregion

        #endregion

        #region Extenal DAC

        public static int DAC_VSOURCEPLUS_MILI      = 4880;
        public static int DAC_VSOURCEMINUS_MILI     = 0;
        public static int DAC_BITS                  = 10;
        public static int DAC_MAX_UI_DIGITS         = 4;
        public static bool DAC_NO_LOW_POWER_MODE    = true;     // 0 - All DACs go shout-down, 1 - Normal operation.
        public static bool DAC_UPDATE_OUTPUTS       = false;    // 0 - Update registers and outputs off all DACs, 1 - Update only registers, don`t change DACs ouputs.

        #endregion

        #endregion
    }
}
