using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;

namespace RepeatAndLearn.Model
{
    public class YandexApi
    {
        const string BaseUrl = "https://translate.yandex.net/api/v1.5/tr.json/detect?";

        readonly IRestClient _client;

        string _accountSid;

        public YandexApi(string accountSid, string secretKey)
        {
            _client = new RestClient(BaseUrl);
            _client.Authenticator = new HttpBasicAuthenticator(accountSid, secretKey);
            _accountSid = accountSid;
        }

        public T Execute<T>(RestRequest request) where T : new()
        {
            request.AddParameter("AccountSid", _accountSid, ParameterType.UrlSegment); // used on every request
            var response = _client.Execute<T>(request);

            if (response.ErrorException == null)
                return response.Data;

            const string message = "Error retrieving response.  Check inner details for more info.";
            var twilioException = new ApplicationException(message, response.ErrorException);
            throw twilioException;

        }
    }
}
