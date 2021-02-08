using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MessageLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public TMPro.TextMeshPro Message;

    /// <summary>
    /// Load message from file in Resources folder
    /// </summary>
    public void LoadMessage()
    {
        TextAsset asset = Resources.Load<TextAsset>("Message");
        Debug.Log(asset.text);

        Message.text = asset.text;
        Message.color = Color.red;
    }
}
