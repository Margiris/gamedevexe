using UnityEngine;
using Pathfinding;
using System.Collections.Generic;
using UnityEngine.Networking;

public abstract class Tank : Character
{

    //public varables:
        //Tank settings
    public float visionAngle = 60; // vision angle (half of it)
    public float visionRange = 10;
    public float localSearchRadius = 5; // distance - how far to search for player from last known position
    public int localSearchTryLocation = 3; // how many locations to try after target lost
    public int localSearchLookAroundTimes = 2;
    public int fromAngleToTurnHead = 15;
    public int toAngleToTurnHead = 90; // max value should be 90 for realistic reaction
    public LayerMask obstacleMask;
    public float speedMultiplier = 0.8f;

    protected Transform head;
    public float widthOfFirePathChecker = 0.1f;

    [SyncVar]
    public bool isAlerted = false;

    //private variables:
    protected float normalSpeed;
    protected float alertedSpeed;
    protected IAstarAI ai;
    protected bool targetInAttackRange = false;
    protected AIDestinationSetter aiDestinationSetter;
    protected Patrol aiPatrol;
    protected int localSearchLocationsTried = 0;
    protected int localSearchLookedAround = 0;
    protected Vector2 searchAreaCenter;
    protected Head headScript;
    protected bool isLooking = false;
    protected EnemyHPBar HpBar;

    protected Animator animator;

    protected Animator alertionIndicatorAnimator;


    [SyncVar]
    protected float distanceToPlayer;
    [SyncVar]
    protected Vector2 directionToPlayer;

    //from single player stuff
    //protected bool isTargetDestinationPlayer = false;
    //protected Allerting playerAllerting;
    //protected Transform PLKP; //PlayerLastKnownPosition
    //protected GameObject player;
    //protected bool prevTargetInVision = false;

    protected bool targetInVision = false;

    //to multiplayer stuff
    protected List<int> prevTargetsInVision = new List<int>();
    protected List<int> targetsInVision;
    protected Dictionary<int, Transform> PLKPs;
    protected Dictionary<int, GameObject> players;
    protected Dictionary<int, Allerting> playerAlertings;
    [SyncVar]
    protected int targetedPlayerID = -1;
    protected bool noOneSeen = true;
    [SyncVar]
    protected int otherID = -1;
    [SyncVar]
    Vector2 directionToOther;
    [SyncVar]
    float distanceToOther;
    protected bool issensitive = true;

    // Use this for initialization
    protected virtual void Start () {
        //PLKP = GameObject.Find("PlayerLastKnownPosition").transform;
        //player = GameObject.Find("player");
        isAlerted = false;
        HpBar = transform.GetComponent<EnemyHPBar>();
        HpBar.Initiate();
        if (isServer)
        {
            UpdatePLayerList();

            head = transform.Find("body");
            headScript = head.GetComponent<Head>();
            animator = GetComponent<Animator>();

            alertionIndicatorAnimator = transform.GetComponent<Animator>();

            aiDestinationSetter = GetComponent<AIDestinationSetter>();
            aiPatrol = GetComponent<Patrol>();
            ai = GetComponent<IAstarAI>();
            //playerAllerting = player.GetComponent<Allerting>();
            healthCurrent = healthMax;
            normalSpeed = ai.maxSpeed;
            alertedSpeed = normalSpeed;
        }
        else
        {
            ai = GetComponent<IAstarAI>();
            ai.canMove = false;
            gameObject.GetComponent<AIPath>().enabled = false;
            gameObject.GetComponent<Patrol>().enabled = false;
            gameObject.GetComponent<SimpleSmoothModifier>().enabled = false;
        }
    }

