using System.Collections.Generic;

namespace UiPathTeam.OrchestratorMaintenanceMode
{
    public class MaintenanceResponse
    {
        public MaintenanceState State { get; set; }

        public long JobStopsAttempted { get; set; }

        public long JobKillsAttempted { get; set; }

        public long TriggersSkipped { get; set; }

        public long SystemTriggersSkipped { get; set; }

        public IEnumerable<MaintenanceLogRecord> Logs { get; set; }
    }
}
