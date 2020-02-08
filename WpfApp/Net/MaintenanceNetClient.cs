using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UiPathTeam.OrchestratorMaintenanceMode.Net
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
                    State = MaintenanceState.UNAVAILABLE;
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

        public object Response { get; private set; }

        public ErrorResponse ErrorResponse => Response as ErrorResponse;

        public MaintenanceSetting Maintenance => Response as MaintenanceSetting;

        public MaintenanceNetClient()
        {
            Url = string.Empty;
            _tenancyName = "host";
            _userName = "admin";
            _password = string.Empty;
            _credentials = null;
            _authValue = null;
            _requestUri = string.Empty;
            _cts = new CancellationTokenSource();
            ResetResponse();
        }

        private void ResetResponse()
        {
            ResponseMessage = null;
            ResponseBody = null;
            Response = null;
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
                var rspx = JsonConvert.DeserializeObject<AjaxResponse>(ResponseBody);
                AuthToken = rspx.Result;
                return true;
            }
            else
            {
                Response = JsonConvert.DeserializeObject<ErrorResponse>(ResponseBody);
                return false;
            }
        }

        public async Task<bool> Get(bool canRetry = true)
        {
            State = MaintenanceState.UNAVAILABLE;
            RequestUri = string.Format("{0}/api/Maintenance/Get", Url);
            var request = new HttpRequestMessage(HttpMethod.Get, RequestUri);
            request.Headers.Add(AUTHORIZATION, _authValue);
            ResponseMessage = await _httpClient.SendAsync(request, _cts.Token);
            ResponseBody = await ResponseMessage.Content.ReadAsStringAsync();
            switch (ResponseMessage.StatusCode)
            {
                case HttpStatusCode.OK:
                    Response = JsonConvert.DeserializeObject<MaintenanceSetting>(ResponseBody);
                    State = Maintenance.State;
                    return true;
                case HttpStatusCode.Unauthorized:
                    if (canRetry)
                    {
                        if (await Authenticate())
                        {
                            return await Get(false);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    break;
                default:
                    break;
            }
            Response = JsonConvert.DeserializeObject<ErrorResponse>(ResponseBody);
            return false;
        }

        public async Task<bool> StartDraining(bool canRetry = true)
        {
            RequestUri = string.Format("{0}/api/Maintenance/Start?phase=Draining", Url);
            var request = new HttpRequestMessage(HttpMethod.Post, RequestUri);
            request.Headers.Add(AUTHORIZATION, _authValue);
            ResponseMessage = await _httpClient.SendAsync(request, _cts.Token);
            ResponseBody = await ResponseMessage.Content.ReadAsStringAsync();
            switch (ResponseMessage.StatusCode)
            {
                case HttpStatusCode.NoContent:
                    return true;
                case HttpStatusCode.Unauthorized:
                    if (canRetry)
                    {
                        if (await Authenticate())
                        {
                            return await StartDraining(false);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    break;
                default:
                    break;
            }
            Response = JsonConvert.DeserializeObject<ErrorResponse>(ResponseBody);
            return false;
        }

        public async Task<bool> StartSuspended(bool isForceEnabled, bool isKillJobsEnabled, bool canRetry = true)
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
                    return true;
                case HttpStatusCode.Unauthorized:
                    if (canRetry)
                    {
                        if (await Authenticate())
                        {
                            return await StartSuspended(isForceEnabled, isKillJobsEnabled, false);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    break;
                default:
                    break;
            }
            Response = JsonConvert.DeserializeObject<ErrorResponse>(ResponseBody);
            return false;
        }

        public async Task<bool> End(bool canRetry = true)
        {
            RequestUri = string.Format("{0}/api/Maintenance/End", Url);
            var request = new HttpRequestMessage(HttpMethod.Post, RequestUri);
            request.Headers.Add(AUTHORIZATION, _authValue);
            ResponseMessage = await _httpClient.SendAsync(request, _cts.Token);
            ResponseBody = await ResponseMessage.Content.ReadAsStringAsync();
            switch (ResponseMessage.StatusCode)
            {
                case HttpStatusCode.NoContent:
                    return true;
                case HttpStatusCode.Unauthorized:
                    if (canRetry)
                    {
                        if (await Authenticate())
                        {
                            return await End(false);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    break;
                default:
                    break;
            }
            Response = JsonConvert.DeserializeObject<ErrorResponse>(ResponseBody);
            return false;
        }
    }
}
