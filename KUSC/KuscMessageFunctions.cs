using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUSC
{
    class KuscMessageFunctions
    {

        #region Class c`tor verbs
        
        KuscLogs _kuscLogs = new KuscLogs();
        KuscUtil _KuscUtil = new KuscUtil();
        private static string statusMsg = string.Empty;
        
        #endregion

        #region MCU control message group

        public static bool GroupControlMcu(KuscMessageParams.MESSAGE_REQUEST request, string data)
        {
            switch (request)
            {
                case KuscMessageParams.MESSAGE_REQUEST.CONTROL_SYSTEM_START:
                    KuscUtil.UpdateSystemRegisters();
                    statusMsg = "MCU: System init and start ok";
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.CONTROL_TEST_LEDS:
                    statusMsg = "MCU: Turn leds ok";
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.CONTROL_RESET_MCU:
                    statusMsg = "MCU: Reset MCU ok";
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.CONTROL_PA1_SET:
                    statusMsg = "MCU: Set PA1 ok";
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.CONTROL_PA2_SET:
                    statusMsg = "MCU: Set PA2 ok";
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.CONTROL_KEEP_ALIVE:
                    statusMsg = "MCU: System is running";
                    break;
            }
            KuscUtil.UpdateStatusOk(statusMsg);

            //KuscLogs.LogPrintCommand(statusMsg);

            return true;
        }
        #endregion

        #region MCU status and version group

        public static bool GroupStatusAndVersion(KuscMessageParams.MESSAGE_REQUEST request, string data)
        {
            switch (request)
            {
                case KuscMessageParams.MESSAGE_REQUEST.STATUS_GET_MCU_FW_VERSION:
                    KuscUtil.UpdateMcuFwVersion(data);
                    statusMsg = "MCU: Read MCU FW Version";
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.STATUS_MCU_RUN_TIME:
                    KuscUtil.UpdateRunTime(data);
                    statusMsg =  "MCU: Read MCU run-time OK";
                    break;
            }

            // Update status log and field:
            KuscUtil.UpdateStatusOk(statusMsg);

            return true;
        }
        #endregion

        #region MCU ADC group

        public static bool GroupAdc(KuscMessageParams.MESSAGE_REQUEST request, string data)
        {
            switch (request)
            {
                case KuscMessageParams.MESSAGE_REQUEST.ADC_OPERATION:
                    statusMsg = "MCU: Turn ADC ok";
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.ADC_CHANNEL_MODE:
                    statusMsg = "MCU: Set ADC set channel mode ok";
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.ADC_CONVERSION_MODE:
                    statusMsg = "MCU: Set ADC conversion mode ok";
                    break;
            }

            // Update status log and field:
            KuscUtil.UpdateStatusOk(statusMsg);

            return true;
        }
        #endregion

        #region MCU Synthesizers down / up

        public static bool GroupSynthesizers(KuscMessageParams.MESSAGE_REQUEST request, string data)
        {
            switch (request)
            {
                case KuscMessageParams.MESSAGE_REQUEST.SYNTH_DOWN_SET:
                    statusMsg = "System: Send syntesizer TX (Down) setting serial packet";
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.SYNTH_UP_SET:
                    statusMsg = "System: Send syntesizer RX (Up) setting serial packet";
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.SYNTH_UP_OPER:
                    KuscUtil.UpdateSynthUpOper();
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.SYNTH_DOWN_OPER:
                    KuscUtil.UpdateSynthDownOper();
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.SYNTH_UP_READ_DATA:
                    KuscUtil.ReadSynthUp(data);
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.SYNTH_DOWN_READ_DATA:
                    KuscUtil.ReadSynthDown(data);
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.SYNTH_REQ_ANTHER_TX_REG:
                    KuscUtil.ReqAntherTxRegister();
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.SYNTH_REQ_ANTHER_RX_REG:
                    KuscUtil.ReqAntherRxRegister();
                    break;

            }

            // Update status log and field:
            KuscUtil.UpdateStatusOk(statusMsg);

            return true;
        }
        #endregion

        #region MCU FLASH

        public static bool GroupFlashMemory(KuscMessageParams.MESSAGE_REQUEST request, string data)
        {
            switch (request)
            {
                case KuscMessageParams.MESSAGE_REQUEST.FLASH_EREASE_MEMORY:
                    statusMsg = "MCU: Erease flash memory ok";
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.FLASH_READ_CONDITION:
                    KuscUtil.UpdateFlashCondition(data);
                    statusMsg = "MCU: Read flash status ok";
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.FLASH_REQUEST_RAW_DATA:
                    statusMsg = "MCU: Request flash raw packet ok";
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.FLASH_SEND_RAW_DATA:
                    KuscUtil.UpdateAdcTable(data);
                    statusMsg = "MCU: Receive flash raw data packet ok";
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.FLASH_NO_SAMPLE_YET:
                    statusMsg = "MCU: You request number of samples that are bigger then actually MCU have";
                    break;
            }

            // Update status log and field:
            KuscUtil.UpdateStatusOk(statusMsg);

            return true;
        }
        #endregion

        #region MCU DAC

        public static bool GroupDAC(KuscMessageParams.MESSAGE_REQUEST request, string data)
        {
            switch (request)
            {
                case KuscMessageParams.MESSAGE_REQUEST.DAC_SET_VALUE:
                    statusMsg = "MCU: Set DAC value";
                    break;

                case KuscMessageParams.MESSAGE_REQUEST.DAC_READ_VALUE:
                    KuscUtil.DacReadValue(data);
                    break;
            }

            // Update status log and field:
            KuscUtil.UpdateStatusOk(statusMsg);

            return true;
        }
        #endregion
    }
}
