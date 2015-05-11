using System;
using System.Diagnostics;
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
            var apiKey = System.Environment.GetEnvironmentVariable("LUIS_API_KEY");
            Assert.IsFalse(string.IsNullOrWhiteSpace(apiKey), "Must provide API Key");

            var luis = new TextToEntitiesAndIntent(luisApp, apiKey);
            var result = await luis.DetectEntitiesAndIntentFromText("ONE DOES NOT SIMPLY OCR SOME TEXT FROM AN IMAGE");
            Assert.IsNotNull(result);
            Trace.TraceInformation(result.ToString(Newtonsoft.Json.Formatting.Indented));
            Assert.AreEqual("Mordor", (string)(result["intents"][0]["intent"]));
            Assert.AreEqual("ocr some text from an image", (string)(result["entities"][0]["entity"]));
        }
    }
}
