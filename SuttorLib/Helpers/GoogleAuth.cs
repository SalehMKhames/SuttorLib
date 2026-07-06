using Google.Apis.Auth.OAuth2;
using System.Threading.Tasks;

namespace SuttorLib.Helpers
{
    public static class GoogleAuth
    {
        public static async Task<string> GetAccessTokenAsync()
        {
            var credential = GoogleCredential.FromFile("Data/FCM-Key.json")
                .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

            var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
            return accessToken;
        }
    }
}
