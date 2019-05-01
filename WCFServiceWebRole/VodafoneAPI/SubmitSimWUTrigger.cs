using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace WCFServiceWebRole.VodafoneAPI
{
    public class SubmitSimWUTrigger
    {
        public static int Execute(ref Dictionary<string, string> returnedValuesMap, ref StringBuilder output, ref VodafoneM2M.gdspHeader headerVodafone, string simId)
        {
            VodafoneM2M.SubmitWUTriggerv31Client service = new VodafoneM2M.SubmitWUTriggerv31Client("SubmitSimWUTriggerHttps");

            VodafoneM2M.submitWUTriggerv3 parameters = new VodafoneM2M.submitWUTriggerv3();
            parameters.deviceId = simId;
            VodafoneM2M.submitWUTriggerv3Response result = null;
            result = service.submitWUTriggerv3(headerVodafone, parameters);

            if (result != null)
            {
                VodafoneM2M.wuTriggerv3Response respItem = result.@return;
                if (respItem != null)
                {
                    VodafoneM2M.returnCode rtnCode = respItem.returnCode;
                    if (rtnCode != null)
                    {

                        string majorCode = rtnCode.majorReturnCode;
                        string minorCode = rtnCode.minorReturnCode;

                        //Dictionary<string, string> returnedValuesMap = new Dictionary<string, string>();
                        string deviceId = respItem.deviceId;
                        string messageReference = respItem.messageReference;
                        string triggerType = respItem.triggerType;
                        returnedValuesMap["MinorCode"] = minorCode;
                        returnedValuesMap["MajorCode"] = majorCode;
                        returnedValuesMap["deviceId"] = deviceId;
                        returnedValuesMap["messageReference"] = messageReference;
                        returnedValuesMap["triggerType"] = triggerType;

                        var test = returnedValuesMap.GetEnumerator();
                        while (test.MoveNext())
                        {
                            KeyValuePair<string, string> o = test.Current;
                            output.Append(o.Key + " -> " + o.Value + ", ");
                        }
                    }
                }
                else
                {
                    output.Append("Fail: The returned SubmitSimWUTrigger is null." + "<br/>");
                    return 1;
                }
            }
            else
            {
                output.Append("Fail: The returned SubmitSimWUTriggerResponse is null." + "<br/>");
                return 1;
            }
            return 0;
        }
    }
}