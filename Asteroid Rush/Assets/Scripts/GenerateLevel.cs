using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateLevel : MonoBehaviour
{
	// The grid
	private static GameObject[,] grid = null;
	private int gridWidth = 0;
	private int gridHeight = 0;

	[Header("Prefabs")]
	[SerializeField] private GameObject[] tilePrefabs;
	[SerializeField] private GameObject[] objectPrefabs;
	[SerializeField] private GameObject[] playerPrefabs;
	[SerializeField] private GameObject[] enemyPrefabs;
	[SerializeField] private GameObject[] orePrefabs;
	[SerializeField] private GameObject spaceshipPrefab;

	[Space]
	[Header("Zone Parameters")]
	[SerializeField] private GameObject[] parentZones;
	[SerializeField] private int enemyZoneWidth;
	[SerializeField] private int enemyZoneHeight;
	[SerializeField] private int minZoneWidth;
	[SerializeField] private int maxZoneWidth;
	[SerializeField] private int minZoneHeight;
	[SerializeField] private int maxZoneHeight;

	[Space]
	[Header("Grid Size Parameters")]
	[SerializeField, Min(20)] private int minGridWidth;
	[SerializeField, Min(20)] private int maxGridWidth;
	[SerializeField, Min(20)] private int minGridHeight;
	[SerializeField, Min(20)] private int maxGridHeight;

	[Space]
	[Header("Grid Item Parameters")]
	[SerializeField, Range(0, 1)] private float percentEnemies;
	[SerializeField, Range(0, 1)] private float percentOres;

	// Start is called before the first frame update
	void Start()
	{
		gridWidth = Random.Range(minGridWidth, maxGridWidth);
		gridHeight = Random.Range(minGridHeight, maxGridHeight);
		ResetGrid();
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			gridWidth = Random.Range(minGridWidth, maxGridWidth);
			gridHeight = Random.Range(minGridHeight, maxGridHeight);
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
	/// Set the item at the specified position in the grid to the given value.
	/// </summary>
	/// <param name="row">The row to access</param>
	/// <param name="col">The column to access</param>
	/// <param name="value">The value to assign</param>
	public static void SetGridItem(int row, int col, GameObject value)
	{
		if (grid[row, col] != null)
		{
			if (grid[row, col].TryGetComponent(out Tile tileScript)) Destroy(tileScript.occupant);
			Destroy(grid[row, col]);
		}
		grid[row, col] = value;
	}

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
		while (grid[randomPos.y, randomPos.x].GetComponent<Tile>().tileType != TileType.Basic && grid[randomPos.y, randomPos.x].GetComponent<Tile>().occupant != null);

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
		foreach (GameObject tile in grid)
		{
			if (tile.TryGetComponent(out Tile tileScript)) Destroy(tileScript.occupant);
			Destroy(tile);
		}

		foreach(GameObject parentZone in parentZones)
		{
			for(int i = 0; i < parentZone.transform.childCount; i++)
			{
				Destroy(parentZone.transform.GetChild(i).gameObject);
			}
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

		// Create spaceship / player spawn zone in center
		#region Spaceship Zone
		GameObject shipZoneObj = new GameObject("ShipZone");
		shipZoneObj.transform.parent = parentZones[0].transform;
		Zone shipZone = shipZoneObj.AddComponent<Zone>();
		shipZone.xPos = gridWidth / 2 - 1;
		shipZone.zPos = gridHeight / 2 - 1;
		shipZone.width = 3;
		shipZone.height = 3;

		for (int row = gridHeight / 2 - 1; row <= gridHeight / 2 + 1; row++)
		{
			for (int col = gridWidth / 2 - 1; col <= gridWidth / 2 + 1; col++)
			{
				grid[row, col] = Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, shipZoneObj.transform);
				shipZone.tiles.Add(grid[row, col]);
			}
		}

		grid[gridHeight / 2, gridWidth / 2].GetComponent<Tile>().occupant = Instantiate(spaceshipPrefab, new Vector3(gridWidth / 2, spaceshipPrefab.transform.position.y, gridHeight / 2), Quaternion.identity);
		grid[gridHeight / 2 + 1, gridWidth / 2 - 1].GetComponent<Tile>().occupant = Instantiate(playerPrefabs[0], new Vector3(gridWidth / 2 - 1, playerPrefabs[0].transform.position.y, gridHeight / 2 + 1), playerPrefabs[0].transform.rotation);
		grid[gridHeight / 2 + 1, gridWidth / 2 + 1].GetComponent<Tile>().occupant = Instantiate(playerPrefabs[1], new Vector3(gridWidth / 2 + 1, playerPrefabs[1].transform.position.y, gridHeight / 2 + 1), playerPrefabs[1].transform.rotation);
		#endregion

		// Create alien spawn zones on each edge
		#region Enemy Zones
		GameObject[] enemyZoneObjs = new GameObject[4];
		for (int i = 0; i < enemyZoneObjs.Length; i++)
		{
			enemyZoneObjs[i] = new GameObject("EnemyZone");
			enemyZoneObjs[i].transform.parent = parentZones[1].transform;
			enemyZoneObjs[i].AddComponent<Zone>();
			enemyZoneObjs[i].GetComponent<Zone>().width = enemyZoneWidth;
			enemyZoneObjs[i].GetComponent<Zone>().height = enemyZoneHeight;
		}

		int randomPos = Random.Range(1, gridWidth - enemyZoneWidth - 1);
		for (int col = randomPos; col < randomPos + enemyZoneWidth; col++)
		{
			grid[0, col] = Instantiate(tilePrefabs[1], new Vector3(col, 0, 0), tilePrefabs[1].transform.rotation, enemyZoneObjs[0].transform);
			enemyZoneObjs[0].GetComponent<Zone>().tiles.Add(grid[0, col]);
		}

		randomPos = Random.Range(1, gridWidth - enemyZoneWidth - 1);
		for (int col = randomPos; col < randomPos + enemyZoneWidth; col++)
		{
			grid[gridHeight - 1, col] = Instantiate(tilePrefabs[1], new Vector3(col, 0, gridHeight - 1), tilePrefabs[1].transform.rotation, enemyZoneObjs[2].transform);
			enemyZoneObjs[2].GetComponent<Zone>().tiles.Add(grid[gridHeight - 1, col]);
		}

		randomPos = Random.Range(1, gridHeight - enemyZoneHeight - 1);
		for (int row = randomPos; row < randomPos + enemyZoneWidth; row++)
		{
			grid[row, 0] = Instantiate(tilePrefabs[1], new Vector3(0, 0, row), tilePrefabs[1].transform.rotation, enemyZoneObjs[1].transform);
			enemyZoneObjs[1].GetComponent<Zone>().tiles.Add(grid[row, 0]);
		}

		randomPos = Random.Range(1, gridHeight - enemyZoneHeight - 1);
		for (int row = randomPos; row < randomPos + enemyZoneWidth; row++)
		{
			grid[row, gridWidth - 1] = Instantiate(tilePrefabs[1], new Vector3(gridWidth - 1, 0, row), tilePrefabs[1].transform.rotation, enemyZoneObjs[3].transform);
			enemyZoneObjs[3].GetComponent<Zone>().tiles.Add(grid[row, gridWidth - 1]);
		}
		#endregion

		// Create borders along edges
		#region Borders
		GameObject borderZoneObj = new GameObject("BorderZone");
		borderZoneObj.transform.parent = parentZones[2].transform;
		Zone borderZone = borderZoneObj.AddComponent<Zone>();
		borderZone.width = gridWidth;
		borderZone.height = gridHeight;

		for (int col = 0; col < gridWidth; col++)
		{
			if (col == 0 || col == gridWidth - 1)
			{
				if (grid[0, col] == null)
				{
					grid[0, col] = Instantiate(tilePrefabs[4], new Vector3(col, tilePrefabs[4].transform.position.y, 0), Quaternion.identity, borderZoneObj.transform);
					borderZone.tiles.Add(grid[0, col]);
				}

				if (grid[gridHeight - 1, col] == null)
				{
					grid[gridHeight - 1, col] = Instantiate(tilePrefabs[4], new Vector3(col, tilePrefabs[4].transform.position.y, gridHeight - 1), Quaternion.identity, borderZoneObj.transform);
					borderZone.tiles.Add(grid[gridHeight - 1, col]);
				}
			}
			else
			{
				if (grid[0, col] == null)
				{
					grid[0, col] = Instantiate(tilePrefabs[3], new Vector3(col, tilePrefabs[3].transform.position.y, 0), Quaternion.identity, borderZoneObj.transform);
					borderZone.tiles.Add(grid[0, col]);
				}

				if (grid[gridHeight - 1, col] == null)
				{
					grid[gridHeight - 1, col] = Instantiate(tilePrefabs[3], new Vector3(col, tilePrefabs[3].transform.position.y, gridHeight - 1), Quaternion.identity, borderZoneObj.transform);
					borderZone.tiles.Add(grid[gridHeight - 1, col]);
				}
			}
		}

		for (int row = 0; row < gridWidth; row++)
		{
			if (row == 0 || row == gridWidth - 1)
			{
				if (grid[row, 0] == null)
				{
					grid[row, 0] = Instantiate(tilePrefabs[4], new Vector3(0, tilePrefabs[4].transform.position.y, row), Quaternion.identity, borderZoneObj.transform);
					borderZone.tiles.Add(grid[row, 0]);
				}

				if (grid[row, gridWidth - 1] == null)
				{
					grid[row, gridWidth - 1] = Instantiate(tilePrefabs[4], new Vector3(gridWidth - 1, tilePrefabs[4].transform.position.y, row), Quaternion.identity, borderZoneObj.transform);
					borderZone.tiles.Add(grid[row, gridWidth - 1]);
				}
			}
			else
			{
				if (grid[row, 0] == null)
				{
					grid[row, 0] = Instantiate(tilePrefabs[3], new Vector3(0, tilePrefabs[3].transform.position.y, row), Quaternion.identity, borderZoneObj.transform);
					borderZone.tiles.Add(grid[row, 0]);
				}

				if (grid[row, gridWidth - 1] == null)
				{
					grid[row, gridWidth - 1] = Instantiate(tilePrefabs[3], new Vector3(gridWidth - 1, tilePrefabs[3].transform.position.y, row), Quaternion.identity, borderZoneObj.transform);
					borderZone.tiles.Add(grid[row, gridWidth - 1]);
				}
			}
		}
		#endregion

		// Create general zones that can potentially exist on top of the core zones
		#region Non-Core Zones
		int z = 1;
		int x = 1;
		while (z < gridHeight - minZoneHeight)
		{
			GameObject zoneObj = new GameObject();
			zoneObj.transform.parent = parentZones[3].transform;
			Zone zone = zoneObj.AddComponent<Zone>();
			zone.width = Random.Range(minZoneWidth, Mathf.Clamp(maxZoneWidth, minZoneWidth, gridWidth - x));
			zone.height = Random.Range(minZoneHeight, Mathf.Clamp(maxZoneHeight, minZoneHeight, gridHeight - z));
			zone.xPos = x;
			zone.zPos = z;
			zone.zoneType = ZoneTypes.Field; //zone.zoneType = (ZoneTypes)Random.Range(0, 5);
			zoneObj.name = zone.zoneType.ToString() + "Zone";

			zone.BuildZone(tilePrefabs, objectPrefabs);

			if (gridWidth - (x + zone.width + 1) >= minZoneWidth)
			{
				x += zone.width;
			}
			else
			{
				z += grid[z, 1].transform.parent.GetComponent<Zone>().height;
				x = 1;
			}
		}
		#endregion

		// Create ore spawn zones a set distance away from the spaceship zone
		#region Ore Zones
		GameObject[] oreZoneObjs = new GameObject[Random.Range(2, 4)];
		for (int i = 0; i < oreZoneObjs.Length; i++)
		{
			oreZoneObjs[i] = new GameObject("OreZone");
			oreZoneObjs[i].transform.parent = parentZones[4].transform;
			Zone oreZone = oreZoneObjs[i].AddComponent<Zone>();

			Vector2Int randomEdge = new Vector2Int();
			do
			{
				if (Random.Range(0f, 1f) < 0.5f)
				{
					randomEdge.x = Random.Range(1, gridWidth - 1);
					randomEdge.y = Random.Range(0f, 1f) < 0.5f ? 1 : gridHeight - 2;
				}
				else
				{
					randomEdge.x = Random.Range(0f, 1f) < 0.5f ? 1 : gridWidth - 2;
					randomEdge.y = Random.Range(1, gridHeight - 1);
				}
			} while (grid[randomEdge.y, randomEdge.x] == null || grid[randomEdge.y, randomEdge.x].transform.parent.GetComponent<Zone>().isOreZone);

			Zone chosenZone = grid[randomEdge.y, randomEdge.x].transform.parent.GetComponent<Zone>();
			chosenZone.isOreZone = true;

			oreZone.xPos = chosenZone.xPos;
			oreZone.zPos = chosenZone.zPos;
			oreZone.width = chosenZone.width;
			oreZone.height = chosenZone.height;
			oreZone.zoneType = chosenZone.zoneType;
			oreZone.isOreZone = chosenZone.isOreZone;

			int randomX;
			int randomZ;

			do
			{
				randomX = Random.Range(oreZone.xPos, oreZone.xPos + oreZone.width);
				randomZ = Random.Range(oreZone.zPos, oreZone.zPos + oreZone.height);
			} while (grid[randomZ, randomX].transform.parent != chosenZone.transform || grid[randomZ, randomX].GetComponent<Tile>().tileType == TileType.Pit || grid[randomZ, randomX].GetComponent<Tile>().occupant != null);

			int numOres = 0;
			do
			{
				List<Vector2Int> validPositions = new List<Vector2Int>();

				SetGridItem(randomZ, randomX, Instantiate(tilePrefabs[0], new Vector3(randomX, 0, randomZ), tilePrefabs[0].transform.rotation, oreZoneObjs[i].transform));
				grid[randomZ, randomX].GetComponent<Tile>().occupant = Instantiate(orePrefabs[0], new Vector3(randomX, orePrefabs[0].transform.position.y, randomZ), Quaternion.identity, oreZoneObjs[i].transform);

				if (randomX > oreZone.xPos) validPositions.Add(new Vector2Int(randomX - 1, randomZ));
				if (randomX < oreZone.xPos + oreZone.width - 1) validPositions.Add(new Vector2Int(randomX + 1, randomZ));
				if (randomZ > oreZone.zPos) validPositions.Add(new Vector2Int(randomX, randomZ - 1));
				if (randomZ < oreZone.zPos + oreZone.height - 1) validPositions.Add(new Vector2Int(randomX, randomZ + 1));

				if (validPositions.Count > 0)
				{
					Vector2Int chosenPosition = validPositions[Random.Range(0, validPositions.Count)];
					randomX = chosenPosition.x;
					randomZ = chosenPosition.y;
				}
				else
				{
					break;
				}

				numOres++;
			} while (Random.Range(0f, 1f) < 0.5f && numOres < 3);
		}
		#endregion

		//Spawn tiles anywhere that doesn't have them
		#region Filler Tiles
		for (int row = 0; row < gridHeight; row++)
		{
			for (int col = 0; col < gridWidth; col++)
			{
				if (grid[row, col] == null) grid[row, col] = Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation);
			}
		}
		#endregion
	}
}
