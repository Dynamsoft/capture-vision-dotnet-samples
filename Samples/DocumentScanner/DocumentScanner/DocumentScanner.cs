using Dynamsoft.CVR;
using Dynamsoft.License;
using Dynamsoft.Core;
using Dynamsoft.Utility;
using Dynamsoft.DDN;

namespace DocumentScanner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int errorCode = 1;
            string errorMsg;

            // Initialize license.
            // You can request and extend a trial license from https://www.dynamsoft.com/customer/license/trialLicense?product=ddn&utm_source=samples&package=dotnet
            // The string 'DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9' here is a free public trial license. Note that network connection is required for this license to work.
            errorCode = LicenseManager.InitLicense("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9", out errorMsg);
            if (errorCode != (int)EnumErrorCode.EC_OK && errorCode != (int)EnumErrorCode.EC_LICENSE_CACHE_USED)
            {
                Console.WriteLine("License initialization failed: ErrorCode: " + errorCode + ", ErrorString: " + errorMsg);
            }
            else
            {
                using (CaptureVisionRouter cvr = new CaptureVisionRouter())
                {
                    while (true)
                    {
                        Console.WriteLine("\n>> Input your image full path:");
                        Console.WriteLine("\n>> 'Enter' for sample image or 'Q'/'q' to quit");
                        string? imageFile = Console.ReadLine();                        
                        if (imageFile.ToLower() == "q")
                        {
                            return;
                        }
                        if (string.IsNullOrEmpty(imageFile))
                        {
                            imageFile = "../../../../../../Images/document-sample.jpg";
                        }
                        string imagePathTrim = imageFile.Trim('\"');
                        if (!File.Exists(imagePathTrim))
                        {
                            Console.WriteLine("The image does not exist.");
                            continue;
                        }

                        using (CapturedResult? result = cvr.Capture(imageFile, PresetTemplate.PT_DETECT_AND_NORMALIZE_DOCUMENT))
                        {
                            if (result == null)
                            {
                                Console.WriteLine("No normalized documents.");
                            }
                            else
                            {
                                if (result.GetErrorCode() != 0)
                                {
                                    Console.WriteLine("Error: " + result.GetErrorCode() + ", " + result.GetErrorString());
                                }
                                NormalizedImagesResult? normalizedImagesResult = result.GetNormalizedImagesResult();
                                if (normalizedImagesResult != null)
                                {
                                    NormalizedImageResultItem[] items = normalizedImagesResult.GetItems();
                                    Console.WriteLine("Normalized " + items.Length + " documents");
                                    foreach (NormalizedImageResultItem normalizedItem in items)
                                    {
                                        string outPath = "normalizedResult_" + Array.IndexOf(items, normalizedItem) + ".png";
                                        ImageManager imageManager = new ImageManager();
                                        var image = normalizedItem.GetImageData();
                                        if (image != null)
                                        {
                                            errorCode = imageManager.SaveToFile(image, outPath);
                                            if (errorCode == 0)
                                            {
                                                Console.WriteLine("Document " + Array.IndexOf(items, normalizedItem) + " file: " + outPath);
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