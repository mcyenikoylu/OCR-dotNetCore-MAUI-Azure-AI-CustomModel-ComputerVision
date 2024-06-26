﻿using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.AI.FormRecognizer.Models;

namespace OCRVisionTest
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        public string strlines = "";
        public Stream sourceStream = null;
        // // Add your Computer Vision subscription key and endpoint
        // static string subscriptionKey = "5e7857a4a3d145ecbd33b1b28b69e5d5";
        // static string endpoint = "https://gasolinereceipt.cognitiveservices.azure.com/";
        // public string READ_TEXT_URL_IMAGE = "https://www.spailor.com/wp-content/uploads/azureai/IMG_4435.jpg";// "https://raw.githubusercontent.com/Bliitze/VIN-Pate-Image/main/1200x0.jpg";

        /// <summary>
        /// Azure AI Custom Model
        /// </summary>
        static string endpoint = "https://eastus.api.cognitive.microsoft.com/";
        static string apiKey = "4f4a8982dbe844e6888e8cc404665e20";
        static public AzureKeyCredential credential = new AzureKeyCredential(apiKey);
        DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(endpoint), credential);
        string modelId = "receipt-gas-tr-model-mcy";
        Uri fileUri = new Uri("https://www.spailor.com/wp-content/uploads/azureai/IMG_4437.jpg");

        public MainPage()
        {
            InitializeComponent();

            //TxtUrl.Text = READ_TEXT_URL_IMAGE;
        }

        private async void OnCounterClicked(object sender, EventArgs e)
        {
            try
            {
                // // var pickResult = await FilePicker.PickAsync(new PickOptions()
                // // {
                // //     PickerTitle = "Pick jpeg or png image",
                // //     // Currently usable image types
                // //     FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>()
                // //     {
                // //         [DevicePlatform.Android] = new List<string>() { "image/png", "image/jpeg" },
                // //         [DevicePlatform.WinUI] = new List<string>() { ".png", ".jpg", ".jpeg" },
                // //         [DevicePlatform.iOS] = new List<string>() { "image/png", "image/jpeg" },
                // //     })
                // // });
                // // // null if user cancelled the operation
                // // if (pickResult is null)
                // // {
                // //     return;
                // // }

                // //TakePhoto();

                // READ_TEXT_URL_IMAGE = TxtUrl.Text;
                // //TxtUrl.Text = READ_TEXT_URL_IMAGE;
                // //pickResult.FullPath;

                // image1.Source = READ_TEXT_URL_IMAGE;
                // resultlb.Text = "Analyzing...";

                // // Create a client
                // ComputerVisionClient client = Authenticate(endpoint, subscriptionKey);

                // // Extract text (OCR) from a URL image using the Read API
                // await ReadFileUrl(client, READ_TEXT_URL_IMAGE);//READ_TEXT_URL_IMAGE

                AnalyzeDocumentOperation operation = await client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, modelId, fileUri);
                AnalyzeResult result = operation.Value;

                Console.WriteLine($"Document was analyzed with model with ID: {result.ModelId}");

                foreach (AnalyzedDocument document in result.Documents)
                {
                    Console.WriteLine($"Document of type: {document.DocumentType}");

                    foreach (KeyValuePair<string, DocumentField> fieldKvp in document.Fields)
                    {
                        string fieldName = fieldKvp.Key;
                        DocumentField field = fieldKvp.Value;

                        Console.WriteLine($"Field '{fieldName}': ");

                        Console.WriteLine($"  Content: '{field.Content}'");
                        Console.WriteLine($"  Confidence: '{field.Confidence}'");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "ok");
            }
        }
        // public async void TakePhoto()
        // {
        //     // if (MediaPicker.Default.IsCaptureSupported)
        //     // {
        //     FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

        //     if (photo != null)
        //     {
        //         // save the file into local storage
        //         string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
        //         READ_TEXT_URL_IMAGE = localFilePath;
        //         sourceStream = await photo.OpenReadAsync();
        //         using FileStream localFileStream = File.OpenWrite(localFilePath);

        //         await sourceStream.CopyToAsync(localFileStream);
        //         image1.Source = localFilePath;

        //     }
        //     // }
        // }

        public ComputerVisionClient Authenticate(string endpoint, string key)
        {
            ComputerVisionClient client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
            return client;
        }

        public async Task ReadFileUrl(ComputerVisionClient client, string urlFile)//string urlFile
        {
            // Read text from URL
            var textHeaders = await client.ReadAsync(urlFile);

            // After the request, get the operation location (operation ID)
            string operationLocation = textHeaders.OperationLocation;
            //Thread.Sleep(2000);

            // Retrieve the URI where the extracted text will be stored from the Operation-Location header.
            // We only need the ID and not the full URL
            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            // Extract the text
            ReadOperationResult results;

            do
            {
                results = await client.GetReadResultAsync(Guid.Parse(operationId));
            }
            while
            (
                (results.Status == OperationStatusCodes.Running || results.Status == OperationStatusCodes.NotStarted)
            );

            // Display the found text.
            // Console.WriteLine();
            var textUrlFileResults = results.AnalyzeResult.ReadResults;

            foreach (ReadResult page in textUrlFileResults)
            {
                foreach (Line line in page.Lines)
                {
                    Console.WriteLine(line.Text);
                    strlines = strlines + line.Text;
                }
            }
            resultlb.Text = "Result: " + strlines;
            // Console.WriteLine();
        }
        //
    }
}