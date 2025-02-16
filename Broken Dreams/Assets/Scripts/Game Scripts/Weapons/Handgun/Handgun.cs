﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Handgun : MonoBehaviour {

    //Animation
    public Animator animator;

    //Functionality

    public Camera player;
    public float damage = 5;
    public float range = 5;
    public float force = 5;
    public float fireRateDelay = 0.35f;
    public bool canShoot = true;
    public int startingAmmo = 0;
    public int magazineSize = 7;

    protected int ammo = 0;
    protected int reserveAmmo = 0;

    //VIsuals
    public bool hasEffect = false;
    public GameObject muzzleFlash;
    public GameObject muzzleGlashLocation;
    public ParticleHitManager particleManager;
    public Image crosshair;

    //Mouse counter
    int fireAmount = 0;

    void Awake()
    {
        if (startingAmmo > 0)
        {
            reserveAmmo += startingAmmo;
            if (reserveAmmo >= magazineSize)
            {
                ammo += magazineSize;
                reserveAmmo -= magazineSize;
            }
            else
            {
                ammo += reserveAmmo;
                reserveAmmo = 0;
            }
        }
    }
   
    void Update()
    {
        if(fireAmount > 3)
        {
            fireAmount = 0;
       //     canShoot = true;
        }


        if (!WeaponWheel.isShowing && !InventoryMenu.inventroyIsUp)
        {
            if (Input.GetMouseButton(0))
            {
                if (canShoot && ammo > 0)
                    StartCoroutine(Shoot(fireRateDelay));

                fireAmount++;
            }
            if (Input.GetKeyDown(KeyCode.R))
                StartCoroutine(Reload(2f));
        }

        //Check the rang eof enemies
        rangeCheck();

    }

    IEnumerator Shoot(float x)
    {
        canShoot = false;
        int rand = Random.RandomRange(1, 3);
        switch (rand)
        {
            case 1:
                animator.Play("Shoot1");
                break;
            case 2:
                animator.Play("Shoot2");
                break;
        }
        if (!(this.animator.GetCurrentAnimatorStateInfo(1).IsName("Shoot1") && this.animator.GetCurrentAnimatorStateInfo(1).IsName("Shoot2")))
            Fire();
        yield return new WaitForSeconds(x);
        canShoot = true;
        StopCoroutine(Shoot(x));
    }

    IEnumerator Reload(float x)
    {
        if (reserveAmmo > 0 && ammo < magazineSize)
        {
            canShoot = false;
            animator.Play("Reload");
            yield return new WaitForSeconds(x);
            Reload();
            canShoot = true;
        }

        StopCoroutine(Reload(x));
    }

    //Functionality

    void Fire()
    {
        //   muzzleFlash.Play();
        ammo--;
        RaycastHit hit;
        if (Physics.Raycast(player.transform.position, player.transform.forward, out hit, range, 1 << LayerMask.NameToLayer("Default")))
        {
            //Debug.Log(hit.transform.name);

            if (hit.transform.GetComponent<DestroyableObject>() != null)
            {
                hit.transform.GetComponent<DestroyableObject>().takeDamage(damage);
            }

            else if (hit.transform.GetComponent<DamagePoint>() != null)
            {
                hit.transform.GetComponent<DamagePoint>().takeDamage(damage);
            }

            if(hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * 1000 * force);
            }

        }


        manageEffects();
    }

    void rangeCheck()
    {
        RaycastHit hit;
        if (Physics.Raycast(player.transform.position, player.transform.forward, out hit, range, 1 << LayerMask.NameToLayer("Default")))
        {
            if(crosshair != null)
            {
                if (hit.collider.GetComponent<DamageSystem>() != null || hit.collider.GetComponent<DamagePoint>() != null)
                {
                    crosshair.color = Color.red;
                }
                else
                {
                    crosshair.color = Color.white;
                }
            }
        }
    }

    void manageEffects()
    {
        RaycastHit hit;

        if (Physics.Raycast(player.transform.position, player.transform.forward, out hit, range, 1 << LayerMask.NameToLayer("Default")))
        {
            //Declares Components

            GameObject impactEffect = null;
            GameObject impactDecal = null;            
            AudioClip impactSound = null;

            //Find out what type of material we're dealing with

            string tag; //Tag of object you're hitting

            if (hit.collider.gameObject.GetComponent<MaterialInfo>() != null)
                tag = hit.collider.gameObject.GetComponent<MaterialInfo>().materialTag;
            else
                tag = "Generic";

            MaterialObject curentObject = particleManager.getMaterialObject(tag);
            impactEffect = curentObject.particleEffect;
            impactDecal = curentObject.decal;
                


            //Make Sure the components aren't missing

            if (impactEffect != null)
            {
                GameObject effectParticle = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal)) as GameObject;
                Destroy(effectParticle, 2f);
            }

            if (impactDecal != null)
            {
                GameObject effectDecal = GameObject.Instantiate(impactDecal, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(effectDecal, 10f);
            }

            if (hasEffect)
            {
                GameObject effectMuzzle = GameObject.Instantiate(muzzleFlash, muzzleGlashLocation.transform);
                Destroy(effectMuzzle, 2f);
            }

        }
    }

    void Reload()
    {
        int ammoValue;
        if (reserveAmmo > 0)
        {
            while (ammo < magazineSize && reserveAmmo > 0)
            {
                ammo++;
                reserveAmmo--;
            }
        }
    }
   
    //Outisde Access Functions

    public void addAmmo(int x)
    {
        reserveAmmo += x*magazineSize;
    }

    public int getAmmo()
    {
        return ammo;
    }

    public int getReserveAmmo()
    {
        return reserveAmmo;
    }
}
