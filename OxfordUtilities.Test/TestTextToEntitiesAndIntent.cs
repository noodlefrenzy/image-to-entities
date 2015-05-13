using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OxfordUtilities.Test
{
    [TestClass]
    public class TestTextToEntitiesAndIntent
    {
        [TestMethod]
        public async Task TestEntitiesFromMemeText()
        {
            var luisApp = System.Environment.GetEnvironmentVariable("LUIS_APP_ID");
            Assert.IsFalse(string.IsNullOrWhiteSpace(luisApp), "Must provide app ID");
            var apiKey = System.Environment.GetEnvironmentVariable("LUIS_API_KEY");
            Assert.IsFalse(string.IsNullOrWhiteSpace(apiKey), "Must provide API Key");

            var luis = new TextToEntitiesAndIntent(luisApp, apiKey);
            var result = await luis.DetectEntitiesAndIntentFromText("ONE DOES NOT SIMPLY OCR SOME TEXT FROM AN IMAGE");
            Assert.IsNotNull(result);
            Trace.TraceInformation(result.ToString(Newtonsoft.Json.Formatting.Indented));
            Assert.AreEqual("Mordor", (string)(result["intents"][0]["intent"]));
            Assert.AreEqual("ocr some text from an image", (string)(result["entities"][0]["entity"]));
        }

        [TestMethod]
        public async Task TestFullPipeline()
        {
            var visionApiKey = System.Environment.GetEnvironmentVariable("VISION_API_KEY");
            Assert.IsFalse(string.IsNullOrWhiteSpace(visionApiKey), "Must provide API Key");
            var luisApp = System.Environment.GetEnvironmentVariable("LUIS_APP_ID");
            Assert.IsFalse(string.IsNullOrWhiteSpace(luisApp), "Must provide app ID");
            var luisApiKey = System.Environment.GetEnvironmentVariable("LUIS_API_KEY");
            Assert.IsFalse(string.IsNullOrWhiteSpace(luisApiKey), "Must provide API Key");

            var mordorImage = "http://i.imgur.com/5ocZvsW.jpg";
            var imageToText = new ImageToText(visionApiKey);
            var result = await imageToText.ProcessImageToTextAsync(await ImageUtilities.SingleChannelAsync(new Uri(mordorImage), ImageUtilities.Channel.Blue));
            var lines = ImageToText.ExtractLinesFromResponse(result).ToList();
            lines.RemoveAt(lines.Count - 1); // Remove memegenerator.net line.
            var text = string.Join(" ", lines);

            var luis = new TextToEntitiesAndIntent(luisApp, luisApiKey);
            var luisResult = await luis.DetectEntitiesAndIntentFromText(text);
            Trace.TraceInformation(luisResult.ToString(Newtonsoft.Json.Formatting.Indented));
            Assert.IsNotNull(luisResult);
        }
    }
}
