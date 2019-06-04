using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

abstract public class Character : NetworkBehaviour
{
    public float healthMax; //{ get; protected set; }
    [SerializeField]
    public float healthCurrent { get; protected set; } 

    protected abstract void Die(); // override

    // Use this for initialization
    protected virtual void Initiate ()
	{
		SetMaxHealth(); //check if this even work if nested
	}

    public int GetHealth()
    {
        return Mathf.RoundToInt(healthCurrent);
    }

    public virtual void Damage(float amount)
	{
		healthCurrent -= amount;
        if (healthCurrent <= 0)
        {
            healthCurrent = 0;
            Die();
        }
    }

	public void Heal(float amount)
	{
		healthCurrent = Mathf.Min(amount + healthCurrent, healthMax);
    }

	public void SetMaxHealth()
	{
		healthCurrent = healthMax;
	}
}