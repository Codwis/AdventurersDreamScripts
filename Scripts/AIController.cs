using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Mathematics;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Stats))]
public class AIController : MonoBehaviour
{
    [Tooltip("AI info object")] public AIInfo info;
    [Tooltip("Headbone looks at this")] public Transform headFollow;

    public bool canMove = true;
    public bool follower = false;
    public bool saveOgStandSpot;
    private Vector3 standSpot;

    public Transform sword;
    public Transform swordHolderHand;
    public Transform swordHolster;

    public bool commander = false;
    public bool wanderer = false;
    public bool scaredy = false;

    private Transform wanderLow, wanderHigh;
    private Vector3 currentWanderSpot;

    [NonSerialized] public AIMeleeWeapons aiMelee;
    [NonSerialized] public AIRanged aiRange;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform currentSpot;
    private Transform targetHead;
    private Transform ogHeadSpot;
    private bool chasing = false;
    private Stats stats;

    [HideInInspector] public Transform currentAgro = null;

    private Tool pickUp;
    private Work work;
    public Transform lookOfInterest;
    private const float descale = 0.3f;
    private const float closerRatio = 0.5f;
    private LayerMask toolMask;

    [NonSerialized] public Transform player;

    private void Start()
    {
        TryGetComponent<Work>(out work);
        agent = GetComponent<NavMeshAgent>();

        if(info.aiType == AiType.range) //if its a ranged type set the controller for the script
        {
            GetComponentInChildren<AIRanged>().SetController(this);
        }

        if (saveOgStandSpot) standSpot = transform.position;

        if(info.randomSize)
        {
            float random = UnityEngine.Random.Range(info.minRandom, info.maxRandom);
            transform.localScale = new Vector3(random, random, random);
        }

        toolMask = 1 << LayerMask.NameToLayer("Tool");

        ogHeadSpot = Instantiate(headFollow.gameObject, headFollow.parent).transform;
        animator = GetComponent<Animator>();

        TryGetComponent<AIMeleeWeapons>(out aiMelee);
        TryGetComponent<AIRanged>(out aiRange);
        TryGetComponent<Stats>(out stats);
    }

    private void CheckToolDistance()
    {
        if(Vector3.Distance(transform.position, pickUp.transform.position) < info.pickupRange)
        {
            work.Equiptool(pickUp);
            animator.Play("pickup");
            pickUp = null;
        }
    }

