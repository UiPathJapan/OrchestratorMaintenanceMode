using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace UiPathTeam.OrchestratorMaintenanceMode
{
    public static class JsonAuthenticateResponse
    {
        public static AuthenticateResponse Parse(string value)
        {
            var root = JObject.Parse(value);
            return new AuthenticateResponse()
            {
                Result = (string)((JValue)root["result"]).Value,
                TargetUrl = (string)((JValue)root["targetUrl"]).Value,
                Success = (bool)((JValue)root["success"]).Value,
                Error = ParseError(((JValue)root["error"]).Value)
            };
        }

        private static AuthenticateResponse.ErrorInfo ParseError(object obj)
        {
            return obj != null
                ? new AuthenticateResponse.ErrorInfo()
                    {
                        Code = (int)(long)((JValue)((JObject)obj)["code"]).Value,
                        Message = (string)((JValue)((JObject)obj)["message"]).Value,
                        Details = (string)((JValue)((JObject)obj)["details"]).Value,
                        ValidationErrors = ParseValidationErrors(((JValue)(((JObject)obj)["validationErrors"])).Value)
                    }
                : null;
        }

        private static List<AuthenticateResponse.ValidationErrorInfo> ParseValidationErrors(object obj)
        {
            if (obj != null)
            {
                var vv = new List<AuthenticateResponse.ValidationErrorInfo>();
                foreach (JObject obj1 in (JArray)obj)
                {
                    var v = new AuthenticateResponse.ValidationErrorInfo()
                    {
                        Message = (string)((JValue)obj1["message"]).Value
                    };
                    foreach (JValue obj2 in (JArray)obj1["members"])
                    {
                        v.Members.Add((string)obj2.Value);
                    }
                    vv.Add(v);
                }
                return vv;
            }
            else
            {
                return null;
            }
        }
    }
}
