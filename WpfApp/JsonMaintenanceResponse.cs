using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace UiPathTeam.OrchestratorMaintenanceMode
{
    public static class JsonMaintenanceResponse
    {
        public static MaintenanceResponse Parse(string value)
        {
            var root = JObject.Parse(value);
            return new MaintenanceResponse()
            {
                State = MaintenanceStateConverter.Parse((string)((JValue)root["state"]).Value),
                JobKillsAttempted = (long)((JValue)root["jobKillsAttempted"]).Value,
                JobStopsAttempted = (long)((JValue)root["jobStopsAttempted"]).Value,
                TriggersSkipped = (long)((JValue)root["triggersSkipped"]).Value,
                SystemTriggersSkipped = (long)((JValue)root["systemTriggersSkipped"]).Value,
                Logs = ParseMaintenanceLogs((JArray)root["maintenanceLogs"])
            };
        }

        private static List<MaintenanceLogRecord> ParseMaintenanceLogs(JArray logs)
        {
            if (logs != null)
            {
                var gg = new List<MaintenanceLogRecord>();
                foreach (JObject log in logs)
                {
                    if (((JValue)log["timeStamp"]).Value != null && ((JValue)log["timeStamp"]).Type == JTokenType.Date)
                        gg.Add(new MaintenanceLogRecord((string)((JValue)log["state"]).Value, ((DateTime)((JValue)log["timeStamp"])).ToLocalTime()));
                    else
                        gg.Add(new MaintenanceLogRecord((string)((JValue)log["state"]).Value));
                }
                return gg;
            }
            else
            {
                return null;
            }
        }
    }
}
