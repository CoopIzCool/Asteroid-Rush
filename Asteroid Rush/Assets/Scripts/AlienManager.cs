using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// handles the spawn of aliens
public class AlienManager : MonoBehaviour
{
    [SerializeField] private GameObject alienPrefab;
    [SerializeField] private GameObject spawnZoneContainer;
    private const int NUM_PER_SPAWN = 1; // should be less than the number of spawn tiles

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

            List<Tile> movableTiles = TurnHandler.Instance.FindAvailableTiles(alien.GetComponent<Character>());
            if(movableTiles.Count <= 0) {
                Debug.Log("oops nowhere to move");
                continue; // no where to move
            }

            Tile closestTile = movableTiles[0];
            closestDistance = Vector3.Distance(closestPlayer.transform.position, closestTile.gameObject.transform.position);
            foreach(Tile tile in movableTiles) {
                float distance = Vector3.Distance(closestPlayer.transform.position, closestTile.gameObject.transform.position);
                if(distance < closestDistance) {
                    distance = closestDistance;
                    closestTile = tile;
                }
            }

            alien.GetComponent<Character>().MoveToTile(closestTile);
        }
    }

    public void RemoveAlien(Alien alienScript) {
        activeAliens.Remove(alienScript.gameObject);
        Destroy(alienScript.gameObject);
    }

    // helper function to find if a player is in an adjacent tile. If there are multiple, returns the first one found. Null if no adjacent
    private GameObject FindAdjacentPlayer(GameObject alien) {
        Tile tile = alien.GetComponent<Alien>().CurrentTile;
        Vector2Int alienTile = new Vector2Int(tile.xPos, tile.zPos);

        foreach (GameObject player in PlayerCharacters) {
            tile = player.GetComponent<Character>().CurrentTile;
            Vector2Int playerTile = new Vector2Int(tile.xPos, tile.zPos);

            if (Mathf.Abs(alienTile.x - playerTile.x) <= 1 || Mathf.Abs(alienTile.y - playerTile.y) <= 1) {
                return player;
            }
        }

        return null;
    }
}
