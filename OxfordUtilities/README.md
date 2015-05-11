# image-to-entities

C# using Project Oxford to convert images to entities via Vision API's OCR => LUIS

Tests that call the API expect environment variables containing the keys/appids:

* VISION_API_KEY for ImageToText / OCR
* LUIS_APP_ID / LUIS_API_KEY for TextToEntitiesAndIntent

Note that I'm handing back JObjects from the calls - check the test cases for parsing details.  
ImageToText.Extract* are some utility methods for pulling lines etc. from the OCR results.
