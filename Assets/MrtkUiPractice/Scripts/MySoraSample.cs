using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MySoraSample : MonoBehaviour
{
    public enum SampleType
    {
        MultiSendrecv,
        MultiRecvonly,//複数、受信のみ
        MultiSendonly,//複数、送信のみ
        Sendonly,//
        Recvonly,
    }

    Sora sora;

    public SampleType sampleType;
    //実行中に変えられたくないので実行時に固定するためとのこと
    private SampleType fixedSampleType;

    //非マルチストリームで利用する
    private uint trackId = 0;//複数じゃないから0で固定ということか?
    public GameObject renderTarget;//これはあれか?画像の表示先か?

    //マルチストリームで利用する
    private Dictionary<uint, GameObject> tracks = new Dictionary<uint, GameObject>();
    public GameObject scrollViewContent;
    public GameObject baseContent;//これは?

    //以下共通
    public string signalingUrl = "";
    public string channelId = "";
    public string signalingKey = "";

    public bool captureUnityCamera;
    public Camera capturedCamera;

    public Sora.VideoCodec videoCodec = Sora.VideoCodec.VP9;

    public bool unityAudioInput = false;
    public AudioSource audioSourceInput;
    public bool unityAudioOutput;
    public AudioSource audioSourceOutput;

    public string videoCapturerDevice = "";
    public string audioRecordingDevice = "";
    public string audioPlayoutDevice = "";

    public bool Recvonly
    {
        get => fixedSampleType == SampleType.Recvonly || fixedSampleType == SampleType.MultiRecvonly;
    }
    public bool MultiRecv
    {
        get => fixedSampleType == SampleType.MultiRecvonly || fixedSampleType == SampleType.MultiSendrecv;
    }
    public bool Multistream
    {
        get => fixedSampleType == SampleType.MultiSendonly || fixedSampleType == SampleType.MultiRecvonly || fixedSampleType == SampleType.MultiSendrecv;
    }
    public Sora.Role Role
    {
        get
        {
            return
                fixedSampleType == SampleType.Sendonly ? Sora.Role.Sendonly :
                fixedSampleType == SampleType.Recvonly ? Sora.Role.Recvonly :
                fixedSampleType == SampleType.MultiSendonly ? Sora.Role.Sendonly :
                fixedSampleType == SampleType.MultiRecvonly ? Sora.Role.Recvonly : Sora.Role.Sendrecv;
        }
    }

    private Queue<short[]> audioBuffer = new Queue<short[]>();
    private int audioBufferSamples = 0;
    private int audioBufferPosition = 0;

    private void DumpDeviceInfo(string name, Sora.DeviceInfo[] infos)
    {
        Debug.LogFormat("------------ {0} --------------", name);
        foreach (var info in infos)
        {
            Debug.LogFormat("DeviceName={0} UniqueName={1}", info.DeviceName, info.UniqueName);
        }
    }

    private void Start()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        var x = WebCamTexture.devices;
        var y = Microphone.devices;
#endif

        DumpDeviceInfo("video capturer devices", Sora.GetVideoCapturerDevices());
        DumpDeviceInfo("audio recording devices", Sora.GetAudioRecordingDevices());
        DumpDeviceInfo("audio playout devices", Sora.GetAudioPlayoutDevices());

        if (!MultiRecv)
        {
            var image = renderTarget.GetComponent<RawImage>();
            image.texture = new Texture2D(640, 480, TextureFormat.RGBA32, mipChain: false);//mipChainて何だ?
        }

        StartCoroutine(Render());
        StartCoroutine(GetStats());
    }

    #region Coroutines
    private IEnumerator Render()
    {
        yield return new WaitForEndOfFrame();
        if (sora != null)
        {
            // Unity 側でレンダリングが完了した時（yield return new WaitForEndOfFrame() の後）に呼ぶイベント
            // 指定した Unity カメラの映像を Sora 側のテクスチャにレンダリングしたりする
            sora.OnRender();
        }

        if (sora != null && !Recvonly)
        {
            var samples = AudioRenderer.GetSampleCountForCaptureFrame();
            if (AudioSettings.speakerMode == AudioSpeakerMode.Stereo)
            {
                using (var buf = new NativeArray<float>(samples * 2, Allocator.Temp))
                {
                    AudioRenderer.Render(buf);
                    sora.ProcessAudio(buf.ToArray(), 0, samples);
                }
            }
        }
    }

    private IEnumerator GetStats()
    {
        while (true)
        {
            yield return new WaitForSeconds(10);//10秒に1回実行する
            if (sora == null)
            {
                continue;
            }

            sora.GetStats(stats =>
            {
                Debug.LogFormat("GetStats:{0}", stats);
            });
        }
    }
    #endregion

    private void Update()
    {
        if (sora == null)
        {
            return;
        }

        sora.DispatchEvents();//なんだこれ?

        if (!MultiRecv)
        {
            if (trackId != 0)
            {
                var image = renderTarget.GetComponent<RawImage>();
                sora.RenderTrackToTexture(trackId, image.texture);
            }
        }
        else
        {
            foreach (var track in tracks)
            {
                var image = track.Value.GetComponent<RawImage>();
                sora.RenderTrackToTexture(track.Key, image.texture);
            }
        }
    }

    private void InitSora()
    {
        DisposeSora();

        sora = new Sora();
        if (!MultiRecv)
        {
            sora.OnAddTrack = (trackId) =>
            {
                Debug.LogFormat("OnAddTrack: trackId={0}", trackId);
                this.trackId = trackId;
            };

            sora.OnRemoveTrack = (track) =>
            {
                Debug.LogFormat("OnRemoveTrack: trackId={0}", trackId);
                this.trackId = 0;
            };
        }
        else
        {
            sora.OnAddTrack = (trackId) =>
            {
                Debug.LogFormat("OnAddTrack: trackId={0}", trackId);
                var obj = Instantiate(baseContent, Vector3.zero, Quaternion.identity);//PositionとRotationを指定してオブジェクトを生成
                obj.name = string.Format("track {0}", trackId);
                obj.transform.SetParent(scrollViewContent.transform);
                obj.SetActive(true);

                var image = obj.GetComponent<RawImage>();
                image.texture = new Texture2D(320, 240, TextureFormat.RGBA32, false);
                tracks.Add(trackId, obj);
            };

            sora.OnRemoveTrack = (trackId) =>
            {
                Debug.LogFormat("OnRemoveTrack: trackId={0}", trackId);
                if (tracks.ContainsKey(trackId))
                {
                    Destroy(tracks[trackId]);
                    tracks.Remove(trackId);
                }
            };
        }

        sora.OnNotify = (json) =>
        {
            Debug.LogFormat("OnNotify: {0}", json);
        };

        sora.OnHandleAudio = (buf, samples, channels) =>
        {
            lock (audioBuffer)
            {
                audioBuffer.Enqueue(buf);
                audioBufferSamples += samples;
            }
        };

        if (unityAudioOutput)
        {
            var audioClip = AudioClip.Create("AudioClip", 480000, 1, 48000, true, (data) =>
            {
                lock (audioBuffer)
                {
                    if (audioBufferSamples < data.Length)
                    {
                        for (int i = 0; i < data.Length; i++)
                        {
                            data[i] = 0.0f;
                        }
                        return;
                    }

                    var p = audioBuffer.Peek();
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = p[audioBufferPosition] / 32768.0f;
                        ++audioBufferPosition;
                        if (audioBufferPosition >= p.Length)
                        {
                            audioBuffer.Dequeue();
                            p = audioBuffer.Peek();
                            audioBufferPosition = 0;
                        }
                    }
                    audioBufferSamples -= data.Length;
                }
            });
            audioSourceOutput.clip = audioClip;
            audioSourceOutput.Play();
        }

        if (!Recvonly)
        {
            AudioRenderer.Start();
            audioSourceInput.Play();
        }
    }

    private void DisposeSora()
    {
        if (sora != null)
        {
            sora.Dispose();
            sora = null;
            Debug.Log("Sora is Disposed");
            if (MultiRecv)
            {
                foreach (var track in tracks)
                {
                    Destroy(track.Value);
                }
                tracks.Clear();
            }

            if (!Recvonly)
            {
                audioSourceInput.Stop();
                AudioRenderer.Stop();
            }

            if (unityAudioOutput)
            {
                audioSourceOutput.Stop();
            }
        }
    }

    [Serializable]
    private class Settings
    {
        public string signaling_url = "";
        public string channel_id = "";
        public string signaling_key = "";
    }

    [Serializable]
    private class Metadata
    {
        public string signaling_key;
    }

    public void OnClickStart()
    {
        //開発用の機能とのこと。
        //.env.jsonファイルがあり、signalingUrlとchannelIdがセットされていない場合に、
        //.env.jsonファイルを読み込んで上記情報をセットする
        if (signalingUrl.Length == 0 && channelId.Length == 0 && System.IO.File.Exists(".env.json"))
        {
            var settings = JsonUtility.FromJson<Settings>(File.ReadAllText(".env.json"));
            signalingUrl = settings.signaling_url;
            channelId = settings.channel_id;
            signalingKey = settings.signaling_key;
        }

        if (signalingUrl.Length == 0)
        {
            Debug.LogError("シグナリングURLが設定されていません");
            return;
        }
        if (channelId.Length == 0)
        {
            Debug.LogError("チャンネルIDが設定されていません");
            return;
        }

        //signalingKeyがある場合はメタデータを設定する
        string metadata = "";
        if (signalingKey.Length != 0)
        {
            var md = new Metadata()
            {
                signaling_key = signalingKey,
            };
            metadata = JsonUtility.ToJson(md);
        }

        InitSora();

        var config = new Sora.Config()
        {
            SignalingUrl = signalingUrl,
            ChannelId = channelId,
            Metadata = metadata,
            Role = Role,
            Multistream = Multistream,
            VideoCodec = videoCodec,
            UnityAudioInput = unityAudioInput,
            UnityAudioOutput = unityAudioOutput,
            VideoCapturerDevice = videoCapturerDevice,
            AudioRecordingDevice = audioRecordingDevice,
            AudioPlayoutDevice = audioPlayoutDevice,
        };

        if (captureUnityCamera && capturedCamera != null)
        {
            config.CapturerType = Sora.CapturerType.UnityCamera;
            config.UnityCamera = capturedCamera;
        }

        var success = sora.Connect(config);
        if (!success)
        {
            sora.Dispose();
            sora = null;
            Debug.LogErrorFormat("Sora.Connect failed: signalingUrl={0}, channelId={1}", signalingUrl, channelId);
            return;
        }

        Debug.LogFormat("Sora is Created: signalingUrl={0}, channelId={1}", signalingUrl, channelId);
    }

    public void OnClickEnd()
    {
        DisposeSora();
    }

    void OnApplicationQuit()
    {
        DisposeSora();
    }
}
