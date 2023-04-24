using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Miner : Character
{
    #region Fields
    [Header("Miner Components:")]
    [SerializeField]
    private float mineSpeed;
    #endregion
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void SpecialAction()
    {
        base.SpecialAction();
    }
}
