
using Dynamsoft.Core;
using Dynamsoft.CVR;
using Dynamsoft.DBR;
using Dynamsoft.DCP;
using Dynamsoft.License;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GS1AIScanner
{
    internal class GS1AIResult
    {
        public class GS1AIData
        {
            public string AI;
            public string Description;
            public string Value;
        }

        private List<GS1AIData> datas;

        public GS1AIResult(ParsedResultItem item)
        {
            Dictionary<string, GS1AIData> records = new Dictionary<string, GS1AIData>();
            int count = item.GetFieldCount();
            for (int i = 0; i < count; i++)
            {
                string name = item.GetFieldName(i);
                string value = item.GetFieldValue(name);

                string[] paths = name.Split('.');
                if (paths.Length == 2)
                {
                    string ai = paths[0];
                    if (!records.ContainsKey(ai))
                        records.Add(ai, new GS1AIData() { AI = ai });

                    if (paths[1] == ai + "AI")
                    {
                        records[ai].Description = value;
                    }
                    else if (paths[1] == ai + "Data")
                    {
                        records[ai].Value = value;
                    }
                }
            }

            datas = new List<GS1AIData>();
            foreach (var value in records.Values)
                datas.Add(value);

            datas.Sort((x, y) => x.AI.CompareTo(y.AI));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in datas)
            {
                sb.Append($"AI: {item.AI} ({item.Description}), Value: {item.Value}\n");
            }

            return sb.ToString();
        }
    }

    internal class Program
    {
        public static void PrintResult(DecodedBarcodesResult barcodesResult, ParsedResult parsedResult)
        {
            BarcodeResultItem[] barcodeItems = barcodesResult.GetItems();
            ParsedResultItem[] parsedItems = parsedResult.GetItems();
            int length = Math.Min(barcodeItems.Length, parsedItems.Length);
            for (int i = 0; i < length; i++)
            {
                Console.WriteLine($"Barcode result: {barcodeItems[i].GetText()}");
                var dlResult = new GS1AIResult(parsedItems[i]);
                string str = dlResult.ToString();
                if (str == "")
                {
                    Console.WriteLine("No Parsed GS1 Application Identifiers (AIs) detected.");
                }
                else
                {
                    Console.WriteLine("Parsed GS1 Application Identifiers (AIs):");
                    Console.WriteLine(str);
                }
            }
        }

        internal static void Main(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            Console.WriteLine("**********************************************************");
            Console.WriteLine("Welcome to Dynamsoft Capture Vision - GS1AIScanner Sample");
            Console.WriteLine("**********************************************************");

            int errorCode = 1;
            string errorMsg;

            // Initialize license.
            // You can request and extend a trial license from https://www.dynamsoft.com/customer/license/trialLicense?product=dcv&utm_source=samples&package=dotnet
            // The string 'DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9' here is a free public trial license. Note that network connection is required for this license to work.
            errorCode = LicenseManager.InitLicense("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9", out errorMsg);
            if (errorCode != (int)EnumErrorCode.EC_OK && errorCode != (int)EnumErrorCode.EC_UNSUPPORTED_JSON_KEY_WARNING)
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
                        string imagePath = Console.ReadLine();

                        if (imagePath.ToLower() == "q")
                        {
                            return;
                        }
                        if (string.IsNullOrEmpty(imagePath))
                        {
                            imagePath = "../../../../../../Images/gs1-ai-sample.png";
                        }
                        imagePath = imagePath.Trim('\"');
                        if (!File.Exists(imagePath))
                        {
                            Console.WriteLine("The image does not exist.");
                            continue;
                        }

                        string templatePath = "../../../../../../CustomTemplates/ReadGS1AIBarcode.json";
                        errorCode = cvRouter.InitSettingsFromFile(templatePath, out errorMsg);
                        if (errorCode == (int)EnumErrorCode.EC_UNSUPPORTED_JSON_KEY_WARNING)
                        {
                            Console.WriteLine("Warning: " + errorCode + ", " + errorMsg);
                        }
                        else if (errorCode != (int)EnumErrorCode.EC_OK)
                        {
                            Console.WriteLine("Error: " + errorCode + ", " + errorMsg);
                            continue;
                        }

                        CapturedResult[] results = cvRouter.CaptureMultiPages(imagePath, "");
                        if (results == null || results.Length == 0)
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
                                else if (result.GetErrorCode() != (int)EnumErrorCode.EC_OK)
                                {
                                    Console.WriteLine("Error: " + result.GetErrorCode() + ", " + result.GetErrorString());
                                }

                                FileImageTag tag = result.GetOriginalImageTag() as FileImageTag;
                                int pageNumber = tag != null ? tag.GetPageNumber() : index;

                                DecodedBarcodesResult barcodeResult = result.GetDecodedBarcodesResult();
                                ParsedResult parsedResult = result.GetParsedResult();
                                if (barcodeResult == null || barcodeResult.GetItems().Length == 0 || parsedResult == null || parsedResult.GetItems().Length == 0)
                                {
                                    Console.WriteLine("Page-" + (pageNumber + 1) + " No parsed results.");
                                }
                                else
                                {
                                    Console.WriteLine("Page-" + (pageNumber + 1) + " Parsed.");
                                    PrintResult(barcodeResult, parsedResult);
                                }
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Press Enter to quit.");
            Console.ReadLine();
        }
    }
}