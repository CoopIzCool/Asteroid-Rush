using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    public Tile Tile { get; set; }
    public int TurnsLeft { get; set; }

    void Start()
    {
        TurnsLeft = 2;
    }
}