    private void FixedUpdate()
    {
        agent.isStopped = false;
        if(lookOfInterest != null)
        {
            if (CheckForDistance(lookOfInterest.position) > info.agroRange)
            {
                lookOfInterest = null;
            }
        }


        if (agent.velocity.magnitude / agent.speed > 0.1f)
        {
            animator.SetFloat("MoveSpeed", agent.velocity.magnitude / agent.speed);
        }
        else
        {
            animator.SetFloat("MoveSpeed", 0);
        }

        if (pickUp != null)
        {
            CheckToolDistance();
        }
        if(!canMove)
        {
            return;
        }

        int check = 0;
        if(!scaredy)
        {
            check = CheckForTargets();
        }
        

        if (check == 0 || scaredy && follower)
        {
            if(currentSpot == null)
            {
                animator.SetBool("Chasing", false);
                chasing = false;
            }
            if (follower)
            {
                CheckForFriendly();
            }
        }
        else if (check > 0 && !scaredy)
        {
            pickUp = null;
        }

        if (wanderer || scaredy && chasing || scaredy && currentAgro != null) //wanderer
        {
            if(currentSpot == null)
            {
                if (agent.remainingDistance < agent.stoppingDistance || !wanderTimerStarted)
                {
                    currentWanderSpot = RandomSpotWanderTo();

                    StartCoroutine(WanderTimer());
                    SetAgentTarget(currentWanderSpot);
                }
                else
                {
                    TurnTowardsSpot(currentWanderSpot);
                }
            }
        }

        if (currentSpot != null && !scaredy) //chasing
        {

            if(currentSpot.gameObject.layer == LayerMask.NameToLayer("Dead"))
            {
                currentSpot = null;
                chasing = false;
            }
            else if (CheckForDistance() <= info.agroRange)
            {
                SetAgentTarget(currentSpot.position);
            }
            else
            {
                chasing = false;
                currentSpot = null;
            }
        }

        if (work && lookOfInterest == null && currentSpot == null) //work
        {
            if (work is Farmer farm)
            {
                if(farm.currentPlot == null)
                {
                    work.FindWork(this);
                }
                else
                {
                    if(work.permamentTool != null)
                    {
                        if(work.permamentTool.toolType == farm.currentPlot.toolTypeRequired)
                        {
                            chasing = true;
                            if (farm.currentPlot.readyToHarvest)
                            {
                                GetComponent<Animator>().SetBool("Chasing", true);
                            }
                            else
                            {
                                GetComponent<Animator>().SetBool("Chasing", false);
                            }
                            SetAgentTarget(farm.currentPlot.transform.position);
                            return;
                        }
                    }
                    if(work.currentTool == null)
                    {
                        CheckForTools();
                    }
                    else if(work.currentTool.toolType != farm.currentPlot.toolTypeRequired)
                    {
                        work.RemoveTool(work.currentTool);
                        CheckForTools();
                    }
                    else
                    {
                        SetAgentTarget(farm.currentPlot.transform.position);
                    }
                }
            }
        }
        else if(lookOfInterest != null)
        {
            agent.isStopped = true;
        }

        if(currentSpot != null)
        {
            TurnTowardsSpot(currentSpot.position);
        }
        else if (targetHead != null) //if ai is stopped and there is target rotate the ai
        {
            TurnTowardsSpot(targetHead.position);
        }
        else if(lookOfInterest != null)
        {
            TurnTowardsSpot(lookOfInterest.position);
        }
    }

    private void LateUpdate()
    {
        if (targetHead != null && currentSpot != null)
        {
            //Calculates the angle to target
            Vector3 aiToTarget = currentSpot.position - transform.position;
            float angle = Vector3.Angle(transform.forward, aiToTarget);

            if(angle > -90 && angle < 90) //if target is infront then move the head smoothly
            {
                headFollow.position = Vector3.Slerp(headFollow.position, targetHead.position, 0.75f * Time.deltaTime);
            }
        }
        else if(targetHead != null && wanderer)
        {
            //Calculates the angle to target
            Vector3 aiToTarget = currentWanderSpot - transform.position;
            float angle = Vector3.Angle(transform.forward, aiToTarget);

            if (angle > -90 && angle < 90) //if target is infront then move the head smoothly
            {
                headFollow.position = Vector3.Slerp(headFollow.position, targetHead.position, 0.75f * Time.deltaTime);
            }
        }
        else //If there is no target then move back to normal
        {
            if(lookOfInterest == null)
            {
                headFollow.position = Vector3.Slerp(headFollow.position, ogHeadSpot.position, 0.5f * Time.deltaTime);
            }
        }

        if(!chasing && !wanderer)
        {
            if(targetHead != null)
            {
                headFollow.position = Vector3.Slerp(headFollow.position, targetHead.position, 3 * Time.deltaTime);
            }
            if(lookOfInterest != null)
            {
                TurnTowardsSpot(lookOfInterest.position);
                if(targetHead == null)
                {
                    headFollow.position = Vector3.Slerp(headFollow.position, lookOfInterest.position, 3 * Time.deltaTime);
                }
            }
            else if (standSpot != null)
            {
                if(CheckForDistance(standSpot) > 1f)
                {
                    SetAgentTarget(standSpot);
                }
            }
        }
    }

    private const float wanderTime = 10;
    private bool wanderTimerStarted = false;
    private IEnumerator WanderTimer()
    {
        wanderTimerStarted = true;
        yield return new WaitForSeconds(wanderTime);
        wanderTimerStarted = false;
    }

