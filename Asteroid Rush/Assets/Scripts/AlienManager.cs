using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// handles the spawn of aliens
public class AlienManager : MonoBehaviour
{
    [SerializeField] private GameObject alienPrefab;
    [SerializeField] private GameObject spawnZoneContainer;
    [SerializeField] private GameObject trapPrefab;
    [SerializeField] private GameObject slowZonePrefab;
    private const int NUM_PER_SPAWN = 2; // should be less than the number of spawn tiles

    private static AlienManager instance;
    public static AlienManager Instance { get { return instance; } }

    public GameObject[] PlayerCharacters { get; set; } // set by grid generator
    public GenerateLevel Grid { get; set; } // set by grid generator

    private List<GameObject> slowZones;
    private List<GameObject> traps;
    private List<GameObject> activeAliens;
    private List<GameObject> spawnedAliens;
    private int turnsBeforeSpawn;

    // variables for animating a turn
    int currentAlien; // index of the alien currently moving
    const float MOVE_SPEED = 8.0f;

    // use awake because this is referenced by GenerateLevel.cs Start()
    void Awake()
    {
        instance = this;
        activeAliens = new List<GameObject>();
        slowZones = new List<GameObject>();
        traps = new List<GameObject>();
        turnsBeforeSpawn = 3;
    }

   // called every frame during the aliens' turn
   public void UpdateTurn() {
        if(!activeAliens[currentAlien].GetComponent<Character>().Animating) {
            // attack at the end of moving if next to a player
            GameObject adjacentPlayer = FindAdjacentPlayer(activeAliens[currentAlien]);
            if(adjacentPlayer != null) {
                adjacentPlayer.GetComponent<Character>().TakeDamage(activeAliens[currentAlien].GetComponent<Alien>().Damage);
            }

            currentAlien++;

            if(currentAlien > activeAliens.Count - 1) {
                if(spawnedAliens != null) {
                    activeAliens.AddRange(spawnedAliens);
                    spawnedAliens = null;
                }
                TurnHandler.Instance.EndAlienTurn();
            } else {
                AssignAlienMovement(activeAliens[currentAlien].GetComponent<Alien>());
            }
        }
   }

    // operates all of the aliens and handles spawning new ones
    public void TakeTurn() {
        // manage traps and slow zones
        for(int i = 0; i < slowZones.Count; i++) {
            slowZones[i].GetComponent<SlowZone>().TurnsLeft--;
            if(slowZones[i].GetComponent<SlowZone>().TurnsLeft < 0) {
                Destroy(slowZones[i]);
                slowZones.RemoveAt(i);
                i--;
            }
        }

        // spawn new aliens
        turnsBeforeSpawn--;
        if(turnsBeforeSpawn <= 0) {
            turnsBeforeSpawn = 5;

            // choose a spawn side
            Transform spawnZones = spawnZoneContainer.gameObject.transform;
            int chosenSide = Random.Range(0, spawnZones.childCount - 1);
            Transform spawnZone = spawnZones.GetChild(chosenSide);
            spawnedAliens = new List<GameObject>();
            for(int i = 0; i < NUM_PER_SPAWN; i++) {
                Transform tileSpot = spawnZone.GetChild(i);

                GameObject newAlien = Instantiate(alienPrefab);
                spawnedAliens.Add(newAlien);
                newAlien.GetComponent<Alien>().MoveToTile(tileSpot.gameObject.GetComponent<Tile>());
            }
        }

        // start animating aliens
        if(activeAliens.Count <= 0) {
            if(spawnedAliens != null) {
                activeAliens.AddRange(spawnedAliens);
                spawnedAliens = null;
            }
            TurnHandler.Instance.EndAlienTurn();
        } else {
            currentAlien = 0;
            AssignAlienMovement(activeAliens[currentAlien].GetComponent<Alien>());
        }
    }

    // helper function to find the closest player and create a path to it
    private void AssignAlienMovement(Alien alien) {
        // find the closest player character
        GameObject closestPlayer = PlayerCharacters[0];
        float closestDistance = Vector3.Distance(closestPlayer.transform.position, alien.transform.position);
        for(int i = 1; i < PlayerCharacters.Length; i++) {
            // check for an open tile next to the character because otherwise there will be no path
            List<Vector2Int> testDirections = new List<Vector2Int>() {
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, -1),
            };
            Tile playerTile = PlayerCharacters[i].GetComponent<Character>().CurrentTile;
            bool openSpot = false;
            foreach(Vector2Int testDirection in testDirections) {
                GameObject tileObject = GenerateLevel.GetGridItem(playerTile.zPos + testDirection.y, playerTile.xPos + testDirection.x);
                if(tileObject != null && tileObject.GetComponent<Tile>().IsAvailableTile()) {
                    openSpot = true;
                    break;
                }
            }
            if(!openSpot) {
                continue;
            }

            // check if this player is closer
            float distance = Vector3.Distance(PlayerCharacters[i].transform.position, alien.transform.position);
            if(distance < closestDistance) {
                closestDistance = distance;
                closestPlayer = PlayerCharacters[i];
            }
        }

        List<Tile> path = GenerateLevel.FindPath(alien.GetComponent<Character>().CurrentTile, closestPlayer.GetComponent<Character>().CurrentTile);
        if(path != null) {
            path.RemoveAt(path.Count - 1); // remove last element because it is the tile the player is standing on
            alien.SetPath(path, MOVE_SPEED);
        }
    }

    public void RemoveAlien(Alien alienScript) {
        activeAliens.Remove(alienScript.gameObject);
        Destroy(alienScript.gameObject);
    }

    // helper function to find if a player is in an adjacent tile. If there are multiple, returns the first one found. Null if no adjacent
    private GameObject FindAdjacentPlayer(GameObject alien) {
        Tile alienTile = alien.GetComponent<Alien>().CurrentTile;

        foreach (GameObject player in PlayerCharacters) {
            if(player.GetComponent<Character>().Alive())
            {
                Tile playerTile = player.GetComponent<Character>().CurrentTile;
                if (player.GetComponent<Character>().CurrentTile.IsAdjacent(alienTile))
                {
                    return player;
                }
            }
        }

        return null;
    }

    // fighter's trap ability
    public void AddTrap(Tile placement) {
        GameObject trap = Instantiate(trapPrefab);
        traps.Add(trap);
        trap.transform.position = placement.transform.position;
        trap.GetComponent<Trap>().Tile = placement;
    }

    // supporter's slow zone ability
    public void AddSlowZone(Tile placement) {
        GameObject slowZone = Instantiate(slowZonePrefab);
        traps.Add(slowZone);
        slowZone.transform.position = placement.transform.position;
        slowZone.GetComponent<SlowZone>().Tile = placement;
    }
}
