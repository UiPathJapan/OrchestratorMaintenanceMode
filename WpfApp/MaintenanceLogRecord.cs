using System;

namespace UiPathTeam.OrchestratorMaintenanceMode
{
    public class MaintenanceLogRecord
    {
        public MaintenanceState State { get; set; }

        public DateTime Timestamp { get; set; }

        public MaintenanceLogRecord(string state, DateTime timestamp)
        {
            State = MaintenanceStateConverter.Parse(state);
            Timestamp = timestamp;
        }
        public MaintenanceLogRecord(string state)
        {
            State = MaintenanceStateConverter.Parse(state);
            Timestamp = new DateTime();
        }
    }
}
