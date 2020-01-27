using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UiPathTeam.OrchestratorMaintenanceMode
{
    public class AuthenticateResponse
    {
        public class ValidationErrorInfo
        {
            public string Message { get; set; }

            public List<string> Members { get; } = new List<string>();
        }

        public class ErrorInfo
        {
            public int Code { get; set; }

            public string Message { get; set; }

            public string Details { get; set; }

            public List<ValidationErrorInfo> ValidationErrors { get; set; }
        }

        public string Result { get; set; }

        public string TargetUrl { get; set; }

        public bool Success { get; set; }

        public ErrorInfo Error { get; set; }

        public bool UnAuthorizedRequest { get; set; }
    }
}
