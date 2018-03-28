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

        public static string TECH_USER  = "AB";
        public static string TECH_PASS  = "1234";
        #endregion

        #region Serial configuration

        public static int SERIAL_BAUD_RATE          = 115200; 
        public static int SERIAL_READ_TIMEOUT_MSEC  = 500;
        public static int SERIAL_WRITE_TIMEOUT_MSEC = 500;
        public static int RX_BUF_SIZE_BYTES         = 20;
        #endregion

        #region Logic configuration

        #region SYNTH calculations params

        public static double FREQ_STEP_KHZ  = 10;


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

        // CE states
        internal static string SYNTH_STATE_ON                       = "ON";
        internal static string SYNTH_STATE_OFF                      = "OFF";

        // Synth ID
        public static Int16 TX_SYNTH_ID                             = 0x1;
        public static Int16 RX_SYNTH_ID                             = 0x1;

        public static Int16 SYNTH_NUM_UPDATE_REGISTERS              = 0x3;
        public static Int16 SYNTH_NUM_CYCLE_IN_UPDATE_REGISTERS     = 0x9;
        public static Int16 SYNTH_NUM_BYTE_UPDATE_REGISTER          = 0x5;

        public enum SYNTH_TYPE : int
        {
            SYNTH_TX = 0x00,
            SYNTH_RX = 0x01,
        };

        // Save Synth registers:
        public static Int32 SYNTH_REG04 = 0x30008384;
        public static Int32 SYNTH_REG06 = 0x35006076;
        public static Int32 SYNTH_REG10 = 0xC0193A;

        public static int SYNTH_F_REF_MHZ   = 40;
        public static double SYNTH_F_PFD    = 40.0;
        public static int SYNTH_D           = 1;
        public static int SYNTH_R           = 1;
        public static int SYNTH_T           = 0;
        public static int SYNTH_MOD1        = 16777216;
        public static int SYNTH_MOD2        = 5461;
        public static int SNYTH_F_CHSP_KHZ  = 100;

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

        public static int DAC_VSOURCEPLUS_MILI          = 4880;
        public static int DAC_VSOURCEMINUS_MILI         = 0;
        public static int DAC_BITS                      = 10;
        public static int DAC_MAX_UI_DIGITS             = 4;
        public static int DAC_NUM_BYTE_UPDATE_REGISTER  = 2;
        public static bool DAC_NO_LOW_POWER_MODE        = true;     // 0 - All DACs go shout-down, 1 - Normal operation.
        public static bool DAC_UPDATE_OUTPUTS           = false;    // 0 - Update registers and outputs off all DACs, 1 - Update only registers, don`t change DACs ouputs.

        #endregion

        #region System logs params

        internal static string LOG_FILE_FORMAT_NAME = ".kuscLog";
        #endregion

        #region FLASH

        internal static int SYNTH_TX_CP_DEFAULT_INDEX = 0;  // 0.300
        internal static int SYNTH_RX_CP_DEFAULT_INDEX = 1;  // 0.600
        #endregion

        #endregion

        #region Status messages

        #region Technician messages

        public static string TECH_LOGIN_OK_MSG                          = "Enter to technician mode";
        public static string TECH_LOGIN_FAIL_MSG                        = "Incorrect login parameters";
        #endregion

        #region Serial interface

        public static string MSG_SERIAL_ERR_DONT_FOUND_ANY_COMPORT      = "Don`t found any comport available";
        #endregion

        #region Synthesizers messages

        // TX (Down) synthesizer
        public static string MSG_SYNTH_OK_TX_FREQ_SENT                  = "Host: TX Freqancy send to unit";
        public static string MSG_SYNTH_ERR_TX_INPUT_F_RF_WRONG_RANGE    = "Please insert TX synthesizer F-RF between {0} and {1}";
        public static string MSG_SYNTH_ERR_TX_INPUT_F_IF_WRONG_RANGE    = "Please insert TX synthesizer F-IF between {0} and {1}";
        public static string MSG_SYNTH_ERR_TX_INPUT_WRONG_FORMAT        = "Please insert correct values to TX synthesizer F-IF and F_RF";
        public static string MSG_SYNTH_ERR_TX_SET_PER_BEFORE_READ_STATE = "Please read Synth TX values prioer to set operation state";

        // RX (Up) synthesizer
        public static string MSG_SYNTH_OK_RX_FREQ_SENT                  = "Host: RX Freqancy send to unit";
        public static string MSG_SYNTH_ERR_RX_INPUT_F_RF_WRONG_RANGE    = "Please insert RX synthesizer F-IF between {0} and {1}";
        public static string MSG_SYNTH_ERR_RX_INPUT_F_IF_WRONG_RANGE    = "Please insert RX synthesizer F-IF between {0} and {1}";
        public static string MSG_SYNTH_ERR_RX_INPUT_WRONG_FORMAT        = "Please insert correct values to RX synthesizer F-IF and F_RF";
        public static string MSG_SYNTH_ERR_RX_SET_PER_BEFORE_READ_STATE = "Please read Synth RX values prioer to set operation state";

        // Common synthesizers messages
        public static string MSG_SYNTH_ERR_NOT_CHOOSE_VALID_CP_VALUE    = "Please choose valid senthesizer CP value";
        // Common messages
        public static string MSG_SYNTH_OK_READ_STATUS_OK                = "MCU: Read synthesizer value ok"; 
        #endregion

        #region ADC messages

        public static string MSG_ADC_ERR_INPUT_WRONG_FORMAT             = "In ADC single mode please choose channel to sample";
        public static string MSG_ADC_ERR_INPUT_MISSING                  = "Please choose desire ADC sampling mode";
        #endregion

        #region DAC messages

        public static string MSG_DAC_ERR_DAC_NOT_SELECTED               = "Please choose desire dac";
        public static string MSG_DAC_ERR_VALUE_NOT_IN_RANGE             = "Dac input must be in range of (Vsource_minus {0} [mVdc],  Vref = {1} [mVdc]";
        public static string MSG_DAC_ERR_DAC_A_INPUT_WRONG_FORMAT       = "Please insert dac A value [{0} digits]";
        public static string MSG_DAC_ERR_DAC_B_INPUT_WRONG_FORMAT       = "Please insert dac B value [{0} digits]";
        public static string MSG_DAC_ERR_DAC_C_INPUT_WRONG_FORMAT       = "Please insert dac C value [{0} digits]";
        public static string MSG_DAC_ERR_DAC_D_INPUT_WRONG_FORMAT       = "Please insert dac D value [{0} digits]";
        public static string MSG_DAC_ERR_WRONG_INPUT_INDEX              = "Getting wrong dac index, please check serial communication";
        #endregion

        #region FLASH messages

        internal static string MSG_FLASH_ERR_DONT_VALID_NUM_SAMPLES_REQUEST = "Please insert valid samples number (higher then 0)";
        #endregion

        #region System logs

        internal static string MSG_LOG_ERR_CANT_OPEN_LOG    = "System: can`t open this log, check if you request valid log index";
        internal static string MSG_LOG_OK_OPEN_LOG          = "System: open log ok, Load in the log viewer";
        #endregion

        #endregion
    }
}
