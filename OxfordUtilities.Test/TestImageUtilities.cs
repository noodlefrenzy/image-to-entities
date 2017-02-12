using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OxfordUtilities.Test
{
    [TestClass]
    public class TestImageUtilities
    {
        public static string OutputRoot = Path.GetTempPath();

        [TestMethod]
        public async Task TestMordor()
        {
            await TestAll("http://i.imgur.com/5ocZvsW.jpg", "mordor");
        }

        [TestMethod]
        public async Task TestMorpheus()
        {
            await TestAll("http://i.imgur.com/1wL61Ro.jpg", "morpheus");
        }

        public async Task TestAll(string image, string prefix)
        {
            await TestInvert(image, prefix);
            await TestGreyscale(image, prefix);
            await TestGamma(image, prefix, 0.6);
            await TestGamma(image, prefix, 3);
            await TestSingleChannel(image, prefix, true);
            await TestSingleChannel(image, prefix, false);
        }

        public async Task TestInvert(string image, string prefix)
        {
            var inverted = string.Format(OutputRoot + "{0}_inverted.png", prefix);
            using (var stream = await ImageUtilities.InvertAsync(new Uri(image)))
            using (var outStream = new FileStream(inverted, FileMode.OpenOrCreate, FileAccess.Write))
            {
                await stream.CopyToAsync(outStream);
            }
        }

        public async Task TestGreyscale(string image, string prefix)
        {
            var grey = string.Format(OutputRoot + "{0}_greyscale.png", prefix);
            using (var stream = await ImageUtilities.GreyscaleAsync(new Uri(image)))
            using (var outStream = new FileStream(grey, FileMode.OpenOrCreate, FileAccess.Write))
            {
                await stream.CopyToAsync(outStream);
            }
        }

        public async Task TestGamma(string image, string prefix, double gamma)
        {
            var gammaFile = string.Format(OutputRoot + "{0}_gamma_{1}.png", prefix, gamma.ToString().Replace('.','_'));
            using (var stream = await ImageUtilities.GammaAsync(new Uri(image), gamma))
            using (var outStream = new FileStream(gammaFile, FileMode.OpenOrCreate, FileAccess.Write))
            {
                await stream.CopyToAsync(outStream);
            }
        }

        public async Task TestSingleChannel(string image, string prefix, bool cloneChannel)
        {
            var channelFile = OutputRoot + "{0}_channel_{1}{2}.png";
            foreach (var channel in Enum.GetValues(typeof(ImageUtilities.Channel)))
            {
                var outfile = string.Format(channelFile, prefix, channel, cloneChannel ? "_cloned" : "");
                using (var stream = await ImageUtilities.SingleChannelAsync(new Uri(image), (ImageUtilities.Channel)channel, cloneChannel))
                using (var outStream = new FileStream(outfile, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    await stream.CopyToAsync(outStream);
                }
            }
        }
    }
}
