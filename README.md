# image-to-entities

C# using Project Oxford to convert images to entities via Vision API's OCR => LUIS

# Introduction

Microsoft's [Project Oxford](https://www.projectoxford.ai/) is a suite of MSR-built (with help from other internal teams) APIs for various high-level ML tasks, offered via the Azure Marketplace.

We focus here on their [Vision API](https://www.projectoxford.ai/vision) for image categorization and (in our case) OCR, and [LUIS](https://www.projectoxford.ai/luis) - their service for Language Understanding.

# Getting Started

1. Head to the API pages above and sign up for the services involved - LUIS is invite-only still so it might take some time to get in.
1. Once you have the API keys, clone this repo (or fork/clone) and take a look at the code.
  1. Specifically, take a look at [TestImageToText.cs](https://github.com/noodlefrenzy/image-to-entities/blob/master/OxfordUtilities.Test/TestImageToText.cs) and [TestTextToEntitiesAndIntent.cs](https://github.com/noodlefrenzy/image-to-entities/blob/master/OxfordUtilities.Test/TestTextToEntitiesAndIntent.cs)
  1. These tests are not proper "unit" tests since they invoke the services and don't mock them out - they're meant to make it easier for you to try things out. To use them, set the environment variables listed in the tests with the keys you've received upon signup.
1. Note that for the LUIS-related tests, you'll need to have published a trained model. Follow the directions in LUIS to train and publish - it's easy.

# More Information

Please take a look at my [blog post on the subject](http://www.mikelanzetta.com/2015/05/using-project-oxford-to-pull-entities-from-images/) for additional details, and hit me up either with comments on that or GitHub issues if you have other questions.

# License

MIT