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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
