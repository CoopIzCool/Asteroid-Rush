using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateLevel : MonoBehaviour
{
    // The grid
    private static GameObject[,] grid = null;

    [Header("Prefabs")]
	[SerializeField] private GameObject[] tilePrefabs;
	[SerializeField] private GameObject[] playerPrefabs;
	[SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private GameObject[] orePrefabs;
	[SerializeField] private GameObject spaceshipPrefab;

	[Space]
    [Header("Grid Size Parameters")]
    [SerializeField, Min(5)] private int gridWidth;
    [SerializeField, Min(5)] private int gridHeight;

    [Space]
    [Header("Grid Item Parameters")]
    [SerializeField, Range(0, 1)] private float percentEnemies;
    [SerializeField, Range(0, 1)] private float percentOres;

    // Start is called before the first frame update
    void Start()
    {
        gridWidth = Random.Range(5, 20);
        gridHeight = Random.Range(5, 20);
        ResetGrid();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
			gridWidth = Random.Range(5, 20);
			gridHeight = Random.Range(5, 20);
			ResetGrid();
        }
    }

    /// <summary>
    /// Get the item from the specified position in the grid.
    /// </summary>
    /// <param name="row">The row to access</param>
    /// <param name="col">The column to access</param>
    /// <returns>The item at grid[row, col]</returns>
    public static GameObject GetGridItem(int row, int col) => grid[row, col];

    /// <summary>
    /// Spawns the given prefab at a random unoccupied location in the grid
    /// </summary>
    /// <param name="prefab">The prefab to spawn</param>
    private void SpawnEntityAtRandom(GameObject prefab)
    {
        // Select random positions on the grid until one that is unoccupied is found
		Vector2Int randomPos;
		do
		{
			randomPos = new Vector2Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight));
		}
		while (grid[randomPos.y, randomPos.x].GetComponent<Tile>().occupant != null);

		grid[randomPos.y, randomPos.x].GetComponent<Tile>().occupant = Instantiate(prefab, new Vector3(randomPos.x, prefab.transform.position.y, randomPos.y), prefab.transform.rotation);
	}

    /// <summary>
    /// Destroy the old grid if one exists and generate a new one
    /// </summary>
	public void ResetGrid()
    {
        if (grid != null) DestroyGrid();
        BuildGrid();
    }

    /// <summary>
    /// Destroy all tiles and their occupants in the grid
    /// </summary>
    private void DestroyGrid()
    {
        foreach(GameObject tile in grid)
        {
            Destroy(tile.GetComponent<Tile>().occupant);
            Destroy(tile);
        }

        grid = null;
    }

    /// <summary>
    /// Build a new grid, then spawn all necessary entities
    /// </summary>
    private void BuildGrid()
    {
        // Make new grid
        grid = new GameObject[gridHeight, gridWidth];

        // Create spaceship / player spawn zones in center
        // Create alien spawn zones on each edge
        // Create border spawn zones along edges
        // Create ore spawn zones a set distance away from the spaceship zone
        // Create general zones that can potentially exist on top of the core zones

        // Spawn tiles anywhere that doesn't have them
        for (int row = 0; row < gridHeight; row++)
        {
            for (int col = 0; col < gridWidth; col++)
            {
                grid[row, col] = Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation);
            }
        }

        //      // Spawn spaceship
        //      grid[gridHeight / 2, gridWidth / 2].GetComponent<Tile>().occupant = Instantiate(spaceshipPrefab, new Vector3(gridWidth / 2, spaceshipPrefab.transform.position.y, gridHeight / 2), Quaternion.identity);

        //      // Spawn players
        //      for (int i = 0; i < playerPrefabs.Length; i++)
        //      {
        //          SpawnEntityAtRandom(playerPrefabs[i]);
        //      }

        //      // Spawn enemies
        //for (int i = 0; i < (int)(gridWidth * gridHeight * percentEnemies); i++)
        //{
        //	SpawnEntityAtRandom(enemyPrefabs[0]);
        //}

        //      // Spawn ores
        //for (int i = 0; i < (int)(gridWidth * gridHeight * percentOres); i++)
        //{
        //          SpawnEntityAtRandom(orePrefabs[0]);
        //}
    }
}
