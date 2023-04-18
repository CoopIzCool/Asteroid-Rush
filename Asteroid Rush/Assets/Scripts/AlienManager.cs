using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// handles the spawn of aliens
public class AlienManager : MonoBehaviour
{
    [SerializeField] private GameObject alienPrefab;
    [SerializeField] private GameObject spawnZoneContainer;
    private const int NUM_PER_SPAWN = 3; // should be less than the number of spawn tiles

    private static AlienManager instance;
    public static AlienManager Instance { get { return instance; } }

    public GameObject[] PlayerCharacters { get; set; } // set by grid generator
    public GenerateLevel Grid { get; set; } // set by grid generator
    private Dictionary<Direction, Vector2Int> directionToVector;

    private List<GameObject> activeAliens;
    private int turnsBeforeSpawn;

    // use awake because this is referenced by GenerateLevel.cs Start()
    void Awake()
    {
        instance = this;
        activeAliens = new List<GameObject>();
        turnsBeforeSpawn = 3;

        directionToVector = new Dictionary<Direction, Vector2Int>();
        directionToVector[Direction.Up] = new Vector2Int(0, 1);
        directionToVector[Direction.Down] = new Vector2Int(0, -1);
        directionToVector[Direction.Left] = new Vector2Int(-1, 0);
        directionToVector[Direction.Right] = new Vector2Int(0, 1);
    }

    // operates all of the aliens and handles spawning new ones
    public void TakeTurn() {
        // spawn new aliens
        turnsBeforeSpawn--;
        if(turnsBeforeSpawn <= 0) {
            turnsBeforeSpawn = 3;

            // choose a spawn side
            Transform spawnZones = spawnZoneContainer.gameObject.transform;
            int chosenSide = Random.Range(0, spawnZones.childCount - 1);
            Transform spawnZone = spawnZones.GetChild(chosenSide);
            for(int i = 0; i < NUM_PER_SPAWN; i++) {
                Transform tileSpot = spawnZone.GetChild(i);

                GameObject newAlien = Instantiate(alienPrefab);
                activeAliens.Add(newAlien);
                newAlien.GetComponent<Alien>().MoveToTile(tileSpot.gameObject.GetComponent<Tile>());
            }
        }

        // move all aliens
        foreach(GameObject alien in activeAliens) {
            // attack if next to a player
            GameObject adjacentPlayer = FindAdjacentPlayer(alien);
            if(adjacentPlayer != null) {
                adjacentPlayer.GetComponent<Character>().TakeDamage(alien.GetComponent<Alien>().Damage);
                continue; // end this alien's turn
            }

            // move if not attacking
            GameObject closestPlayer = PlayerCharacters[0];
            float closestDistance = Vector3.Distance(closestPlayer.transform.position, alien.transform.position);
            for(int i = 1; i < PlayerCharacters.Length; i++) {
                float distance = Vector3.Distance(PlayerCharacters[i].transform.position, alien.transform.position);
                if(distance < closestDistance) {
                    closestDistance = distance;
                    closestPlayer = PlayerCharacters[i];
                }
            }

            Vector3 tileTransform = alien.GetComponent<Alien>().CurrentTile.transform.position;
            Vector2Int currentTile = new Vector2Int((int)tileTransform.x, (int)tileTransform.z);
            Vector2Int targetTile = new Vector2Int((int)closestPlayer.transform.position.x, (int)closestPlayer.transform.position.x);

            // simulate stepping one tile at a time
            for(int i = 0; i < alien.GetComponent<Alien>().Movement; i++) {
                int dist = Mathf.Abs(currentTile.x - targetTile.x) + Mathf.Abs(currentTile.y - targetTile.y);
                if(dist <= 1) {
                    break; // now adjacent to player
                }

                // determine the best order to attempt a move
                Direction[] directionPriority = new Direction[4];
                bool leftBetterThanRight = targetTile.x < currentTile.x;
                bool upBetterThanDown = targetTile.y < currentTile.y;

                if(Mathf.Abs(currentTile.x - targetTile.x) > Mathf.Abs(currentTile.y - targetTile.y)) {
                    // horizontal first
                    directionPriority[0] = (leftBetterThanRight ? Direction.Left : Direction.Right);
                    directionPriority[1] = (upBetterThanDown ? Direction.Up : Direction.Down);
                    directionPriority[2] = (upBetterThanDown ? Direction.Down : Direction.Up);
                    directionPriority[3] = (leftBetterThanRight ? Direction.Right : Direction.Left);
                    // ERROR: fails if the character is forced to move backwards
                } else {
                    // vertical first
                    directionPriority[0] = (upBetterThanDown ? Direction.Up : Direction.Down);
                    directionPriority[1] = (leftBetterThanRight ? Direction.Left : Direction.Right);
                    directionPriority[2] = (leftBetterThanRight ? Direction.Right : Direction.Left);
                    directionPriority[3] = (upBetterThanDown ? Direction.Down : Direction.Up);
                }

                foreach(Direction direction in directionPriority) {
                    // check for an occupied tile in that direction
                    Vector2Int testTile = currentTile + directionToVector[direction];
                    GameObject tile = GenerateLevel.GetGridItem(testTile.x, testTile.y);
                    Debug.Log(tile);
                    if(tile != null) {
                        Debug.Log(tile.GetComponent<Tile>());
                    }
                    if(tile != null && tile.GetComponent<Tile>().occupant == null) {
                        currentTile = testTile;
                    }
                }
            }

            alien.GetComponent<Alien>().MoveToTile(GenerateLevel.GetGridItem(currentTile.x, currentTile.y).GetComponent<Tile>());
        }
    }

    public void RemoveAlien(Alien alienScript) {
        activeAliens.Remove(alienScript.gameObject);
        Destroy(alienScript.gameObject);
    }

    // helper function to find if a player is in an adjacent tile. If there are multiple, returns the first one found. Null if no adjacent
    private GameObject FindAdjacentPlayer(GameObject alien) {
        Vector3 tileTransform = alien.GetComponent<Alien>().CurrentTile.transform.position;
        Vector2Int alienTile = new Vector2Int((int)tileTransform.x, (int)tileTransform.z);

        foreach (GameObject player in PlayerCharacters) {
            tileTransform = player.GetComponent<Character>().CurrentTile.transform.position;
            Vector2Int playerTile = new Vector2Int((int)tileTransform.x, (int)tileTransform.z);

            if(Mathf.Abs(alienTile.x - playerTile.x) <= 1 || Mathf.Abs(alienTile.y - playerTile.y) <= 1) {
                return player;
            }
        }

        return null;
    }
}
