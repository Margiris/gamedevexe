using UnityEngine;

public abstract class Character : MonoBehaviour
{
    public float healthMax; //{ get; protected set; }
    public float healthCurrent { get; protected set; }

    protected abstract void Die(); // override

    // Use this for initialization
    protected virtual void Initiate()
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
        if (!(healthCurrent <= 0)) return;
        healthCurrent = 0;
        Die();
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