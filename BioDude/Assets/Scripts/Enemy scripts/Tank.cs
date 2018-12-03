using UnityEngine;
using Pathfinding;

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
    protected GameObject player;
    public float widthOfFirePathChecker = 0.1f;

    public bool isAlerted = false;

    //private variables:
    protected float normalSpeed;
    protected float alertedSpeed;
    protected IAstarAI ai;
    protected bool targetInVision = false;
    protected float distanceToPlayer;
    protected bool targetInAttackRange = false;
    protected AIDestinationSetter aiDestinationSetter;
    protected Patrol aiPatrol;
    protected bool prevTargetInVision = false;
    protected int localSearchLocationsTried = 0;
    protected int localSearchLookedAround = 0;
    protected bool isTargetDestinationPlayer = false;
    protected Vector2 searchAreaCenter;
    protected Allerting playerAllerting;
    protected Head headScript;
    protected bool isLooking = false;
    protected Vector2 directionToPlayer;
    protected Transform PLKP; //PlayerLastKnownPosition
    protected EnemyHPBar HpBar;

    protected Animator animator;

    protected Animator alertionIndicatorAnimator;
    protected SpriteRenderer alertionIndicatorSpriteRenderer;
    protected Sprite targetInVisionSprite;
    protected Sprite isAlertedSprite;

    // Use this for initialization
    public void Instantiate () {
        PLKP = GameObject.Find("PlayerLastKnownPosition").transform;
        player = GameObject.Find("player");
        head = transform.Find("body");
        headScript = head.GetComponent<Head>();
        animator = GetComponent<Animator>();
        
        alertionIndicatorAnimator = transform.Find("EnemyCanvas/AlertionIndicator").GetComponent<Animator>();
        alertionIndicatorSpriteRenderer = transform.Find("EnemyCanvas/AlertionIndicator").GetComponent<SpriteRenderer>();
        targetInVisionSprite = Resources.Load<Sprite>("e");
        isAlertedSprite = Resources.Load<Sprite>("q");
        
        aiDestinationSetter = GetComponent<AIDestinationSetter>();
        aiPatrol = GetComponent<Patrol>();
        ai = GetComponent<IAstarAI>();
        playerAllerting = player.GetComponent<Allerting>();
        HpBar = transform.Find("EnemyCanvas").GetComponent<EnemyHPBar>();
        HpBar.Initiate();
        healthCurrent = healthMax;
        normalSpeed = ai.maxSpeed;
        alertedSpeed = normalSpeed;
    }

    //PUBLIC METHODS:

    //Stop pursuing player
    public virtual void ReturnToPatrol()
    {
        aiDestinationSetter.enabled = false;
        aiPatrol.enabled = true;
        isLooking = false;
        isAlerted = false;
    }

    //call this method to make enemy go to last known player position
    public void PursuePlayer()
    {
        isAlerted = true;
        localSearchLocationsTried = 0;
        localSearchLookedAround = 0;
        aiPatrol.enabled = false;
        aiDestinationSetter.enabled = true;
        isLooking = false;
        if (playerAllerting.howManySeeMe > 0) // if someone can see player right now
        {
            isTargetDestinationPlayer = true;
            aiDestinationSetter.target = player.transform;
            ai.canSearch = true;
        }
        else
        {
            isTargetDestinationPlayer = false;
            aiDestinationSetter.target = PLKP;
            ai.canSearch = false;
            ai.SearchPath();
        }
    }

    //PRIVATE METHODS:

    protected void CheckVision()
    {
        directionToPlayer = (player.transform.position - transform.position).normalized;

        distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
        targetInVision = false;
        if (distanceToPlayer < visionRange) // if in range
        {
            if (Vector3.Angle(head.transform.up, directionToPlayer) < visionAngle) // if in vision angle
            {
                if (!Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask)) //if no obstacles in between
                {
                    targetInVision = true;
                    //lastPositionTargetSeen.position = player.transform.position; // Needs to be updated because other tanks might not see player and would attempt to go to last known location
                }
            }
        }

        //Set head target direction:
        if (!isLooking)
        {
            if (isAlerted)
            {
                if(targetInVision)
                    headScript.SetTargetAngle(VectorToAngle(directionToPlayer));
                else
                    headScript.SetTargetAngle(VectorToAngle((PLKP.transform.position - transform.position).normalized));
            }
            else
                headScript.SetTargetAngle(VectorToAngle(transform.up));
            //////////////////////////// or
            /*
            if (targetInVision)
                headScript.SetTargetAngle(VectorToAngle(directionToPlayer));
            else
                headScript.SetTargetAngle(VectorToAngle(transform.up)); // this should always set head rotation to zero. better solution required
            */
        }

        if (prevTargetInVision != targetInVision)
            VisionStatusChange();
        prevTargetInVision = targetInVision;
    }

    protected void BechaviourIfCantSeePlayer()
    {
        if (isAlerted && !targetInVision) // pursuing to last known location(local search) or player if someone can see him
        {
            if (isTargetDestinationPlayer && playerAllerting.howManySeeMe == 0) // if someone saw player till now and this enemy was pursuing player
            {
                PursuePlayer();
            }
            else if (!isTargetDestinationPlayer && playerAllerting.howManySeeMe > 0) // if someone can see player from now on this enemy should purue him
            {
                PursuePlayer();
            }

            if (ai.reachedEndOfPath && !ai.pathPending) // went to last known location and player not seen
            {
                LookAround();
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
                PerformLocalSearch();
                return;
            }
            float randomRotation = 0;
            if (localSearchLookedAround == localSearchLookAroundTimes)
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

    protected void VisionStatusChange()
    {
        if (isAlerted)
        {
            if (targetInVision) // can see on its own now
            {
                playerAllerting.howManySeeMe++;
                PursuePlayer();
            }
            else // can't see anymore
            {
                playerAllerting.howManySeeMe--;
                CantSeePlayerAnyMore();
            }
        }
        else
        {
            if (targetInVision)
            {
                playerAllerting.howManySeeMe++;
                PursuePlayer();
            }
            else //if someone will force to not allerted then enemy can see player
            {
                playerAllerting.howManySeeMe--;
                //ai.canMove = false;
            }
        }

    }

    protected virtual void CantSeePlayerAnyMore()
    {
        PLKP.position = player.transform.position;
        PursuePlayer();
    }

    // FOR CALCULATIONS:

    // these might be placed in more general location
    protected float VectorToAngle(Vector2 vect)
    {
        return Mathf.Atan2(vect.y, vect.x) * Mathf.Rad2Deg - 90;
    }

    // OVERRIDES:
    public override void Damage(float amount)
    {
        PursuePlayer();
        base.Damage(amount);
        HpBar.SetHealth(GetHealth());
    }

    protected override void Die()
    {
        GameObject.Find("LevelManager").GetComponent<LevelManager>().EnemyDefeated();
    }
    
    protected void SetAlertionIndicator()
    {
        if (isAlerted)
        {
            ai.maxSpeed = alertedSpeed;
            if (targetInVision)
            {
                alertionIndicatorSpriteRenderer.sprite = targetInVisionSprite;
                alertionIndicatorAnimator.SetFloat("Speed", 2);
            }
            else
            {
                alertionIndicatorSpriteRenderer.sprite = isAlertedSprite;
                alertionIndicatorAnimator.SetFloat("Speed", 1);
            }
        }
        else
        {
            ai.maxSpeed = normalSpeed;
//            alertionIndicatorSpriteRenderer.sprite = new Sprite();
            alertionIndicatorAnimator.SetFloat("Speed", 0);
        }
    }
}
