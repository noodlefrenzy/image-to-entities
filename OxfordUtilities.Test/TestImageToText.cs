using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace OxfordUtilities.Test
{
    [TestClass]
    public class TestImageToText
    {
        private const string ExampleResponse = @"
{
  ""language"": ""en"",
  ""textAngle"": 0,
  ""orientation"": ""Up"",
  ""regions"": [
    {
      ""boundingBox"": ""81,63,1340,1055"",
      ""lines"": [
        {
          ""boundingBox"": ""321,63,855,117"",
          ""words"": [
            {
              ""boundingBox"": ""321,63,174,94"",
              ""text"": ""Set""
            },
            {
              ""boundingBox"": ""529,87,126,69"",
              ""text"": ""an""
            },
            {
              ""boundingBox"": ""693,65,483,115"",
              ""text"": ""example.""
            }
          ]
        },
        {
          ""boundingBox"": ""292,769,916,116"",
          ""words"": [
            {
              ""boundingBox"": ""777,790,198,95"",
              ""text"": ""you""
            },
            {
              ""boundingBox"": ""1014,790,194,70"",
              ""text"": ""are.""
            }
          ]
        }
      ]
    },
    {
      ""boundingBox"": ""81,63,1340,1055"",
      ""lines"": [
        {
          ""boundingBox"": ""487,1002,526,116"",
          ""words"": [
            {
              ""boundingBox"": ""487,1002,236,116"",
              ""text"": ""Spiril""
            },
            {
              ""boundingBox"": ""741,1014,272,67"",
              ""text"": ""Science""
            }
          ]
        }
      ]
    }
  ]
}";

        [TestMethod]
        public void TestExtractLinesFromResponse()
        {
            // Copied (and pruned) from Project Oxford OCR example response: https://dev.projectoxford.ai/docs/services/54ef139a49c3f70a50e79b7d/operations/5527970549c3f723cc5363e4
            var response = JObject.Parse(ExampleResponse);

            var expected = new[] { "Set an example.", "you are.", "Spiril Science" };
            var lines = ImageToText.ExtractLinesFromResponse(response).ToArray();
            CollectionAssert.AreEqual(expected, lines);
        }

        [TestMethod]
        public void TestExtractTextFromResponse()
        {
            // Copied (and pruned) from Project Oxford OCR example response: https://dev.projectoxford.ai/docs/services/54ef139a49c3f70a50e79b7d/operations/5527970549c3f723cc5363e4
            var response = JObject.Parse(ExampleResponse);

            var expected = "Set an example. you are. Spiril Science";
            var actual = ImageToText.ExtractTextFromResponse(response);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestExtractLinesWithLocationFromResponse()
        {
            // Copied (and pruned) from Project Oxford OCR example response: https://dev.projectoxford.ai/docs/services/54ef139a49c3f70a50e79b7d/operations/5527970549c3f723cc5363e4
            var response = JObject.Parse(ExampleResponse);

            var expected = new[] {
                Tuple.Create(63, "Set an example."),
                Tuple.Create(769, "you are."),
                Tuple.Create(1002, "Spiril Science") };
            var lines = ImageToText.ExtractLinesWithLocationFromResponse(response).ToArray();
            CollectionAssert.AreEqual(expected, lines);
        }

        [TestMethod]
        public async Task TestOCRFromMordor()
        {
            var apiKey = System.Environment.GetEnvironmentVariable("VISION_API_KEY");
            Assert.IsFalse(string.IsNullOrWhiteSpace(apiKey), "Must provide API Key");

            var imageToText = new ImageToText(apiKey);
            var mordorImage = "http://i.imgur.com/5ocZvsW.jpg";
            var mordorTxt = new[] { "ONE DOES NOT", "SIMPLY", "OCR SOME TEXT FROM AN", "IMAGE" };
            var result = await imageToText.ProcessImageToTextAsync(await ImageUtilities.SingleChannelAsync(new Uri(mordorImage), ImageUtilities.Channel.Blue));
            var lines = ImageToText.ExtractLinesFromResponse(result).ToList();
            lines.RemoveAt(lines.Count - 1); // Remove memegenerator.net line
            CollectionAssert.AreEqual(mordorTxt, lines, "[{0}] != [{1}]", string.Join(",", mordorTxt), string.Join(",", lines));
        }

        [TestMethod]
        public async Task TestOCRFromMorpheus()
        {
            var apiKey = System.Environment.GetEnvironmentVariable("VISION_API_KEY");
            Assert.IsFalse(string.IsNullOrWhiteSpace(apiKey), "Must provide API Key");

            var imageToText = new ImageToText(apiKey);
            var morpheusImage = "http://i.imgur.com/1wL61Ro.jpg";
            var morpheusText = new[] { "WHAT IF I TOLD", "YOU", "IT WAS STARING YOU RIGHT IN", "THE FACE?" };
            var result = await imageToText.ProcessImageToTextAsync(await ImageUtilities.GammaAsync(new Uri(morpheusImage), 2.5));
            var lines = ImageToText.ExtractLinesFromResponse(result).ToList();
            Assert.IsTrue(lines.Any());
            lines.RemoveAt(lines.Count - 1); // Remove memegenerator.net line
            CollectionAssert.AreEqual(morpheusText, lines, "[{0}] != [{1}]", string.Join(",", morpheusText), string.Join(",", lines));
        }
    }
}
