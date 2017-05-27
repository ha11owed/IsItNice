using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Capture;

namespace ClientLogic.WinRT
{
    /// <summary>
    /// Wrapper for the WinRT cammera object.
    /// </summary>
    public class Camera
    {
        public async Task<FileContainer> TakePicture()
        {
            var cam = new CameraCaptureUI();
            cam.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Png;
#if DEBUG
            // In debug mode we spare some traffic
            cam.PhotoSettings.MaxResolution = CameraCaptureUIMaxPhotoResolution.VerySmallQvga;
#endif
            var storageFile = await cam.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if (null != storageFile)
            {
                var fileContainer = new FileContainer(storageFile);
                return fileContainer;
            }
            else
            {
                return null;
            }
        }
    }
}
