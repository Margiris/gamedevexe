﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Character : MonoBehaviour
{
    public float healthMax; //{ get; protected set; }
    [SerializeField]
    public float healthCurrent { get; protected set; } 

    public abstract void Die(); // override

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
        Mathf.Min(amount + healthCurrent, healthMax);
    }

	public void SetMaxHealth()
	{
		healthCurrent = healthMax;
	}
}