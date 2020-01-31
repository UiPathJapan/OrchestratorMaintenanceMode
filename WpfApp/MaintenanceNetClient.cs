using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UiPathTeam.OrchestratorMaintenanceMode
{
    internal class MaintenanceNetClient
    {
        private static readonly string AUTHORIZATION = "Authorization";
        private static readonly string BEARER_SP = "Bearer ";
        private static readonly string APPLICATION_JSON = "application/json";

        private static readonly HttpClient _httpClient = new HttpClient();
        private string _tenancyName;
        private string _userName;
        private string _password;
        private string _credentials;
        private string _authValue;
        private string _requestUri;
        private MaintenanceResponse _maintenanceResponse;
        private CancellationTokenSource _cts;

        public string Url
        {
            get
            {
                return _requestUri;
            }
            set
            {
                _requestUri = value;
                ResetResponse();
            }
        }

        public string TenancyName
        {
            get
            {
                return _tenancyName;
            }
            set
            {
                _tenancyName = value;
                _credentials = null;
            }
        }

        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;
                _credentials = null;
            }
        }

        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                _credentials = null;
            }
        }

        public string AuthToken
        {
            get
            {
                return _authValue != null ? _authValue.Substring(BEARER_SP.Length) : null;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _authValue = null;
                    Maintenance = null;
                }
                else
                {
                    _authValue = BEARER_SP + value;
                }
            }
        }

        public bool HasAuthToken => _authValue != null;

        public MaintenanceState State { get; private set; }

        public string RequestUri { get; private set; }

        public HttpResponseMessage ResponseMessage { get; private set; }

        public string ResponseBody { get; private set; }

        public ErrorResponse ErrorResponse { get; private set; }

        public MaintenanceResponse Maintenance
        {
            get
            {
                return _maintenanceResponse;
            }
            private set
            {
                if (value != null)
                {
                    _maintenanceResponse = value;
                }
                else
                {
                    _maintenanceResponse = null;
                    State = MaintenanceState.UNAVAILABLE;
                }
            }
        }

        public MaintenanceNetClient()
        {
            Url = string.Empty;
            _tenancyName = "host";
            _userName = "admin";
            _password = string.Empty;
            _credentials = null;
            _authValue = null;
            _requestUri = string.Empty;
            Maintenance = null;
            _cts = new CancellationTokenSource();
            ResetResponse();
        }

        private void ResetResponse()
        {
            ResponseMessage = null;
            ResponseBody = null;
            ErrorResponse = null;
        }

        public void Cancel()
        {
            _cts.Cancel();
            _cts = new CancellationTokenSource();
        }

        public async Task<bool> Authenticate()
        {
            AuthToken = null;
            RequestUri = string.Format("{0}/api/Account", Url);
            var request = new HttpRequestMessage(HttpMethod.Post, RequestUri);
            if (_credentials == null)
            {
                _credentials = string.Format("{{\"tenancyName\":\"{0}\",\"usernameOrEmailAddress\":\"{1}\",\"password\":\"{2}\"}}", TenancyName, UserName, Password);
            }
            request.Content = new StringContent(_credentials, Encoding.UTF8, APPLICATION_JSON);
            ResponseMessage = await _httpClient.SendAsync(request, _cts.Token);
            ResponseBody = await ResponseMessage.Content.ReadAsStringAsync();
            if (ResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                var rspx = JsonAuthenticateResponse.Parse(ResponseBody);
                AuthToken = rspx.Result;
                return true;
            }
            else
            {
                ErrorResponse = JsonErrorResponse.Parse(ResponseBody);
                return false;
            }
        }

        public async Task<HttpStatusCode> Get()
        {
            Maintenance = null;
            RequestUri = string.Format("{0}/api/Maintenance/Get", Url);
            var request = new HttpRequestMessage(HttpMethod.Get, RequestUri);
            request.Headers.Add(AUTHORIZATION, _authValue);
            ResponseMessage = await _httpClient.SendAsync(request, _cts.Token);
            ResponseBody = await ResponseMessage.Content.ReadAsStringAsync();
            if (ResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                Maintenance = JsonMaintenanceResponse.Parse(ResponseBody);
                State = Maintenance.State;
            }
            else if (ResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
            {
            }
            else
            {
                ErrorResponse = JsonErrorResponse.Parse(ResponseBody);
            }
            return ResponseMessage.StatusCode;
        }

        public async Task<HttpStatusCode> StartDraining()
        {
            RequestUri = string.Format("{0}/api/Maintenance/Start?phase=Draining", Url);
            var request = new HttpRequestMessage(HttpMethod.Post, RequestUri);
            request.Headers.Add(AUTHORIZATION, _authValue);
            ResponseMessage = await _httpClient.SendAsync(request, _cts.Token);
            ResponseBody = await ResponseMessage.Content.ReadAsStringAsync();
            switch (ResponseMessage.StatusCode)
            {
                case HttpStatusCode.NoContent:
                case HttpStatusCode.Unauthorized:
                    break;
                default:
                    ErrorResponse = JsonErrorResponse.Parse(ResponseBody);
                    break;
            }
            return ResponseMessage.StatusCode;
        }

        public async Task<HttpStatusCode> StartSuspended(bool isForceEnabled, bool isKillJobsEnabled)
        {
            RequestUri = string.Format("{0}/api/Maintenance/Start?phase=Suspended", Url);
            if (isForceEnabled)
            {
                RequestUri += "&force=true";
            }
            if (isKillJobsEnabled)
            {
                RequestUri += "&killJobs=true";
            }
            var request = new HttpRequestMessage(HttpMethod.Post, RequestUri);
            request.Headers.Add(AUTHORIZATION, _authValue);
            ResponseMessage = await _httpClient.SendAsync(request, _cts.Token);
            ResponseBody = await ResponseMessage.Content.ReadAsStringAsync();
            switch (ResponseMessage.StatusCode)
            {
                case HttpStatusCode.NoContent:
                case HttpStatusCode.Unauthorized:
                    break;
                default:
                    ErrorResponse = JsonErrorResponse.Parse(ResponseBody);
                    break;
            }
            return ResponseMessage.StatusCode;
        }

        public async Task<HttpStatusCode> End()
        {
            RequestUri = string.Format("{0}/api/Maintenance/End", Url);
            var request = new HttpRequestMessage(HttpMethod.Post, RequestUri);
            request.Headers.Add(AUTHORIZATION, _authValue);
            ResponseMessage = await _httpClient.SendAsync(request, _cts.Token);
            ResponseBody = await ResponseMessage.Content.ReadAsStringAsync();
            switch (ResponseMessage.StatusCode)
            {
                case HttpStatusCode.NoContent:
                case HttpStatusCode.Unauthorized:
                    break;
                default:
                    ErrorResponse = JsonErrorResponse.Parse(ResponseBody);
                    break;
            }
            return ResponseMessage.StatusCode;
        }
    }
}
