using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UiPathTeam.OrchestratorMaintenanceMode.Net;

namespace UiPathTeam.OrchestratorMaintenanceMode
{
    internal class MaintenanceClient : INotifyPropertyChanged
    {
        #region FIELDS

        private MaintenanceNetClient _netClient = new MaintenanceNetClient();
        private PersistentStore _ps = new PersistentStore();
        private string _statusText;

        #endregion

        #region PROPERTIES

        public List<string> UrlList { get; set; }

        public string Url
        {
            get
            {
                return _netClient.Url;
            }
            set
            {
                if (_netClient.Url != value)
                {
                    _netClient.Url = value;
                    NotifyPropertyChanged(nameof(Url));
                    OnCredentialsChanged();
                    _ps.FindCredentials(_netClient.Url, (url, tenancyname, username, password, token) =>
                    {
                        _netClient.TenancyName = tenancyname;
                        _netClient.UserName = username;
                        _netClient.Password = password;
                        _netClient.AuthToken = token;
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
                return _netClient.TenancyName;
            }
            set
            {
                if (_netClient.TenancyName != value)
                {
                    _netClient.TenancyName = value;
                    OnCredentialsChanged();
                }
            }
        }

        public string UserName
        {
            get
            {
                return _netClient.UserName;
            }
            set
            {
                if (_netClient.UserName != value)
                {
                    _netClient.UserName = value;
                    OnCredentialsChanged();
                }
            }
        }

        public string Password
        {
            get
            {
                return _netClient.Password;
            }
            set
            {
                if (_netClient.Password != value)
                {
                    _netClient.Password = value;
                    OnCredentialsChanged();
                }
            }
        }

        public bool IsForceEnabled { get; set; }

        public bool IsKillJobsEnabled { get; set; }

        public MaintenanceState State => _netClient.State;

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
            IsForceEnabled = false;
            IsKillJobsEnabled = false;
            StatusText = string.Empty;
            Logs = new ObservableCollection<LogRecord>();
            _ps.Load();
            _ps.EnumUrls((url) => 
            {
                UrlList.Add(url);
            });
            _ps.FindLastCredentials((url, tenancyname, username, password, token) =>
            {
                _netClient.Url = url;
                _netClient.TenancyName = tenancyname;
                _netClient.UserName = username;
                _netClient.Password = password;
                _netClient.AuthToken = token;
            });
        }

        #region CALLBACKS

        public void OnClose()
        {
            _ps.Save();
        }

        private void OnCredentialsChanged()
        {
            _netClient.AuthToken = null;
            StatusText = string.Empty;
            Logs.Clear();
        }

        #endregion

        #region API CALLS

        public void Cancel()
        {
            _netClient.Cancel();
        }

        public async Task<bool> Authenticate()
        {
            var name = "Authentication";
            try
            {
                StatusRequesting(name);
                UserName = UserName.Trim();
                if (await _netClient.Authenticate())
                {
                    StatusSucceeded(name);
                    return true;
                }
                OnErrorResponse(name);
            }
            catch (Exception ex)
            {
                OnException(ex, name);
            }
            finally
            {
                UpdateCredentialsInPersistentStore();
            }
            return false;
        }

        public async Task<bool> Get(bool canRetry = true)
        {
            var name = "Get";
            if (_netClient.HasAuthToken)
            {
                try
                {
                    StatusRequesting(name);
                    if (await _netClient.Get())
                    {
                        StatusText = string.Format(
                            "Current Mode={0}\nJob: Stops attempted={1} Kills attempted={2}\nTriggers skipped={3}\nSystem triggers skipped={4}",
                            _netClient.Maintenance.StateString,
                            _netClient.Maintenance.JobStopsAttempted,
                            _netClient.Maintenance.JobKillsAttempted,
                            _netClient.Maintenance.TriggersSkipped,
                            _netClient.Maintenance.SystemTriggersSkipped);
                        foreach (var record in _netClient.Maintenance.MaintenanceLogs)
                        {
                            Logs.Add(new LogRecord(record));
                        }
                        return true;
                    }
                    OnErrorResponse(name);
                }
                catch (Exception ex)
                {
                    OnException(ex, name);
                }
                finally
                {
                    UpdateCredentialsInPersistentStore();
                }
            }
            else if (canRetry)
            {
                if (await Authenticate())
                {
                    return await Get(false);
                }
            }
            else
            {
                StatusFailed(name);
            }
            return false;
        }

        public async Task<bool> StartDraining(bool canRetry = true)
        {
            var name = "Start(Draining)";
            if (_netClient.HasAuthToken)
            {
                try
                {
                    StatusRequesting(name);
                    if (await _netClient.StartDraining())
                    {
                        StatusSucceeded(name);
                        return true;
                    }
                    OnErrorResponse(name);
                }
                catch (Exception ex)
                {
                    OnException(ex, name);
                }
            }
            else if (canRetry)
            {
                if (await Authenticate())
                {
                    return await StartDraining(false);
                }
            }
            else
            {
                StatusFailed(name);
            }
            return false;
        }

        public async Task<bool> StartSuspended(bool canRetry = true)
        {
            var name = "Start(Suspended)";
            if (_netClient.HasAuthToken)
            {
                try
                {
                    if (IsForceEnabled)
                    {
                        name = name.Replace(")", "/Force)");
                    }
                    if (IsKillJobsEnabled)
                    {
                        name = name.Replace(")", "/KillJobs)");
                    }
                    StatusRequesting(name);
                    if (await _netClient.StartSuspended(IsForceEnabled, IsKillJobsEnabled))
                    {
                        StatusSucceeded(name);
                        return true;
                    }
                    OnErrorResponse(name);
                }
                catch (Exception ex)
                {
                    OnException(ex, name);
                }
            }
            else if (canRetry)
            {
                if (await Authenticate())
                {
                    return await StartSuspended(false);
                }
            }
            else
            {
                StatusFailed(name);
            }
            return false;
        }

        public async Task<bool> End(bool canRetry = true)
        {
            var name = "End";
            if (_netClient.HasAuthToken)
            {
                try
                {
                    StatusRequesting(name);
                    if (await _netClient.End())
                    {
                        StatusSucceeded(name);
                        return true;
                    }
                    OnErrorResponse(name);
                }
                catch (Exception ex)
                {
                    OnException(ex, name);
                }
            }
            else if (canRetry)
            {
                if (await Authenticate())
                {
                    return await End(false);
                }
            }
            else
            {
                StatusFailed(name);
            }
            return false;
        }

        private void UpdateCredentialsInPersistentStore()
        {
            _ps.UpdateCredentials(_netClient.Url, _netClient.TenancyName, _netClient.UserName, _netClient.Password, _netClient.AuthToken);
            if (!UrlList.Contains(_netClient.Url))
            {
                UrlList.Add(_netClient.Url);
                NotifyPropertyChanged(nameof(UrlList));
            }
        }

        #endregion

        #region DISPLAY HELPERS

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnException(Exception ex, string name)
        {
            StatusFailed(name);
            var sb = new StringBuilder();
            sb.Append(ex.Message);
            for (ex = ex.InnerException; ex != null; ex = ex.InnerException)
            {
                sb.AppendLine();
                sb.Append(ex.Message);
            }
            if (_netClient.ResponseBody != null)
            {
                sb.AppendLine();
                sb.AppendFormat("Response={0}", _netClient.ResponseBody);
            }
            Logs.Add(new LogRecord(sb.ToString()));
        }

        private void OnErrorResponse(string name)
        {
            StatusFailed(name);
            var sb = new StringBuilder();
            sb.AppendFormat("Status={0} ({1})", (int)_netClient.ResponseMessage.StatusCode, _netClient.ResponseMessage.ReasonPhrase);
            if (_netClient.ErrorResponse != null)
            {
                sb.AppendLine();
                sb.AppendFormat("ErrorCode={0}", _netClient.ErrorResponse.ErrorCode);
                sb.AppendLine();
                sb.AppendFormat("Message={0}", _netClient.ErrorResponse.Message);
            }
            else if (_netClient.ResponseBody != null)
            {
                sb.AppendLine();
                sb.AppendFormat("Response={0}", _netClient.ResponseBody);
            }
            Logs.Add(new LogRecord(sb.ToString()));
        }

        private void StatusRequesting(string name)
        {
            StatusText = string.Format("Requesting {0}...", name);
            Logs.Clear();
        }

        private void StatusSucceeded(string name)
        {
            StatusText = string.Format("{0} succeeded.", name);
        }

        private void StatusFailed(string name)
        {
            StatusText = string.Format("{0} failed.", name);
        }

        #endregion
    }
}
