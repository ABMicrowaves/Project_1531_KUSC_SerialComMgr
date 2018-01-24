using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUSC
{
    class KuscMessageParams
    {


        #region Uart messages

        public static char MSG_MAGIC_A = '$';

        public enum MESSAGE_GROUP : int
        {
            CONTROL_MSG = 0x01,
            MCU_STATUS_VERSION_MSG = 0x02,
            ADC_MSG = 0x03,
            SYNTH_MSG = 0x04,
            FLASH = 0x05,
            DAC = 0x06,
        };
        #endregion

        #region Sub messages

        public enum MESSAGE_REQUEST : int
        {
            // Control MCU:
            CONTROL_RESET_MCU = 0x10,
            CONTROL_RESET_CPLD = 0x11,
            CONTROL_PA1_ENABLE = 0x12,
            CONTROL_PA1_SET_POWER = 0x12,
            CONTROL_PA2_ENABLE = 0x13,
            CONTROL_PA2_SET_POWER = 0x14,
            CONTROL_TEST_LEDS = 0x15,

            // MCU status and version:
            STATUS_MCU_RUN_TIME = 0x21,
            STATUS_MCU_FW_VERSION = 0x22,
            STATUS_MCU_CPLD_VERSION = 0x23,

            // ADC
            ADC_ENABLE = 0x31,
            ADC_CHANNEL_SELECT_MODE = 0x32,
            ADC_CONVERSION_RESULT_FORMAT,

        }
        
        #endregion
    }
}
