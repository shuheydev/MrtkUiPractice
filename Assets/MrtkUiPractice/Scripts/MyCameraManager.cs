using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyCameraManager : MonoBehaviour
{
    public Camera capturedCamera;

    public GameObject cameraViewer;

    private RawImage rawImageComponent;
    // Start is called before the first frame update
    void Start()
    {
        rawImageComponent = cameraViewer.GetComponentInChildren<RawImage>();
        webCamTexture = new WebCamTexture();
        rawImageComponent.texture = webCamTexture;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartCamera()
    {
        if (capturedCamera == null)
        {
            Debug.LogError("No camera set");
            return;
        }

        var image = cameraViewer.GetComponentInChildren<RawImage>();

    }

    private WebCamTexture webCamTexture;

    public void StartWebCam()
    {
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.Log("No camera devices");
            return;
        }
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.Log("Camera not allowed");
            return;
        }

        webCamTexture.Play();
    }

    public RawImage rawImage;
    public void StartWebCam2()
    {
        webCamTexture = new WebCamTexture();
        rawImage.texture = webCamTexture;
        webCamTexture.Play();
    }

    public void StopWebCam2()
    {
        webCamTexture.Stop();
    }
}
