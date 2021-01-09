using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZXing.Mobile;

namespace sdmkaryawan.Services
{
    public interface IQrScanningService
    {
        Task<string> ScanAsync();
        CameraResolution SelectLowestResolutionMatchingDisplayAspectRatio(List<CameraResolution> availableResolutions);
    }
}
