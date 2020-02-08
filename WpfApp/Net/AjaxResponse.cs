using Newtonsoft.Json;

namespace UiPathTeam.OrchestratorMaintenanceMode.Net
{
    internal class AjaxResponse
    {
        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("targetUrl")]
        public string TargetUrl { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error")]
        public ErrorInfo Error { get; set; }

        public AjaxResponse()
        {
        }
    }
}
