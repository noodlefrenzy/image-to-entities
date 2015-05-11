using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OxfordUtilities
{
    public class TextToEntitiesAndIntent
    {
        public TextToEntitiesAndIntent(string applicationId, string apiKey)
        {
            this.applicationId = applicationId;
            this.apiKey = apiKey;
        }

        private readonly string applicationId;
        private readonly string apiKey;

        private const string LUISAPIURIFormat = "https://api.projectoxford.ai/luis/v1/application?id={0}&subscription-key={1}&q={2}";

        public async Task<JObject> DetectEntitiesAndIntentFromText(string text)
        {
            var luisUri = string.Format(LUISAPIURIFormat, this.applicationId, this.apiKey, Uri.EscapeUriString(text));
            var request = WebRequest.Create(luisUri);
            request.Method = "GET";

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)(await request.GetResponseAsync().ConfigureAwait(false));
            }
            catch (WebException we)
            {
                response = (HttpWebResponse)we.Response;
            }

            JObject responseJson;
            using (var responseStream = new StreamReader(response.GetResponseStream()))
            {
                var responseStr = await responseStream.ReadToEndAsync().ConfigureAwait(false);
                responseJson = JObject.Parse(responseStr); // I'm fine throwing a parse error here.
            }

            if (response.StatusCode == HttpStatusCode.OK) // Could probably relax this to "non-failing" codes.
            {
                return responseJson;
            }
            else
            {
                throw new Exception(string.Format("Failed call: failed to detect entities in '{0}' - code {1} - details\n{2}",
                    text, response.StatusCode, responseJson.ToString(Newtonsoft.Json.Formatting.Indented)));
            }
        }
    }
}
