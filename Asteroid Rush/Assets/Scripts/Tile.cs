using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType { 
    Basic,
    Pit,
    Border
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

    public void SetAvailabillitySelector(bool status)
    {
        illumination.SetActive(status);
    }
}
