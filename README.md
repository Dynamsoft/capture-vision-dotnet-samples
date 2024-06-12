# Dynamsoft Capture Vision Samples for .NET Edition

This repository contains multiple samples that demonstrate how to use the [Dynamsoft Capture Vision](https://www.dynamsoft.com/capture-vision/docs/core/introduction/) .NET Edition.

## System Requirements

- Windows:
  - Supported Versions: Windows 7 and higher, or Windows Server 2003 and higher
  - Architecture: x64 and x86
  - Development Environment: Visual Studio 2012 or higher.

- Linux:
  - Supported Distributions: Ubuntu 14.04.4+ LTS, Debian 8+, CentOS 6+
  - Architectures: x64
  - Minimum GLIBC Version: GLIBC_2.18 or higher

- Supported .NET versions
  - .NET Framework 3.5 and above
  - .NET 6, 7, 8

## Samples

| Sample            | Description |
|---------------|----------------------|
|[`CaptureFromAnImage`](Samples/HelloWorld/CaptureFromAnImage)          | The simplest way to capture content from an image file with Dynamsoft Capture Vision SDK.            |
|[`CaptureFromMultipleImages`](Samples/HelloWorld/CaptureFromMultipleImages)          | The simplest way to capture content from image files in a directory with Dynamsoft Capture Vision SDK.            |
|[`MRZScanner`](Samples/UseCases/MRZScanner)          | Capture and extract user's information from machine-readable travel documents with Dynamsoft Capture Vision SDK.            |
|[`DriverLicenseScanner`](Samples/UseCases/DriverLicenseScanner)          | Capture and extract user's information from driver license/ID with Dynamsoft Capture Vision SDK.            |
|[`VINScanner`](Samples/UseCases/VINScanner)          | Capture and extract vehicle's information from Vehicle Identification Number (VIN) with Dynamsoft Capture Vision SDK.            |

## License

The library requires a license to work, you use the API `LicenseManager.InitLicense` to initialize license key and activate the SDK.

These samples use a free public trial license which require network connection to function. You can request a 30-day free trial license via the <a href="https://www.dynamsoft.com/customer/license/trialLicense?product=cvs&utm_source=github&package=dotnet" target="_blank">Request a Trial License</a> link which works offline.

## Contact Us

<a href="https://www.dynamsoft.com/company/contact/">Contact Dynamsoft</a> if you have any questions.
