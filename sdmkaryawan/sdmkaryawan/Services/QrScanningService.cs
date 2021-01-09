using System;
using System.Collections.Generic;
using System.Text;
using ZXing.Mobile;
using Xamarin.Forms;
using System.Threading.Tasks;
using Xamarin.Essentials;
using System.Linq;

[assembly: Dependency(typeof(sdmkaryawan.Services.QrScanningService))]

namespace sdmkaryawan.Services
{
    public class QrScanningService : IQrScanningService
    {
        public async Task<string> ScanAsync()
        {

            //var optionsDefault = new MobileBarcodeScanningOptions();
            //var optionsCustom = new MobileBarcodeScanningOptions();

            var displayOrientationHeight = DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait ? DeviceDisplay.MainDisplayInfo.Height : DeviceDisplay.MainDisplayInfo.Width;
            var displayOrientationWidth = DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait ? DeviceDisplay.MainDisplayInfo.Width : DeviceDisplay.MainDisplayInfo.Height;
            //calculatiing our targetRatio
            var targetRatio = displayOrientationHeight / displayOrientationWidth;

            var options = new ZXing.Mobile.MobileBarcodeScanningOptions();

            if (targetRatio > 1.6 && targetRatio < 1.85)
            {
                options = new ZXing.Mobile.MobileBarcodeScanningOptions()
                {
                    PossibleFormats = new List<ZXing.BarcodeFormat>() { ZXing.BarcodeFormat.QR_CODE },
                    TryHarder = true,
                    DisableAutofocus = false,
                    DelayBetweenAnalyzingFrames = 0,
                    InitialDelayBeforeAnalyzingFrames = 0,
                    UseNativeScanning = true
                };

  
            }
            else
            {
                options = new ZXing.Mobile.MobileBarcodeScanningOptions()
                {
                    PossibleFormats = new List<ZXing.BarcodeFormat>() { ZXing.BarcodeFormat.QR_CODE },
                    CameraResolutionSelector = DependencyService.Get<IQrScanningService>().SelectLowestResolutionMatchingDisplayAspectRatio,
                    TryHarder = true,
                    DisableAutofocus = false,
                    DelayBetweenAnalyzingFrames = 0,
                    InitialDelayBeforeAnalyzingFrames = 0,
                    UseNativeScanning = true
                };

                
            }




            var scanner = new MobileBarcodeScanner()
            {
                TopText = "Harap Dekatkan dengan QR Code yang terdapat pada halaman 10.10.0.10/menu",
                BottomText = "Sedang Membaca Data",
            };

            try
            {
                var scanResult = await scanner.Scan(options);
                return scanResult.Text;
            }
            catch
            {

            }
            return "";



        }

        public CameraResolution SelectLowestResolutionMatchingDisplayAspectRatio(List<CameraResolution> availableResolutions)
        {
            CameraResolution result = null;

            try
            {
                //a tolerance of 0.1 should not be visible to the user
                double aspectTolerance = 0.01;
                var displayOrientationHeight = DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait ? DeviceDisplay.MainDisplayInfo.Height : DeviceDisplay.MainDisplayInfo.Width;
                var displayOrientationWidth = DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait ? DeviceDisplay.MainDisplayInfo.Width : DeviceDisplay.MainDisplayInfo.Height;
                //calculatiing our targetRatio
                var targetRatio = displayOrientationHeight / displayOrientationWidth;
                var targetHeight = displayOrientationHeight;
                var minDiff = double.MaxValue;
                //camera API lists all available resolutions from highest to lowest, perfect for us
                //making use of this sorting, following code runs some comparisons to select the lowest resolution that matches the screen aspect ratio and lies within tolerance
                //selecting the lowest makes Qr detection actual faster most of the time
                foreach (var r in availableResolutions.Where(r => Math.Abs(((double)r.Width / r.Height) - targetRatio) < aspectTolerance))
                {
                    //slowly going down the list to the lowest matching solution with the correct aspect ratio
                    if (Math.Abs(r.Height - targetHeight) < minDiff)
                        minDiff = Math.Abs(r.Height - targetHeight);
                    result = r;
                }
                
            }
            catch
            {

            }
            return result;
        }
    }
}
