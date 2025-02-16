﻿using UnityEngine;
using System.Collections;

public class Commercials : MonoBehaviour
{

    public MovieTexture[] clips;
    public AudioSource TVSpeaker;
    int currentClipCount = 0;
    public static bool isPlaying;

    void OnEnable()
    {
        isPlaying = true;
        TVSpeaker.mute = false;
    }

    void OnDisable()
    {
        isPlaying = false;
        TVSpeaker.mute = true;
    }

    void Awake()
    {
        ChangeClip();
        GetComponent<Renderer>().material.mainTexture = clips[currentClipCount];
        TVSpeaker.clip = clips[currentClipCount].audioClip;
        clips[currentClipCount].Play();
        TVSpeaker.Play();
    }

    void Update()
    {
        if (currentClipCount <= clips.Length)
        {
            if (clips[currentClipCount].isPlaying == false)
            {
                ChangeClip();
                GetComponent<Renderer>().material.mainTexture = clips[currentClipCount];
                TVSpeaker.clip = clips[currentClipCount].audioClip;
                clips[currentClipCount].Play();
                TVSpeaker.Play();
            }
        }
    }

    void ChangeClip()
    {
        clips[currentClipCount].Stop();
        int nextClip = Random.RandomRange(0, clips.Length);
        if (currentClipCount != nextClip)
            currentClipCount = nextClip;
        else
            ChangeClip();
    }

}
