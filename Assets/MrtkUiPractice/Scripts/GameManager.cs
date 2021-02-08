using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    [RuntimeInitializeOnLoadMethod]
    static void InitApplication()
    {
        new GameObject("TestObject");
    }

    public void CopyTextFileFromResources()
    {
        var data = Resources.Load<TextAsset>("Message").bytes;

        string persistentDataPath = Application.persistentDataPath;
        Debug.Log($"persistentDataPath:{persistentDataPath}");

        string destinationFilePath = Path.Combine(persistentDataPath, "Message.json");
        Debug.Log($"persistentDataPath:{destinationFilePath}");

        File.WriteAllBytes(destinationFilePath, data);
        Debug.Log($"file exists?:{File.Exists(destinationFilePath)}");
    }

    public void CopyFileFromResources()
    {
        var data = Resources.Load<TextAsset>("PngPicture.png").bytes;

        string persistentDataPath = Application.persistentDataPath;
        Debug.Log($"persistentDataPath:{persistentDataPath}");

        string destinationFilePath = Path.Combine(persistentDataPath, "PngPicture.png");
        Debug.Log($"persistentDataPath:{destinationFilePath}");

        File.WriteAllBytes(destinationFilePath, data);
        Debug.Log($"file exists?:{File.Exists(destinationFilePath)}");
    }
}
