using Dynamsoft.Core;
using Dynamsoft.CVR;
using Dynamsoft.License;
using Dynamsoft.DCP;

namespace DriverLicenseScanner
{
    class DriverLicenseResult
    {
        public string? CodeType { get; set; }
        public string? VersionNumber { get; set; }
        public string? LicenseNumber { get; set; }
        public string? VehicleClass { get; set; }
        public string? FullName { get; set; }
        public string? LastName { get; set; }
        public string? GivenName { get; set; }
        public string? Gender { get; set; }
        public string? BirthDate { get; set; }
        public string? IssuedDate { get; set; }
        public string? ExpirationDate { get; set; }

        public DriverLicenseResult(ParsedResultItem item)
        {
            CodeType = item.GetCodeType();

            if (CodeType != "AAMVA_DL_ID" && CodeType != "AAMVA_DL_ID_WITH_MAG_STRIPE" && CodeType != "SOUTH_AFRICA_DL")
                return;

            if (item.GetFieldValidationStatus("licenseNumber") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("licenseNumber") != null)
            {
                LicenseNumber = item.GetFieldValue("licenseNumber");
            }
            if (item.GetFieldValidationStatus("AAMVAVersionNumber") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("AAMVAVersionNumber") != null)
            {
                VersionNumber = item.GetFieldValue("AAMVAVersionNumber");
            }
            if (item.GetFieldValidationStatus("vehicleClass") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("vehicleClass") != null)
            {
                VehicleClass = item.GetFieldValue("vehicleClass");
            }
            if (item.GetFieldValidationStatus("lastName") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("lastName") != null)
            {
                LastName = item.GetFieldValue("lastName");
            }
            if (item.GetFieldValidationStatus("surName") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("surName") != null)
            {
                LastName = item.GetFieldValue("surName");
            }
            if (item.GetFieldValidationStatus("givenName") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("givenName") != null)
            {
                GivenName = item.GetFieldValue("givenName");
            }
            if (item.GetFieldValidationStatus("fullName") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("fullName") != null)
            {
                FullName = item.GetFieldValue("fullName");
            }
            if (item.GetFieldValidationStatus("sex") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("sex") != null)
            {
                Gender = item.GetFieldValue("sex");
            }
            if (item.GetFieldValidationStatus("gender") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("gender") != null)
            {
                Gender = item.GetFieldValue("gender");
            }
            if (item.GetFieldValidationStatus("birthDate") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("birthDate") != null)
            {
                BirthDate = item.GetFieldValue("birthDate");
            }
            if (item.GetFieldValidationStatus("issuedDate") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("issuedDate") != null)
            {
                IssuedDate = item.GetFieldValue("issuedDate");
            }
            if (item.GetFieldValidationStatus("expirationDate") != EnumValidationStatus.VS_FAILED && item.GetFieldValue("expirationDate") != null)
            {
                ExpirationDate = item.GetFieldValue("expirationDate");
            }

            if (string.IsNullOrEmpty(FullName))
            {
                FullName = LastName + GivenName;
            }
        }

        public override string ToString()
        {
            return $"Parsed Information:\n" +
                   $"\tCode Type: {CodeType}\n" +
                   $"\tLicense Number: {LicenseNumber}\n" +
                   $"\tVehicle Class: {VehicleClass}\n" +
                   $"\tLast Name: {LastName}\n" +
                   $"\tGiven Name: {GivenName}\n" +
                   $"\tFull Name: {FullName}\n" +
                   $"\tGender: {Gender}\n" +
                   $"\tDate of Birth: {BirthDate}\n" +
                   $"\tIssued Date: {IssuedDate}\n" +
                   $"\tExpiration Date: {ExpirationDate}\n";
        }
    }



    internal class Program
    {
        public static void PrintResult(ParsedResult result)
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
                Console.WriteLine("Detected " + items.Length + " Driver License(s).");
                foreach (var item in items)
                {
                    var dlResult = new DriverLicenseResult(item);
                        Console.WriteLine(dlResult.ToString());
                }
            }
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("**********************************************************");
            Console.WriteLine("Welcome to Dynamsoft Capture Vision - DriverLicense Sample");
            Console.WriteLine("**********************************************************");

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
                using (CaptureVisionRouter cvr = new CaptureVisionRouter())
                {
                    while (true)
                    {
                        Console.WriteLine("\n>> Input your image full path:");
                        Console.WriteLine("\n>> 'Enter' for sample image or 'Q'/'q' to quit");
                        string? imagePath = Console.ReadLine();

                        if (imagePath.ToLower() == "q")
                        {
                            return;
                        }
                        if (string.IsNullOrEmpty(imagePath))
                        {
                            imagePath = "../../../../../../Images/driver-license-sample.jpg";
                        }
                        string imagePathTrim = imagePath.Trim('\"');
                        if (!File.Exists(imagePathTrim))
                        {
                            Console.WriteLine("The image does not exist.");
                            continue;
                        }

                        using (CapturedResult? result = cvr.Capture(imagePathTrim, "ReadDriversLicense"))
                        {
                            if (result == null)
                            {
                                Console.WriteLine("No parsed results.");
                            }
                            else
                            {

                                if (result.GetErrorCode() != 0)
                                {
                                    Console.WriteLine("Error: " + result.GetErrorCode() + ", " + result.GetErrorString());
                                }
                                ParsedResult? parsedResult = result.GetParsedResult();
                                if (parsedResult == null || parsedResult.GetItems().Length == 0)
                                {
                                    Console.WriteLine("No parsed results.");
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