using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace UiPathTeam.OrchestratorMaintenanceMode
{
    public class MaintenanceClient : IDisposable, INotifyPropertyChanged
    {
        #region FIELDS

        private HttpClient _httpClient = new HttpClient();
        private PersistentStore _ps = new PersistentStore();
        private string _url;
        private string _tenancyName;
        private string _userName;
        private string _password;
        private string _authToken;
        private string _statusText;

        #endregion

        #region PROPERTIES

        public List<string> UrlList { get; set; }

        public string Url
        {
            get
            {
                return _url;
            }
            set
            {
                if (_url != value)
                {
                    _url = value;
                    NotifyPropertyChanged(nameof(Url));
                    OnCredentialsChanged();
                    _ps.FindCredentials(_url, (url, tenancyname, username, password, token) =>
                    {
                        _tenancyName = tenancyname;
                        _userName = username;
                        _password = password;
                        _authToken = string.IsNullOrEmpty(token) ? null : token;
                        NotifyPropertyChanged(nameof(TenancyName));
                        NotifyPropertyChanged(nameof(UserName));
                        NotifyPropertyChanged(nameof(Password));
                    });
                }
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
                if (_tenancyName != value)
                {
                    _tenancyName = value;
                    OnCredentialsChanged();
                }
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
                if (_userName != value)
                {
                    _userName = value;
                    OnCredentialsChanged();
                }
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
                if (_password != value)
                {
                    _password = value;
                    OnCredentialsChanged();
                }
            }
        }

        public bool IsForceEnabled { get; set; }

        public bool IsKillJobsEnabled { get; set; }

        public MaintenanceState State { get; private set; }

        public string StatusText
        {
            get
            {
                return _statusText;
            }
            set
            {
                _statusText = value;
                NotifyPropertyChanged(nameof(StatusText));
            }
        }

        public ObservableCollection<LogRecord> Logs { get; }

        #endregion

        public MaintenanceClient()
        {
            UrlList = new List<string>();
            _url = string.Empty;
            _tenancyName = "host";
            _userName = "admin";
            _password = string.Empty;
            IsForceEnabled = false;
            IsKillJobsEnabled = false;
            _authToken = null;
            State = MaintenanceState.UNAVAILABLE;
            StatusText = string.Empty;
            Logs = new ObservableCollection<LogRecord>();
            _ps.Load();
            _ps.EnumUrls((url) => 
            {
                UrlList.Add(url);
            });
            _ps.FindLastCredentials((url, tenancyname, username, password, token) =>
            {
                _url = url;
                _tenancyName = tenancyname;
                _userName = username;
                _password = password;
                _authToken = string.IsNullOrEmpty(token) ? null : token;
            });
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        #region CALLBACKS

        public void OnClose()
        {
            _ps.Save();
        }

        private void OnCredentialsChanged()
        {
            _authToken = null;
            State = MaintenanceState.UNAVAILABLE;
            StatusText = string.Empty;
            Logs.Clear();
        }

        #endregion

        #region API CALLS

        public async Task<bool> Authenticate()
        {
            try
            {
                StatusText = "Authenticating...";
                Logs.Clear();
                UserName = UserName.Trim();
                _authToken = null;
                State = MaintenanceState.UNAVAILABLE;
                var uri = string.Format("{0}/api/Account", Url);
                var json = string.Format("{{\"tenancyName\":\"{0}\",\"usernameOrEmailAddress\":\"{1}\",\"password\":\"{2}\"}}", TenancyName, UserName, Password);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(uri, content);
                var responseBody = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    try
                    {
                        var rspx = JsonAuthenticateResponse.Parse(responseBody);
                        _authToken = rspx.Result;
                        StatusText = "Authentication succeeded.";
                    }
                    catch (Exception ex)
                    {
                        OnException(ex, "Authentication succeeded: Unexpected response.", responseBody);
                    }
                    AddCredentials();
                    return true;
                }
                else
                {
                    try
                    {
                        var rspx = JsonErrorResponse.Parse(responseBody);
                        OnErrorResponse(response, rspx, "Authentication failed.");
                    }
                    catch (Exception ex)
                    {
                        OnException(ex, "Authentication failed: Unexpected response.", responseBody);
                    }
                    AddCredentials();
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnException(ex, "Authentication failed.");
                AddCredentials();
                return false;
            }
        }

        private void AddCredentials()
        {
            _ps.AddCredentials(Url, TenancyName, UserName, Password, _authToken);
            if (!UrlList.Contains(Url))
            {
                UrlList.Add(Url);
                NotifyPropertyChanged(nameof(UrlList));
            }
        }

        public async Task<bool> Get(bool authIfNeeded = true)
        {
            if (_authToken == null)
            {
                if (!authIfNeeded)
                {
                    throw new InvalidOperationException("AuthToken=null authIfNeeded=false");
                }
                if (await Authenticate())
                {
                    authIfNeeded = false;
                }
                else
                {
                    return false;
                }
            }
            try
            {
                StatusText = "Requesting Get...";
                Logs.Clear();
                State = MaintenanceState.UNAVAILABLE;
                var uri = string.Format("{0}/api/Maintenance/Get", Url);
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Add(@"Authorization", "Bearer " + _authToken);
                var response = await _httpClient.SendAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var maintenance = JsonMaintenanceResponse.Parse(await response.Content.ReadAsStringAsync());
                        State = maintenance.State;
                        StatusText = string.Format(
                            "Current Mode={0}\nJob: Stops attempted={1} Kills attempted={2}\nTriggers skipped={3}\nSystem triggers skipped={4}",
                            Enum.GetName(typeof(MaintenanceState), maintenance.State),
                            maintenance.JobStopsAttempted,
                            maintenance.JobKillsAttempted,
                            maintenance.TriggersSkipped,
                            maintenance.SystemTriggersSkipped);
                        foreach (var record in maintenance.Logs)
                        {
                            Logs.Add(new LogRecord(record));
                        }
                    }
                    catch (Exception ex)
                    {
                        OnException(ex, "Get succeeded: Unexpected response.", responseBody);
                    }
                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && authIfNeeded)
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
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var rspx = JsonErrorResponse.Parse(responseBody);
                        OnErrorResponse(response, rspx, "Get failed.");
                    }
                    catch (Exception ex)
                    {
                        OnException(ex, "Get failed: Unexpected response.", responseBody);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnException(ex, "Get failed.");
                return false;
            }
        }

        public async Task<bool> Start(bool authIfNeeded = true)
        {
            if (_authToken == null)
            {
                if (!authIfNeeded)
                {
                    throw new InvalidOperationException("AuthToken=null authIfNeeded=false");
                }
                if (await Authenticate())
                {
                    authIfNeeded = false;
                }
                else
                {
                    return false;
                }
            }
            try
            {
                var uri = string.Format("{0}/api/Maintenance/Start", Url);
                if (State == MaintenanceState.DRAINING)
                {
                    uri += "?phase=Suspended";
                    StatusText = "Requesting Start phase=Suspended...";
                    if (IsForceEnabled)
                    {
                        uri += "&force=true";
                        StatusText = StatusText.Replace("...", " force=true...");
                    }
                    if (IsKillJobsEnabled)
                    {
                        uri += "&killJobs=true";
                        StatusText = StatusText.Replace("...", " killJobs=true...");
                    }
                }
                else
                {
                    uri += "?phase=Draining";
                    StatusText = "Requesting Start phase=Draining...";
                }
                var request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Headers.Add(@"Authorization", "Bearer " + _authToken);
                var response = await _httpClient.SendAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    StatusText = string.Format("Start phase={0} succeeded.", State == MaintenanceState.DRAINING ? "Suspended" : "Draining");
                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && authIfNeeded)
                {
                    if (!await Authenticate())
                    {
                        return false;
                    }
                    else if (!await Get(false))
                    {
                        return false;
                    }
                    else if (State == MaintenanceState.NONE || State == MaintenanceState.DRAINING)
                    {
                        return await Start(false);
                    }
                    else if (State == MaintenanceState.SUSPENDED)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var rspx = JsonErrorResponse.Parse(responseBody);
                        OnErrorResponse(response, rspx, string.Format("Start phase={0} failed.", State == MaintenanceState.DRAINING ? "Suspended" : "Draining"));
                    }
                    catch (Exception ex)
                    {
                        OnException(ex, string.Format("Start phase={0} failed: Unexpected response.", State == MaintenanceState.DRAINING ? "Suspended" : "Draining"), responseBody);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnException(ex, string.Format("Start phase={0} failed.", State == MaintenanceState.DRAINING ? "Suspended" : "Draining"));
                return false;
            }
        }

        public async Task<bool> End(bool authIfNeeded = true)
        {
            if (_authToken == null)
            {
                if (!authIfNeeded)
                {
                    throw new InvalidOperationException("AuthToken=null authIfNeeded=false");
                }
                if (await Authenticate())
                {
                    authIfNeeded = false;
                }
                else
                {
                    return false;
                }
            }
            try
            {
                StatusText = "Requesting End...";
                var uri = string.Format("{0}/api/Maintenance/End", Url);
                var request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Headers.Add(@"Authorization", "Bearer " + _authToken);
                var response = await _httpClient.SendAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    StatusText = "End succeeded.";
                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && authIfNeeded)
                {
                    if (!await Authenticate())
                    {
                        return false;
                    }
                    else if (!await Get(false))
                    {
                        return false;
                    }
                    else if (State == MaintenanceState.NONE)
                    {
                        return true;
                    }
                    else if (State == MaintenanceState.DRAINING || State == MaintenanceState.SUSPENDED)
                    {
                        return await End(false);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var rspx = JsonErrorResponse.Parse(responseBody);
                        OnErrorResponse(response, rspx, "End failed.");
                    }
                    catch (Exception ex)
                    {
                        OnException(ex, "End failed: Unexpected response.", responseBody);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnException(ex, "End failed.");
                return false;
            }
        }

        #endregion

        #region DISPLAY HELPERS

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnException(Exception ex, string statusText, string responseBody = null)
        {
            StatusText = statusText;
            var sb = new StringBuilder();
            sb.Append(ex.Message);
            for (ex = ex.InnerException; ex != null; ex = ex.InnerException)
            {
                sb.AppendLine();
                sb.Append(ex.Message);
            }
            if (responseBody != null)
            {
                sb.AppendLine();
                sb.AppendFormat("Response={0}", responseBody);
            }
            Logs.Add(new LogRecord(sb.ToString()));
        }

        private void OnErrorResponse(HttpResponseMessage rsp, ErrorResponse rspx, string statusText)
        {
            StatusText = statusText;
            Logs.Add(new LogRecord("Status={0} ({1})\nErrorCode={2}\nMessage={3}",
                (int)rsp.StatusCode, rsp.ReasonPhrase, rspx.ErrorCode, rspx.Message));
        }

        #endregion
    }
}
