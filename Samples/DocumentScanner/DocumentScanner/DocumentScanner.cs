using Dynamsoft.CVR;
using Dynamsoft.License;
using Dynamsoft.Core;
using Dynamsoft.Utility;
using Dynamsoft.DDN;
using System;
using System.IO;

namespace DocumentScanner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            int errorCode = 1;
            string errorMsg;

            // Initialize license.
            // You can request and extend a trial license from https://www.dynamsoft.com/customer/license/trialLicense?product=dcv&utm_source=samples&package=dotnet
            // The string 'DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9' here is a free public trial license. Note that network connection is required for this license to work.
            errorCode = LicenseManager.InitLicense("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9", out errorMsg);
            if (errorCode != (int)EnumErrorCode.EC_OK && errorCode != (int)EnumErrorCode.EC_LICENSE_WARNING)
            {
                Console.WriteLine("License initialization failed: ErrorCode: " + errorCode + ", ErrorString: " + errorMsg);
            }
            else
            {
                using (CaptureVisionRouter cvRouter = new CaptureVisionRouter())
                {
                    while (true)
                    {
                        Console.WriteLine("\n>> Input your image full path:");
                        Console.WriteLine("\n>> 'Enter' for sample image or 'Q'/'q' to quit");
                        string imageFile = Console.ReadLine();                        
                        if (imageFile.ToLower() == "q")
                        {
                            return;
                        }
                        if (string.IsNullOrEmpty(imageFile))
                        {
                            imageFile = "../../../../../../Images/document-sample.jpg";
                        }
                        imageFile = imageFile.Trim('\"');
                        if (!File.Exists(imageFile))
                        {
                            Console.WriteLine("The image does not exist.");
                            continue;
                        }

                        CapturedResult[] results = cvRouter.CaptureMultiPages(imageFile, PresetTemplate.PT_DETECT_AND_NORMALIZE_DOCUMENT);
                        if (results == null || results.Length == 0)
                        {
                            Console.WriteLine("No document found.");
                        }
                        else
                        {
                            for (int index = 0; index < results.Length; index++)
                            {
                                CapturedResult result = results[index];
                                if (result.GetErrorCode() == (int)EnumErrorCode.EC_UNSUPPORTED_JSON_KEY_WARNING)
                                {
                                    Console.WriteLine("Warning: " + result.GetErrorCode() + ", " + result.GetErrorString());
                                }
                                else if (result.GetErrorCode() != (int)EnumErrorCode.EC_OK)
                                {
                                    Console.WriteLine("Error: " + result.GetErrorCode() + ", " + result.GetErrorString());
                                }
                                ProcessedDocumentResult processedDocumentResult = result.GetProcessedDocumentResult();
                                if (processedDocumentResult == null || processedDocumentResult.GetEnhancedImageResultItems().Length == 0)
                                {
                                    Console.WriteLine("Page-" + (index + 1) + " No document found.");
                                }
                                else
                                {
                                    EnhancedImageResultItem[] items = processedDocumentResult.GetEnhancedImageResultItems();
                                    Console.WriteLine("Page-" + (index + 1) + " Enhanced " + items.Length + " documents");
                                    for (int i = 0; i < items.Length; i++)
                                    {
                                        EnhancedImageResultItem enhancedResult = items[i];
                                        string outPath = "Page_" + (index + 1) + "enhancedResult_" + i + ".png";
                                        ImageIO imageIo = new ImageIO();
                                        var image = enhancedResult.GetImageData();
                                        if (image != null)
                                        {
                                            errorCode = imageIo.SaveToFile(image, outPath);
                                            if (errorCode == 0)
                                            {
                                                Console.WriteLine("Document " + i + " file: " + outPath);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }
            Console.WriteLine("Press any key to quit...");
            Console.Read();
        }
    }
}