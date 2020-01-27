namespace UiPathTeam.OrchestratorMaintenanceMode
{
    public enum MaintenanceState
    {
        NONE,
        DRAINING,
        SUSPENDED,
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
            else
            {
                return MaintenanceState.NONE;
            }
        }
    }
}