    //PUBLIC METHODS:
    public void UpdatePLayerList()
    {
        if (isServer)
        {
            List<GameObject> gamers = GameObject.Find("LevelManager").GetComponent<LevelManager>().GetPlayersData();
            Debug.Log("updating LIST OF PLAYERS: " + gamers.Count);
            List<int> prevPlayersInVision = prevTargetsInVision;

            prevTargetsInVision = new List<int>();
            PLKPs = new Dictionary<int, Transform>();
            players = new Dictionary<int, GameObject>();
            playerAlertings = new Dictionary<int, Allerting>();


            for (int i = 0; i < gamers.Count; i++)
            {
                int ID = gamers[i].GetComponent<Gamer>().getPlayerID();
                playerAlertings.Add(ID, gamers[i].GetComponent<Gamer>().playerAllerting);
                players.Add(ID, gamers[i].GetComponent<Gamer>().player);
                PLKPs.Add(ID, gamers[i].GetComponent<Gamer>().PLKP);
                if (prevPlayersInVision.Contains(ID))
                {
                    prevTargetsInVision.Add(ID);
                }
            }
        }
    }

    protected virtual void Update()
    {
        if (isServer)
        {
            int prevTargetID = targetedPlayerID;
            bool prevTargetInVision = targetInVision;
            //////////// checkoing surroundings and changing targets
            CheckVision();
            // if target appeared in vision
            if (prevTargetID == -1 && targetedPlayerID != -1)
            {
                BecomeAllerted();
            }
            /////////////////////////////////////////////////////////


            if (isAlerted) // if alerted     ///if not then patrolling
            {
                if (targetInVision && !prevTargetInVision) // appeared in vision
                {
                    StartSpecialAttack();
                    SetAlertionIndicator();
                }
                else if (targetInVision) // continually in vision /// following player
                {
                    //if (prevTargetID != targetedPlayerID) // changed target
                    //{
                    //}
                    FollowTarget();
                    SpecialAttack();
                }
                else if (prevTargetInVision) // disappeared from vision
                {
                    if (playerAlertings[targetedPlayerID].howManySeeMe > 0) // still can follow 
                    {

                    }
                    else // can't follow anymore
                    {
                        GoToPLKP();
                    }
                    SetAlertionIndicator();
                    StopSpecialAttack();
                }
                else if (!prevTargetInVision) // continually not in vision
                {
                    if (ai.reachedEndOfPath && !ai.pathPending) // went to last known location and player not seen
                    {
                        LookAround();
                    }
                }
            }
            SetHeadRotation();
        }
    }

    //Stop pursuing player
    public virtual void ReturnToPatrol()
    {
        aiDestinationSetter.enabled = false;
        aiPatrol.enabled = true;
        isLooking = false;
        isAlerted = false;
        issensitive = true;
        targetedPlayerID = -1;
        SetAlertionIndicator();
    }

    //call this method to make enemy go to last known player position
    [Command]
    public void CmdPursuePlayer(int playerID)
    {
        Debug.Log("Atejo alertinimas " + isServer + " / " + isLocalPlayer);
        if (isServer)
        {
            if (!isAlerted) // if idle
            {
                targetedPlayerID = playerID;
                BecomeAllerted();
            }
            else if (issensitive) // allerted but sensitive (my target lost)
            {
                targetedPlayerID = playerID;
                BecomeAllerted();
            }
        }
    }
    
    public void PursuePlayer(int playerID)
    {
        if (isServer)
        {
            if (!isAlerted) // if idle
            {
                targetedPlayerID = playerID;
                BecomeAllerted();
            }
            else if (issensitive) // allerted but sensitive (my target lost)
            {
                targetedPlayerID = playerID;
                BecomeAllerted();
            }
        }
    }

    //PRIVATE METHODS:

    protected virtual void SpecialAttack()
    {

    }

    protected virtual void StartSpecialAttack()
    {

    }

    protected virtual void StopSpecialAttack()
    {

    }

    protected void FollowTarget()
    {
        aiPatrol.enabled = false;
        aiDestinationSetter.enabled = true;
        aiDestinationSetter.target = players[targetedPlayerID].transform;
        ai.canSearch = true;
    }

    protected void GoToPLKP()
    {
        aiPatrol.enabled = false;
        aiDestinationSetter.enabled = true;
        aiDestinationSetter.target = PLKPs[targetedPlayerID];
        ai.canSearch = false;
        ai.SearchPath();
    }

    protected void BecomeAllerted()
    {
        isAlerted = true;
        localSearchLocationsTried = 0;
        localSearchLookedAround = 0;
        isLooking = false;
        if (playerAlertings[targetedPlayerID].howManySeeMe > 0) // if someone can see player right now
        {
            FollowTarget();
        }
        else
        {
            GoToPLKP();
        }
        SetAlertionIndicator();
    }

