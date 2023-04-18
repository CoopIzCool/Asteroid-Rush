using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// handles the spawn of aliens
public class AlienManager : MonoBehaviour
{
    [SerializeField] private GameObject alienPrefab;
    private const int NUM_PER_SPAWN = 3; // should be less than the number of spawn tiles

    private static AlienManager instance;
    public static AlienManager Instance { get { return instance; } }

    public GameObject[] PlayerCharacters { get; set; } // set by grid generator
    public GenerateLevel Grid { get; set; } // set by grid generator

    private List<GameObject> activeAliens;
    private int turnsBeforeSpawn;

    // use awake because this is referenced by GenerateLevel.cs Start()
    void Awake()
    {
        instance = this;
        activeAliens = new List<GameObject>();
        turnsBeforeSpawn = 3;
    }

    // operates all of the aliens and handles spawning new ones
    public void TakeTurn() {
        // spawn new aliens
        turnsBeforeSpawn--;
        if(turnsBeforeSpawn <= 0) {
            turnsBeforeSpawn = 3;

            // choose a spawn side
            GameObject gridObject = Grid.gameObject;
            int chosenSide = Random.Range(0, gridObject.transform.childCount - 1);
            Transform spawnZone = gridObject.transform.GetChild(chosenSide);
            for(int i = 0; i < NUM_PER_SPAWN; i++) {
                Transform tileSpot = spawnZone.GetChild(i);

                GameObject newAlien = Instantiate(alienPrefab);
                activeAliens.Add(newAlien);
                alienPrefab.transform.position = tileSpot.position;
            }
        }

        // move all aliens
        foreach(GameObject alien in activeAliens) {
            Vector3 tileTransform = alien.GetComponent<Alien>().CurrentTile.transform.position;
            Vector2Int alienTile = new Vector2Int((int)tileTransform.x, (int)tileTransform.z);

            // attack if next to a player
            bool attacked = false;
            foreach(GameObject player in PlayerCharacters) {
                tileTransform = player.GetComponent<Character>().CurrentTile.transform.position;
                Vector2Int playerTile = new Vector2Int((int)tileTransform.x, (int)tileTransform.z);

                if(Mathf.Abs(alienTile.x - playerTile.x) <= 1 || Mathf.Abs(alienTile.y - playerTile.y) <= 1) {
                    player.GetComponent<Character>().TakeDamage(alien.GetComponent<Alien>().Damage);
                    attacked = true;
                    break; // only attack one player per turn
                }
            }

            if(attacked) {
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

            Vector2Int targetTile = new Vector2Int((int)closestPlayer.transform.position.x, (int)closestPlayer.transform.position.x);

            // currently, simulate stepping one tile at a time
        }
    }

    public void RemoveAlien(Alien alienScript) {
        activeAliens.Remove(alienScript.gameObject);
        Destroy(alienScript.gameObject);
    }
}
