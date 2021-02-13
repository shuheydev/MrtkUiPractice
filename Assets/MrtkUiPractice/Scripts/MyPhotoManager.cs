using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPhotoManager : MonoBehaviour
{
    private MeshRenderer photoViewer;
    public MeshRenderer PhotoViewer
    {
        get
        {
            if (photoViewer == null)
            {
                var photoviewObject = GameObject.FindGameObjectWithTag("PhotoViewObject");
                photoViewer = photoviewObject.GetComponent<MeshRenderer>();
            }
            return photoViewer;
        }
        set
        {
            photoViewer = value;
        }
    }

    public Texture2D photo;
    // Start is called before the first frame update
    void Start()
    {
        photo = Resources.Load<Texture2D>("PngPicture");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadPhoto()
    {
        this.PhotoViewer.material.mainTexture = photo;
    }
}
