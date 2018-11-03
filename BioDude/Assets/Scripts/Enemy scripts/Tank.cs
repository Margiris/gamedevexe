using Environment;
using GUI_scripts;
using Pathfinding;
using Player_scripts;
using UnityEngine;

namespace Enemy_scripts
{
    public abstract class Tank : Character
    {
        //public variables:
        //Tank settings
        public float visionAngle = 60; // vision angle (half of it)
        public float visionRange = 10;
        public float localSearchRadius = 5; // distance - how far to search for player from last known position
        public int localSearchTryLocation = 3; // how many locations to try after target lost
        public int localSearchLookAroundTimes = 2;
        public int fromAngleToTurnHead = 15;
        public int toAngleToTurnHead = 90; // max value should be 90 for realistic reaction
        public LayerMask obstacleMask;

        protected Transform head;
        private GameObject player;
        public float widthOfFirePathChecker = 0.1f;

        public bool isAlerted;

        //private variables:
        private float normalSpeed;
        private float alertedSpeed;
        protected IAstarAI ai;
        protected bool targetInVision;
        protected float distanceToPlayer;
        private AIDestinationSetter aiDestinationSetter;
        private Patrol aiPatrol;
        private bool prevTargetInVision;
        private int localSearchLocationsTried;
        private int localSearchLookedAround;
        private bool isTargetDestinationPlayer;
        private Vector2 searchAreaCenter;
        private Alerting playerAlerting;
        protected Head headScript;
        private bool isLooking;
        protected Vector2 directionToPlayer;
        private Transform PLKP; //PlayerLastKnownPosition
        private EnemyHPBar HpBar;

        protected Animator animator;

        private Animator alertIndicatorAnimator;
        private SpriteRenderer alertIndicatorSpriteRenderer;
        private Sprite targetInVisionSprite;
        private Sprite isAlertedSprite;

        // Use this for initialization
        public void Instantiate()
        {
            PLKP = GameObject.Find("PlayerLastKnownPosition").transform;
            player = GameObject.Find("player");
            head = transform.Find("body");
            headScript = head.GetComponent<Head>();
            animator = GetComponent<Animator>();

            alertIndicatorAnimator = transform.Find("EnemyCanvas/AlertionIndicator").GetComponent<Animator>();
            alertIndicatorSpriteRenderer =
                transform.Find("EnemyCanvas/AlertionIndicator").GetComponent<SpriteRenderer>();
            targetInVisionSprite = Resources.Load<Sprite>("e");
            isAlertedSprite = Resources.Load<Sprite>("q");

            aiDestinationSetter = GetComponent<AIDestinationSetter>();
            aiPatrol = GetComponent<Patrol>();
            ai = GetComponent<IAstarAI>();
            playerAlerting = player.GetComponent<Alerting>();
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
            if (playerAlerting.howManySeeMe > 0) // if someone can see player right now
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
                    if (!Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask)
                    ) //if no obstacles in between
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
                    headScript.SetTargetAngle(targetInVision
                        ? VectorToAngle(directionToPlayer)
                        : VectorToAngle((PLKP.transform.position - transform.position).normalized));
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

        protected void BehaviourIfCantSeePlayer()
        {
            if (!isAlerted || targetInVision) return;

            if (isTargetDestinationPlayer && playerAlerting.howManySeeMe == 0
            ) // if someone saw player till now and this enemy was pursuing player
            {
                PursuePlayer();
            }
            else if (!isTargetDestinationPlayer && playerAlerting.howManySeeMe > 0
            ) // if someone can see player from now on this enemy should pursue him
            {
                PursuePlayer();
            }

            if (ai.reachedEndOfPath && !ai.pathPending) // went to last known location and player not seen
            {
                LookAround();
            }
        }

        private void
            LookAround() // look around then reached destination and after finished looking find next destination
        {
            isLooking = true;
            if (!headScript.isRotated) return;

            if (localSearchLookedAround == localSearchLookAroundTimes + 1) // finished looking
            {
                localSearchLookedAround = 0;
                isLooking = false;
                PerformLocalSearch();
                return;
            }

            float randomRotation;
            if (localSearchLookedAround == localSearchLookAroundTimes)
            {
                randomRotation = VectorToAngle(transform.up);
            }
            else
            {
                randomRotation = Random.Range(fromAngleToTurnHead, toAngleToTurnHead);
                randomRotation = (localSearchLookedAround % 2 == 0 ? 1 : -1) * randomRotation +
                                 VectorToAngle(transform.up);
            }

            headScript.SetTargetAngle(randomRotation);
            localSearchLookedAround++;
        }

        private void PerformLocalSearch()
        {
            if (localSearchLocationsTried == 0) // just went to last player known location beginning local search
            {
                searchAreaCenter = transform.position;
                aiDestinationSetter.enabled = false;
            }
            else if (localSearchLocationsTried == localSearchTryLocation) // tried all locations return to patrolling
            {
                localSearchLocationsTried = 0;
                ReturnToPatrol();
                return;
            }

            // continue searching
            var randomLocation = new Vector2(
                Random.Range(localSearchRadius / 2, localSearchRadius) * (Random.Range(0, 2) < 1 ? 1 : -1),
                Random.Range(localSearchRadius / 2, localSearchRadius) * (Random.Range(0, 2) < 1 ? 1 : -1));
            randomLocation += searchAreaCenter;
            localSearchLocationsTried++;
            ai.destination = randomLocation;
            ai.SearchPath();

            //if(ai.remainingDistance > 2 * localSearchRadius) // maybe make this into do while loop
            //search for new point because this map point is too far for local search
        }

        private void VisionStatusChange()
        {
            if (isAlerted)
            {
                if (targetInVision) // can see on its own now
                {
                    playerAlerting.howManySeeMe++;
                    PursuePlayer();
                }
                else // can't see anymore
                {
                    playerAlerting.howManySeeMe--;
                    CantSeePlayerAnyMore();
                }
            }
            else
            {
                if (targetInVision)
                {
                    playerAlerting.howManySeeMe++;
                    PursuePlayer();
                }
                else //if someone will force to not alerted then enemy can see player
                {
                    playerAlerting.howManySeeMe--;
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
        private static float VectorToAngle(Vector2 vector)
        {
            return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg - 90;
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
                    alertIndicatorSpriteRenderer.sprite = targetInVisionSprite;
                    alertIndicatorAnimator.SetFloat("Speed", 2);
                }
                else
                {
                    alertIndicatorSpriteRenderer.sprite = isAlertedSprite;
                    alertIndicatorAnimator.SetFloat("Speed", 1);
                }
            }
            else
            {
                ai.maxSpeed = normalSpeed;
                alertIndicatorSpriteRenderer.sprite = new Sprite();
                alertIndicatorAnimator.SetFloat("Speed", 0);
            }
        }
    }
}