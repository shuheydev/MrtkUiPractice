using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class MyVideoManager : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    public VideoPlayer VideoPlayer
    {
        get
        {
            if (videoPlayer == null)
            {
                videoPlayer = GetComponentInChildren<VideoPlayer>();
            }
            return videoPlayer;
        }
        set
        {
            videoPlayer = value;
        }
    }

    public VideoClip videoClip;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void PlayMovie()
    {
        videoClip = Resources.Load<VideoClip>("20201221_110453_HoloLens");

        if (this.VideoPlayer.clip == null)
        {
            this.VideoPlayer.clip = videoClip;
        }
        else
        {
            this.VideoPlayer.Play();
        }
    }

    public void StopMovie()
    {
        this.VideoPlayer.Stop();
    }

    public void PauseMovie()
    {
        if (this.videoClip == null)
        {
            videoClip = Resources.Load<VideoClip>("20201221_110453_HoloLens");
        }
        this.VideoPlayer.Pause();
    }
}
