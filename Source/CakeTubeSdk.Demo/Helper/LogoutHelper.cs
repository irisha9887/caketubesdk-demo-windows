namespace CakeTubeSdk.Demo.Helper
{
    using System.Threading.Tasks;

    using CakeTubeSdk.Core;
    using CakeTubeSdk.Core.ApiParameters;
    using CakeTubeSdk.Core.Services;

    /// <summary>
    /// Logout related properties and methods.
    /// </summary>
    public static class LogoutHelper
    {
        /// <summary>
        /// Access token to perform logout with.
        /// </summary>
        public static string AccessToken { private get; set; }

        /// <summary>
        /// Performs logout from backend.
        /// </summary>
        public static async Task Logout()
        {
            try
            {
                // Resolve backend service
                var partnerBackendService = CakeTubeIoc.Container.Resolve<IVpnServerService>();

                // Logout from backend
                await partnerBackendService.LogoutAsync(new LogoutRequestParams { AccessToken = AccessToken });
            }
            catch
            {
                // Ignored
            }
        }
    }
}