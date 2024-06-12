using Dynamsoft.Core;
using Dynamsoft.CVR;
using Dynamsoft.License;
using Dynamsoft.Utility;
using Dynamsoft.DCP;
using System;

namespace MRZScanner
{
    class VINResult
    {
        public string? CodeType { get; set; }
        public string? WMI { get; set; }
        public string? Region { get; set; }
        public string? VDS { get; set; }
        public string? VIS { get; set; }
        public string? ModelYear { get; set; }
        public string? PlantCode { get; set; }
        public string? VinString { get; set; }

        public VINResult(ParsedResultItem item)
        {
            CodeType = item.GetCodeType();

            if (CodeType != "VIN")
                return ;

            string? fieldValue;

            fieldValue = item.GetFieldValue("vinString");
            if (fieldValue != null)
            {
                VinString = fieldValue;
                if (item.GetFieldValidationStatus("vinString") == EnumValidationStatus.VS_FAILED)
                {
                    VinString += ", Validation Failed";
                }
            }

            if (item.GetFieldValidationStatus("WMI") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("WMI") != null)
            {
                WMI = item.GetFieldValue("WMI");
            }
            if (item.GetFieldValidationStatus("region") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("region") != null)
            {
                Region = item.GetFieldValue("region");
            }
            if (item.GetFieldValidationStatus("VDS") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("VDS") != null)
            {
                VDS = item.GetFieldValue("VDS");
            }
            if (item.GetFieldValidationStatus("VIS") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("VIS") != null)
            {
                VIS = item.GetFieldValue("VIS");
            }
            if (item.GetFieldValidationStatus("modelYear") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("modelYear") != null)
            {
                ModelYear = item.GetFieldValue("modelYear");
            }
            if (item.GetFieldValidationStatus("plantCode") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("plantCode") != null)
            {
                PlantCode = item.GetFieldValue("plantCode");
            }
        }

        public override string ToString()
        {
            return $"VIN String: {VinString}\n" +
                   $"Parsed Information:\n" +
                   $"\tWMI: {WMI}\n" +
                   $"\tRegion: {Region}\n" +
                   $"\tVDS: {VDS}\n" +
                   $"\tVIS: {VIS}\n" +
                   $"\tModelYear: {ModelYear}\n" +
                   $"\tPlantCode: {PlantCode}\n";
        }
    }

    class MyCapturedResultReceiver : CapturedResultReceiver
    {
        public override void OnParsedResultsReceived(ParsedResult result)
        {
            FileImageTag? tag = (FileImageTag?)result.GetOriginalImageTag();
            Console.WriteLine("File: " + tag?.GetFilePath());
            if (result.GetErrorCode() != (int)EnumErrorCode.EC_OK)
            {
                Console.WriteLine("Error: " + result.GetErrorString());
            }
            else
            {
                ParsedResultItem[] items = result.GetItems();
                Console.WriteLine("Detected " + items.Length + " VIN(s).");
                foreach (var item in items)
                {
                    var vinResult = new VINResult(item);
                    Console.WriteLine(vinResult.ToString());
                }
            }
            Console.WriteLine();
        }
    }
    class MyImageSourceStateListener : IImageSourceStateListener
    {
        private CaptureVisionRouter cvr;
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
            Console.WriteLine("*************************************************");
            Console.WriteLine("Welcome to Dynamsoft Capture Vision - VIN Sample");
            Console.WriteLine("*************************************************");

            int errorCode = 1;
            string errorMsg;

            // You can request and extend a trial license from https://www.dynamsoft.com/customer/license/trialLicense?product=cvs&utm_source=samples
            // The string "DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9" here is a free public trial license. Note that network connection is required for this license to work.
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
                    errorCode = cvr.InitSettingsFromFile("VIN.json", out errorMsg);
                    if (errorCode != (int)EnumErrorCode.EC_OK)
                    {
                        Console.WriteLine("VIN template initialization failed: " + errorMsg);
                    }
                    else
                    {
                        while(true)
                        {
                            Console.WriteLine("\n>> Step 1: Enter the image directory path (or 'Q'/'q' to quit):");
                            string? imagePath = Console.ReadLine();
                            if (string.IsNullOrEmpty(imagePath))
                            {
                                Console.WriteLine("Invalid directory path.");
                                continue;
                            }
                            if (imagePath.ToLower() == "q")
                            {
                                return;
                            }
                            if (!Directory.Exists(imagePath))
                            {
                                Console.WriteLine("The directory does not exist.");
                                continue;
                            }

                            fetcher.SetDirectory(imagePath);
                            cvr.SetInput(fetcher);

                            CapturedResultReceiver receiver = new MyCapturedResultReceiver();
                            cvr.AddResultReceiver(receiver);

                            MyImageSourceStateListener listener = new MyImageSourceStateListener(cvr);
                            cvr.AddImageSourceStateListener(listener);

                            int iNum = 0;
                            string templateName = string.Empty;

                            do
                            {
                                Console.WriteLine("\n>> Step 2: Choose a Mode Number");
                                Console.WriteLine("   1. Read VIN from Barcode");
                                Console.WriteLine("   2. Read VIN from Text");

                                if (!int.TryParse(Console.ReadLine(), out iNum))
                                {
                                    Console.WriteLine("Invalid number.");
                                    iNum = 0;
                                }
                            } while (iNum < 1 || iNum > 2);

                            if (iNum == 1)
                                templateName = "ReadVINBarcode";
                            else if (iNum == 2)
                                templateName = "ReadVINText";

                            errorCode = cvr.StartCapturing(templateName, true, out errorMsg);
                        }

                    }

                }
            }
            Console.WriteLine("Press Enter to quit...");
            Console.Read();
        }
    }
}