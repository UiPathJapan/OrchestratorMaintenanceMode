using Newtonsoft.Json.Linq;

namespace UiPathTeam.OrchestratorMaintenanceMode
{
    public static class JsonErrorResponse
    {
        public static ErrorResponse Parse(string value)
        {
            var root = JObject.Parse(value);
            return new ErrorResponse()
            {
                Message = root["message"] != null ? (string)((JValue)root["message"]).Value : "",
                ErrorCode = root["errorCode"] != null ? (int)(long)((JValue)root["errorCode"]).Value : 0
            };
        }
    }
}
