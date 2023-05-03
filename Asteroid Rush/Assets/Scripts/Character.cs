using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField]
    private int miningPower;
    [SerializeField]
    private int oreCount = 0;
    [SerializeField]
    private bool boarded = false;

    [Header("Movement Components:")]
    [SerializeField] private Tile currentTile;
    [SerializeField] private float heightAboveTile;
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
        set { movementPoints = value; }
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

    public int MiningPower
    {
        get { return miningPower; }
        set { miningPower = value; }
    }

    public int OreCount
    {
        get { return oreCount; }
        set { oreCount = value; }
    }

    public bool Boarded
    {
        get { return boarded; }
        set { boarded = value; }
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

            RotateToward(direction);

            Vector3 lastRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(lastRotation.x, Mathf.Atan2(-direction.z, direction.x) / Mathf.PI * 180 - 90, lastRotation.z);

            transform.position += direction * moveSpeed * Time.deltaTime;

            Vector3 newDir = target.transform.position - transform.position;
            newDir.y = 0;

            if(Vector3.Dot(direction, newDir) <= 0) { // passed the target
                transform.position = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z); // snap to position
                nextTile++;

                if(nextTile > currentPath.Count - 1) {
                    // end travel
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
        transform.position += new Vector3(0, heightAboveTile, 0);
        ClaimTile(tile);
    }

    // sets this character as the occupant of the input tile
    private void ClaimTile(Tile tile)
    {
        if(currentTile != null) {
            currentTile.occupant = null;
        }
        tile.occupant = gameObject;
        currentTile = tile;
    }

    // ingores y coordinate
    public void RotateToward(Vector3 facingDirection) {
        Vector3 lastRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(lastRotation.x, Mathf.Atan2(-facingDirection.z, facingDirection.x) / Mathf.PI * 180 - 90, lastRotation.z);
    }

    // used to set up a path for the character to move along
    public void SetPath(List<Tile> path, float moveSpeed) {
        currentPath = path;
        nextTile = 1; // 0 is the starting tile
        this.moveSpeed = moveSpeed;
        ClaimTile(path[path.Count - 1]); // reserve the end tile even though this is not yet on it
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

        //POTION
        if (gameObject.GetComponent<Miner>())
        {
            UsePotion(ShopManager.minerItemsEquipped);
        }
        if(gameObject.GetComponent<Fighter>())
        {
            UsePotion(ShopManager.attackerItemsEquipped);
        }
        if (gameObject.GetComponent<Supporter>())
        {
            UsePotion(ShopManager.supporterItemsEquipped);
        }


        if (health <= 0)
        {
            health = 0;
            Death();
        }


    }

    private void UsePotion(EquipmentButton[] roleItemsEquipped)
    {
        //If potion is equipped, and there is at least one potion left in inventory
        if (roleItemsEquipped[0].isSelected && ShopManager.updatedShopItems[3, 1] >= 1)
        {
            //Restores some health upon reaching half health, and uses a potion
            if (health <= maxHealth / 2)
            {
                health += 3;
                ShopManager.updatedShopItems[3, 1] -= 1;
            }
        }
    }

    public void CollectOre()
    {
        oreCount++;
    }

    protected virtual void Death()
    {
        currentTile.occupant = null;
        gameObject.SetActive(false);
        SceneManager.LoadScene("Defeat");
    }

    public void MineOre(UnrefinedOre ore)
    {
        bool isOreBroken = ore.MineOre(miningPower);
        if(isOreBroken)
        {
            oreCount++;
        }
    }
    #endregion
}
