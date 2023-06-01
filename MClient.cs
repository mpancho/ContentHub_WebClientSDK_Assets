using System.Net;
using Stylelabs.M.Sdk.WebClient;
using Stylelabs.M.Sdk.WebClient.Authentication;

namespace ContentHub_WebClientSDK_Assets
{
    public static class MClient
    {
        private static Lazy<IWebMClient> _client { get; set; }

        public static IWebMClient Client
        {
            get
            {
                if (_client == null)
                {
                    OAuthPasswordGrant auth = new OAuthPasswordGrant
                    {
                        ClientId = "clientid",
                        ClientSecret = "clientsecret",
                        UserName = "username",
                        Password = "password"

                    };

                    _client = new Lazy<IWebMClient>(() => MClientFactory.CreateMClient(new Uri("https://sitecoresandbox.cloud"), auth));

                }
                return _client.Value;
            }
        }
    }
}
