using Dynamsoft.Core;
using Dynamsoft.CVR;
using Dynamsoft.DBR;
using Dynamsoft.License;
using Dynamsoft.DDN;
using Dynamsoft.Utility;
using Dynamsoft.DLR;

namespace CaptureFromAnImage
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int errorCode = 1;
            string errorMsg;
            errorCode = LicenseManager.InitLicense("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9", out errorMsg);
            if (errorCode != (int)EnumErrorCode.EC_OK)
            {
                Console.WriteLine("License initialization: " + errorCode + "," + errorMsg);
            }
            using (CaptureVisionRouter cvr = new CaptureVisionRouter())
            {
                string imageFile = "../../../../../../Images/dcv-sample-image.png";
                CapturedResult? result = cvr.Capture(imageFile, PresetTemplate.PT_DEFAULT);
                Console.WriteLine("File: " + imageFile);
                if (result == null)
                {
                    Console.WriteLine("No captured result.");
                }
                else if (result.GetErrorCode() != 0)
                {
                    Console.WriteLine("Error: " + result.GetErrorCode() + "," + result.GetErrorString());
                }
                else
                {
                    CapturedResultItem[] items = result.GetItems();
                    Console.WriteLine("Captured " + items.Length + " items");
                    foreach (CapturedResultItem item in items)
                    {
                        EnumCapturedResultItemType type = item.GetCapturedResultItemType();
                        if (item is BarcodeResultItem && type == EnumCapturedResultItemType.CRIT_BARCODE)
                        {
                            BarcodeResultItem barcodeItem = (BarcodeResultItem)item;
                            Console.WriteLine(">>Item " + Array.IndexOf(items, item) + ": " + "Barcode," + barcodeItem.GetText());
                        }
                        else if (item is TextLineResultItem && type == EnumCapturedResultItemType.CRIT_TEXT_LINE)
                        {
                            TextLineResultItem textLine = (TextLineResultItem)item;
                            Console.WriteLine(">>Item " + Array.IndexOf(items, item) + ": " + "TextLine," + textLine.GetText());
                        }
                        else if (item is NormalizedImageResultItem && type == EnumCapturedResultItemType.CRIT_NORMALIZED_IMAGE)
                        {
                            NormalizedImageResultItem normalizedImage = (NormalizedImageResultItem)item;
                            ImageManager manager = new ImageManager();
                            ImageData? data = normalizedImage.GetImageData();
                            if (data != null)
                            {
                                string outPath = "normalizedResult_" + Array.IndexOf(items, item) + ".png";
                                errorCode = manager.SaveToFile(data, outPath);
                                if (errorCode == 0)
                                    Console.WriteLine(">>Item " + Array.IndexOf(items, item) + ": " + "NormalizedImage," + outPath);
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