using Dynamsoft.Core;
using Dynamsoft.CVR;
using Dynamsoft.DCP;
using Dynamsoft.License;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace DriverLicenseScanner
{
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
                Console.WriteLine("Detected " + items.Length + " Driver License(s).");
                foreach (var item in items)
                {
                    string codeType = item.GetCodeType();
                    List<KeyValuePair<string, string>> printMap = new List<KeyValuePair<string, string>>();
                    printMap.Add(new KeyValuePair<string, string>("Code Type", codeType));
                    if (codeType == "AAMVA_DL_ID")
                    {
                        // For full field list and details, please refer to the documentation:
                        // https://www.dynamsoft.com/code-parser/docs/core/code-types/aamva-dl-id.html?lang=dotnet#aamva_dl_id-fields
                        string licenseNumber = item.GetFieldValue("licenseNumber");
                        if (!string.IsNullOrEmpty(licenseNumber) && item.GetFieldValidationStatus("licenseNumber") != EnumValidationStatus.VS_FAILED)
                        {
                            printMap.Add(new KeyValuePair<string, string>("License Number", licenseNumber));
                        }
                        string vehicleClass = item.GetFieldValue("vehicleClass");
                        if (!string.IsNullOrEmpty(vehicleClass) && item.GetFieldValidationStatus("vehicleClass") != EnumValidationStatus.VS_FAILED)
                        {
                            printMap.Add(new KeyValuePair<string, string>("Vehicle Class", vehicleClass));
                        }
                        string fullName = item.GetFieldValue("fullName");
                        if (!string.IsNullOrEmpty(fullName) && item.GetFieldValidationStatus("fullName") != EnumValidationStatus.VS_FAILED)
                        {
                            printMap.Add(new KeyValuePair<string, string>("Full Name", fullName));
                        }
                        else
                        {
                            string givenName = item.GetFieldValue("givenName");
                            if (!string.IsNullOrEmpty(givenName) && item.GetFieldValidationStatus("givenName") != EnumValidationStatus.VS_FAILED)
                            {
                                fullName = givenName;
                                string lastName = item.GetFieldValue("lastName");
                                if (!string.IsNullOrEmpty(lastName) && item.GetFieldValidationStatus("lastName") != EnumValidationStatus.VS_FAILED)
                                {
                                    fullName += " " + lastName;
                                }
                                printMap.Add(new KeyValuePair<string, string>("Full Name", fullName));
                            }
                            else
                            {

                                string firstName = item.GetFieldValue("firstName");
                                string middleName = item.GetFieldValue("middleName");
                                string lastName = item.GetFieldValue("lastName");
                                if (!string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(middleName) || !string.IsNullOrEmpty(lastName))
                                {
                                    if (item.GetFieldValidationStatus("firstName") == EnumValidationStatus.VS_FAILED)
                                    {
                                        firstName = null;
                                    }
                                    if (item.GetFieldValidationStatus("middleName") == EnumValidationStatus.VS_FAILED)
                                    {
                                        middleName = null;
                                    }
                                    if (item.GetFieldValidationStatus("lastName") == EnumValidationStatus.VS_FAILED)
                                    {
                                        lastName = null;
                                    }
                                    fullName = (firstName ?? "") + (string.IsNullOrEmpty(middleName) ? "" : " " + middleName) + (string.IsNullOrEmpty(lastName) ? "" : " " + lastName);
                                    printMap.Add(new KeyValuePair<string, string>("Full Name", fullName));
                                }
                            }
                        }
                        string sex = item.GetFieldValue("sex");
                        if (item.GetFieldValidationStatus("sex") != EnumValidationStatus.VS_FAILED)
                        {
                            printMap.Add(new KeyValuePair<string, string>("Sex", sex));
                        }
                        string expirationDate = item.GetFieldValue("expirationDate");
                        if (!string.IsNullOrEmpty(expirationDate) && item.GetFieldValidationStatus("expirationDate") != EnumValidationStatus.VS_FAILED)
                        {
                            printMap.Add(new KeyValuePair<string, string>("Expiration Date", expirationDate));
                        }
                    }
                    else if (codeType == "AAMVA_DL_ID_WITH_MAG_STRIPE")
                    {
                        // For full field list and details, please refer to the documentation:
                        // https://www.dynamsoft.com/code-parser/docs/core/code-types/aamva-dl-id.html?lang=dotnet#aamva_dl_id_with_mag_stripe-fields

                        string licenseNumber = item.GetFieldValue("DLorID_Number");
                        if (!string.IsNullOrEmpty(licenseNumber) && item.GetFieldValidationStatus("DLorID_Number") != EnumValidationStatus.VS_FAILED)
                        {
                            printMap.Add(new KeyValuePair<string, string>("License Number", licenseNumber));
                        }
                        string isoIIN = item.GetFieldValue("ISOIIN");
                        if (!string.IsNullOrEmpty(isoIIN) && item.GetFieldValidationStatus("ISOIIN") != EnumValidationStatus.VS_FAILED)
                        {
                            printMap.Add(new KeyValuePair<string, string>("ISO IIN", isoIIN));
                        }
                        string name = item.GetFieldValue("name");
                        if (!string.IsNullOrEmpty(name) && item.GetFieldValidationStatus("name") != EnumValidationStatus.VS_FAILED)
                        {
                            printMap.Add(new KeyValuePair<string, string>("Name", name));
                        }
                        string sex = item.GetFieldValue("sex");
                        if (!string.IsNullOrEmpty(sex) && item.GetFieldValidationStatus("sex") != EnumValidationStatus.VS_FAILED)
                        {
                            printMap.Add(new KeyValuePair<string, string>("Sex", sex));
                        }
                        string expirationDate = item.GetFieldValue("expirationDate");
                        if (!string.IsNullOrEmpty(expirationDate) && item.GetFieldValidationStatus("expirationDate") != EnumValidationStatus.VS_FAILED)
                        {
                            printMap.Add(new KeyValuePair<string, string>("Expiration Date", expirationDate));
                        }
                    }
                    else if (codeType == "SOUTH_AFRICA_DL")
                    {
                        // For full field list and details, please refer to the documentation:
                        // https://www.dynamsoft.com/code-parser/docs/core/code-types/za-dl.html?lang=dotnet#fields

                        string idNumber = item.GetFieldValue("idNumber");
                        if (!string.IsNullOrEmpty(idNumber) && item.GetFieldValidationStatus("idNumber") != EnumValidationStatus.VS_FAILED)
                        {
                            printMap.Add(new KeyValuePair<string, string>("ID Number", idNumber));
                        }
                        string licenseNumber = item.GetFieldValue("licenseNumber");
                        if (!string.IsNullOrEmpty(licenseNumber) && item.GetFieldValidationStatus("licenseNumber") != EnumValidationStatus.VS_FAILED)
                        {
                            printMap.Add(new KeyValuePair<string, string>("License Number", licenseNumber));
                        }
                        string vehicleLicense = item.GetFieldValue("vehicleLicense");
                        if (!string.IsNullOrEmpty(vehicleLicense) && item.GetFieldValidationStatus("vehicleLicense") != EnumValidationStatus.VS_FAILED)
                        {
                            printMap.Add(new KeyValuePair<string, string>("Vehicle License", vehicleLicense));
                        }
                        string surname = item.GetFieldValue("surname");
                        string initials = item.GetFieldValue("initials");
                        if (!string.IsNullOrEmpty(surname) || !string.IsNullOrEmpty(initials))
                        {
                            if (item.GetFieldValidationStatus("surname") == EnumValidationStatus.VS_FAILED)
                            {
                                surname = null;
                            }
                            if (item.GetFieldValidationStatus("initials") == EnumValidationStatus.VS_FAILED)
                            {
                                initials = null;
                            }
                            string name = (surname ?? "") + (string.IsNullOrEmpty(initials) ? "" : " " + initials);
                            printMap.Add(new KeyValuePair<string, string>("Name", name));
                        }
                        string gender = item.GetFieldValue("gender");
                        if (!string.IsNullOrEmpty(gender) && item.GetFieldValidationStatus("gender") != EnumValidationStatus.VS_FAILED)
                        {
                            printMap.Add(new KeyValuePair<string, string>("Gender", gender));
                        }
                        string licenseValidityTo = item.GetFieldValue("licenseValidityTo");
                        if (!string.IsNullOrEmpty(licenseValidityTo) && item.GetFieldValidationStatus("licenseValidityTo") != EnumValidationStatus.VS_FAILED)
                        {
                            printMap.Add(new KeyValuePair<string, string>("License Valid To", licenseValidityTo));
                        }
                    }
                    else
                    {
                        Console.WriteLine("Unsupported code type: " + codeType);
                        continue;
                    }
                    Console.WriteLine("Parsed Information:");
                    foreach (var pair in printMap)
                    {
                        Console.WriteLine("\t" + pair.Key + ": " + pair.Value);
                    }
                    Console.WriteLine();
                }
            }
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            Console.WriteLine("**********************************************************");
            Console.WriteLine("Welcome to Dynamsoft Capture Vision - DriverLicense Sample");
            Console.WriteLine("**********************************************************");

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
                        string imagePath = Console.ReadLine();

                        if (imagePath.ToLower() == "q")
                        {
                            return;
                        }
                        if (string.IsNullOrEmpty(imagePath))
                        {
                            imagePath = "../../../../../../Images/driver-license-sample.jpg";
                        }
                        imagePath = imagePath.Trim('\"');
                        if (!File.Exists(imagePath))
                        {
                            Console.WriteLine("The image does not exist.");
                            continue;
                        }

                        CapturedResult[] results = cvRouter.CaptureMultiPages(imagePath, "ReadDriversLicense");
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

                                ParsedResult parsedResult = result.GetParsedResult();
                                if (parsedResult == null || parsedResult.GetItems().Length == 0)
                                {
                                    Console.WriteLine("Page-" + (pageNumber + 1) + " No parsed results.");
                                }
                                else
                                {
                                    Console.WriteLine("Page-" + (pageNumber + 1) + " Parsed.");
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