    private Vector3 RandomSpotWanderTo()
    {
        if(wanderLow == null || wanderHigh == null)
        {
            if(currentSpot != null)
            {
                Vector3 re = transform.position - currentSpot.position;
                return re * 10;
            }
            else if(currentAgro != null)
            {
                Vector3 te = transform.position - currentAgro.position;
                return te * 10;
            }
            return transform.position + new Vector3(UnityEngine.Random.Range(-20, 20), 0, UnityEngine.Random.Range(-20, 20));
        }
        float x = UnityEngine.Random.Range(wanderLow.position.x, wanderHigh.position.x);
        float z = UnityEngine.Random.Range(wanderLow.position.z, wanderHigh.position.z);

        Vector3 r = new Vector3(x, 100, z);
        
        Physics.Raycast(r, Vector3.down, out RaycastHit hit, 1000, LayerMask.GetMask("Ground"));
        return hit.point;
    }

    //Sets agents destination
    private void SetAgentTarget(Vector3 spot)
    {

        agent.SetDestination(spot);
    }

    private void OnDestroy()
    {
        Destroy(aiRange);
        Destroy(aiMelee);
        Destroy(agent);
        Destroy(GetComponentInChildren<AIHeadController>());
        Destroy(this);
    }

    public void SetLookOfInterest(Transform source)
    {
        lookOfInterest = source;
    }
    public void RemoveLookOfInterest()
    {
        lookOfInterest = null;
    }

    public void AllowMovement()
    {
        canMove = !canMove;
        if(canMove)
        {
            if(work is Farmer farm)
            {
                farm.currentPlot = null;
            }
        }
    }


    //backs up the ai
    private void BackUp()
    {
        if (currentSpot != null)
        {
            TurnTowardsSpot(currentSpot.position);
        }

        return;
        animator.SetFloat("MoveSpeed", -1);
        agent.Move(-transform.forward * (agent.speed / 1.5f) * Time.deltaTime);



    }

    //Checks if there is another ai who has this script infront of them
    private bool CheckForAnotherEnemy(Vector3 spot)
    {
        if(targetHead != null)
        {
            RaycastHit hit;
            if(Physics.Raycast(spot + transform.forward, targetHead.position - spot, out hit, 10))
            {
                if (hit.transform.root.TryGetComponent<AIController>(out _))
                {
                    return true;
                }
                
            }
        }
        

        return false;
    }

    //Returns the current spot
    public Vector3 GetCurrentTarget()
    {
        if (currentSpot == null)
        {
            return Vector3.zero;
        }

        Vector3 aimSpot = targetHead.position - new Vector3(UnityEngine.Random.Range(-info.accuracy, info.accuracy), UnityEngine.Random.Range(-info.accuracy, info.accuracy));
        return aimSpot;
    }

