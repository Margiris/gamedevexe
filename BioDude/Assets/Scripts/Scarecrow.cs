using GUI_scripts;

public class Scarecrow : Character
{
    private EnemyHPBar HpBar;

    protected override void Die()
    {
        Destroy(gameObject);
    }

    // Use this for initialization
    private void Start()
    {
        HpBar = transform.Find("EnemyCanvas").GetComponent<EnemyHPBar>();
        HpBar.Initiate();
        healthCurrent = healthMax;
    }

    public override void Damage(float amount)
    {
        base.Damage(amount);
        HpBar.SetHealth(GetHealth());
    }
}