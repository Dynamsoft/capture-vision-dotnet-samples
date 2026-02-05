using Dynamsoft.Core;
using Dynamsoft.Core.intermediate_results;
using Dynamsoft.CVR;
using Dynamsoft.DDN;
using Dynamsoft.DDN.intermediate_results;
using Dynamsoft.DCP;
using Dynamsoft.DLR.intermediate_results;
using Dynamsoft.IdUtility;
using Dynamsoft.License;
using Dynamsoft.Utility;

using System;
using System.Collections.Generic;
using System.IO;

namespace MRZScanner
{
    class PortraitZoneData
    {
        public ScaledColourImageUnit scaledColourImageUnit;
        public LocalizedTextLinesUnit localizedTextLinesUnit;
        public RecognizedTextLinesUnit recognizedTextLinesUnit;
        public DetectedQuadsUnit detectedQuadsUnit;
        public DeskewedImageUnit deskewedImageUnit;
    }

    class MyIntermediateResultReceiver : IntermediateResultReceiver
    {
        private Dictionary<string, PortraitZoneData> portraitZoneDataDict = new Dictionary<string, PortraitZoneData>();

        public override void OnScaledColourImageUnitReceived(ScaledColourImageUnit result, IntermediateResultExtraInfo info)
        {
            string hashId = result.GetOriginalImageHashId();
            PortraitZoneData data = getData(hashId);
            data.scaledColourImageUnit = result;
        }

        public override void OnLocalizedTextLinesReceived(LocalizedTextLinesUnit result, IntermediateResultExtraInfo info)
        {
            if (info.isSectionLevelResult)
            {
                string hashId = result.GetOriginalImageHashId();
                PortraitZoneData data = getData(hashId);
                data.localizedTextLinesUnit = result;
            }
        }

        public override void OnRecognizedTextLinesReceived(RecognizedTextLinesUnit result, IntermediateResultExtraInfo info)
        {
            if (info.isSectionLevelResult)
            {
                string hashId = result.GetOriginalImageHashId();
                PortraitZoneData data = getData(hashId);
                data.recognizedTextLinesUnit = result;
            }
        }

        public override void OnDetectedQuadsReceived(DetectedQuadsUnit result, IntermediateResultExtraInfo info)
        {
            if (!info.isSectionLevelResult)
            {
                string hashId = result.GetOriginalImageHashId();
                PortraitZoneData data = getData(hashId);
                data.detectedQuadsUnit = result;
            }
        }

        public override void OnDeskewedImageReceived(DeskewedImageUnit result, IntermediateResultExtraInfo info)
        {
            if (info.isSectionLevelResult)
            {
                string hashId = result.GetOriginalImageHashId();
                PortraitZoneData data = getData(hashId);
                data.deskewedImageUnit = result;
            }
        }

        public int GetPortraitZone(string hashId, out Quadrilateral quad)
        {
            quad = null;
            PortraitZoneData data = getData(hashId);
            if (data != null)
            {
                IdentityProcessor idProcessor = new IdentityProcessor();
                return idProcessor.FindPortraitZone(
                        data.scaledColourImageUnit,
                        data.localizedTextLinesUnit,
                        data.recognizedTextLinesUnit,
                        data.detectedQuadsUnit,
                        data.deskewedImageUnit,
                        out quad);
            }
            return -1;
        }

        private PortraitZoneData getData(string hashId)
        {
            lock (portraitZoneDataDict)
            {
                if (!portraitZoneDataDict.ContainsKey(hashId))
                {
                    portraitZoneDataDict[hashId] = new PortraitZoneData();
                }
                return portraitZoneDataDict[hashId];
            }
        }
    }

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
        public bool isPassport;
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
                isPassport = true;
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
        private static ImageData GetOriginalImage(CapturedResult result)
        {
            CapturedResultItem[] items = result.GetItems();
            foreach (var item in items)
            {
                if (item is OriginalImageResultItem originalImageResultItem)
                {
                    return originalImageResultItem.GetImageData();
                }
            }
            return null;
        }

        private static void SaveProcessedDocumentResult(CapturedResult result, int pageNumber, string imagePathPrefix)
        {
            Console.WriteLine("Extract and save the normalized document image.");

            ProcessedDocumentResult docResult = result.GetProcessedDocumentResult();
            EnhancedImageResultItem[] enhancedImageResultItems = docResult != null ? docResult.GetEnhancedImageResultItems() : null;
            if (enhancedImageResultItems == null || enhancedImageResultItems.Length == 0)
            {
                Console.WriteLine("Page-" + pageNumber + " No normalized document result found.");
                return;
            }

            EnhancedImageResultItem enhancedImageResultItem = enhancedImageResultItems[0];
            string outputPath = imagePathPrefix + pageNumber + "_document.png";
            ImageIO imgIO = new ImageIO();
            ImageData enhancedImage = enhancedImageResultItem.GetImageData();
            if (enhancedImage != null)
            {
                int errorCode = imgIO.SaveToFile(enhancedImage, outputPath);
                if (errorCode == (int)EnumErrorCode.EC_OK)
                    Console.WriteLine("Document file: " + outputPath);
                else
                    Console.WriteLine("Save document file failed, error code: " + errorCode);
            }
        }

