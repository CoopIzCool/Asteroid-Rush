using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;

public class Character : MonoBehaviour
{
    #region Fields
    [Header("Base Character Components:")]
    [SerializeField]
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
    private float startRotation; // some models are rotated differently by default

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
	public int Health
	{
		get { return health; }
	}

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

	// Text field for held ore
	public TMP_Text OreText { get; protected set; }
	#endregion
	// Start is called before the first frame update
	protected virtual void Start()
    {
        health = maxHealth;
        startRotation = transform.rotation.eulerAngles.y;
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
            transform.rotation = Quaternion.Euler(lastRotation.x, Mathf.Atan2(-direction.z, direction.x) / Mathf.PI * 180 - 90 + startRotation, lastRotation.z);

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

        //BOOSTER
        if (gameObject.GetComponent<Miner>())
        {
            UseBooster(ShopManager.minerItemsEquipped, opponent);
        }
        if (gameObject.GetComponent<Fighter>())
        {
            UseBooster(ShopManager.attackerItemsEquipped, opponent);
        }
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
        transform.rotation = Quaternion.Euler(lastRotation.x, Mathf.Atan2(-facingDirection.z, facingDirection.x) / Mathf.PI * 180 - 90 + startRotation, lastRotation.z);
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
	public virtual void TakeDamage(int damage)
    {
        health -= damage;

        if (gameObject.tag == "Character")
        {
            HealthUI.UpdateHealthBar(gameObject, damage);
			int amtDamage = int.Parse(DataTracking.GetData(2)) + damage;
			DataTracking.SetData(2, amtDamage.ToString());
		}
        

        //POTION & GUARD
        if (gameObject.GetComponent<Miner>())
        {
            UseGuard(ShopManager.minerItemsEquipped, damage);
            UsePotion(ShopManager.minerItemsEquipped);
        }
        if(gameObject.GetComponent<Fighter>())
        {
            UseGuard(ShopManager.attackerItemsEquipped, damage);
            UsePotion(ShopManager.attackerItemsEquipped);
        }
        if (gameObject.GetComponent<Supporter>())
        {
            UseGuard(ShopManager.supporterItemsEquipped, damage);
            UsePotion(ShopManager.supporterItemsEquipped);
        }

        Debug.Log(gameObject.name + " has taken " + damage + " damage. They now have " + health + " health");

        if (health <= 0)
        {
            //health = 0;
            //Death();

            //REVIVE
            if (gameObject.GetComponent<Miner>())
            {
                UseRevive(ShopManager.minerItemsEquipped);
            }
            if (gameObject.GetComponent<Fighter>())
            {
                UseRevive(ShopManager.attackerItemsEquipped);
            }
            if (gameObject.GetComponent<Supporter>())
            {
                UseRevive(ShopManager.supporterItemsEquipped);
            }

            if (gameObject.GetComponent<Alien>())
            {
                health = 0;
                Death();
            }
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
                HealthUI.UpdateHealthBar(gameObject, -3);
                health += 3;
                ShopManager.updatedShopItems[3, 1] -= 1;
            }
        }
    }

    private void UseRevive(EquipmentButton[] roleItemsEquipped)
    {
        //If revive is equipped, and there is at least one revive left in inventory
        if (roleItemsEquipped[1].isSelected && ShopManager.updatedShopItems[3, 2] >= 1)
        {
			//Restores some health upon dying, and uses a revive
			health += 5;
			HealthUI.UpdateHealthBar(gameObject, -5);
            ShopManager.updatedShopItems[3, 2] -= 1;
        }

        else
        {
            health = 0;
            Death();
        }
    }

    private void UseBooster(EquipmentButton[] roleItemsEquipped, Character opponent)
    {
        //If booster is equipped, and there is at least one booster left in inventory
        if (roleItemsEquipped[2].isSelected && ShopManager.updatedShopItems[3, 3] >= 1)
        {
            //Attack again
            opponent.TakeDamage(damage);
            ShopManager.updatedShopItems[3, 3] -= 1;
        }
    }

    private void UseGuard(EquipmentButton[] roleItemsEquipped, int damage)
    {
        //If guard is equipped, and there is at least one guard left in inventory
        if (roleItemsEquipped[3].isSelected && ShopManager.updatedShopItems[3, 4] >= 1)
        {
            //Prevents damage, then consumes a guard
            health += damage;
            ShopManager.updatedShopItems[3, 4] -= 1;
        }
    }

    public void CollectOre()
    {
        oreCount++;
		OreText.text = "Ore Held: " + oreCount.ToString();
	}

    protected virtual void Death()
    {
        currentTile.occupant = null;
		gameObject.SetActive(false);
		DataTracking.SaveData();
        Debug.Log("DEATH");
		SceneManager.LoadScene("Defeat");
    }

    public void MineOre(UnrefinedOre ore)
    {
        bool isOreBroken = ore.MineOre(miningPower);
        if(isOreBroken)
        {
            oreCount++;
            OreText.text = "Ore Held: " + oreCount.ToString();
        }
    }
    #endregion
}
