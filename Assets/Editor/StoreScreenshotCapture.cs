using UnityEditor;
using UnityEngine;

namespace CandyBeltSort
{
    public static class StoreScreenshotCapture
    {
        [MenuItem("Candy Belt Sort/Capture Game View PNG")]
        public static void Capture()
        {
            DirectoryEnsure();
            string path = $"store/screenshots/gameview-{System.DateTime.Now:yyyyMMdd-HHmmss}.png";
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log("Screenshot queued: " + path + " (appears after the next Game view frame)");
        }

        static void DirectoryEnsure()
        {
            if (!System.IO.Directory.Exists("store/screenshots"))
                System.IO.Directory.CreateDirectory("store/screenshots");
        }
    }
}
