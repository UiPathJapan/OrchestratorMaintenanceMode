using System.Collections.Generic;
using Newtonsoft.Json;

namespace UiPathTeam.OrchestratorMaintenanceMode.Net
{
    internal class ValidationErrorInfo
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("members")]
        public ICollection<string> Members { get; set; }

        public ValidationErrorInfo()
        {
        }
    }
}