    //Returns if the ai is withing shooting range
    public bool CanShoot()
    {
        if(CheckForDistance() > info.shootRange)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    //Sets agents max speed
    public void SetSpeed(float newSpeed)
    {
        agent.speed = newSpeed;
    }

    //Returns the distance between currentspot and itself
    public float CheckForDistance()
    {
        if (!chasing || currentSpot == null) // return high number if not chasing so melee wont attack air
        {
            return 1000; 
        }
        return Vector3.Distance(transform.position, currentSpot.position);
    }
    public float CheckForDistance(Vector3 spot)
    {
        return Vector3.Distance(transform.position, spot);
    }

    //Returns how close player is to this
    public float CalculateDistanceRatio()
    {
        if(currentSpot != null)
        {
            if (Vector3.Distance(transform.position, currentSpot.position) > agent.stoppingDistance)
            {
                return 1;
            }
            else
            {
                return Vector3.Distance(transform.position, currentSpot.position) / agent.stoppingDistance;
            }
        }
        else
        {
            return 1;
        }
    }

    public void SetTarget(Transform source)
    {
        animator.SetBool("Chasing", true);
        chasing = true;
        currentSpot = source;
        agent.SetDestination(currentSpot.position);
    }

    
    public void Agro(Transform source)
    {
        currentAgro = source;
    }

    private void NormalState()
    {

    }
    private void DrawSword()
    {
        if(animator.GetBool("Chasing"))
        {
            sword.SetParent(swordHolderHand);
        }
        else
        {
            sword.SetParent(swordHolster);
        }
        sword.localPosition = Vector3.zero;
        sword.localRotation = quaternion.identity;
    }

    //Tries to find a player withing a sphere
    private int CheckForTargets()
    {
        if(currentAgro != null)
        {
            currentSpot = currentAgro;

            if (Vector3.Distance(transform.position, currentSpot.position) < info.backupRange)
            {
                agent.ResetPath();
                BackUp();
            }

            targetHead = currentSpot.transform.GetComponentInParent<RagdollController>().head;
            if(targetHead.gameObject.layer == LayerMask.NameToLayer("Dead"))
            {
                currentAgro = null;
            }
            else
            {
                SetAgentTarget(currentAgro.position);
                animator.SetBool("Chasing", true);
                chasing = true;
                return 1;
            }
        }

        if (stats.faction.enemies == 0) return 0;

        //gets colliders within the sphere
        Collider[] colliders = Physics.OverlapSphere(transform.position, info.agroRange, stats.faction.enemies);

        if(colliders.Length != 0)
        {
            float dist = 0;
            Stats closest = null;

            //goes through all colliders and tries to find player
            foreach(Collider col in colliders)
            {
                Stats tempStats = col.transform.GetComponentInParent<Stats>();
                if (tempStats == null) continue;
                //Checks that they are alive
                if (!tempStats.alive)
                {
                    continue;
                }


                if(closest == null)
                {
                    closest = col.transform.GetComponentInParent<Stats>();
                    dist = Vector3.Distance(transform.position, col.transform.position);
                }
                else if(tempStats == closest)
                {
                    continue;
                }
                else if(Vector3.Distance(transform.position, col.transform.position) < dist)
                {
                    closest = col.transform.GetComponentInParent<Stats>();
                    dist = Vector3.Distance(transform.position, col.transform.position);
                }

                if (col.transform.GetComponentInParent<PlayerController>())
                {
                    currentSpot = col.transform.root;

                    //If player too close then reset the path and backup
                    if (Vector3.Distance(transform.position, currentSpot.position) < info.backupRange)
                    {
                        agent.ResetPath();
                        BackUp();
                    }
                    else
                    {

                        //Check if the target is lower than them and find closest edge
                        if (transform.position.y - col.transform.position.y > 0.5f && TryGetComponent<RangedScript>(out _))
                        {
                            agent.FindClosestEdge(out NavMeshHit hit);
                            SetAgentTarget(hit.position);
                        }
                        else
                        {
                            SetAgentTarget(col.transform.position);
                        }
                        targetHead = currentSpot.transform.GetComponentInParent<RagdollController>().head;
                    }
                    animator.SetBool("Chasing", true);
                    chasing = true;
                    return 1;
                }
                else if(col.transform.GetComponentInParent<AIController>())
                {
                    currentSpot = col.transform;

                    if (Vector3.Distance(transform.position, currentSpot.position) < info.backupRange)
                    {
                        agent.ResetPath();
                        BackUp();
                    }
                    else
                    {

                        if (transform.position.y - col.transform.position.y > 0.5f)
                        {
                            agent.FindClosestEdge(out NavMeshHit hit);
                            SetAgentTarget(hit.position);
                        }
                        else
                        {
                            SetAgentTarget(col.transform.position);
                        }
                        targetHead = currentSpot.transform.GetComponentInParent<RagdollController>().head;
                    }
                    animator.SetBool("Chasing", true);
                    chasing = true;
                    return 1;
                }
            }

            animator.SetBool("Chasing", false);
            chasing = false;
            currentSpot = null;
            targetHead = null;
        }
        else
        {
            currentSpot = null;
        }
        if(work is Farmer farmer)
        {
            if(farmer.currentPlot != null)
            {
                return 0;
            }
        }

        animator.SetBool("Chasing", false);
        chasing = false;
        currentSpot = null;
        return 0;
    }

    private void CheckForFriendly()
    {

        LayerMask mask = new LayerMask();
        mask |= stats.faction.friendly;
        mask |= (1 << gameObject.layer);

        //gets colliders within the sphere
        Collider[] colliders = Physics.OverlapSphere(transform.position, info.agroRange, mask);

        if (colliders.Length != 0)
        {
            //goes through all colliders and tries to find player
            foreach (Collider col in colliders)
            {
                if (!col.transform.GetComponentInParent<Stats>().alive) continue;
                if (col.transform.GetComponentInParent<PlayerController>())
                {
                    currentSpot = col.transform.root;

                    //If player too close then reset the path and backup
                    if (Vector3.Distance(transform.position, currentSpot.position) < info.backupRange)
                    {
                        agent.ResetPath();
                        BackUp();
                    }
                    else
                    {
                        if (transform.position.y - col.transform.position.y > 0.5f)
                        {
                            agent.FindClosestEdge(out NavMeshHit hit);
                            SetAgentTarget(hit.position);
                        }
                        else
                        {
                            SetAgentTarget(col.transform.position);
                        }
                        targetHead = currentSpot.transform.root.GetComponent<RagdollController>().head;
                    }
                    return;
                }
                else if (col.transform.GetComponentInParent<AIController>())
                {

                    if(!col.transform.GetComponentInParent<AIController>().commander)
                    {
                        continue;
                    }
                    currentSpot = col.transform;

                    if (Vector3.Distance(transform.position, currentSpot.position) < info.backupRange)
                    {
                        agent.ResetPath();
                        BackUp();
                    }
                    else
                    {

                        if (transform.position.y - col.transform.position.y > 0.5f)
                        {
                            agent.FindClosestEdge(out NavMeshHit hit);
                            SetAgentTarget(hit.position);
                        }
                        else
                        {
                            SetAgentTarget(col.transform.position);
                        }
                        targetHead = currentSpot.transform.GetComponentInParent<RagdollController>().head;
                    }
                    return;
                }
            }

            animator.SetBool("Chasing", false);
            chasing = false;
            currentSpot = null;
        }
        else
        {
            currentSpot = null;
        }
    }

    
    private void CheckForTools()
    {
        //gets colliders within the sphere
        Collider[] colliders = Physics.OverlapSphere(transform.position, info.agroRange, toolMask);

        if (colliders.Length != 0)
        {
            //goes through all colliders and tries to find player
            foreach (Collider col in colliders)
            {
                if(col.TryGetComponent<Tool>(out Tool tool))
                {
                    if(work is Farmer farm)
                    {
                        if (tool.toolType == farm.currentPlot.toolTypeRequired)
                        {
                            currentSpot = col.transform;
                            SetAgentTarget(currentSpot.position);
                            pickUp = tool;
                            return;
                        }
                    }
                }

            }
        }
        else
        {
            currentSpot = null;
        }
    }

    public void Dead(Transform stat)
    {
        if(currentSpot == stat)
        {
            chasing = false;
            currentSpot = null;
        }
    }

    public float turnSpeed = 4;
    //Turns smoothly towards the spot
    private void TurnTowardsSpot(Vector3 spot)
    {
        Vector3 direction = (spot - transform.position).normalized;

        Quaternion lookRot = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, turnSpeed * Time.deltaTime);

        animator.SetFloat("TurnSpeed", 1 * Time.deltaTime);
    }

    //Returns how much room there is infront
    public float CheckForwardDistance(float maxDistance, int layermask)
    {
        //Checks if there is space for the sword infront
        if (Physics.Raycast(transform.position + transform.forward * closerRatio, transform.forward, out RaycastHit hit, maxDistance, layermask))
        {
            return (maxDistance - hit.distance) * descale;
        }
        else
        {
            return 0;
        }
    }

    public void PlayStepSound()
    {

    }
}
public enum AiType { melee, range, mix }
