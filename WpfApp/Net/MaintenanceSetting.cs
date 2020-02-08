using System.Collections.Generic;
using Newtonsoft.Json;

namespace UiPathTeam.OrchestratorMaintenanceMode.Net
{
    internal class MaintenanceSetting
    {
        [JsonProperty("state")]
        public string StateString { get; set; }

        [JsonProperty("maintenanceLogs")]
        public ICollection<MaintenanceStateLog> MaintenanceLogs { get; set; }

        [JsonProperty("jobStopsAttempted")]
        public int? JobStopsAttempted { get; set; }

        [JsonProperty("jobKillsAttempted")]
        public int? JobKillsAttempted { get; set; }

        [JsonProperty("triggersSkipped")]
        public int? TriggersSkipped { get; set; }

        [JsonProperty("systemTriggersSkipped")]
        public int? SystemTriggersSkipped { get; set; }

        [JsonIgnore]
        public MaintenanceState State => MaintenanceStateConverter.Parse(StateString);

        public MaintenanceSetting()
        {
        }
    }
}
