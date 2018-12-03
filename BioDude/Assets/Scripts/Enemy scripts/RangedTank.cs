using UnityEngine;

// NOTE: Children order is important fro this script to work

public class RangedTank : Tank
{
    //private:
    private Firearm firearm;

	// Use this for initialization
	void Start () {
        Instantiate();
        //head = transform.GetChild(1);
        firearm = transform.GetComponentInChildren<Firearm>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        CheckVision();
        SetAlertionIndicator();

        if(targetInVision) // if enemy can see target
        {
            if(headScript.isRotated)
            {
                //check if target can be fired by bullet and only then here disallow moving
                Debug.DrawLine(transform.position + head.transform.right * widthOfFirePathChecker, (Vector2) transform.position + (Vector2)head.transform.right * widthOfFirePathChecker + directionToPlayer * distanceToPlayer, Color.blue);
                Debug.DrawLine(transform.position - head.transform.right * widthOfFirePathChecker, (Vector2)transform.position - (Vector2)head.transform.right * widthOfFirePathChecker + directionToPlayer * distanceToPlayer, Color.blue);
                if (!Physics2D.Raycast(transform.position + head.transform.right * widthOfFirePathChecker, directionToPlayer, distanceToPlayer, obstacleMask) &&
                   !Physics2D.Raycast(transform.position - head.transform.right * widthOfFirePathChecker, directionToPlayer, distanceToPlayer, obstacleMask))
                {
                    ai.canMove = false;
                    firearm.Shoot();
                }
                else
                {
                    ai.canMove = true;
                }
            }
        }
        else
        {
            BechaviourIfCantSeePlayer();
        }
    }

    public override void ReturnToPatrol()
    {
        base.ReturnToPatrol();
        ai.canMove = true;
    }

    protected override void CantSeePlayerAnyMore()
    {
        base.CantSeePlayerAnyMore();
        ai.canMove = true;
    }
    
    protected override void Die()
    {
        base.Die();
        // enemy death: smokes and stoped movement
        Destroy(gameObject);
    }
}
