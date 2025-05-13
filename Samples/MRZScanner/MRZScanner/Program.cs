using Dynamsoft.Core;
using Dynamsoft.CVR;
using Dynamsoft.License;
using Dynamsoft.DCP;

namespace MRZScanner
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    class MRZResult
    {
        public string docId;
        public string docType;
        public string nationality;
        public string issuer;
        public string dateOfBirth;
        public string dateOfExpiry;
        public string gender;
        public string surname;
        public string givenname;

        public List<string> rawText = new List<string>();

        public MRZResult(ParsedResultItem item)
        {
            docType = item.GetCodeType();

            if (docType == "MRTD_TD3_PASSPORT")
            {
                if (item.GetFieldValidationStatus("passportNumber") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("passportNumber") != null)
                {
                    docId = item.GetFieldValue("passportNumber");
                }
            }
            else if (item.GetFieldValidationStatus("documentNumber") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("documentNumber") != null)
            {
                docId = item.GetFieldValue("documentNumber");
            }

            string line;
            line = item.GetFieldValue("line1");
            if (line != null)
            {
                if (item.GetFieldValidationStatus("line1") == EnumValidationStatus.VS_FAILED)
                {
                    line += ", Validation Failed";
                }
                rawText.Add(line);
            }

            line = item.GetFieldValue("line2");
            if (line != null)
            {
                if (item.GetFieldValidationStatus("line2") == EnumValidationStatus.VS_FAILED)
                {
                    line += ", Validation Failed";
                }
                rawText.Add(line);
            }

            line = item.GetFieldValue("line3");
            if (line != null)
            {
                if (item.GetFieldValidationStatus("line3") == EnumValidationStatus.VS_FAILED)
                {
                    line += ", Validation Failed";
                }
                rawText.Add(line);
            }

            if (item.GetFieldValidationStatus("nationality") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("nationality") != null)
            {
                nationality = item.GetFieldValue("nationality");
            }
            if (item.GetFieldValidationStatus("issuingState") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("issuingState") != null)
            {
                issuer = item.GetFieldValue("issuingState");
            }
            if (item.GetFieldValidationStatus("dateOfBirth") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("dateOfBirth") != null)
            {
                dateOfBirth = item.GetFieldValue("dateOfBirth");
            }
            if (item.GetFieldValidationStatus("dateOfExpiry") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("dateOfExpiry") != null)
            {
                dateOfExpiry = item.GetFieldValue("dateOfExpiry");
            }
            if (item.GetFieldValidationStatus("sex") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("sex") != null)
            {
                gender = item.GetFieldValue("sex");
            }
            if (item.GetFieldValidationStatus("primaryIdentifier") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("primaryIdentifier") != null)
            {
                surname = item.GetFieldValue("primaryIdentifier");
            }
            if (item.GetFieldValidationStatus("secondaryIdentifier") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("secondaryIdentifier") != null)
            {
                givenname = item.GetFieldValue("secondaryIdentifier");
            }
        }

        public override string ToString()
        {
            string msg = "Raw Text:\n";
            for (int idx = 0; idx < rawText.Count; ++idx)
            {
                msg += "\tLine " + (idx + 1) + ": " + rawText[idx] + "\n";
            }
            msg += "Parsed Information:\n";
            msg += "\tDocument Type: " + docType + "\n";
            msg += "\tDocument ID: " + docId + "\n";
            msg += "\tSurname: " + surname + "\n";
            msg += "\tGiven Name: " + givenname + "\n";
            msg += "\tNationality: " + nationality + "\n";
            msg += "\tIssuing Country or Organization: " + issuer + "\n";
            msg += "\tGender: " + gender + "\n";
            msg += "\tDate of Birth(YYMMDD): " + dateOfBirth + "\n";
            msg += "\tExpiration Date(YYMMDD): " + dateOfExpiry + "\n";

            return msg;
        }
    }

    internal class Program
    {
        public static void PrintResult(ParsedResult result)
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
                Console.WriteLine("Detected " + items.Length + " MRZ Result(s).");
                foreach (var item in items)
                {
                    var mrzResult = new MRZResult(item);
                    Console.WriteLine(mrzResult.ToString());
                }
            }
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            Console.WriteLine("*************************************************");
            Console.WriteLine("Welcome to Dynamsoft Capture Vision - MRZ Sample");
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
                        Console.WriteLine("\n>> Input your image full path:");
                        Console.WriteLine("\n>> 'Enter' for sample image or 'Q'/'q' to quit");
                        string imagePath = Console.ReadLine();
                        if (imagePath.ToLower() == "q")
                        {
                            return;
                        }
                        if (string.IsNullOrEmpty(imagePath))
                        {
                            imagePath = "../../../../../../Images/passport-sample.jpg";
                        }
                        imagePath = imagePath.Trim('\"');
                        if (!File.Exists(imagePath))
                        {
                            Console.WriteLine("The image does not exist.");
                            continue;
                        }

                        CapturedResult[] results = cvRouter.CaptureMultiPages(imagePath, "ReadPassportAndId");
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
                                else if (result.GetErrorCode() != (int)EnumErrorCode.EC_OK)
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