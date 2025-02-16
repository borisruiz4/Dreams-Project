﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DamageMe : MonoBehaviour {
  
    bool canBeDamaged = true;
    Animator damageAnim;
    public AudioSource normalDamageSound;
    public AudioSource progressiveDamageSound;
    public bool godMode;
    bool isInProgressiveDanagerArea = false;
    float damageAmountRecieve = 1;

    void Awake()
    {
        damageAnim = GetComponent<Animator>();
    }

    void Update()
    {
        if(isInProgressiveDanagerArea)
            PlayerHealth.health -= Time.deltaTime*damageAmountRecieve;

    }

    public void takeDamage(float x)
    {
        if (PlayerHealth.health > 0 && !godMode)
            if (canBeDamaged)
        {
            canBeDamaged = false;
            damageAnim.Play("NormalDamage");
            PlayerHealth.health -= x;
            normalDamageSound.Play();
            StartCoroutine(DamageDelay());
            PlayerHealth.InDanger = true;
        }
    }

    public void enterProgressiveDamageArea(float x)
    {
        if (PlayerHealth.health > 0 && !godMode)
        {
            damageAnim.Play("ProgressiveDamage");
            isInProgressiveDanagerArea = true;
            progressiveDamageSound.Play();
            PlayerHealth.InDanger = true;
            damageAmountRecieve = x;
        }
    }

    public void exitProgressiveDamageArea()
    {
        if (PlayerHealth.health > 0)
        {
            damageAnim.Play("ProgressiveDamage_Recover");
            PlayerHealth.InDanger = false;
            isInProgressiveDanagerArea = false;
            damageAmountRecieve = 1;
        }
    }

    public IEnumerator DamageDelay()
    {
        yield return new WaitForSeconds(1f);
        canBeDamaged = true;
        StopCoroutine(DamageDelay());
        PlayerHealth.InDanger = false;
    }
}
