using Dynamsoft.Core;
using Dynamsoft.CVR;
using Dynamsoft.DBR;
using Dynamsoft.Utility;
using Dynamsoft.License;
using Dynamsoft.DLR;
using Dynamsoft.DDN;
namespace CaptureFromMultipleImages
{
    class MyCapturedResultReceiver : CapturedResultReceiver
    {
        public override void OnCapturedResultReceived(CapturedResult result)
        {
            FileImageTag? tag = (FileImageTag?)result.GetOriginalImageTag();
            Console.WriteLine(tag!.GetFilePath());
            Console.WriteLine(tag!.GetPageNumber());
            if (result.GetErrorCode() != (int)EnumErrorCode.EC_OK)
            {
                Console.WriteLine("Error: " + result.GetErrorString());
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
                            int errorCode = manager.SaveToFile(data, outPath);
                            if (errorCode == 0)
                                Console.WriteLine(">>Item " + Array.IndexOf(items, item) + ": " + "NormalizedImage," + outPath);
                        }
                    }

                }
            }
        }
    }
    class MyImageSourceStateListener : IImageSourceStateListener
    {
        private CaptureVisionRouter? cvr = null;
        public MyImageSourceStateListener(CaptureVisionRouter cvr)
        {
            this.cvr = cvr;
        }

        public void OnImageSourceStateReceived(EnumImageSourceState state)
        {
            if (state == EnumImageSourceState.ISS_EXHAUSTED)
            {
                if (cvr != null)
                {
                    cvr.StopCapturing();
                }
            }
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            int errorCode = 1;
            string errorMsg;
            errorCode = LicenseManager.InitLicense("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9", out errorMsg);
            if (errorCode != (int)EnumErrorCode.EC_OK && errorCode != (int)EnumErrorCode.EC_LICENSE_CACHE_USED)
            {
                Console.WriteLine("License initialization failed: ErrorCode: " + errorCode + ", ErrorString: " + errorMsg);
            }
            else
            {
                using (CaptureVisionRouter cvr = new CaptureVisionRouter())
                using (DirectoryFetcher fetcher = new DirectoryFetcher())
                {

                    string? imgPath = null;
                    while (imgPath == null)
                    {
                        Console.WriteLine(">> Input your image directory full path:");
                        imgPath = Console.ReadLine();
                    }

                    fetcher.SetDirectory(imgPath);
                    cvr.SetInput(fetcher);

                    CapturedResultReceiver receiver = new MyCapturedResultReceiver();
                    cvr.AddResultReceiver(receiver);

                    MyImageSourceStateListener listener = new MyImageSourceStateListener(cvr);
                    cvr.AddImageSourceStateListener(listener);

                    errorCode = cvr.StartCapturing(PresetTemplate.PT_DEFAULT, true, out errorMsg);
                    if (errorCode != (int)EnumErrorCode.EC_OK)
                    {
                        Console.WriteLine("error: " + errorMsg);
                    }
                }
            }
            Console.WriteLine("Press any key to quit...");
            Console.Read();
        }
    }
}