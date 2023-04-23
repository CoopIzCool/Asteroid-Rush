using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    #region Fields
    [Header("Base Character Components:")]
    private int health;
    [SerializeField]
    private int maxHealth;
    [SerializeField]
    private int movementPoints;
    [SerializeField]
    private int damage;

    [Header("Movement Components:")]
    [SerializeField] private Tile currentTile;
    private Vector3 tileOffset = new Vector3(0, 0.5f, 0);
    [SerializeField]private bool moved;
    #endregion

    #region Properties
    public int Movement
    {
        get { return movementPoints; }
    }    

    public Tile CurrentTile
    {
        get { return currentTile; }
        set { currentTile = value; }
    }

    public bool Moved
    {
        get { return moved; }
        set { moved = value; }
    }
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void Attack(Character opponent)
    {

    }

    public virtual void SpecialAction()
    {

    }

    public void MoveToTile(Tile tile)
    {
        transform.position = tile.transform.position;
        transform.position += tileOffset;
        if(currentTile != null) {
            currentTile.occupant = null;
        }
        tile.occupant = gameObject;
        currentTile = tile;
    }


    #region Damage and Death
    public void TakeDamage(int damage)
    {
        health -= damage;
        if(health <= 0)
        {
            health = 0;
            Death();
        }
    }

    protected virtual void Death()
    {

    }
    #endregion
}
