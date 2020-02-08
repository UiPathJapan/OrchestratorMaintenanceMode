namespace UiPathTeam.OrchestratorMaintenanceMode.Net
{
    public enum MaintenanceState
    {
        NONE,
        DRAINING,
        SUSPENDED,
        UNKNOWN,
        UNAVAILABLE
    }

    public static class MaintenanceStateConverter
    {
        public static MaintenanceState Parse(string value)
        {
            value = value.ToLowerInvariant();
            if (value == "draining")
            {
                return MaintenanceState.DRAINING;
            }
            else if (value == "suspended")
            {
                return MaintenanceState.SUSPENDED;
            }
            else if (value == "none")
            {
                return MaintenanceState.NONE;
            }
            else
            {
                return MaintenanceState.UNKNOWN;
            }
        }
    }
}
