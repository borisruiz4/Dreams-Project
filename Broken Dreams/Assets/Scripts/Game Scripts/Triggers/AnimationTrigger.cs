﻿using UnityEngine;
using System.Collections;

public class AnimationTrigger : MonoBehaviour {

    public Animator animator;
    public string tagName;
    public string animationName;

    void OnTriggerEnter(Collider col)
    {
        if(col.tag == tagName)
        {
            animator.Play(animationName);
        }
    }
}
