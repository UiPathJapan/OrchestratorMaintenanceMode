using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UiPathTeam.OrchestratorMaintenanceMode
{
    public class LogRecord
    {
        public DateTime Timestamp { get; set; }

        public string Message { get; set; }

        public LogRecord(MaintenanceLogRecord record)
        {
            Timestamp = record.Timestamp;
            Message = string.Format("Mode={0}", Enum.GetName(typeof(MaintenanceState), record.State));
        }

        public LogRecord(string message)
        {
            Timestamp = DateTime.Now;
            Message = message;
        }

        public LogRecord(string format, object arg0)
        {
            Timestamp = DateTime.Now;
            Message = string.Format(format, arg0);
        }

        public LogRecord(string format, object arg0, object arg1)
        {
            Timestamp = DateTime.Now;
            Message = string.Format(format, arg0, arg1);
        }

        public LogRecord(string format, object arg0, object arg1, object arg2)
        {
            Timestamp = DateTime.Now;
            Message = string.Format(format, arg0, arg1, arg2);
        }

        public LogRecord(string format, object arg0, object arg1, object arg2, object arg3)
        {
            Timestamp = DateTime.Now;
            Message = string.Format(format, arg0, arg1, arg2, arg3);
        }
    }
}
