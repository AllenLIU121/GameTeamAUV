using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoClipCom : MonoBehaviour
{
    public VideoPlayer videoClipPlayer;
    // Start is called before the first frame update
    void Start()
    {
        videoClipPlayer = this.transform.GetComponent<VideoPlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (videoClipPlayer.isPaused)
        {
            this.gameObject.SetActive(false);
        }

    }
}
