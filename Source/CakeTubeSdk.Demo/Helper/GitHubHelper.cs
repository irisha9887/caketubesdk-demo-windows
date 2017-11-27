namespace CakeTubeSdk.Demo.Helper
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;

    using CakeTubeSdk.Core.Infrastructure;
    using CakeTubeSdk.Demo.View;

    using Microsoft.Practices.ServiceLocation;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class GitHubHelper
    {
        private const string GithubOtpHeader = "X-GitHub-OTP";
        private const string ApiUrl = "https://api.github.com/authorizations";
        private const string ClientId = "70ed6ffd4b08b3119208";
        private const string ClientSecret = "fe02229ef77aa489f748f346e3e337490fd5b8ce";
        
        /// <summary>
        /// Gets GiHub OAuth token.
        /// </summary>
        /// <param name="login">User login.</param>
        /// <param name="pass">User password.</param>
        /// <returns>OAuth token or string.Empty in case of failure.</returns>
        public static async Task<string> GetGithubOAuthToken(string login, string pass)
        {            
            try
            {
                var response = await LoginGithub(login, pass);
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                        && response.Headers.Contains(GithubOtpHeader))
                    {
                        CakeTubeLogger.Trace("Two-factor authentication enabled");

                        var requestAuthCode = ServiceLocator.Current.GetInstance<RequestAuthCode>();

                        requestAuthCode.ShowDialog();
                        if (requestAuthCode.DialogResult != true)
                        {
                            CakeTubeLogger.Trace("Cancel authorization!");
                            return string.Empty;
                        }

                        var authCode = requestAuthCode.RequestAuthCodeViewModel.AuthCode;
                        CakeTubeLogger.Trace("Sending authentication code...");

                        response = await LoginGithub(login, pass, authCode);
                        if (!response.IsSuccessStatusCode)
                        {
                            CakeTubeLogger.Trace("Two-factor authentication failed!");
                            return string.Empty;
                        }
                    }
                    else
                    {
                        CakeTubeLogger.Trace("Unable to get OAuth token from GitHub!");
                        return string.Empty;
                    }
                }

                CakeTubeLogger.Trace("Got valid response from GitHub");
                var responseString = await response.Content.ReadAsStringAsync();
                var responseJson = JObject.Parse(responseString);

                return responseJson["token"].ToObject<string>();
            }
            catch (Exception exception)
            {
                CakeTubeLogger.Error("Can not login via GitHub", exception);
                return string.Empty;
            }
        }

        /// <summary>
        /// Performs login to GitHub.
        /// </summary>
        /// <param name="login">User login.</param>
        /// <param name="pass">User password.</param>
        /// <param name="authCode">Optional authentication code for two-factor authentication.</param>
        /// <returns><see cref="HttpResponseMessage"/></returns>
        private static async Task<HttpResponseMessage> LoginGithub(string login, string pass, string authCode = null)
        {
            var authString = Convert.ToBase64String(Encoding.Default.GetBytes($"{login}:{pass}"));
            var parameters = new
                                 {
                                     scopes = new string[] { },
                                     client_id = ClientId,
                                     client_secret = ClientSecret
                                 };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", login);

                using (var message = new HttpRequestMessage(HttpMethod.Post, ApiUrl))
                {
                    message.Headers.Accept.ParseAdd("application/json");
                    message.Headers.Authorization = AuthenticationHeaderValue.Parse($"Basic {authString}");

                    // Two-factor authentication
                    if (authCode != null)
                    {
                        message.Headers.Add(GithubOtpHeader, authCode);
                    }

                    var content = new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");
                    message.Content = content;

                    CakeTubeLogger.Trace("Trying to get OAuth token from GitHub...");
                    return await client.SendAsync(message);
                }
            }
        }
    }
}