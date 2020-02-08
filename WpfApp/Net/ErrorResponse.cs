using Newtonsoft.Json;

namespace UiPathTeam.OrchestratorMaintenanceMode.Net
{
    internal class ErrorResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("errorCode")]
        public int ErrorCode { get; set; }

        public ErrorResponse()
        {
        }
    }
}