    protected void CheckVision()
    {
        targetsInVision = new List<int>();
        targetInVision = false;
        distanceToPlayer = 9999999;
        distanceToOther = 9999999;
        directionToOther = new Vector2();
        float distanceToMyTarget = 9999999;
        Vector2 directionToMyTarget = new Vector2();
        otherID = -1;
        bool noTargets = true;
        noOneSeen = true;
        issensitive = true;
        foreach (var player in players)
        {
            //Debug.Log(player);
            //Debug.Log(player.Value);
            //Debug.Log(player.Value.transform);
            float _distanceToPlayer = Vector2.Distance(player.Value.transform.position, transform.position);
            Vector2 _directionToPlayer = (player.Value.transform.position - transform.position).normalized;
            if (_distanceToPlayer < visionRange) // if in range
            {
                if (Vector3.Angle(head.transform.up, _directionToPlayer) < visionAngle) // if in vision angle
                {
                    if (!Physics2D.Raycast(transform.position, _directionToPlayer, _distanceToPlayer, obstacleMask)) //if no obstacles in between
                    {
                        targetsInVision.Add(player.Key);
                        if(_distanceToPlayer < distanceToPlayer)
                        {
                            distanceToPlayer = _distanceToPlayer;
                            targetedPlayerID = player.Key;
                            targetInVision = true;
                            directionToPlayer = _directionToPlayer;
                            noTargets = false;
                            noOneSeen = false;
                            issensitive = false;
                        }
                    }
                }
            }
            if (noTargets && playerAlertings[player.Key].howManySeeMe > 0) // no one in vision and someone else can see this player
            {
                if(_distanceToPlayer < distanceToOther)
                {
                    distanceToOther = _distanceToPlayer;
                    directionToOther = _directionToPlayer;
                    otherID = player.Key;
                    noOneSeen = false;
                }
                if (player.Key == targetedPlayerID)
                {
                    distanceToMyTarget = _distanceToPlayer;
                    directionToMyTarget = _directionToPlayer;
                    issensitive = false;
                }
            }
        }
        if (noTargets && !noOneSeen) // if no one in Vision and someone is visable 
        {
            if (isAlerted)
            {
                if (playerAlertings[targetedPlayerID].howManySeeMe > 0) // set my target if someone else can see it
                {

                    distanceToPlayer = distanceToMyTarget;
                    directionToPlayer = directionToMyTarget;
                }
                else
                {
                    targetedPlayerID = otherID;
                    distanceToPlayer = distanceToOther;
                    directionToPlayer = directionToOther;
                }
            }
        }

        for(int i = 0; i < targetsInVision.Count; i++)
        {
            int idx = prevTargetsInVision.IndexOf(targetsInVision[i]);
            if(idx == -1) // player appeared into vision
            {
                PlayerEnterVision(targetsInVision[i]);
            }
            else // player still is in vision - no changes
            {
                prevTargetsInVision.RemoveAt(idx);
            }
        }
        for (int i = 0; i < prevTargetsInVision.Count; i++) // players who disappeared from view
        {
            PlayerExitVision(prevTargetsInVision[i]);
        }
        prevTargetsInVision = targetsInVision;
    }
    
    protected void SetHeadRotation()
    {
        //Set head target direction: to player in vision or PLKP going to or if in patrol mode straight if looking dont set
        if (!isLooking)
        {
            if(isAlerted && localSearchLocationsTried == 0 && targetedPlayerID == -1)
            {
                Debug.Log("Tank LOGIC ERROR player target not selected, but alerted");
                if (headScript != null)
                    headScript.SetTargetAngle(VectorToAngle(transform.up));
                else
                    Debug.Log("Failure HeadScript not set");
            }
            else if (isAlerted && localSearchLocationsTried == 0) // alerted but not searching area around PLKP
            {
                if (playerAlertings[targetedPlayerID].howManySeeMe > 0) // me or others can see my target
                {
                    headScript.SetTargetAngle(VectorToAngle(directionToPlayer));
                }
                else // going to PLKP, because no one see my target
                {
                    headScript.SetTargetAngle(VectorToAngle((PLKPs[targetedPlayerID].transform.position - transform.position).normalized));
                }
            }
            else {
                if (headScript != null)
                    headScript.SetTargetAngle(VectorToAngle(transform.up));
                else
                    Debug.Log("Failure HeadScript not set");
            }
        }
    }

