using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType { 
    Basic,
    Pit,
    Border,
    EnemySpawn
}

public class Tile : MonoBehaviour
{
    // The object currently occupying this tile
    public GameObject occupant = null;
    public TileType tileType;
    public int xPos;
    public int zPos;
    [SerializeField]
    private GameObject illumination;
    public bool rocketAccessible = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsAvailableTile()
    {
        if(occupant == null)
        {
            if (tileType == TileType.Basic)
                return true;
        }
        return false;
    }

    public bool IsAttackableTile()
    {
        if(occupant == null)
        {
            if (tileType == TileType.Basic || tileType == TileType.Pit)
                return true;
        }
        return false;
    }

    //Check if the tile has a character on it and whether or not the entity trying to attack is on the same side
    public bool IsAttackable(Character attackingCharacter)
    {
        if(occupant != null)
        {
            if(occupant.GetComponent<Character>())
            {
                if(occupant.GetComponent<Character>().IsPlayer !=attackingCharacter.IsPlayer && attackingCharacter.Range > 0)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool IsMineable()
    {
        if (occupant != null)
        {
            if (occupant.GetComponent<UnrefinedOre>())
            {
                Debug.Log("This is an ore");
                return true;
            }
        }
        return false;
    }

    public void SetAvailabillitySelector(bool status)
    {
        illumination.SetActive(status);
    }

    public bool IsAdjacent(Tile other) {
        return Mathf.Abs(xPos - other.xPos) + Mathf.Abs(zPos - other.zPos) <= 1;
    }
}
