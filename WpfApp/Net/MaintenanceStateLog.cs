using System;
using Newtonsoft.Json;

namespace UiPathTeam.OrchestratorMaintenanceMode.Net
{
    internal class MaintenanceStateLog
    {
        [JsonProperty("state")]
        public string StateString { get; set; }

        [JsonProperty("timeStamp")]
        public string TimestampString { get; set; }

        [JsonIgnore]
        public MaintenanceState State => MaintenanceStateConverter.Parse(StateString);

        [JsonIgnore]
        public DateTime Timestamp => TimestampString != null ? DateTime.Parse(TimestampString) : new DateTime(0);

        [JsonIgnore]
        public bool HasTimestamp => !string.IsNullOrEmpty(TimestampString);

        public MaintenanceStateLog()
        {
        }

        public MaintenanceStateLog(string state)
        {
            StateString = state;
            TimestampString = DateTime.Now.ToString();
        }
    }
}
