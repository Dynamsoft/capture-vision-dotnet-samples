# Dynamsoft Capture Vision Samples for .NET Edition

This repository contains multiple samples that demonstrate how to use the [Dynamsoft Capture Vision](https://www.dynamsoft.com/capture-vision/docs/core/introduction/?lang=dotnet) .NET Edition.

## System Requirements

- Windows:
  - Supported Versions: Windows 8 and higher, or Windows Server 2012 and higher
  - Architecture: x64 and x86
  - Development Environment: Visual Studio 2012 or higher.

- Linux:
  - Supported Distributions: Ubuntu 14.04.4+ LTS, Debian 8+, CentOS 7+
  - Architectures: x64
  - Minimum GLIBC Version: GLIBC_2.18 or higher

- Supported .NET versions
  - .NET Framework 3.5 and above
  - .NET 6, 7, 8

## Samples

| Sample            | Description |
|---------------|----------------------|
|[`MRZScanner`](Samples/MRZScanner)          | Capture and extract user's information from machine-readable travel documents with Dynamsoft Capture Vision SDK.            |
|[`DriverLicenseScanner`](Samples/DriverLicenseScanner)          | Capture and extract user's information from driver license/ID with Dynamsoft Capture Vision SDK.            |
|[`VINScanner`](Samples/VINScanner)          | Capture and extract vehicle's information from Vehicle Identification Number (VIN) with Dynamsoft Capture Vision SDK.            |
|[`DocumentScanner`](Samples/DocumentScanner)          | The simplest way to detect and normalize a document from an image and save the result as a new image.            |
|[`GS1AIScanner`](Samples/GS1AIScanner) | Shows how to extract and interpret GS1 Application Identifiers (AIs) from GS1 barcodes. |

## License

The library requires a license to work, you use the API `LicenseManager.InitLicense` to initialize license key and activate the SDK.

These samples use a free public trial license which require network connection to function. You can request a 30-day free trial license via the <a href="https://www.dynamsoft.com/customer/license/trialLicense?product=dcv&utm_source=github&package=dotnet" target="_blank">Request a Trial License</a> link which works offline.

## Contact Us

<a href="https://www.dynamsoft.com/company/contact/">Contact Dynamsoft</a> if you have any questions.
