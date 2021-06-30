using System;
using System.Collections.Generic;
using System.Management;
using System.Numerics;
using System.Runtime.InteropServices;
using Windows.Foundation;

namespace CaptureCore
{

    public class CameraInfo
    {
        public string Name { get; set; }
        public int Index { get; set; }
    }

    public static class CameraEnumerationHelper
    {
        public static IEnumerable<CameraInfo> EnumerateCameras()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE (PNPClass = 'Image' OR PNPClass = 'Camera')"))
            {
                var managementObjectCollection = searcher.Get();
                int i = managementObjectCollection.Count -1;
                foreach (var device in managementObjectCollection)
                {
                    yield return new CameraInfo
                    {
                        Index = i,
                        Name = device["Caption"].ToString()
                    };
                    i--;
                }
            }
        }
    }
}
