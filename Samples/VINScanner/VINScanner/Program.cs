using Dynamsoft.Core;
using Dynamsoft.CVR;
using Dynamsoft.License;
using Dynamsoft.DCP;
using System;
using System.IO;

namespace MRZScanner
{
    class VINResult
    {
        public string CodeType { get; set; }
        public string WMI { get; set; }
        public string Region { get; set; }
        public string VDS { get; set; }
        public string VIS { get; set; }
        public string ModelYear { get; set; }
        public string PlantCode { get; set; }
        public string VinString { get; set; }

        public VINResult(ParsedResultItem item)
        {
            CodeType = item.GetCodeType();

            if (CodeType != "VIN")
                return;

            string fieldValue;

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

    internal class Program
    {
        static public void PrintResult(ParsedResult result)
        {
            FileImageTag tag = (FileImageTag)result.GetOriginalImageTag();
            Console.WriteLine("File: " + tag?.GetFilePath());
            if (result.GetErrorCode() != (int)EnumErrorCode.EC_OK && result.GetErrorCode() != (int)EnumErrorCode.EC_UNSUPPORTED_JSON_KEY_WARNING)
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

        static void Main(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            Console.WriteLine("*************************************************");
            Console.WriteLine("Welcome to Dynamsoft Capture Vision - VIN Sample");
            Console.WriteLine("*************************************************");

            int errorCode = 1;
            string errorMsg;

            // Initialize license.
            // You can request and extend a trial license from https://www.dynamsoft.com/customer/license/trialLicense?product=dcv&utm_source=samples&package=dotnet
            // The string 'DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9' here is a free public trial license. Note that network connection is required for this license to work.
            errorCode = LicenseManager.InitLicense("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9", out errorMsg);
            if (errorCode != (int)EnumErrorCode.EC_OK && errorCode != (int)EnumErrorCode.EC_LICENSE_CACHE_USED)
            {
                Console.WriteLine("License initialization failed: ErrorCode: " + errorCode + ", ErrorString: " + errorMsg);
            }
            else
            {
                using (CaptureVisionRouter cvRouter = new CaptureVisionRouter())
                {
                    while (true)
                    {
                        Console.WriteLine("\n>> Step 1: Enter the image path (or 'Q'/'q' to quit):");
                        string imagePath = Console.ReadLine();
                        if (string.IsNullOrEmpty(imagePath))
                        {
                            Console.WriteLine("Invalid path.");
                            continue;
                        }
                        if (imagePath.ToLower() == "q")
                        {
                            return;
                        }
                        imagePath = imagePath.Trim('\"');
                        if (!File.Exists(imagePath))
                        {
                            Console.WriteLine("The image does not exist.");
                            continue;
                        }
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

                        CapturedResult[] results = cvRouter.CaptureMultiPages(imagePath, templateName);
                        if (results == null)
                        {
                            Console.WriteLine("No parsed results.");
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
                                else if (result.GetErrorCode() != 0)
                                {
                                    Console.WriteLine("Error: " + result.GetErrorCode() + ", " + result.GetErrorString());
                                }
                                ParsedResult parsedResult = result.GetParsedResult();
                                if (parsedResult == null || parsedResult.GetItems().Length == 0)
                                {
                                    Console.WriteLine("Page-" + (index + 1) + " No parsed results.");
                                }
                                else
                                {
                                    PrintResult(parsedResult);
                                }
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Press Enter to quit...");
            Console.Read();
        }
    }
}