        private static void SavePortraitZone(MyIntermediateResultReceiver irReceiver, string hashId, ImageData originalImageData, int pageNumber, string imagePathPrefix)
        {
            Console.WriteLine("Extract and save the portrait zone image.");

            if (originalImageData == null)
            {
                Console.WriteLine("Page-" + pageNumber + " Original image data not exists.");
                return;
            }

            Quadrilateral quad;
            int errorCode = irReceiver.GetPortraitZone(hashId, out quad);
            if (errorCode != (int)EnumErrorCode.EC_OK)
            {
                Console.WriteLine("Page-" + pageNumber + " No portrait zone found, error code: " + errorCode);
                return;
            }

            ImageProcessor imgProcessor = new ImageProcessor();
            ImageData croppedImage;
            croppedImage = imgProcessor.CropAndDeskewImage(originalImageData, quad, 0, 0, 0, out errorCode);
            if (errorCode != (int)EnumErrorCode.EC_OK)
            {
                Console.WriteLine("Crop image failed, error code: " + errorCode);
                return;
            }

            string outputPath = imagePathPrefix + pageNumber + "_portrait.png";
            ImageIO imgIO = new ImageIO();
            errorCode = imgIO.SaveToFile(croppedImage, outputPath);
            if (errorCode == (int)EnumErrorCode.EC_OK)
            {
                Console.WriteLine("Portrait file: " + outputPath);
            }
            else
            {
                Console.WriteLine("Save portrait file failed, error code: " + errorCode);
            }
        }

        private static void ProcessResult(CapturedResult result, MyIntermediateResultReceiver irReceiver, int printIndex)
        {
            if (result.GetErrorCode() == (int)EnumErrorCode.EC_UNSUPPORTED_JSON_KEY_WARNING)
            {
                Console.WriteLine("Warning: " + result.GetErrorCode() + ", " + result.GetErrorString());
            }
            else if (result.GetErrorCode() != (int)EnumErrorCode.EC_OK)
            {
                Console.WriteLine("Error: " + result.GetErrorCode() + ", " + result.GetErrorString());
            }

            int pageNumber = printIndex + 1;
            string imagePathPrefix = "";

            FileImageTag tag = result.GetOriginalImageTag() as FileImageTag;
            if (tag != null)
            {
                imagePathPrefix = Path.GetFileNameWithoutExtension(tag.GetFilePath()) + "_";

                pageNumber = tag.GetPageNumber() + 1;
                Console.WriteLine("File: " + tag.GetFilePath());
                Console.WriteLine("Page: " + pageNumber);
            }

            ParsedResult parsedResult = result.GetParsedResult();
            ParsedResultItem[] parsedResultItems = parsedResult != null ? parsedResult.GetItems() : null;
            if (parsedResultItems == null || parsedResultItems.Length == 0)
            {
                Console.WriteLine("No parsed results in page " + pageNumber + ".");
                return;
            }

            string hashId = result.GetOriginalImageHashId();
            bool isPassport = false;
            if (parsedResult.GetErrorCode() != (int)EnumErrorCode.EC_OK && parsedResult.GetErrorCode() != (int)EnumErrorCode.EC_UNSUPPORTED_JSON_KEY_WARNING)
            {
                Console.WriteLine("Error: " + parsedResult.GetErrorCode() + ", " + parsedResult.GetErrorString());
            }
            else
            {
                foreach (ParsedResultItem parsedResultItem in parsedResultItems)
                {
                    MRZResult mrzResult = new MRZResult(parsedResultItem);
                    Console.WriteLine(mrzResult);
                    if (!isPassport)
                        isPassport = mrzResult.isPassport;
                }
            }

            if (isPassport)
            {
                ImageData originalImage = GetOriginalImage(result);
                SaveProcessedDocumentResult(result, pageNumber, imagePathPrefix);
                SavePortraitZone(irReceiver, hashId, originalImage, pageNumber, imagePathPrefix);
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
            if (errorCode != (int)EnumErrorCode.EC_OK && errorCode != (int)EnumErrorCode.EC_LICENSE_WARNING)
            {
                Console.WriteLine("License initialization failed: ErrorCode: " + errorCode + ", ErrorString: " + errorMsg);
            }
            else
            {
                using (CaptureVisionRouter cvRouter = new CaptureVisionRouter())
                using (IntermediateResultManager irManager = cvRouter.GetIntermediateResultManager())
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

                        MyIntermediateResultReceiver irReceiver = new MyIntermediateResultReceiver();
                        irManager.AddResultReceiver(irReceiver);
                        CapturedResult[] results = cvRouter.CaptureMultiPages(imagePath, "ReadPassportAndId");
                        irManager.RemoveResultReceiver(irReceiver);

                        if (results == null || results.Length == 0)
                        {
                            Console.WriteLine("No parsed results.");
                        }
                        else
                        {
                            for (int index = 0; index < results.Length; index++)
                            {
                                ProcessResult(results[index], irReceiver, index);
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