    protected void LookAround() // look around then reached destination and after finished looking find next destination
    {
        isLooking = true;
        if (headScript.isRotated) // if finished rotating to the angle
        {
            if (localSearchLookedAround == localSearchLookAroundTimes + 1) // finished looking
            {
                localSearchLookedAround = 0;
                isLooking = false;
                if(localSearchLocationsTried == 0 && otherID != -1) // went to PLKP and others can see other player
                {
                    // change target if anyone can see anyone
                    PursuePlayer(otherID);
                }
                else
                {
                    PerformLocalSearch();
                }
                return;
            }
            float randomRotation = 0;
            if (localSearchLookedAround == localSearchLookAroundTimes) // lastly ortate to front
            {
                randomRotation = VectorToAngle(transform.up);
            }
            else
            {
                randomRotation = Random.Range(fromAngleToTurnHead, toAngleToTurnHead);
                randomRotation = (localSearchLookedAround % 2 == 0 ? 1 : -1) * randomRotation + VectorToAngle(transform.up);
            }
            headScript.SetTargetAngle(randomRotation);
            localSearchLookedAround++;
        }
    }

    protected void PerformLocalSearch()
    {
        if (localSearchLocationsTried == 0) // just went to last player known location begining local search
        {
            searchAreaCenter = transform.position;
            aiDestinationSetter.enabled = false;
        }
        else if (localSearchLocationsTried == localSearchTryLocation) // tried all loations return to patroling
        {
            localSearchLocationsTried = 0;
            ReturnToPatrol();
            return;
        }
        // continue searching
        Vector2 randomLocation = new Vector2(
            Random.Range(localSearchRadius / 2, localSearchRadius) * (Random.Range(0, 2) < 1 ? 1 : -1),
            Random.Range(localSearchRadius / 2, localSearchRadius) * (Random.Range(0, 2) < 1 ? 1 : -1));
        randomLocation += searchAreaCenter;
        localSearchLocationsTried++;
        ai.destination = randomLocation;
        ai.SearchPath();

        //if(ai.remainingDistance > 2 * localSearchRadius) // maybe make this into do while loop
        //search for new point beacause this map point is too far for local search
    }
    
    protected void PlayerEnterVision(int playerID)
    {
        playerAlertings[playerID].howManySeeMe++;
    }

    protected void PlayerExitVision(int playerID)
    {
        playerAlertings[playerID].howManySeeMe--;
        PLKPs[playerID].position = players[playerID].transform.position;
    }

    // FOR CALCULATIONS:

    // these might be placed in more general location
    protected float VectorToAngle(Vector2 vect)
    {
        return Mathf.Atan2(vect.y, vect.x) * Mathf.Rad2Deg - 90;
    }
    
    public void DamageAlerting(float amount, int PlayerID, Vector3 position)
    {
        if(PlayerID != -1 && isServer)
        {
            PLKPs[PlayerID].position = position;
            PursuePlayer(PlayerID);
        }
        Damage(amount);
    }
    // OVERRIDES:
    public override void Damage(float amount)
    {
        if (isServer)
        {
            base.Damage(amount);
            HpBar.RpcSetHealth(GetHealth());
        }
    }

    protected override void Die()
    {
        GameObject.Find("LevelManager").GetComponent<LevelManager>().EnemyDefeated(gameObject);
    }
    
    protected void SetAlertionIndicator()
    {
        if (isAlerted)
        {
            ai.maxSpeed = alertedSpeed;
            if (targetInVision)
            {
                alertionIndicatorAnimator.SetInteger("Alertion", 2);
            }
            else
            {
                alertionIndicatorAnimator.SetInteger("Alertion", 1);
            }
        }
        else
        {
            ai.maxSpeed = normalSpeed;
            alertionIndicatorAnimator.SetInteger("Alertion", 0);
        }
    }
}
