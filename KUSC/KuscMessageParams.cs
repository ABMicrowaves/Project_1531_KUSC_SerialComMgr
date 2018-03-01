﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUSC
{
    class KuscMessageParams
    {


        #region Uart messages

        public static char MSG_MAGIC_A                      = '$';
        public static int RX_SLEEP_TIME_MSEC                = 10;
        public static int MIN_RX_MSG_SIZE                   = 5;
        public static int MSG_MAGIC_LOCATION                = 0;
        public static int MSG_GROUP_LOCATION                = 1;
        public static int MSG_REQUEST_LOCATION              = 2;
        public static int MSG_REQUEST_DATA_SIZE_LOCATION    = 3;
        public static int MSG_REQUEST_DATA_LOCATION         = 4;

        public static int MSG_RX_BUFFER_SIZE                = 50;

        public enum MESSAGE_GROUP : int
        {
            CONTROL_MSG             = 0x01,
            MCU_STATUS_VERSION_MSG  = 0x02,
            ADC_MSG                 = 0x03,
            SYNTH_MSG               = 0x04,
            FLASH                   = 0x05,
            DAC                     = 0x06,
        };
        #endregion

        #region Sub messages

        public enum MESSAGE_REQUEST : int
        {
            // Control MCU:
            CONTROL_SYSTEM_START        = 0x10,
            CONTROL_RESET_MCU           = 0x11,
            CONTROL_RESET_CPLD          = 0x12,
            CONTROL_PA1_SET             = 0x13,
            CONTROL_PA2_SET             = 0x14,
            CONTROL_TEST_LEDS           = 0x15,
            CONTROL_KEEP_ALIVE          = 0x16,

            // MCU status and version:
            STATUS_MCU_RUN_TIME         = 0x21,
            STATUS_GET_MCU_FW_VERSION   = 0x22,
            STATUS_GET_CPLD_VERSION     = 0x23,
            STATUS_SET_MCU_FW_VERSION   = 0x24,
            STATUS_SET_CPLD_VERSION     = 0x25,

            // ADC
            ADC_OPERATION               = 0x31,
            ADC_CHANNEL_MODE            = 0x32,
            ADC_CONVERSION_MODE         = 0x33,

            // Synthesizer (Up / Down):
            SYNTH_TX_INIT_SET           = 0x40,
            SYNTH_RX_INIT_SET           = 0x41,
            SYNTH_DOWN_SET              = 0x42,
            SYNTH_UP_SET                = 0x43,

            // Flash memory
            FLASH_EREASE_MEMORY         = 0x51,
            FLASH_READ_CONDITION        = 0x52,
            FLASH_REQUEST_RAW_DATA      = 0x53,
            FLASH_SEND_RAW_DATA         = 0x54,
            FLASH_NO_SAMPLE_YET         = 0x55,

            //DAC
            DAC_SET_VALUE               = 0x61,
        }

        #endregion
    }
}
