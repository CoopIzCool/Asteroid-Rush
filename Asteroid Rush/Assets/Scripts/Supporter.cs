using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// player character with support abilities
public class Supporter : Character
{
	protected override void Start()
	{
		base.Start();

		OreText = GameObject.Find("SupporterOre").GetComponent<TMP_Text>();
	}

	public override void SpecialAction()
    {
        AlienManager.Instance.AddSlowZone(CurrentTile);
    }
}
