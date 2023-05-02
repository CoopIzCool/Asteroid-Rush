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
    protected int damage;
    [SerializeField]
    protected int attackRange;
    [SerializeField]
    private bool isPlayer;

    [Header("Movement Components:")]
    [SerializeField] private Tile currentTile;
    private Vector3 tileOffset = new Vector3(0, 0.5f, 0);
    [SerializeField]private bool moved;

    // variables for animating movement
    private int nextTile;
    private float moveSpeed;
    private List<Tile> currentPath;
    #endregion

    #region Properties
    public int MaxHealth
    {
        get { return maxHealth; }
    }

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

    public bool Animating { get { return currentPath != null; } }
    public int Range
    {
        get { return attackRange; }
    }

    public bool IsPlayer
    {
        get { return isPlayer; }
    }
    #endregion
    // Start is called before the first frame update
    protected virtual void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        // animate movement
        if(currentPath != null) {
            Tile target = currentPath[nextTile];
            Vector3 direction = target.transform.position - transform.position;
            direction.y = 0;
            direction.Normalize();

            transform.position += direction * moveSpeed * Time.deltaTime;

            Vector3 newDir = target.transform.position - transform.position;
            newDir.y = 0;

            if(Vector3.Dot(direction, newDir) <= 0) { // passed the target
                transform.position = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z); // snap to position
                nextTile++;

                if(nextTile > currentPath.Count - 1) {
                    // end travel
                    MoveToTile(currentPath[nextTile - 1]);
                    currentPath = null;
                }
            }
        }
    }

    public virtual void Attack(Character opponent)
    {
        opponent.TakeDamage(damage);
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

    // used to set up a path for the character to move along
    public void SetPath(List<Tile> path, float moveSpeed) {
        currentPath = path;
        nextTile = 1; // 0 is the starting tile
        this.moveSpeed = moveSpeed;
    }

    public bool Alive()
    {
        return health > 0;
    }
    #region Damage and Death
    public void TakeDamage(int damage)
    {
        health -= damage;

        int amtDamage = int.Parse(DataTracking.GetData(2)) + damage;
		DataTracking.SetData(2, amtDamage.ToString());
        if(gameObject.tag == "Character") HealthUI.UpdateHealthBar(gameObject, damage);
        Debug.Log(gameObject.name + " has taken " + damage + " damage. They now have " + health + " health");
        if(health <= 0)
        {
            health = 0;
            Death();
        }
    }

    protected virtual void Death()
    {
        Destroy(gameObject);
    }
    #endregion
}
