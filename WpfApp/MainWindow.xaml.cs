using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UiPathTeam.OrchestratorMaintenanceMode
{
    public partial class MainWindow : Window
    {
        private MaintenanceClient _client;

        public MainWindow()
        {
            InitializeComponent();
            _client = new MaintenanceClient();
            _client.PropertyChanged += OnPropertyChanged;
            DataContext = _client;
            passwordBox.Password = _client.Password;
            getButton.IsEnabled = true;
            startButton.IsEnabled = false;
            endButton.IsEnabled = false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _client.OnClose();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Password")
            {
                passwordBox.Password = _client.Password;
            }
        }

        private void UpdateButtons(bool enabled)
        {
            if (!enabled)
            {
                getButton.IsEnabled = false;
                startButton.IsEnabled = false;
                endButton.IsEnabled = false;
            }
            else if (_client.State == MaintenanceState.NONE)
            {
                getButton.IsEnabled = true;
                startButton.IsEnabled = true;
                endButton.IsEnabled = false;
            }
            else if (_client.State == MaintenanceState.DRAINING)
            {
                getButton.IsEnabled = true;
                startButton.IsEnabled = true;
                endButton.IsEnabled = true;
            }
            else if (_client.State == MaintenanceState.SUSPENDED)
            {
                getButton.IsEnabled = true;
                startButton.IsEnabled = false;
                endButton.IsEnabled = true;
            }
            else
            {
                getButton.IsEnabled = true;
                startButton.IsEnabled = false;
                endButton.IsEnabled = false;
            }
        }

        private async void SendGet(object sender, RoutedEventArgs e)
        {
            _client.UserName = usernameTextBox.Text;
            _client.Password = passwordBox.Password;
            UpdateButtons(false);
            await _client.Get();
            UpdateButtons(true);
        }

        private async void SendStart(object sender, RoutedEventArgs e)
        {
            _client.UserName = usernameTextBox.Text;
            _client.Password = passwordBox.Password;
            UpdateButtons(false);
            if (await _client.Get())
            {
                if (_client.State == MaintenanceState.NONE)
                {
                    if (await _client.StartDraining())
                    {
                        await _client.Get();
                    }
                }
                if (_client.State == MaintenanceState.DRAINING)
                {
                    if (await _client.StartSuspended())
                    {
                        await _client.Get();
                    }
                }
            }
            UpdateButtons(true);
        }

        private async void SendEnd(object sender, RoutedEventArgs e)
        {
            _client.UserName = usernameTextBox.Text;
            _client.Password = passwordBox.Password;
            UpdateButtons(false);
            if (await _client.Get())
            {
                if (_client.State == MaintenanceState.DRAINING || _client.State == MaintenanceState.SUSPENDED)
                {
                    if (await _client.End())
                    {
                        await _client.Get();
                    }
                }
            }
            UpdateButtons(true);
        }
    }
}
