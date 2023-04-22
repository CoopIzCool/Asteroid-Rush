using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowZone : MonoBehaviour
{
    public Tile Tile { get; set; }
    public int TurnsLeft { get; set; }

    void Start()
    {
        TurnsLeft = 3;
    }
}
