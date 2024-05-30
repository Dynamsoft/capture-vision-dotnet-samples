using Dynamsoft.Core;
using Dynamsoft.CVR;
using Dynamsoft.DLR;
using Dynamsoft.License;
using Dynamsoft.Utility;
using Dynamsoft.DCP;
using System.Collections;

namespace MRZScanner
{
    using System;
    using System.Collections.Generic;

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
            if (docType == "MRTD_TD1_ID")
            {
                if (item.GetFieldValue("line1") != null)
                {
                    line = item.GetFieldValue("line1");
                    if (item.GetFieldValidationStatus("line1") == EnumValidationStatus.VS_FAILED)
                    {
                        line += ", Validation Fail";
                    }
                    rawText.Add(line);
                }

                if (item.GetFieldValue("line2") != null)
                {
                    line = item.GetFieldValue("line2");
                    if (item.GetFieldValidationStatus("line2") == EnumValidationStatus.VS_FAILED)
                    {
                        line += ", Validation Fail";
                    }
                    rawText.Add(line);
                }

                if (item.GetFieldValue("line3") != null)
                {
                    line = item.GetFieldValue("line3");
                    if (item.GetFieldValidationStatus("line3") == EnumValidationStatus.VS_FAILED)
                    {
                        line += ", Validation Fail";
                    }
                    rawText.Add(line);
                }
            }
            else
            {
                if (item.GetFieldValue("line1") != null)
                {
                    line = item.GetFieldValue("line1");
                    if (item.GetFieldValidationStatus("line1") == EnumValidationStatus.VS_FAILED)
                    {
                        line += ", Validation Fail";
                    }
                    rawText.Add(line);
                }

                if (item.GetFieldValue("line2") != null)
                {
                    line = item.GetFieldValue("line2");
                    if (item.GetFieldValidationStatus("line2") == EnumValidationStatus.VS_FAILED)
                    {
                        line += ", Validation Fail";
                    }
                    rawText.Add(line);
                }
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

    class MyCapturedResultReceiver : CapturedResultReceiver
    {
        public override void OnRecognizedTextLinesReceived(RecognizedTextLinesResult result)
        {
            //FileImageTag tag = (FileImageTag)result.GetOriginalImageTag();
            //Console.WriteLine("File: " + tag.GetFilePath());
            //if (result.GetErrorCode() != (int)EnumErrorCode.EC_OK)
            //{
            //    Console.WriteLine("Error: " + result.GetErrorString());
            //}
            //else
            //{
            //    TextLineResultItem[] items = result.GetItems();
            //    Console.WriteLine("Decoded " + items.Length + " text lines");
            //    foreach (TextLineResultItem item in items)
            //    {
            //        TextLineResultItem textLineItem = (TextLineResultItem)item;
            //        Console.WriteLine(">>Line result : " + Array.IndexOf(items, item) + ": " + textLineItem.GetText());
            //    }
            //}
            //Console.WriteLine();
        }

        public override void OnParsedResultsReceived(ParsedResult result)
        {
            FileImageTag tag = (FileImageTag)result.GetOriginalImageTag();
            Console.WriteLine("File: " + tag?.GetFilePath());
            if (result.GetErrorCode() != (int)EnumErrorCode.EC_OK)
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
    }
    class MyImageSourceStateListener : IImageSourceStateListener
    {
        private CaptureVisionRouter cvr = null;
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
            Console.WriteLine("Welcome to Dynamsoft Capture Vision - MRZ Sample");
            Console.WriteLine("*************************************************");

            int errorCode = 1;
            string errorMsg;
            errorCode = LicenseManager.InitLicense("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9", out errorMsg);
            Console.WriteLine("License initialization: " + errorMsg);

            CaptureVisionRouter cvr = new CaptureVisionRouter();

            errorCode = cvr.InitSettingsFromFile("MRZ.json", out errorMsg);
            Console.WriteLine("InitSettings: " + errorMsg);

            Console.WriteLine("Enter the image path:");
            string imagePath = Console.ReadLine();

            DirectoryFetcher fetcher = new DirectoryFetcher();
            fetcher.SetDirectory(imagePath);
            cvr.SetInput(fetcher);

            CapturedResultReceiver receiver = new MyCapturedResultReceiver();
            cvr.AddResultReceiver(receiver);

            MyImageSourceStateListener listener = new MyImageSourceStateListener(cvr);
            cvr.AddImageSourceStateListener(listener);

            errorCode = cvr.StartCapturing("", true, out errorMsg);
            if (errorCode != (int)EnumErrorCode.EC_OK)
            {
                Console.WriteLine("error: " + errorMsg);
            }

            Console.WriteLine("Press any key to quit...");
            Console.Read();
        }
    }
}