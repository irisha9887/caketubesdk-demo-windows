# ANCHORFREE PARTNER VPN WINDOWS SDK

## Description
Windows SDK is a part of Anchorfree Partner SDK which contains the client-side libraries and server-side applications needed to implement custom VPN infrastructure.

Download [the last version SDK](https://firebasestorage.googleapis.com/v0/b/web-portal-for-partners.appspot.com/o/products%2FCakeTubeSDK_Win_version_1.2.2.70_signed.zip?alt=media&token=b46b0b9b-106e-4c6f-9818-d8db54c3c1e4)

The Windows SDK provides API allowing:
* authenticate clients on VPN Server
* connect clients to VPN Server

## Prerequisites
OS: Windows 7, 8.0, 8.1, 10
Software: .NET Framework 4.5
Nuget dependencies: 
  <package id="CommonServiceLocator" version="1.3" targetFramework="net45" />
  <package id="Newtonsoft.Json" version="9.0.1" targetFramework="net45" />  
  <package id="Unity" version="4.0.1" targetFramework="net45" />

## Setup
In order to use the SDK the steps described below must be performed.
1. Install TAP Driver as Administrator via command line: ./tapinstall.exe install "AFTap.inf" "aftap0901"
2. Install VPN Windows Service as Administrator via command line: ./VpnService.exe -install <ServiceName>
3. To uninstall service later: ./VpnService.exe -uninstall <ServiceName>Bootstrap

## Bootstrap
### CakeTubeWindowsBootstrapper Class
Performs initialization of SDK

#### Constructor
| Syntax | Description |
| -------|:-----------:|
| CakeTubeWindowsBootstrapper(BootstrapServerConfiguration, VpnConnectionConfiguration)| Initializes the class new instance. |

#### Methods
| Syntax | Description |
| -------|:-----------:|
| void Bootstrap(ICakeTubeIocContainer) | Initializes all SDK states and dependencies.|

### BootstrapServerConfiguration Class
Parameters required to configure Server service

#### Properties
| Syntax | Description |
| -------|:-----------:|
| List< Uri > VpnServerUrlList | List of vpn server addresses. When length bigger than one, sdk will randomly pick one for each request. |
| string CarrierId | Carrier id. |

### VpnConnectionConfiguration Class

Parameters required to configure Connection service

#### Properties
| Syntax | Description |
| -------|:-----------:|
| string VpnWindowsServiceName | Name of the service you installed in SETUP section. |

#### Example
```
var cakeTubeConfiguration = new BootstrapServerConfiguration
            {
                CarrierId = "5",
                VpnServerUrlList = new List<Uri> { <urls> }                
            };

var vpnConnectionConfiguration = new VpnConnectionConfiguration
            {
            VpnWindowsServiceName = "YourVpnServiceName"
            };

var cakeTubeBootstrapper = new CakeTubeWindowsBootstrapper(cakeTubeConfiguration, vpnConnectionConfiguration);            
cakeTubeBootstrapper.Bootstrapp(new UnityCakeTubeIocContainer());
```

## Authentication
Anchorfree Partner VPN Backend use OAuth authentication as a primary authentication method.

Steps to implement OAuth:
* Deploy and configure OAuth service. Service should be publicly available in Internet.
* Configure Partner Backend to use OAuth service.
* Implement client OAuth for your application.
* Retrieve access token in client app, this token will be used to initialize and sign in Windows Partner SDK.

### VpnServerService Class

Manages client user: authentication, credentials retrieval, user info.
Recommended place to create this service is an Application singleton class.

#### Methods
| Syntax | Description |
| -------|:-----------:|
| static IVpnServerService Create() | Creates VpnServerService instance. |
| Task< VpnLogoutResponse > LogoutAsync(LogoutRequestParams) | Performs logout on server side. |
| Task< VpnIsConnectedResponse > IsConnectedAsync() | Checks if VPN connection is established. |
| Task< UserConfigResponse > GetConfigAsync() | Gets configuration data from server. |
| Task< VpnCountersResponse > GetTrafficCountersAsync(GetCountersRequestParams) | Gets incoming and outcoming vpn traffic from the server. |
| Task< VpnLoginResponce > LoginAsync(VpnLoginParams) | Logs in to vpn server. |
| Task< VpnCountriesResponse > GetCountriesAsync(string) | Gets available vpn countries. Requires access token as parameter. |
| Task< VpnCheckCredentialsResponse > CheckCredentialsAsync(CheckCredentialsParams) | Cheks if received credentials are still valid. |
| Task< VpnCredentialsResponce > GetCredentialsAsync(GetCredentialsParams) | Gets credentials for establishing vpn connection. |

## Connection

### VpnConnectionService Class

Manages device's VPN connection. To establish VPN connection user should manually install TAP driver and Vpn Windows Service as described in the Setup section.

Recommended place to create this service is an Application singleton class. This service is used to communicate with VPN Windows service and manages connection.

#### Methods
| Syntax | Description |
| -------|:-----------:|
| static IVpnConnectionService Create() | Creates VpnConnectionService instance. |
| Task< bool > ConnectAsync(VpnCredentials) | Opens Vpn connection. |
| void Disconnect() | Closes VPN connection. |


#### Properties
| Syntax | Description |
| -------|:-----------:|
| VpnConnectionState VpnConnectionState | Provides current connection state. |

#### Events
| Syntax | Description |
| -------|:-----------:|
| VpnConnectionStateChangedEventHandler VpnStateChanged | Notifies subscribers about Vpn Connection state changes. |
| VpnTrafficChangedEventHandler VpnTrafficChanged | Notifies subscribers about changes of consumed network traffic value |
