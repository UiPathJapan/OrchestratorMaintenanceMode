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

        private bool _buttonsEnabled;

        public MainWindow()
        {
            InitializeComponent();
            _client = new MaintenanceClient();
            _client.PropertyChanged += OnPropertyChanged;
            DataContext = _client;
            passwordBox.Password = _client.Password;
            drainingCheckBox.IsChecked = true;
            suspendedCheckBox.IsChecked = true;
            UpdateButtons(true);
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
            _buttonsEnabled = enabled;
            if (enabled)
            {
                getButton.IsEnabled = true;
                startButton.IsEnabled = drainingCheckBox.IsChecked == true | suspendedCheckBox.IsChecked == true;
                endButton.IsEnabled = true;
                cancelButton.IsEnabled = false;
            }
            else
            {
                getButton.IsEnabled = false;
                startButton.IsEnabled = false;
                endButton.IsEnabled = false;
                cancelButton.IsEnabled = true;
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
            if (drainingCheckBox.IsChecked == true)
            {
                if (await _client.StartDraining())
                {
                    if (suspendedCheckBox.IsChecked == true)
                    {
                        if (await _client.StartSuspended())
                        {
                            await _client.Get();
                        }
                    }
                    else
                    {
                        await _client.Get();
                    }
                }
            }
            else if (suspendedCheckBox.IsChecked == true)
            {
                if (await _client.StartSuspended())
                {
                    await _client.Get();
                }
            }
            UpdateButtons(true);
        }

        private async void SendEnd(object sender, RoutedEventArgs e)
        {
            _client.UserName = usernameTextBox.Text;
            _client.Password = passwordBox.Password;
            UpdateButtons(false);
            if (await _client.End())
            {
                await _client.Get();
            }
            UpdateButtons(true);
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            _client.Cancel();
        }

        private void DrainingChecked(object sender, RoutedEventArgs e)
        {
            if (_buttonsEnabled)
            {
                startButton.IsEnabled = true;
            }
        }

        private void DrainingUnchecked(object sender, RoutedEventArgs e)
        {
            if (_buttonsEnabled)
            {
                startButton.IsEnabled = suspendedCheckBox.IsChecked == true;
            }
        }

        private void SuspendedChecked(object sender, RoutedEventArgs e)
        {
            if (_buttonsEnabled)
            {
                startButton.IsEnabled = true;
            }
        }

        private void SuspendedUnchecked(object sender, RoutedEventArgs e)
        {
            if (_buttonsEnabled)
            {
                startButton.IsEnabled = drainingCheckBox.IsChecked == true;
            }
        }
    }
}
