using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OxfordUtilities
{
    /// <summary>
    /// Process images using Microsoft Oxford API's OCR to pull out text.
    /// </summary>
    public class ImageToText
    {
        public ImageToText(string apiKey)
        {
            this.apiKey = apiKey;
        }

        private readonly string apiKey;

        private const string OCRURIFormat = "https://api.projectoxford.ai/vision/v1/ocr?language={0}&detectOrientation={1}";

        private const string DefaultLanguage = "unk";

        private const bool DefaultDetectOrientation = true;

        private const string APIKeyHeader = "Ocp-Apim-Subscription-Key";

        public static IEnumerable<string> ExtractLinesFromResponse(JObject responseJson)
        {
            return from r in responseJson["regions"]
                   from l in r["lines"]
                   select string.Join(" ", from w in l["words"] select (string)w["text"]);
        }

        public static IEnumerable<Tuple<int, string>> ExtractLinesWithLocationFromResponse(JObject responseJson)
        {
            Func<JToken, int> BBTop = bb => int.Parse(((string)bb["boundingBox"]).Split(',')[1]);
            return from r in responseJson["regions"]
                   from l in r["lines"]
                   select Tuple.Create(BBTop(l), string.Join(" ", from w in l["words"] select (string)w["text"]));
        }

        public static string ExtractTextFromResponse(JObject responseJson)
        {
            return string.Join(" ", from r in responseJson["regions"]
                                    from l in r["lines"]
                                    from w in l["words"]
                                    select (string)w["text"]);
        }

        public async Task<JObject> ProcessImageToTextAsync(Uri imageUri, string language = DefaultLanguage, bool detectOrientation = DefaultDetectOrientation)
        {
            var requestBody = new JObject();
            requestBody["Url"] = imageUri.ToString();

            var ocrUri = string.Format(OCRURIFormat, language, detectOrientation);
            Trace.TraceInformation("Attempting to fetch OCR from {0} using\n{1}", ocrUri, requestBody.ToString(Newtonsoft.Json.Formatting.Indented));
            var request = WebRequest.Create(ocrUri);
            request.ContentType = "application/json";
            request.Method = "POST";
            request.Headers.Add(APIKeyHeader, this.apiKey);

            using (var requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false))
            {
                var bodyBytes = Encoding.UTF8.GetBytes(requestBody.ToString());
                await requestStream.WriteAsync(bodyBytes, 0, bodyBytes.Length).ConfigureAwait(false);
            }

            return await _ReadResponseAsync(request, imageUri.ToString()).ConfigureAwait(false);
        }

        public Task<JObject> ProcessImageToTextAsync(string imageUri, string language = DefaultLanguage, bool detectOrientation = DefaultDetectOrientation)
        {
            return ProcessImageToTextAsync(new Uri(imageUri), language, detectOrientation);
        }

        public async Task<JObject> ProcessImageToTextAsync(Stream stream, string language = DefaultLanguage, bool detectOrientation = DefaultDetectOrientation)
        {
            var ocrUri = string.Format(OCRURIFormat, language, detectOrientation);
            var request = WebRequest.Create(ocrUri);
            request.ContentType = "application/octet-stream";
            request.Method = "POST";
            request.Headers.Add(APIKeyHeader, this.apiKey);

            using (var requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false))
            {
                await stream.CopyToAsync(requestStream).ConfigureAwait(false);
            }

            return await _ReadResponseAsync(request, "stream").ConfigureAwait(false);
        }

        public async Task<JObject> ProcessImageFileToTextAsync(string filename,
            string language = DefaultLanguage, bool detectOrientation = DefaultDetectOrientation, CancellationToken? cancelToken = null)
        {
            var tok = cancelToken ?? CancellationToken.None;
            var request = WebRequest.Create(string.Format(OCRURIFormat, language, detectOrientation));
            request.ContentType = "application/octet-stream";
            request.Method = "POST";

            using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            using (var requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false))
            {
                request.ContentLength = fileStream.Length;
                await fileStream.CopyToAsync(requestStream, 8192, tok).ConfigureAwait(false);
            }

            return await _ReadResponseAsync(request, filename).ConfigureAwait(false);
        }

        private async Task<JObject> _ReadResponseAsync(WebRequest request, string inputAddress)
        {
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
                throw new Exception(string.Format("Failed call: {0} failed to OCR - code {1} - details\n{2}", 
                    inputAddress, response.StatusCode, responseJson.ToString(Newtonsoft.Json.Formatting.Indented)));
            }
        }
    }
}
