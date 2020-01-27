using System;
using System.Globalization;
using System.Windows.Data;

namespace UiPathTeam.OrchestratorMaintenanceMode
{
    public class MaintenanceTimestampConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dt = (DateTime)value;
            if (dt.Ticks > 0)
            {
                return dt.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                return "Previously...";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
