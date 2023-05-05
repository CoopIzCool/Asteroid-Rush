using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alien : Character
{
    public int Damage { get { return damage; } set { damage = value; } }
    public GameObject CurrentTrap { get; set; }

	public GameObject HealthBar { get; set; }

	protected override void Start()
	{
		base.Start();
		RotateToward(TurnHandler.Instance.RocketObject.transform.position - transform.position);
	}

	public override void TakeDamage(int damage)
	{
		base.TakeDamage(damage);
		for (int i = 0; i < damage; i++)
		{
			// Interesting Fact: Destroy activates at the end of the frame.
			// If you do not include the "- i" here, Destroy() will just activate on the last child multiple times.
			// You could use DestroyImmediate() to achieve the same effect more cleanly, but Unity highly recommends against using it.
			if (HealthBar.transform.childCount > i) Destroy(HealthBar.transform.GetChild(HealthBar.transform.childCount - 1 - i).gameObject);
			else break;
		}
	}

	protected override void Death()
    {
        AlienManager.Instance.RemoveAlien(this);
        Destroy(HealthBar);
		if(CurrentTrap != null) {
			Destroy(CurrentTrap);
		}

        int numKills = int.Parse(DataTracking.GetData(3)) + 1;
        DataTracking.SetData(3, numKills.ToString());
    }
}
