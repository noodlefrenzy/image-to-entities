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

        //https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/a6519235-2dc3-4e63-b336-0f8a1f0af24b?subscription-key=8cb90fe9596c4befb99305aa5166fe3a&q=one%20does%20not%20simple%20sprint%20into%20mordor&verbose=true
        //https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/d84af5f8-edf5-42e6-8af4-66b0fa596b0c?subscription-key=9b1a721ac26c413585bb0b26ea3601fb&q=ONE%20DOES%20NOT%20SIMPLY%20OCR%20SOME%20TEXT%20FROM%20AN%20IMAGE

        private const string LUISAPIURIFormat = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/{0}?subscription-key={1}&q={2}";

        public async Task<LUISResult> DetectEntitiesAndIntentFromText(string text)
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
                var result = new LUISResult();
                result.Intent = (string)responseJson["topScoringIntent"]["intent"];
                result.IntentScore = (double)responseJson["topScoringIntent"]["score"];
                foreach (var entity in responseJson["entities"])
                {
                    var name = (string)entity["type"];
                    var val = (string)entity["entity"];
                    var score = (double)entity["score"];
                    LUISEntity curEnt;
                    if (!result.Entities.TryGetValue(name, out curEnt))
                    {
                        curEnt = new LUISEntity() { EntityName = name };
                        result.Entities[name] = curEnt;
                    }
                    curEnt.EntityValues.Add(val);
                    curEnt.EntityScores.Add(score);
                }

                return result;
            }
            else
            {
                throw new Exception(string.Format("Failed call: failed to detect entities in '{0}' - code {1} - details\n{2}",
                    text, response.StatusCode, responseJson.ToString(Newtonsoft.Json.Formatting.Indented)));
            }
        }
    }
}
