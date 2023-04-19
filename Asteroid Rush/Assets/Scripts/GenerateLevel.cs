using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GenerateLevel : MonoBehaviour
{
	// The grid
	private static GameObject[,] grid = null;
	private static int gridWidth = 0;
	private static int gridHeight = 0;

	public static int GridWidth
	{
		get { return gridWidth; }
	}

	public static int GridHeight
	{
		get { return gridHeight; }
	}

	// Validation fields
	private List<Vector2Int> corePositions = new List<Vector2Int>();
	private Vector2Int shipPosition;

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
	[SerializeField] private int minGridWidth;
	[SerializeField] private int maxGridWidth;
	[SerializeField] private int minGridHeight;
	[SerializeField] private int maxGridHeight;

	[Space]
	[Header("Grid Item Parameters")]
	[SerializeField, Min(4)] private int totalOres;
	[SerializeField, Min(1)] private int numOresPerZone;

	[Space]
	[Header("Camera Logic & Misc")]
	[SerializeField] CameraFixedRotation cameraLogic;

	// Start is called before the first frame update
	void Start()
	{
		//do ResetGrid(); while (!IsGridValid());
		ResetGrid();
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			//do ResetGrid(); while (!IsGridValid());
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
		bool isGridValid;
		do
		{
			gridWidth = Random.Range(minGridWidth, maxGridWidth);
			gridHeight = Random.Range(minGridHeight, maxGridHeight);
			if (grid != null) DestroyGrid();
			isGridValid = BuildGrid();
		}
		while (!isGridValid);
	}

	/// <summary>
	/// Destroy all tiles and their occupants in the grid
	/// </summary>
	private void DestroyGrid()
	{
		for(int row = 0; row < grid.GetLength(0); row++)
		{
			for (int col = 0; col < grid.GetLength(1); col++)
			{
				if (grid[row, col] != null)
				{
					if (grid[row, col].TryGetComponent(out Tile tileScript)) Destroy(tileScript.occupant);
					Destroy(grid[row, col]);
				}
			}
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
	/// <returns>True if a valid grid is built, false otherwise</returns>
	private bool BuildGrid()
	{
		// Make new grid
		grid = new GameObject[gridHeight, gridWidth];

		//Set offset of the camera based on grid height
		cameraLogic.XShift = gridHeight / 2.0f;
		cameraLogic.ZShift = gridWidth / 2.0f;
		cameraLogic.SetRadiusAndCenter();

		// Create spaceship / player spawn zone in center
		#region Spaceship Zone
		GameObject shipZoneObj = new GameObject("ShipZone");
		shipZoneObj.transform.parent = parentZones[0].transform;
		Zone shipZone = shipZoneObj.AddComponent<Zone>();
		shipZone.width = Random.Range(minZoneWidth, maxZoneWidth) - 1;
		shipZone.height = Random.Range(minZoneHeight, maxZoneHeight) - 1;
		shipZone.xPos = gridWidth / 2 - shipZone.width / 2 + shipZone.width % 2;
		shipZone.zPos = gridHeight / 2 - shipZone.height / 2 + shipZone.height % 2;

		for (int row = shipZone.zPos; row <= shipZone.zPos + shipZone.height; row++)
		{
			for (int col = shipZone.xPos; col <= shipZone.xPos + shipZone.width; col++)
			{
				grid[row, col] = Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, shipZoneObj.transform);
				shipZone.tiles.Add(grid[row, col]);
				grid[row, col].GetComponent<Tile>().xPos = col;
				grid[row, col].GetComponent<Tile>().zPos = row;
			}
		}

		shipPosition = new Vector2Int(gridHeight / 2, gridWidth / 2);
		grid[gridHeight / 2, gridWidth / 2].GetComponent<Tile>().occupant = Instantiate(spaceshipPrefab, new Vector3(gridWidth / 2, spaceshipPrefab.transform.position.y, gridHeight / 2), spaceshipPrefab.transform.rotation, shipZoneObj.transform);
		grid[gridHeight / 2 + 1, gridWidth / 2 - 1].GetComponent<Tile>().occupant = Instantiate(playerPrefabs[0], new Vector3(gridWidth / 2 - 1, playerPrefabs[0].transform.position.y, gridHeight / 2 + 1), playerPrefabs[0].transform.rotation, shipZoneObj.transform);
		grid[gridHeight / 2 + 1, gridWidth / 2 + 1].GetComponent<Tile>().occupant = Instantiate(playerPrefabs[1], new Vector3(gridWidth / 2 + 1, playerPrefabs[1].transform.position.y, gridHeight / 2 + 1), playerPrefabs[1].transform.rotation, shipZoneObj.transform);

		//Set initial character tile to players
		grid[gridHeight / 2 + 1, gridWidth / 2 - 1].GetComponent<Tile>().occupant.GetComponent<Character>().CurrentTile = grid[gridHeight / 2 + 1, gridWidth / 2 - 1].GetComponent<Tile>();
		grid[gridHeight / 2 + 1, gridWidth / 2 + 1].GetComponent<Tile>().occupant.GetComponent<Character>().CurrentTile = grid[gridHeight / 2 + 1, gridWidth / 2 + 1].GetComponent<Tile>();
		
		#endregion

		// Create general zones that can potentially exist on top of the core zones
		#region Travel Zones
		//int z = 1;
		//int x = 1;
		//while (z < gridHeight - minZoneHeight)
		for (int i = 0; i < 4; i++)
		{
			GameObject zoneObj = new GameObject();
			zoneObj.transform.parent = parentZones[3].transform;
			Zone zone = zoneObj.AddComponent<Zone>();
			//zone.width = Random.Range(minZoneWidth, Mathf.Clamp(maxZoneWidth, minZoneWidth, gridWidth - x));
			//zone.height = Random.Range(minZoneHeight, Mathf.Clamp(maxZoneHeight, minZoneHeight, gridHeight - z));
			//zone.xPos = x;
			//zone.zPos = z;
			zone.width = Random.Range(minZoneWidth, maxZoneWidth);
			zone.height = Random.Range(minZoneHeight, maxZoneHeight);
			//zone.zoneType = (ZoneTypes)Random.Range(2, 5);
			zone.zoneType = ZoneTypes.Tunnel;
			zoneObj.name = zone.zoneType.ToString() + "Zone";

			switch (i)
			{
				case 0:
					zone.xPos = zone.width + 1;
					zone.zPos = 1;
					break;
				case 1:
					zone.xPos = zone.width + 1;
					zone.zPos = gridHeight - 1 - zone.height;
					break;
				case 2:
					zone.xPos = 1;
					zone.zPos = zone.height + 1;
					break;
				default:
					zone.xPos = gridWidth - 1 - zone.width;
					zone.zPos = zone.height + 1;
					break;
			}

			zone.BuildZone(tilePrefabs, objectPrefabs);

			//if (gridWidth - (x + zone.width + 1) >= minZoneWidth)
			//{
			//	x += zone.width;
			//}
			//else
			//{
			//	z += grid[z, 1].transform.parent.GetComponent<Zone>().height;
			//	x = 1;
			//}
		}
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

		//int randomPos = gridWidth / 2 - enemyZoneWidth / 2;
		//corePositions.Add(new Vector2Int(0, randomPos));
		int travelZonePos = parentZones[3].transform.GetChild(0).GetComponent<Zone>().xPos;
		int travelZoneDim = parentZones[3].transform.GetChild(0).GetComponent<Zone>().width;
		Tile tile;
		for (int col = travelZonePos; col < travelZonePos + travelZoneDim; col++)
		{
			if (enemyZoneObjs[0].transform.childCount == enemyZoneWidth) break;

			tile = grid[1, col].GetComponent<Tile>();
			if (tile.tileType == TileType.Basic && tile.occupant == null)
			{
				grid[0, col] = Instantiate(tilePrefabs[1], new Vector3(col, 0, 0), tilePrefabs[1].transform.rotation, enemyZoneObjs[0].transform);
				enemyZoneObjs[0].GetComponent<Zone>().tiles.Add(grid[0, col]);
				grid[0, col].GetComponent<Tile>().xPos = col;
				grid[0, col].GetComponent<Tile>().zPos = 0;
			}
		}

		//randomPos = gridWidth / 2 - enemyZoneWidth / 2;
		//corePositions.Add(new Vector2Int(gridHeight - 1, randomPos));
		travelZonePos = parentZones[3].transform.GetChild(1).GetComponent<Zone>().xPos;
		travelZoneDim = parentZones[3].transform.GetChild(1).GetComponent<Zone>().width;
		for (int col = travelZonePos; col < travelZonePos + travelZoneDim; col++)
		{
			if (enemyZoneObjs[2].transform.childCount == enemyZoneWidth) break;

			tile = grid[gridHeight - 2, col].GetComponent<Tile>();
			if (tile.tileType == TileType.Basic && tile.occupant == null)
			{
				grid[gridHeight - 1, col] = Instantiate(tilePrefabs[1], new Vector3(col, 0, gridHeight - 1), tilePrefabs[1].transform.rotation, enemyZoneObjs[2].transform);
				enemyZoneObjs[2].GetComponent<Zone>().tiles.Add(grid[gridHeight - 1, col]);
				grid[gridHeight - 1, col].GetComponent<Tile>().xPos = col;
				grid[gridHeight - 1, col].GetComponent<Tile>().zPos = gridHeight - 1;
			}
		}

		//randomPos = gridHeight / 2 - enemyZoneWidth / 2;
		//corePositions.Add(new Vector2Int(randomPos, 0));
		travelZonePos = parentZones[3].transform.GetChild(2).GetComponent<Zone>().zPos;
		travelZoneDim = parentZones[3].transform.GetChild(2).GetComponent<Zone>().height;
		for (int row = travelZonePos; row < travelZonePos + travelZoneDim; row++)
		{
			if (enemyZoneObjs[1].transform.childCount == enemyZoneWidth) break;

			tile = grid[row, 1].GetComponent<Tile>();
			if (tile.tileType == TileType.Basic && tile.occupant == null)
			{
				grid[row, 0] = Instantiate(tilePrefabs[1], new Vector3(0, 0, row), tilePrefabs[1].transform.rotation, enemyZoneObjs[1].transform);
				enemyZoneObjs[1].GetComponent<Zone>().tiles.Add(grid[row, 0]);
				grid[row, 0].GetComponent<Tile>().xPos = 0;
				grid[row, 0].GetComponent<Tile>().zPos = row;
			}
		}

		//randomPos = gridHeight / 2 - enemyZoneWidth / 2;
		//corePositions.Add(new Vector2Int(randomPos, gridWidth - 1));
		travelZonePos = parentZones[3].transform.GetChild(3).GetComponent<Zone>().zPos;
		travelZoneDim = parentZones[3].transform.GetChild(3).GetComponent<Zone>().height;
		for (int row = travelZonePos; row < travelZonePos + travelZoneDim; row++)
		{
			if (enemyZoneObjs[3].transform.childCount == enemyZoneWidth) break;

			tile = grid[row, gridWidth - 2].GetComponent<Tile>();
			if (tile.tileType == TileType.Basic && tile.occupant == null)
			{
				grid[row, gridWidth - 1] = Instantiate(tilePrefabs[1], new Vector3(gridWidth - 1, 0, row), tilePrefabs[1].transform.rotation, enemyZoneObjs[3].transform);
				enemyZoneObjs[3].GetComponent<Zone>().tiles.Add(grid[row, gridWidth - 1]);
				grid[row, gridWidth - 1].GetComponent<Tile>().xPos = gridWidth - 1;
				grid[row, gridWidth - 1].GetComponent<Tile>().zPos = row;
			}
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
					grid[0, col].GetComponent<Tile>().xPos = col;
					grid[0, col].GetComponent<Tile>().zPos = 0;
				}

				if (grid[gridHeight - 1, col] == null)
				{
					grid[gridHeight - 1, col] = Instantiate(tilePrefabs[4], new Vector3(col, tilePrefabs[4].transform.position.y, gridHeight - 1), Quaternion.identity, borderZoneObj.transform);
					borderZone.tiles.Add(grid[gridHeight - 1, col]);
					grid[gridHeight - 1, col].GetComponent<Tile>().xPos = col;
					grid[gridHeight - 1, col].GetComponent<Tile>().zPos = gridHeight - 1;
				}
			}
			else
			{
				if (grid[0, col] == null)
				{
					grid[0, col] = Instantiate(tilePrefabs[3], new Vector3(col, tilePrefabs[3].transform.position.y, 0), Quaternion.identity, borderZoneObj.transform);
					borderZone.tiles.Add(grid[0, col]);
					grid[0, col].GetComponent<Tile>().xPos = col;
					grid[0, col].GetComponent<Tile>().zPos = 0;
				}

				if (grid[gridHeight - 1, col] == null)
				{
					grid[gridHeight - 1, col] = Instantiate(tilePrefabs[3], new Vector3(col, tilePrefabs[3].transform.position.y, gridHeight - 1), Quaternion.identity, borderZoneObj.transform);
					borderZone.tiles.Add(grid[gridHeight - 1, col]);
					grid[gridHeight - 1, col].GetComponent<Tile>().xPos = col;
					grid[gridHeight - 1, col].GetComponent<Tile>().zPos = gridHeight - 1;
				}
			}
		}

		for (int row = 0; row < gridHeight; row++)
		{
			if (row == 0 || row == gridHeight - 1)
			{
				if (grid[row, 0] == null)
				{
					grid[row, 0] = Instantiate(tilePrefabs[4], new Vector3(0, tilePrefabs[4].transform.position.y, row), Quaternion.identity, borderZoneObj.transform);
					borderZone.tiles.Add(grid[row, 0]);
					grid[row, 0].GetComponent<Tile>().xPos = 0;
					grid[row, 0].GetComponent<Tile>().zPos = row;
				}

				if (grid[row, gridWidth - 1] == null)
				{
					grid[row, gridWidth - 1] = Instantiate(tilePrefabs[4], new Vector3(gridWidth - 1, tilePrefabs[4].transform.position.y, row), Quaternion.identity, borderZoneObj.transform);
					borderZone.tiles.Add(grid[row, gridWidth - 1]);
					grid[row, gridWidth - 1].GetComponent<Tile>().xPos = gridWidth - 1;
					grid[row, gridWidth - 1].GetComponent<Tile>().zPos = row;
				}
			}
			else
			{
				if (grid[row, 0] == null)
				{
					grid[row, 0] = Instantiate(tilePrefabs[3], new Vector3(0, tilePrefabs[3].transform.position.y, row), Quaternion.identity, borderZoneObj.transform);
					borderZone.tiles.Add(grid[row, 0]);
					grid[row, 0].GetComponent<Tile>().xPos = 0;
					grid[row, 0].GetComponent<Tile>().zPos = row;
				}

				if (grid[row, gridWidth - 1] == null)
				{
					grid[row, gridWidth - 1] = Instantiate(tilePrefabs[3], new Vector3(gridWidth - 1, tilePrefabs[3].transform.position.y, row), Quaternion.identity, borderZoneObj.transform);
					borderZone.tiles.Add(grid[row, gridWidth - 1]);
					grid[row, gridWidth - 1].GetComponent<Tile>().xPos = gridWidth - 1;
					grid[row, gridWidth - 1].GetComponent<Tile>().zPos = row;
				}
			}
		}
		#endregion

		// Create ore spawn zones a set distance away from the spaceship zone
		#region Ore Zones
		GameObject[] oreZoneObjs = new GameObject[4];
		for (int i = 0; i < oreZoneObjs.Length; i++)
		{
			oreZoneObjs[i] = new GameObject("OreZone");
			oreZoneObjs[i].transform.parent = parentZones[4].transform;
			Zone oreZone = oreZoneObjs[i].AddComponent<Zone>();
			oreZone.width = Random.Range(minZoneWidth, maxZoneWidth);
			oreZone.height = Random.Range(minZoneHeight, maxZoneHeight);
			oreZone.zoneType = (ZoneTypes)Random.Range(0, 2);

			//Vector2Int randomEdge = new Vector2Int();
			//do
			//{
			//	if (Random.Range(0f, 1f) < 0.5f)
			//	{
			//		randomEdge.x = Random.Range(1, gridWidth - 1);
			//		randomEdge.y = Random.Range(0f, 1f) < 0.5f ? 1 : gridHeight - 2;
			//	}
			//	else
			//	{
			//		randomEdge.x = Random.Range(0f, 1f) < 0.5f ? 1 : gridWidth - 2;
			//		randomEdge.y = Random.Range(1, gridHeight - 1);
			//	}
			//} while (grid[randomEdge.y, randomEdge.x] == null || grid[randomEdge.y, randomEdge.x].transform.parent.GetComponent<Zone>().isOreZone);

			//Zone chosenZone = grid[randomEdge.y, randomEdge.x].transform.parent.GetComponent<Zone>();
			//chosenZone.isOreZone = true;

			//oreZone.xPos = chosenZone.xPos;
			//oreZone.zPos = chosenZone.zPos;
			//oreZone.width = chosenZone.width;
			//oreZone.height = chosenZone.height;
			//oreZone.zoneType = chosenZone.zoneType;
			//oreZone.isOreZone = chosenZone.isOreZone;

			Tuple<Zone, Zone> adjacentZones;
			bool oreToTravel = false;
			bool travelToShip = false;
			switch (i)
			{
				case 0:
					oreZone.xPos = 1;
					oreZone.zPos = 1;
					adjacentZones = new Tuple<Zone, Zone>(grid[oreZone.height + 1, 1].transform.parent.GetComponent<Zone>(), grid[1, oreZone.width + 1].transform.parent.GetComponent<Zone>());

					for(int col = adjacentZones.Item1.xPos; col < adjacentZones.Item1.xPos + adjacentZones.Item1.width; col++)
					{
						Tile adjacentTile = grid[adjacentZones.Item1.zPos, col].GetComponent<Tile>();
						if (adjacentTile.tileType == TileType.Basic && adjacentTile.occupant == null)
						{
							oreToTravel = true;
							break;
						}
					}

					for (int row = adjacentZones.Item1.zPos; row < adjacentZones.Item1.zPos + adjacentZones.Item1.height; row++)
					{
						Tile adjacentTile = grid[row, adjacentZones.Item1.xPos + adjacentZones.Item1.width - 1].GetComponent<Tile>();
						if (adjacentTile.tileType == TileType.Basic && adjacentTile.occupant == null)
						{
							travelToShip = true;
							break;
						}
					}

					if(!oreToTravel || !travelToShip)
					{
						oreToTravel = false;
						travelToShip = false;
						for (int row = adjacentZones.Item2.zPos; row < adjacentZones.Item2.zPos + adjacentZones.Item2.height; row++)
						{
							Tile adjacentTile = grid[row, adjacentZones.Item2.xPos].GetComponent<Tile>();
							if (adjacentTile.tileType == TileType.Basic && adjacentTile.occupant == null)
							{
								oreToTravel = true;
								break;
							}
						}

						for (int col = adjacentZones.Item2.xPos; col < adjacentZones.Item2.xPos + adjacentZones.Item2.width; col++)
						{
							Tile adjacentTile = grid[adjacentZones.Item2.zPos + adjacentZones.Item2.height - 1, col].GetComponent<Tile>();
							if (adjacentTile.tileType == TileType.Basic && adjacentTile.occupant == null)
							{
								travelToShip = true;
								break;
							}
						}
					}

					if (!oreToTravel || !travelToShip) return false;
					break;
				case 1:
					oreZone.xPos = 1;
					oreZone.zPos = gridHeight - 1 - oreZone.height;
					adjacentZones = new Tuple<Zone, Zone>(grid[oreZone.zPos - 1, 1].transform.parent.GetComponent<Zone>(), grid[oreZone.zPos, oreZone.width + 1].transform.parent.GetComponent<Zone>());

					for (int col = adjacentZones.Item1.xPos; col < adjacentZones.Item1.xPos + adjacentZones.Item1.width; col++)
					{
						Tile adjacentTile = grid[adjacentZones.Item1.zPos + adjacentZones.Item1.height - 1, col].GetComponent<Tile>();
						if (adjacentTile.tileType == TileType.Basic && adjacentTile.occupant == null)
						{
							oreToTravel = true;
							break;
						}
					}

					for (int row = adjacentZones.Item1.zPos; row < adjacentZones.Item1.zPos + adjacentZones.Item1.height; row++)
					{
						Tile adjacentTile = grid[row, adjacentZones.Item1.xPos + adjacentZones.Item1.width - 1].GetComponent<Tile>();
						if (adjacentTile.tileType == TileType.Basic && adjacentTile.occupant == null)
						{
							travelToShip = true;
							break;
						}
					}

					if (!oreToTravel || !travelToShip)
					{
						oreToTravel = false;
						travelToShip = false;
						for (int row = adjacentZones.Item2.zPos; row < adjacentZones.Item2.zPos + adjacentZones.Item2.height; row++)
						{
							Tile adjacentTile = grid[row, adjacentZones.Item2.xPos].GetComponent<Tile>();
							if (adjacentTile.tileType == TileType.Basic && adjacentTile.occupant == null)
							{
								oreToTravel = true;
								break;
							}
						}

						for (int col = adjacentZones.Item2.xPos; col < adjacentZones.Item2.xPos + adjacentZones.Item2.width; col++)
						{
							Tile adjacentTile = grid[adjacentZones.Item2.zPos, col].GetComponent<Tile>();
							if (adjacentTile.tileType == TileType.Basic && adjacentTile.occupant == null)
							{
								travelToShip = true;
								break;
							}
						}
					}

					if (!oreToTravel || !travelToShip) return false;
					break;
				case 2:
					oreZone.xPos = gridWidth - 1 - oreZone.width;
					oreZone.zPos = gridHeight - 1 - oreZone.height;
					adjacentZones = new Tuple<Zone, Zone>(grid[oreZone.zPos - 1, oreZone.xPos].transform.parent.GetComponent<Zone>(), grid[oreZone.zPos, oreZone.xPos - 1].transform.parent.GetComponent<Zone>());

					for (int col = adjacentZones.Item1.xPos; col < adjacentZones.Item1.xPos + adjacentZones.Item1.width; col++)
					{
						Tile adjacentTile = grid[adjacentZones.Item1.zPos + adjacentZones.Item1.height - 1, col].GetComponent<Tile>();
						if (adjacentTile.tileType == TileType.Basic && adjacentTile.occupant == null)
						{
							oreToTravel = true;
							break;
						}
					}

					for (int row = adjacentZones.Item1.zPos; row < adjacentZones.Item1.zPos + adjacentZones.Item1.height; row++)
					{
						Tile adjacentTile = grid[row, adjacentZones.Item1.xPos].GetComponent<Tile>();
						if (adjacentTile.tileType == TileType.Basic && adjacentTile.occupant == null)
						{
							travelToShip = true;
							break;
						}
					}

					if (!oreToTravel || !travelToShip)
					{
						oreToTravel = false;
						travelToShip = false;
						for (int row = adjacentZones.Item2.zPos; row < adjacentZones.Item2.zPos + adjacentZones.Item2.height; row++)
						{
							Tile adjacentTile = grid[row, adjacentZones.Item2.xPos + adjacentZones.Item2.width - 1].GetComponent<Tile>();
							if (adjacentTile.tileType == TileType.Basic && adjacentTile.occupant == null)
							{
								oreToTravel = true;
								break;
							}
						}

						for (int col = adjacentZones.Item2.xPos; col < adjacentZones.Item2.xPos + adjacentZones.Item2.width; col++)
						{
							Tile adjacentTile = grid[adjacentZones.Item2.zPos, col].GetComponent<Tile>();
							if (adjacentTile.tileType == TileType.Basic && adjacentTile.occupant == null)
							{
								travelToShip = true;
								break;
							}
						}
					}

					if (!oreToTravel || !travelToShip) return false;
					break;
				default:
					oreZone.xPos = gridWidth - 1 - oreZone.width;
					oreZone.zPos = 1;
					adjacentZones = new Tuple<Zone, Zone>(grid[oreZone.height + 1, oreZone.xPos].transform.parent.GetComponent<Zone>(), grid[1, oreZone.xPos - 1].transform.parent.GetComponent<Zone>());

					for (int col = adjacentZones.Item1.xPos; col < adjacentZones.Item1.xPos + adjacentZones.Item1.width; col++)
					{
						Tile adjacentTile = grid[adjacentZones.Item1.zPos, col].GetComponent<Tile>();
						if (adjacentTile.tileType == TileType.Basic && adjacentTile.occupant == null)
						{
							oreToTravel = true;
							break;
						}
					}

					for (int row = adjacentZones.Item1.zPos; row < adjacentZones.Item1.zPos + adjacentZones.Item1.height; row++)
					{
						Tile adjacentTile = grid[row, adjacentZones.Item1.xPos].GetComponent<Tile>();
						if (adjacentTile.tileType == TileType.Basic && adjacentTile.occupant == null)
						{
							travelToShip = true;
							break;
						}
					}

					if (!oreToTravel || !travelToShip)
					{
						oreToTravel = false;
						travelToShip = false;
						for (int row = adjacentZones.Item2.zPos; row < adjacentZones.Item2.zPos + adjacentZones.Item2.height; row++)
						{
							Tile adjacentTile = grid[row, adjacentZones.Item2.xPos + adjacentZones.Item2.width - 1].GetComponent<Tile>();
							if (adjacentTile.tileType == TileType.Basic && adjacentTile.occupant == null)
							{
								oreToTravel = true;
								break;
							}
						}

						for (int col = adjacentZones.Item2.xPos; col < adjacentZones.Item2.xPos + adjacentZones.Item2.width; col++)
						{
							Tile adjacentTile = grid[adjacentZones.Item2.zPos + adjacentZones.Item2.height - 1, col].GetComponent<Tile>();
							if (adjacentTile.tileType == TileType.Basic && adjacentTile.occupant == null)
							{
								travelToShip = true;
								break;
							}
						}
					}

					if (!oreToTravel || !travelToShip) return false;
					break;
			}

			oreZone.BuildZone(tilePrefabs, objectPrefabs);

			int randomX;
			int randomZ;

			//do
			//{
			//	randomX = Random.Range(oreZone.xPos, oreZone.xPos + oreZone.width);
			//	randomZ = Random.Range(oreZone.zPos, oreZone.zPos + oreZone.height);
			//} while (grid[randomZ, randomX].transform.parent != chosenZone.transform || grid[randomZ, randomX].GetComponent<Tile>().tileType == TileType.Pit || grid[randomZ, randomX].GetComponent<Tile>().occupant != null);

			int numOres = 0;
			do
			{
				do
				{
					randomX = Random.Range(oreZone.xPos, oreZone.xPos + oreZone.width);
					randomZ = Random.Range(oreZone.zPos, oreZone.zPos + oreZone.height);
				} while (grid[randomZ, randomX].GetComponent<Tile>().tileType == TileType.Pit || grid[randomZ, randomX].GetComponent<Tile>().occupant != null);

				SetGridItem(randomZ, randomX, Instantiate(tilePrefabs[0], new Vector3(randomX, 0, randomZ), tilePrefabs[0].transform.rotation, oreZoneObjs[i].transform));
				grid[randomZ, randomX].GetComponent<Tile>().occupant = Instantiate(orePrefabs[0], new Vector3(randomX, orePrefabs[0].transform.position.y, randomZ), Quaternion.identity, oreZoneObjs[i].transform);
				grid[randomZ, randomX].GetComponent<Tile>().xPos = randomX;
				grid[randomZ, randomX].GetComponent<Tile>().zPos = randomZ;

				numOres++;
			} while (numOres < numOresPerZone);

			corePositions.Add(new Vector2Int(randomZ, randomX));
		}

		int remainingOres = totalOres / (numOresPerZone * oreZoneObjs.Length);
		for(int i = 0; i < remainingOres; i++)
		{
			Zone randomZone = oreZoneObjs[Random.Range(0, oreZoneObjs.Length)].GetComponent<Zone>();
			int randomX;
			int randomZ;
			int timesRun = 0;
			bool badZone = false;

			do
			{
				randomX = Random.Range(randomZone.xPos, randomZone.xPos + randomZone.width);
				randomZ = Random.Range(randomZone.zPos, randomZone.zPos + randomZone.height);

				if (timesRun > 100)
				{
					badZone = true;
					break;
				}

				timesRun++;
			} while (grid[randomZ, randomX].GetComponent<Tile>().tileType == TileType.Pit || grid[randomZ, randomX].GetComponent<Tile>().occupant != null);

			if (badZone) continue;

			SetGridItem(randomZ, randomX, Instantiate(tilePrefabs[0], new Vector3(randomX, 0, randomZ), tilePrefabs[0].transform.rotation, oreZoneObjs[i].transform));
			grid[randomZ, randomX].GetComponent<Tile>().occupant = Instantiate(orePrefabs[0], new Vector3(randomX, orePrefabs[0].transform.position.y, randomZ), Quaternion.identity, oreZoneObjs[i].transform);
			grid[randomZ, randomX].GetComponent<Tile>().xPos = randomX;
			grid[randomZ, randomX].GetComponent<Tile>().zPos = randomZ;
		}
		#endregion

		//Spawn tiles anywhere that doesn't have them
		#region Filler Tiles
		//for (int row = 0; row < gridHeight; row++)
		//{
		//	for (int col = 0; col < gridWidth; col++)
		//	{
		//		if (grid[row, col] == null)
		//		{
		//			grid[row, col] = Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, parentZones[5].transform);
		//			grid[row, col].GetComponent<Tile>().xPos = col;
		//			grid[row, col].GetComponent<Tile>().zPos = row;
		//		}
		//	}
		//}
		#endregion

		return true;
	}

	/// <summary>
	/// Runs a modified version of Dijkstra's Algorithm to check if there is a path from the player spawn to all important landmarks in the current level
	/// </summary>
	/// <returns>Whether the current level is valid</returns>
	private bool IsGridValid()
	{
		#region Dijkstra's Algorithm
		//List<GameObject> openList = new List<GameObject> { startTile };
		//List<GameObject> closedList = new List<GameObject>();
		List<Vector2Int> openList = new List<Vector2Int> { new Vector2Int(shipPosition.x, shipPosition.y) };
		List<Vector2Int> closedList = new List<Vector2Int>();

		// Current tile
		Vector2Int current = openList[0];
		int currentZ;
		int currentX;

		// Run until there are no tiles left to search
		while (openList.Count > 0)
		{
			current = openList[0];
			currentZ = current.x;
			currentX = current.y;

			// If the item is reached, we are successful
			if (corePositions.Contains(current))
			{
				corePositions.Remove(current);
			}
			else if (corePositions.Count == 0)
			{
				break;
			}

			// Check the four cardinal directions
			for (int i = 0; i < 4; i++)
			{
				Vector2Int endTile;

				switch (i)
				{
					case 0:
						if (currentX == gridWidth - 1) continue;
						endTile = new Vector2Int(currentZ, currentX + 1);
						break;
					case 1:
						if (currentZ == 0) continue;
						endTile = new Vector2Int(currentZ - 1, currentX);
						break;
					case 2:
						if (currentX == 0) continue;
						endTile = new Vector2Int(currentZ, currentX - 1);
						break;
					default:
						if (currentZ == gridHeight - 1) continue;
						endTile = new Vector2Int(currentZ + 1, currentX);
						break;
				}

				// Add any unsearched tiles
				if (!openList.Contains(endTile) && !closedList.Contains(endTile) && grid[endTile.x, endTile.y].GetComponent<Tile>().tileType == TileType.Basic)
				{
					if (grid[endTile.x, endTile.y].GetComponent<Tile>().occupant == null || grid[endTile.x, endTile.y].GetComponent<Tile>().occupant.tag == "Ore")
					{
						openList.Add(endTile);
					}
				}
			}
			openList.Remove(current);
			closedList.Add(current);
		}

		// If the item was never found, we failed
		if (corePositions.Count > 0)
		{
			Debug.Log("-----------------");
			Debug.Log("MISSED POSITIONS:");
			Debug.Log("-----------------");
			corePositions.ForEach(position =>
			{
				Debug.Log(grid[position.x, position.y].transform.parent);
				Debug.Log(position);
			});
			corePositions.Clear();
			return false;
		}
		#endregion

		return true;
	}
}
