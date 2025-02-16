﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightOccluder : MonoBehaviour
{

    //   public GameObject

    GameObject player;
    public float distanceOcclude = 3;
    public bool bakeMode;

    void Awake()
    {
        player = GameObject.Find("Player");
    }

    void Update()
    {
        //   Debug.Log("CURRENT STATUS : " + gameObject.GetComponent<Renderer>().isVisible);

        //    gameObject.GetComponent<Renderer>().enabled = gameObject.GetComponent<Renderer>().isVisible;

        float distance = Vector3.Distance(gameObject.transform.position, player.transform.position);

        if (distance > distanceOcclude)
        {
            Transform[] temp = new Transform[gameObject.GetComponentsInChildren<Transform>().Length - 1];
            for (int i = 0; i < gameObject.GetComponentsInChildren<Transform>().Length; i++)
            {
                if (!bakeMode)
                    gameObject.GetComponentsInChildren<Light>()[i].enabled = false;
            }
        }
        else
        {
            Transform[] temp = new Transform[gameObject.GetComponentsInChildren<Transform>().Length - 1];
            for (int i = 0; i < gameObject.GetComponentsInChildren<Transform>().Length; i++)
            {
                if (!bakeMode)
                    gameObject.GetComponentsInChildren<Light>()[i].enabled = true;
            }
        }

    }
}
