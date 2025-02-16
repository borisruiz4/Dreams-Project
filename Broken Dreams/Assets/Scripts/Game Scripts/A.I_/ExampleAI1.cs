﻿using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;

public class ExampleAI : MonoBehaviour
{

    //Worst Case Scenario, the inclusion of the aypoints may require the Navigation Mesh to be rebaked.

    private bool chase = false;
    private bool patrol = true;
    private bool active = true;
    private bool outOfSight = true;
    private bool isOnWaypoint = false;
    private int waypointCount = 0;
    private int currentWaypoint = 0;
    private UnityEngine.AI.NavMeshAgent agent;
    private float lostValue;

    public Transform target;
    public Collider AIFOV;
    public Animator animator;
    public Transform[] waypoints;
    public float destinationResetTime = 1.0f;
    public Collider hitBox;
    public Collider AIAttackRange;


    void Awake()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        waypointCount = waypoints.Length;
        changeWaypoint();
    }

    void Update()
    {
        //Debug.Log("IS on Waypoint: " + agent.destination);
        float distance = Vector3.Distance(agent.transform.position, target.transform.position);
  //      if(chase)
       // Debug.Log(lostValue);
        if (distance < 15)
            lostValue = 0;
        else
            lostValue += 0.01f;

        if(lostValue > 7 && chase)
        {
            chase = false;
            //   waypointCount += 1;
            //  waypoints[waypointCount].position = agent.destination;
            patrol = true;
        }

        if (chase)
        {
            agent.speed = 4;
            AIFOV.enabled = false;
            AIAttackRange.enabled = true;
        }
        else
        {
            agent.speed = 1.5f;
            AIFOV.enabled = true;
            AIAttackRange.enabled = false;
        }

        if (chase && !patrol && active && lostValue < 7)
        {
           // Debug.Log("IS CHASING");
            StartCoroutine(chaseTarget());
        }

        else if (patrol && !chase && active)
        {
            //Debug.Log("IS PATROLING");
            StartCoroutine(patrolRoom());
        }
    }

    IEnumerator chaseTarget()
    {
        animator.Play("Run");
        yield return new WaitForSeconds(0.1f);
        agent.SetDestination(target.position);
       // active = true;
        StopCoroutine(chaseTarget());
    }

    IEnumerator attack()
    {
       // agent.Stop();
        active = false;
        animator.CrossFade("Attack", 0.3f);
        yield return new WaitForSeconds(0.1f);
        hitBox.enabled = true;
        yield return new WaitForSeconds(0.1f);
        hitBox.enabled = false;
       // agent.Resume();
       // yield return new WaitForSeconds(1f);
        active = true;
        StopCoroutine(attack());
    }

    IEnumerator patrolArea()
    {
        //animator.CrossFade("Walk");
        yield return new WaitForSeconds(2f);
        agent.Stop();
     //   Debug.Log("Patrol Stopped");
        //animator.CrossFade("Idle");
        int newWaypoint = Random.RandomRange(0, waypointCount);
        agent.SetDestination(waypoints[newWaypoint].position);
        agent.Resume();
        yield return new WaitForSeconds(2f);
        active = true;
        //Debug.Log("Patrol Continued");
        StopCoroutine(patrolArea());
    }

    IEnumerator patrolRoom()
    {
        float distance = Vector3.Distance(agent.transform.position, agent.destination);

       if (distance < 1f)
        {
            if (!chase)
            {
                active = false;
                animator.CrossFade("Idle", 1f);
                yield return new WaitForSeconds(6f);
                active = true;
            }
        }
        if (agent.transform.position != agent.destination)
        {
            if (!chase && active)
            {
                animator.CrossFade("Walk", 0f);
            }
        }
        else
        {
           StartCoroutine(resetPath());
        }



        StopCoroutine(patrolRoom());
    }

    IEnumerator resetPath()
    {
            active = false;
            agent.Stop();
            changeWaypoint();
            yield return new WaitForSeconds(6f);
            agent.Resume();
            active = true;
            StopCoroutine(resetPath());
    }

    void changeWaypoint()
    {
        int newWaypoint = Random.RandomRange(0, waypointCount);
        agent.SetDestination(waypoints[newWaypoint].position);
        currentWaypoint = newWaypoint;
    }


    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            if (chase)
            {
                outOfSight = false;
                lostValue = 0;
            }
            chase = true;
            patrol = false;
            //  tappedWaitForSecondsOrTap(); 
            //Debug.Log("FOUND YOU");

            if (chase && !AIFOV.enabled)
            {
                StartCoroutine(attack());
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            if (chase)
            {
                outOfSight = true;
            }
           // StartCoroutine(patrolRoom());
        }
    }






    //Advanced Courontines

    private float __gWaitSystem;
    void tappedWaitForSecondsOrTap()
    {
        __gWaitSystem = 0.0f;
    }
    IEnumerator WaitForSecondsOrTap(float seconds)
     {
     __gWaitSystem = seconds;
     while ( __gWaitSystem>0.0 )
         {
         __gWaitSystem -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
         }
     }


}