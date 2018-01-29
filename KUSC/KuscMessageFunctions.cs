using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUSC
{
    class KuscMessageFunctions
    {
        #region MCU control message group

        public static bool GroupControlMcu(KuscMessageParams.MESSAGE_REQUEST request, string data)
        {
            switch (request)
            {
                case KuscMessageParams.MESSAGE_REQUEST.CONTROL_TEST_LEDS:
                    KuscUtil.UpdateStatusOk("MCU: Turn leds ok");
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.CONTROL_RESET_MCU:
                    KuscUtil.UpdateStatusOk("MCU: Reset MCU ok");
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.CONTROL_RESET_CPLD:
                    KuscUtil.UpdateStatusOk("MCU: Reset CPLD ok");
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.CONTROL_PA1_SET:
                    KuscUtil.UpdateStatusOk("MCU: Set PA1 ok");
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.CONTROL_PA2_SET:
                    KuscUtil.UpdateStatusOk("MCU: Set PA2 ok");
                    break;
            }
            return true;
        }
        #endregion

        #region MCU status and version group

        public static bool GroupStatusAndVersion(KuscMessageParams.MESSAGE_REQUEST request, string data)
        {
            switch (request)
            {
                case KuscMessageParams.MESSAGE_REQUEST.STATUS_MCU_FW_VERSION:
                    KuscUtil.UpdateStatusOk("MCU: Read MCU FW Version");
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.STATUS_MCU_CPLD_VERSION:
                    KuscUtil.UpdateStatusOk("MCU: Read CPLD FW Version");
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.STATUS_MCU_RUN_TIME:
                    KuscUtil.UpdateStatusOk("MCU: Read MCU run-time OK");
                    break;
            }
            return true;
        }
        #endregion

        #region MCU ADC group

        public static bool GroupAdc(KuscMessageParams.MESSAGE_REQUEST request, string data)
        {
            switch (request)
            {
                case KuscMessageParams.MESSAGE_REQUEST.ADC_ENABLE:
                    KuscUtil.UpdateStatusOk("MCU: Turn ADC ok");
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.ADC_CHANNEL_SINGLE_MODE:
                    KuscUtil.UpdateStatusOk("MCU: Set ADC single channel mode ok");
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.ADC_CHANNEL_CIRC_MODE:
                    KuscUtil.UpdateStatusOk("MCU: Set ADC circular channel mode ok");
                    break;
            }
            return true;
        }
        #endregion

        #region MCU Synthesizers down / up

        public static bool GroupSynthesizers(KuscMessageParams.MESSAGE_REQUEST request, string data)
        {
            switch (request)
            {
                case KuscMessageParams.MESSAGE_REQUEST.SYNTH_DOWN_SET:
                    KuscUtil.UpdateStatusOk("MCU: Set syntesizer down ok");
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.SYNTH_UP_SET:
                    KuscUtil.UpdateStatusOk("MCU: Set syntesizer up ok");
                    break;
            }
            return true;
        }
        #endregion

        #region MCU FLASH

        public static bool GroupFlashMemory(KuscMessageParams.MESSAGE_REQUEST request, string data)
        {
            switch (request)
            {
                case KuscMessageParams.MESSAGE_REQUEST.FLASH_EREASE_MEMORY:
                    KuscUtil.UpdateStatusOk("MCU: Erease flash memory ok");
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.FLASH_READ_CONDITION:
                    KuscUtil.UpdateStatusOk("MCU: Read flash status ok");
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.FLASH_READ_RAW_DATA:
                    KuscUtil.UpdateStatusOk("MCU: Read flash raw data ok");
                    break;
            }
            return true;
        }
        #endregion

        #region MCU DAC

        public static bool GroupDAC(KuscMessageParams.MESSAGE_REQUEST request, string data)
        {
            switch (request)
            {
                case KuscMessageParams.MESSAGE_REQUEST.DAC_SET_VALUE:
                    KuscUtil.UpdateStatusOk("MCU: Set DAC value");
                    break;
            }
            return true;
        }
        #endregion
    }
}
