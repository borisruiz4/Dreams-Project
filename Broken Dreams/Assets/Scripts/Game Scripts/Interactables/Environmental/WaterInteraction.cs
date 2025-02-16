﻿using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.ImageEffects;

public class WaterInteraction : MonoBehaviour
{

    public static GameObject waterInstance;

    public static bool isOnLightWater = false;
    public static bool isOnDeepWater = false;
    public static bool isOnWater = false;
    public static bool isUnderWater = false;
    public static bool isSemiUnderWater = false;
    public Color normalColor;
    public Color underWaterColor;
    private float fogDensity;
    public bool normalSettings = true;

    public GameObject playerHead;

    void Awake()
    {
        if (normalSettings)
        {
            normalColor = RenderSettings.fogColor;
            fogDensity = RenderSettings.fogDensity;

        }
    }

    void Update()
    {

        if (isOnWater)
        {
            if (playerHead.transform.position.y < waterInstance.transform.position.y)
                isUnderWater = true;
            else
                isUnderWater = false;
        }

        if (isOnDeepWater)
        {
            if (gameObject.transform.position.y + 0.5f < waterInstance.transform.position.y)
                isSemiUnderWater = true;
            else
                isSemiUnderWater = false;
        }
        

        if(isUnderWater)
        {
            RenderSettings.fogDensity = 0.06f;
            RenderSettings.fogColor = underWaterColor;
            playerHead.GetComponent<MotionBlur>().blurAmount = playerHead.GetComponent<MotionBlur>().blurAmount + 0.01f;

        }
        else
        {
            if (!PlayerSanity.isDraining)
            {
                if (!normalSettings)
                    RenderSettings.fogDensity = 0.002f;
                else
                    RenderSettings.fogDensity = fogDensity;
                RenderSettings.fogColor = normalColor;
            }
        }


        if (isOnDeepWater || isOnLightWater)
            isOnWater = true;
        else
            isOnWater = false;

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Water")
        {
            waterInstance = other.gameObject;
            isOnLightWater = true;
        }
        else if (other.tag == "Deep Water")
        {
            waterInstance = other.gameObject;
            isOnDeepWater = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Water")
        {
            isOnLightWater = false;
        }
        else if (other.tag == "Deep Water")
        {
            isOnDeepWater = false;
        }
    }
}