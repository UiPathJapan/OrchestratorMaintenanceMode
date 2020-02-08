using System.Collections.Generic;
using Newtonsoft.Json;

namespace UiPathTeam.OrchestratorMaintenanceMode.Net
{
    internal class ErrorInfo
    {
        [JsonProperty("code")]
        public int? Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("details")]
        public string Details { get; set; }

        [JsonProperty("validationErrors")]
        public ICollection<ValidationErrorInfo> ValidationErrors { get; set; }

        public ErrorInfo()
        {
        }
    }
}
