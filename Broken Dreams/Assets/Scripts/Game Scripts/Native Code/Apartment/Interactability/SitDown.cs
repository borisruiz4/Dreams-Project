﻿using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;

public class SitDown : MonoBehaviour
{
    public GameObject player;
    public bool canSitDown = false;
    public static bool sitDown = false;
    public static bool isSatDown = false;
    public bool satDown;
    bool getOut = false;
    bool resetActivator = true;
    public int tag;

    public GameObject chair;
    public GameObject chair_Position;
    public GameObject chair_ExitPosition;

    void Update()
    {

        satDown = isSatDown;
       // Debug.Log("SitDown : " + sitDown + " CanSitDown : " + canSitDown + " isSatDown : " + isSatDown);
    
        if (Raycast_Pickup.chairInstance == chair)
        {
            canSitDown = true;
        }
        else
        {
            canSitDown = false;
        }

        if (sitDown)
        {

            if (Raycast_Pickup.chairInstance == chair)
            {
                if (!isSatDown)
                    StartCoroutine(enter());

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    getOut = true;
                }
            }
        }

        if (getOut)
            StartCoroutine(exit());

    }




    private IEnumerator exit()
    {
        player.transform.position = Vector3.Slerp(player.transform.position, chair_ExitPosition.transform.position, 5f * Time.deltaTime);
        yield return new WaitForSeconds(0.8f);
        player.GetComponent<CharacterController>().detectCollisions = true;
        player.GetComponent<Rigidbody>().detectCollisions = true;
        player.GetComponent<FirstPersonController>().enabled = true;
        Raycast_Pickup.mouseClickToggle = false;
        sitDown = false;
        isSatDown = false;
        resetActivator = true;
        getOut = false;
        canSitDown = false;
    }

    private IEnumerator enter()
    {
        player.GetComponent<CharacterController>().detectCollisions = false;
        player.GetComponent<Rigidbody>().detectCollisions = false;
        player.GetComponent<FirstPersonController>().enabled = false;
        player.transform.position = Vector3.Slerp(player.transform.position, chair_Position.transform.position, 5f * Time.deltaTime);
        player.transform.rotation = Quaternion.Slerp(player.transform.rotation, chair_Position.transform.rotation, 4f * Time.deltaTime);
        yield return new WaitForSeconds(0.9f);
        isSatDown = true;
        if (resetActivator)
        {
            StartCoroutine(player.GetComponent<FirstPersonController>().mouseLookReset());
            resetActivator = false;
        }
        player.GetComponent<FirstPersonController>().enabled = true;
        StopCoroutine(enter());
    }

}
