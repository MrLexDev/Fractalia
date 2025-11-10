using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public class ScreenshotCapture : MonoBehaviour
{
    [Header("References")]
    public Camera targetCamera;
    public SphereFieldCameraController controller;

    [Header("Settings")]
    public string fileNamePrefix = "fractalia";

    private bool _isCapturingScreenshot;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void DownloadImage(string base64Png, string fileName);
#endif

    public bool IsCapturing => _isCapturingScreenshot;

    public void CaptureScreenshot()
    {
        if (!isActiveAndEnabled)
        {
            Debug.LogWarning("ScreenshotCapture component must be enabled to capture screenshots.");
            return;
        }

        if (!_isCapturingScreenshot)
        {
            StartCoroutine(CaptureScreenshotCoroutine());
        }
    }

    private IEnumerator CaptureScreenshotCoroutine()
    {
        if (_isCapturingScreenshot)
        {
            yield break;
        }

        _isCapturingScreenshot = true;
        yield return new WaitForEndOfFrame();

        Camera captureCamera = GetCaptureCamera();
        if (!captureCamera)
        {
            Debug.LogWarning("Unable to capture screenshot because no camera is available.");
            _isCapturingScreenshot = false;
            yield break;
        }

        int width = Screen.width;
        int height = Screen.height;

        RenderTexture previousTarget = captureCamera.targetTexture;
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture tempRenderTexture = RenderTexture.GetTemporary(width, height, 24);
        Texture2D texture = null;

        try
        {
            captureCamera.targetTexture = tempRenderTexture;
            captureCamera.Render();

            RenderTexture.active = tempRenderTexture;
            texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply(false, false);

            byte[] pngData = texture.EncodeToPNG();
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            string prefix = string.IsNullOrEmpty(fileNamePrefix) ? "screenshot" : fileNamePrefix;
            string fileName = $"{prefix}_{timestamp}.png";

#if UNITY_WEBGL && !UNITY_EDITOR
            string base64 = Convert.ToBase64String(pngData);
            DownloadImage(base64, fileName);
#else
            string filePath = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllBytes(filePath, pngData);
            Debug.Log($"Screenshot saved to {filePath}");
#endif
        }
        finally
        {
            captureCamera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            if (tempRenderTexture != null)
            {
                RenderTexture.ReleaseTemporary(tempRenderTexture);
            }

            if (texture)
            {
                Destroy(texture);
            }

            _isCapturingScreenshot = false;
        }
    }

    private Camera GetCaptureCamera()
    {
        if (targetCamera)
        {
            return targetCamera;
        }

        if (controller && controller.mainCamera)
        {
            return controller.mainCamera;
        }

        return Camera.main;
    }
}
