namespace CakeTubeSdk.Demo.ViewModel.Control
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Windows.Threading;

    using CakeTubeSdk.Core;
    using CakeTubeSdk.Core.ApiParameters;
    using CakeTubeSdk.Core.Infrastructure;
    using CakeTubeSdk.Core.Services;
    using CakeTubeSdk.Core.Storage;
    using CakeTubeSdk.Core.Vpn;
    using CakeTubeSdk.Demo.Helper;
    using CakeTubeSdk.Demo.Logger;
    using CakeTubeSdk.Windows;
    using CakeTubeSdk.Windows.Vpn;

    using Prism.Commands;
    using Prism.Mvvm;

    /// <summary>
    /// Main screen view model.
    /// </summary>
    public class MainScreenViewModel : BindableBase
    {
        /// <summary>
        /// Machine GUID from registry.
        /// </summary>
        private static readonly string MachineId;

        /// <summary>
        /// CakeTube VPN server service instance.
        /// </summary>
        private IVpnServerService vpnServerService;

        /// <summary>
        /// CakeTube VPN connection service instance.
        /// </summary>
        private IVpnConnectionService vpnConnectionService;

        private VpnServiceInfoStorage vpnServiceInfoStorage;
        
        private VpnConnectionInfo vpnConnectionInfo;

        private VpnWindowsServiceHandler vpnWindowsServiceHandler;

        /// <summary>
        /// Device id for backend login method.
        /// </summary>
        private string deviceId;

        /// <summary>
        /// Carrier id for backend service.
        /// </summary>
        private string carrierId;

        /// <summary>
        /// Backend url for backend service.
        /// </summary>
        private string backendUrl;

        /// <summary>
        /// Country for backend get credentials method.
        /// </summary>
        private string country;

        /// <summary>
        /// Message which is displayed in case of errors.
        /// </summary>
        private string errorText;

        /// <summary>
        /// Access token for backend methods.
        /// </summary>
        private string accessToken;

        /// <summary>
        /// User password for VPN.
        /// </summary>
        private string password;

        /// <summary>
        /// VPN service IP address.
        /// </summary>
        private string vpnIpServerServer;

        /// <summary>
        /// VPN service IP address.
        /// </summary>
        private string vpnIp;

        /// <summary>
        /// Received bytes count.
        /// </summary>
        private string bytesReceived;

        /// <summary>
        /// Sent bytes count.
        /// </summary>
        private string bytesSent;

        /// <summary>
        /// VPN connection status.
        /// </summary>
        private string status;

        /// <summary>
        /// Remaining traffic response.
        /// </summary>
        private string remainingTrafficResponse;

        /// <summary>
        /// Error visibility flag.
        /// </summary>
        private bool isErrorVisible;

        /// <summary>
        /// Connect button visibility flag.
        /// </summary>
        private bool isConnectButtonVisible;

        /// <summary>
        /// Disconnect button visibility flag.
        /// </summary>
        private bool isDisconnectButtonVisible;

        /// <summary>
        /// Connect command.
        /// </summary>
        private ICommand connectCommand;

        /// <summary>
        /// Disconnect command.
        /// </summary>
        private ICommand disconnectCommand;

        /// <summary>
        /// Clear log command.
        /// </summary>
        private ICommand clearLogCommand;

        /// <summary>
        /// Timer to update remaining traffic information.
        /// </summary>
        private DispatcherTimer dispatcherTimer;

        /// <summary>
        /// Use service flag.
        /// </summary>
        private bool useService = true;

        /// <summary>
        /// Name of windows service to use to establish VPN connection.
        /// </summary>
        private string serviceName = "CakeTube Demo Vpn Service";

        /// <summary>
        /// Log contents.
        /// </summary>
        private string logContents;

        /// <summary>
        /// Countries list.
        /// </summary>
        private IEnumerable<string> countriesList;

        /// <summary>
        /// Login command.
        /// </summary>
        private ICommand loginCommand;

        /// <summary>
        /// Logout command
        /// </summary>
        private ICommand logoutCommand;

        /// <summary>
        /// Login button visibility flag.
        /// </summary>
        private bool isLoginButtonVisible;

        /// <summary>
        /// Logout button visibility flag.
        /// </summary>
        private bool isLogoutButtonVisible;

        /// <summary>
        /// Logged in flag.
        /// </summary>
        private bool isLoggedIn;

        /// <summary>
        /// GitHub login.
        /// </summary>
        private string gitHubLogin;

        /// <summary>
        /// GitHub password.
        /// </summary>
        private string gitHubPassword;

        /// <summary>
        /// Reconnect on wake up event.
        /// </summary>
        private bool reconnectOnWakeUp = true;

        /// <summary>
        /// <see cref="MainScreenViewModel"/> static constructor. Performs <see cref="MachineId"/> initialization.
        /// </summary>
        static MainScreenViewModel()
        {
            MachineId = RegistryHelper.GetMachineGuid();
        }

        /// <summary>
        /// <see cref="MainScreenViewModel"/> default constructor.
        /// </summary>
        public MainScreenViewModel()
        {
            // Init view model
            var dateTime = DateTime.Now;
            this.DeviceId = $"{MachineId}-{dateTime:dd-MM-yy}";
            this.CarrierId = "afdemo";
            this.BackendUrl = "https://backend.northghost.com";
            this.IsConnectButtonVisible = false;
            this.SetStatusDisconnected();
            this.SetStatusLoggedOut();

            // Init remaining traffic timer
            this.InitializeTimer();

            // Init logging
            this.InitializeLogging();

            // Init predefined carriers and countries            
            this.InitializeCountriesList();
        }

        /// <summary>
        /// Device id for backend login method.
        /// </summary>
        public string DeviceId
        {
            get => this.deviceId;
            set => this.SetProperty(ref this.deviceId, value);
        }

        /// <summary>
        /// Carrier id for backend service.
        /// </summary>
        public string CarrierId
        {
            get => this.carrierId;
            set => this.SetProperty(ref this.carrierId, value);
        }

        /// <summary>
        /// Backend url for backend service.
        /// </summary>
        public string BackendUrl
        {
            get => this.backendUrl;
            set => this.SetProperty(ref this.backendUrl, value);
        }

        /// <summary>
        /// Message which is displayed in case of errors.
        /// </summary>
        public string ErrorText
        {
            get => this.errorText;
            set => this.SetProperty(ref this.errorText, value);
        }

        /// <summary>
        /// Access token for backend methods.
        /// </summary>
        public string AccessToken
        {
            get => this.accessToken;
            set => this.SetProperty(ref this.accessToken, value);
        }

        /// <summary>
        /// User password for VPN.
        /// </summary>
        public string Password
        {
            get => this.password;
            set => this.SetProperty(ref this.password, value);
        }

        /// <summary>
        /// VPN service IP address.
        /// </summary>
        public string VpnIpServer
        {
            get => this.vpnIpServerServer;
            set => this.SetProperty(ref this.vpnIpServerServer, value);
        }

        /// <summary>
        /// VPN service IP address.
        /// </summary>
        public string VpnIp
        {
            get => this.vpnIp;
            set => this.SetProperty(ref this.vpnIp, value);
        }

        /// <summary>
        /// Remaining traffic response.
        /// </summary>
        public string RemainingTrafficResponse
        {
            get => this.remainingTrafficResponse;
            set => this.SetProperty(ref this.remainingTrafficResponse, value);
        }

        /// <summary>
        /// Error visibility flag.
        /// </summary>
        public bool IsErrorVisible
        {
            get => this.isErrorVisible;
            set => this.SetProperty(ref this.isErrorVisible, value);
        }

        /// <summary>
        /// Connect button visibility flag.
        /// </summary>
        public bool IsConnectButtonVisible
        {
            get => this.isConnectButtonVisible;
            set => this.SetProperty(ref this.isConnectButtonVisible, value);
        }

        /// <summary>
        /// Disconnect button visibility flag.
        /// </summary>
        public bool IsDisconnectButtonVisible
        {
            get => this.isDisconnectButtonVisible;
            set => this.SetProperty(ref this.isDisconnectButtonVisible, value);
        }

        /// <summary>
        /// Received bytes count.
        /// </summary>
        public string BytesReceived
        {
            get => this.bytesReceived;
            set => this.SetProperty(ref this.bytesReceived, value);
        }

        /// <summary>
        /// Sent bytes count.
        /// </summary>
        public string BytesSent
        {
            get => this.bytesSent;
            set => this.SetProperty(ref this.bytesSent, value);
        }

        /// <summary>
        /// VPN connection status.
        /// </summary>
        public string Status
        {
            get => this.status;
            set => this.SetProperty(ref this.status, value);
        }

        /// <summary>
        /// Country for backend get credentials method.
        /// </summary>
        public string Country
        {
            get => this.country;
            set => this.SetProperty(ref this.country, value);
        }

        /// <summary>
        /// Use service flag.
        /// </summary>
        public bool UseService
        {
            get => this.useService;
            set => this.SetProperty(ref this.useService, value);
        }

        /// <summary>
        /// Name of windows service to use to establish VPN connection.
        /// </summary>
        public string ServiceName
        {
            get => this.serviceName;
            set => this.SetProperty(ref this.serviceName, value);
        }

        /// <summary>
        /// Logging enabled flag.
        /// </summary>
        public bool IsLoggingEnabled
        {
            get => CakeTubeLogger.IsEnabled;
            set => CakeTubeLogger.IsEnabled = value;
        }

        /// <summary>
        /// Log contents.
        /// </summary>
        public string LogContents
        {
            get => this.logContents;
            set => this.SetProperty(ref this.logContents, value);
        }

        /// <summary>
        /// Countries list.
        /// </summary>
        public IEnumerable<string> CountriesList
        {
            get => this.countriesList;
            set => this.SetProperty(ref this.countriesList, value);
        }

        /// <summary>
        /// Login button visibility flag.
        /// </summary>
        public bool IsLoginButtonVisible
        {
            get => this.isLoginButtonVisible;
            set => this.SetProperty(ref this.isLoginButtonVisible, value);
        }

        /// <summary>
        /// Logout button visibility flag.
        /// </summary>
        public bool IsLogoutButtonVisible
        {
            get => this.isLogoutButtonVisible;
            set => this.SetProperty(ref this.isLogoutButtonVisible, value);
        }

        /// <summary>
        /// Logged in flag.
        /// </summary>
        public bool IsLoggedIn
        {
            get => this.isLoggedIn;
            set
            {
                this.SetProperty(ref this.isLoggedIn, value);
                this.RaisePropertyChanged(nameof(this.IsLoggedOut));
            }
        }

        /// <summary>
        /// Logged out flag.
        /// </summary>
        public bool IsLoggedOut => !this.isLoggedIn;

        /// <summary>
        /// GitHub login.
        /// </summary>
        public string GitHubLogin
        {
            get => this.gitHubLogin;
            set => this.SetProperty(ref this.gitHubLogin, value);
        }

        /// <summary>
        /// GitHub password.
        /// </summary>
        public string GitHubPassword
        {
            get => this.gitHubPassword;
            set => this.SetProperty(ref this.gitHubPassword, value);
        }

        /// <summary>
        /// Reconnect on wake up event.
        /// </summary>
        public bool ReconnectOnWakeUp
        {
            get => this.reconnectOnWakeUp;
            set => this.SetProperty(ref this.reconnectOnWakeUp, value);
        }

        /// <summary>
        /// Connect command.
        /// </summary>
        public ICommand ConnectCommand => this.connectCommand ?? (this.connectCommand = new DelegateCommand(this.Connect));

        /// <summary>
        /// Disconnect command.
        /// </summary>
        public ICommand DisconnectCommand => this.disconnectCommand ?? (this.disconnectCommand = new DelegateCommand(this.Disconnect));

        /// <summary>
        /// Clear log command.
        /// </summary>
        public ICommand ClearLogCommand => this.clearLogCommand ??
                                           (this.clearLogCommand = new DelegateCommand(this.ClearLog));

        /// <summary>
        /// Login command.
        /// </summary>
        public ICommand LoginCommand => this.loginCommand ?? (this.loginCommand = new DelegateCommand(this.Login));

        /// <summary>
        /// Logout command.
        /// </summary>
        public ICommand LogoutCommand => this.logoutCommand ?? (this.logoutCommand = new DelegateCommand(this.Logout));
        
        /// <summary>
        /// Performs login to the backend server.
        /// </summary>
        private async void Login()
        {
            try
            {
                // Work with UI
                this.IsErrorVisible = false;
                this.IsLoginButtonVisible = false;

                // Perform logout
                await LogoutHelper.Logout();

                // Bootstrap VPN
                this.BootstrapVpn();

                // Perform login
                var loginResponse = await this.vpnServerService.LoginAsync(
                                        new VpnLoginParams
                                            {
                                                AuthenticationMethod = VpnAuthenticationMethod.Anonymous,
                                                DeviceId = this.DeviceId,
                                                DeviceType = DeviceType.Desktop,
                                                OAuthAccessToken = string.Empty,
                                                DeviceName = Environment.MachineName
                                            });

                // Check whether login was successful
                if (!loginResponse.IsSuccess)
                {
                    this.IsLoginButtonVisible = true;
                    this.ErrorText = loginResponse.Error ?? loginResponse.Result.ToString();
                    this.IsErrorVisible = true;
                    return;
                }

                // Remember access token for later usages
                LogoutHelper.AccessToken = loginResponse.AccessToken;
                this.AccessToken = loginResponse.AccessToken;

                this.UpdateCountries();

                // Work with UI
                this.SetStatusLoggedIn();

                // Update remaining traffic
                await this.UpdateRemainingTraffic();
            }
            catch (Exception e)
            {
                // Show error when exception occurred
                this.IsErrorVisible = true;
                this.ErrorText = e.Message;
                this.IsLoginButtonVisible = true;
            }
        }

        /// <summary>
        /// Update available countries list.
        /// </summary>
        private async void UpdateCountries()
        {
            try
            {
                // Get available countries
                var countriesResponse = await this.vpnServerService.GetCountriesAsync(this.AccessToken, VpnProtocolType.Openvpn);

                // Check whether request was successful
                if (!countriesResponse.IsSuccess)
                {
                    this.IsLoginButtonVisible = true;
                    this.ErrorText = countriesResponse.Error ?? countriesResponse.Result.ToString();
                    this.IsErrorVisible = true;
                    return;
                }

                // Get countries from response
                var countries = countriesResponse.VpnCountries.Select(x => x.Country).ToList();
                countries.Insert(0, string.Empty);

                // Remember countries
                this.CountriesList = countries;
            }
            catch (Exception e)
            {
                // Show error when exception occurred
                this.IsLoginButtonVisible = true;
                this.IsErrorVisible = true;
                this.ErrorText = e.Message;
            }
        }

        private async void Logout()
        {
            try
            {
                // Work with UI
                this.IsErrorVisible = false;
                this.IsLogoutButtonVisible = false;
                this.IsLoggedIn = false;

                // Perform logout
                var logoutResponse =
                    await this.vpnServerService.LogoutAsync(new LogoutRequestParams { AccessToken = this.AccessToken });

                // Check whether logout was successful
                if (!logoutResponse.IsSuccess)
                {
                    this.IsLogoutButtonVisible = true;
                    this.ErrorText = logoutResponse.Error ?? logoutResponse.Result.ToString();
                    this.IsErrorVisible = true;
                    return;
                }

                // Erase access token and other related properties
                LogoutHelper.AccessToken = string.Empty;
                this.AccessToken = string.Empty;
                this.VpnIp = string.Empty;
                this.Password = string.Empty;
                this.RemainingTrafficResponse = string.Empty;

                // Work with UI
                this.InitializeCountriesList();
                this.SetStatusLoggedOut();
            }
            catch (Exception e)
            {
                // Show error when exception occurred
                this.IsErrorVisible = true;
                this.ErrorText = e.Message;
                this.IsLoggedIn = true;
                this.IsLogoutButtonVisible = true;
            }
        }

        /// <summary>
        /// Clears log contents.
        /// </summary>
        private void ClearLog()
        {
            this.LogContents = string.Empty;
        }

        /// <summary>
        /// Performs remaining traffic timer initialization.
        /// </summary>
        private void InitializeTimer()
        {
            this.dispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            this.dispatcherTimer.Tick += this.DispatcherTimerOnTick;
            this.dispatcherTimer.Start();
        }

        /// <summary>
        /// Subscribes to VPN client events.
        /// </summary>
        private void InitializeEvents()
        {
            this.vpnConnectionService.VpnStateChanged += this.VpnConnectionServiceOnVpnStateChanged;
            this.vpnConnectionService.VpnTrafficChanged += this.VpnConnectionServiceOnVpnTrafficChanged;
        }

        private void VpnConnectionServiceOnVpnTrafficChanged(VpnTraffic vpnTraffic)
        {
            this.BytesReceived = vpnTraffic.InBytes.ToString();
            this.BytesSent = vpnTraffic.OutBytes.ToString();
        }

        private void VpnConnectionServiceOnVpnStateChanged(VpnConnectionState vpnConnectionState)
        {
            this.Status = vpnConnectionState.ToString();
            switch (vpnConnectionState)
            {
                case VpnConnectionState.Disconnected:
                    this.SetStatusDisconnected();
                    break;
                case VpnConnectionState.Disconnecting:
                    this.IsDisconnectButtonVisible = false;
                    this.IsConnectButtonVisible = false;
                    this.IsLoginButtonVisible = false;
                    this.IsLogoutButtonVisible = false;
                    break;
                case VpnConnectionState.Connected:
                    this.VpnClientOnConnected();
                    break;
                case VpnConnectionState.Connecting:
                    this.IsDisconnectButtonVisible = false;
                    this.IsConnectButtonVisible = false;
                    this.IsLoginButtonVisible = false;
                    this.IsLogoutButtonVisible = false;
                    break;
            }
        }

        /// <summary>
        /// VPN client connected event handler.
        /// </summary>
        private void VpnClientOnConnected()
        {
            this.IsConnectButtonVisible = false;
            this.IsDisconnectButtonVisible = true;
            this.IsLogoutButtonVisible = false;
        }

        /// <summary>
        /// Performs actions related to setting backend status to "Logged out"
        /// </summary>
        private void SetStatusLoggedOut()
        {
            this.IsLoginButtonVisible = true;
            this.IsLogoutButtonVisible = false;
            this.IsLoggedIn = false;
        }

        /// <summary>
        /// Performs actions related to setting backend status to "Logged in"
        /// </summary>
        private void SetStatusLoggedIn()
        {
            this.IsLoginButtonVisible = false;
            this.IsLogoutButtonVisible = true;
            this.IsLoggedIn = true;
        }

        /// <summary>
        /// Performs actions related to setting VPN status to "Disconnected".
        /// </summary>
        private void SetStatusDisconnected()
        {
            this.Status = "Disconnected";
            this.BytesReceived = "0";
            this.BytesSent = "0";
            this.IsDisconnectButtonVisible = false;
            this.IsConnectButtonVisible = true;
            this.IsLogoutButtonVisible = true;
        }

        /// <summary>
        /// Bootstraps VPN according to the selected parameters and initializes VPN events.
        /// </summary>
        private void BootstrapVpn()
        {
            var type = typeof(CakeTubeIoc);
            var field = type.GetField("_container", BindingFlags.NonPublic | BindingFlags.Static);
            field.SetValue(null, null);

            var cakeTubeConfiguration = new BootstrapServerConfiguration
                                            {
                                                CarrierId = this.CarrierId,
                                                VpnServerUrlList = new List<Uri> { new Uri(this.BackendUrl) }
                                            };

            var vpnConnectionConfiguration = new VpnConnectionConfiguration
                                                 {
                                                     VpnWindowsServiceName = this.ServiceName
                                                 };

            var cakeTubeBootstrapper = new CakeTubeWindowsBootstrapper(cakeTubeConfiguration, vpnConnectionConfiguration);
            cakeTubeBootstrapper.Bootstrapp(new UnityCakeTubeIocContainer());

            this.vpnServerService = CakeTubeIoc.Container.Resolve<IVpnServerService>();
            this.vpnConnectionService = CakeTubeIoc.Container.Resolve<VpnConnectionService>();
            this.vpnServiceInfoStorage = CakeTubeIoc.Container.Resolve<VpnServiceInfoStorage>();
            this.vpnConnectionInfo = CakeTubeIoc.Container.Resolve<VpnConnectionInfo>();
            this.vpnWindowsServiceHandler = new VpnWindowsServiceHandler(this.vpnServiceInfoStorage, this.vpnConnectionInfo);

            var isRunning = this.vpnWindowsServiceHandler.IsRunning();

            if (isRunning)
            {
                this.vpnWindowsServiceHandler.Stop();
            }

            this.InitializeEvents();
        }

        /// <summary>
        /// Performs VPN connection.
        /// </summary>
        private async void Connect()
        {
            try
            {
                this.IsConnectButtonVisible = false;
                this.IsDisconnectButtonVisible = false;
                this.IsLoginButtonVisible = false;
                var vpnCredentialsResponse = await this.vpnServerService.GetCredentialsAsync(
                                                 new GetCredentialsParams
                                                     {
                                                         AccessToken = this.AccessToken,
                                                         VpnType = VpnProtocolType.Openvpn,
                                                         WithCertificate = false,
                                                         CountryCode = this.Country
                                                     });

                if (!vpnCredentialsResponse.IsSuccess)
                {
                    throw new Exception(vpnCredentialsResponse.Error);
                }

                var vpnCredentials = vpnCredentialsResponse.VpnCredentials;

                var t = await this.vpnConnectionService.ConnectAsync(
                    new VpnCredentials
                        {
                            Country = this.Country ?? string.Empty,
                            Password = vpnCredentials.Password,
                            Ip = vpnCredentials.Ip,
                            Port = vpnCredentials.Port,
                            Protocol = vpnCredentials.Protocol,
                            UserName = vpnCredentials.UserName
                        });
                
            }
            catch (Exception e)
            {
                // Show error when exception occurred
                this.IsLogoutButtonVisible = true;
                this.IsConnectButtonVisible = true;
                this.IsDisconnectButtonVisible = false;
                this.IsErrorVisible = true;
                this.ErrorText = e.Message;
            }
        }

        /// <summary>
        /// Disconnects from VPN server.
        /// </summary>
        private async void Disconnect()
        {
            try
            {
                // Disconnect VPN
                await this.vpnConnectionService.Disconnect();                
            }
            catch (Exception e)
            {
                // Show error when exception occurred
                this.IsErrorVisible = true;
                this.ErrorText = e.Message;
            }

            // Update UI
            this.SetStatusDisconnected();
        }

        /// <summary>
        /// Remaining traffic timer tick event handler.
        /// </summary>
        private async void DispatcherTimerOnTick(object sender, EventArgs eventArgs)
        {
            // Exit if AccessToken is empty
            if (string.IsNullOrEmpty(this.AccessToken))
            {
                return;
            }

            // Update remaining traffic
            await this.UpdateRemainingTraffic();
        }

        /// <summary>
        /// Performs update of remaining traffic
        /// </summary>
        private async Task UpdateRemainingTraffic()
        {
            try
            {
                // Check if access token is not empty
                if (string.IsNullOrEmpty(this.AccessToken))
                {
                    return;
                }

                // Get remaining traffic
                var remainingTrafficResponseResult =
                    await this.vpnServerService.GetRemainingTrafficAsync(
                        new GetRemaningTrafficParams { AccessToken = this.AccessToken });

                // Check whether request was successful
                if (!remainingTrafficResponseResult.IsSuccess)
                {
                    return;
                }

                // Update UI with response data
                this.RemainingTrafficResponse
                    = remainingTrafficResponseResult.IsUnlimited
                        ? "Unlimited"
                        : $"Bytes remaining: {remainingTrafficResponseResult.TrafficRemaining}\nBytes used: {remainingTrafficResponseResult.TrafficUsed}";
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        /// Performs logging initialization.
        /// </summary>
        private void InitializeLogging()
        {
            var loggerListener = new EventLoggerListener();
            loggerListener.LogEntryArrived += (sender, args) => this.AddLogEntry(args.Entry);
            CakeTubeLogger.AddHandler(loggerListener);
        }

        /// <summary>
        /// Adds new log entry to the log contents.
        /// </summary>
        /// <param name="logEntry">Log entry to add.</param>
        private void AddLogEntry(string logEntry)
        {
            if (string.IsNullOrWhiteSpace(this.LogContents))
            {
                this.LogContents = string.Empty;
            }

            this.LogContents += logEntry + Environment.NewLine;
        }

        /// <summary>
        /// Performs countries list initialization.
        /// </summary>
        private void InitializeCountriesList()
        {
            this.CountriesList = new[] { string.Empty, };
        }
    }
}