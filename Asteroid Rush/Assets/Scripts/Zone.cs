using System.Collections.Generic;
using UnityEngine;

public enum ZoneTypes
{
	Field,
	Pit,
	Island,
	Tunnel,
	Maze
}

public class Zone : MonoBehaviour
{
	public List<GameObject> tiles = new List<GameObject>();
	public ZoneTypes zoneType = ZoneTypes.Field;
	public bool isOreZone = false;
	public int width = 0;
	public int height = 0;
	public int xPos = 0;
	public int zPos = 0;

	public void BuildZone(GameObject[] tilePrefabs, GameObject[] objectPrefabs)
	{
		switch (zoneType)
		{
			case ZoneTypes.Field:
				BuildField(tilePrefabs, objectPrefabs, Random.Range(2, 5), 3, 0.5f);
				break;
			case ZoneTypes.Pit:
				BuildPit(tilePrefabs, objectPrefabs, Random.Range(1, 4), Random.Range(0f, 1f) < 0.5f ? tilePrefabs[2] : objectPrefabs[0]);
				break;
			case ZoneTypes.Island:
				BuildIsland(tilePrefabs, objectPrefabs, Random.Range(4, width - 1), Random.Range(1, 4));
				break;
			case ZoneTypes.Tunnel:
				BuildTunnel(tilePrefabs, objectPrefabs, Random.Range(2, width - 1), Random.Range(2, height - 1));
				break;
			case ZoneTypes.Maze:
				BuildMaze(tilePrefabs, objectPrefabs, Random.Range(6, 9), 4, 1f);
				break;
		}
	}

	private void BuildField(GameObject[] tilePrefabs, GameObject[] objectPrefabs, int numWalls, int maxWallSize, float newWallOdds)
	{
		for (int row = zPos; row < zPos + height; row++)
		{
			for (int col = xPos; col < xPos + width; col++)
			{
				if (GenerateLevel.GetGridItem(row, col) == null)
				{
					GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
					tiles.Add(GenerateLevel.GetGridItem(row, col));
					GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().xPos = col;
					GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().zPos = row;
				}
			}
		}

		// Bail out early if the zone can't support the maximum number of walls
		if (tiles.Count < numWalls * maxWallSize + 1) return;

		GameObject pitOrWall = Random.Range(0f, 1f) < 0.5f ? tilePrefabs[2] : objectPrefabs[0];

		for (int i = 0; i < numWalls; i++)
		{
			int randomX;
			int randomZ;
			do
			{
				randomX = Random.Range(xPos, xPos + width);
				randomZ = Random.Range(zPos, zPos + height);
			} while (GenerateLevel.GetGridItem(randomZ, randomX).transform.parent != transform || GenerateLevel.GetGridItem(randomZ, randomX).GetComponent<Tile>().tileType == TileType.Pit || GenerateLevel.GetGridItem(randomZ, randomX).GetComponent<Tile>().occupant != null);

			int wallSize = 0;

			do
			{
				List<Vector2Int> validPositions = new List<Vector2Int>();
				if (pitOrWall == objectPrefabs[0])
				{
					GenerateLevel.GetGridItem(randomZ, randomX).GetComponent<Tile>().occupant = Instantiate(pitOrWall, new Vector3(randomX, pitOrWall.transform.position.y, randomZ), Quaternion.Euler(pitOrWall.transform.rotation.eulerAngles.x, Random.Range(0f, 360f), 0), transform);
				}
				else
				{
					tiles.Remove(GenerateLevel.GetGridItem(randomZ, randomX));
					GenerateLevel.SetGridItem(randomZ, randomX, Instantiate(pitOrWall, new Vector3(randomX, pitOrWall.transform.position.y, randomZ), Quaternion.identity, transform));
					tiles.Add(GenerateLevel.GetGridItem(randomZ, randomX));
					GenerateLevel.GetGridItem(randomZ, randomX).GetComponent<Tile>().xPos = randomX;
					GenerateLevel.GetGridItem(randomZ, randomX).GetComponent<Tile>().zPos = randomZ;
				}

				if (randomX > xPos && GenerateLevel.GetGridItem(randomZ, randomX - 1).transform.parent == transform) validPositions.Add(new Vector2Int(randomX - 1, randomZ));
				if (randomX < xPos + width - 1 && GenerateLevel.GetGridItem(randomZ, randomX + 1).transform.parent == transform) validPositions.Add(new Vector2Int(randomX + 1, randomZ));
				if (randomZ > zPos && GenerateLevel.GetGridItem(randomZ - 1, randomX).transform.parent == transform) validPositions.Add(new Vector2Int(randomX, randomZ - 1));
				if (randomZ < zPos + height - 1 && GenerateLevel.GetGridItem(randomZ + 1, randomX).transform.parent == transform) validPositions.Add(new Vector2Int(randomX, randomZ + 1));

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

				wallSize++;
			} while (Random.Range(0f, 1f) <= newWallOdds && wallSize < maxWallSize);
		}
	}

	private void BuildPit(GameObject[] tilePrefabs, GameObject[] objectPrefabs, int walkableWidth, GameObject obstacle)
	{
		for (int row = zPos; row < zPos + height; row++)
		{
			for (int col = xPos; col < xPos + width; col++)
			{
				if (GenerateLevel.GetGridItem(row, col) == null)
				{
					if (row < zPos + walkableWidth || row > zPos + height - 1 - walkableWidth || col < xPos + walkableWidth || col > xPos + width - 1 - walkableWidth)
					{
						GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
						tiles.Add(GenerateLevel.GetGridItem(row, col));
						GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().xPos = col;
						GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().zPos = row;
					}
					else
					{
						if (obstacle == objectPrefabs[0])
						{
							GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
							GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().occupant = Instantiate(obstacle, new Vector3(col, obstacle.transform.position.y, row), Quaternion.Euler(obstacle.transform.rotation.eulerAngles.x, Random.Range(0f, 360f), 0), transform);
							tiles.Add(GenerateLevel.GetGridItem(row, col));
							GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().xPos = col;
							GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().zPos = row;
						}
						else
						{
							GenerateLevel.SetGridItem(row, col, Instantiate(obstacle, new Vector3(col, obstacle.transform.position.y, row), Quaternion.identity, transform));
							tiles.Add(GenerateLevel.GetGridItem(row, col));
							GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().xPos = col;
							GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().zPos = row;
						}
					}
				}
			}
		}
	}

	private void BuildIsland(GameObject[] tilePrefabs, GameObject[] objectPrefabs, int islandWidth, int bridgeWidth)
	{
		GameObject pitOrWall = Random.Range(0f, 1f) < 0.5f ? tilePrefabs[2] : objectPrefabs[0];
		bool[] bridges = new bool[4];
		int numBridges = 0;

		for (int i = 0; i < 4; i++)
		{
			if (Random.Range(0f, 1f) < 0.5f)
			{
				bridges[i] = true;
				numBridges++;
				BuildIslandBridge(i, islandWidth, bridgeWidth, tilePrefabs[0]);
			}
		}

		if (numBridges < 2 && !bridges[0])
		{
			BuildIslandBridge(0, islandWidth, bridgeWidth, tilePrefabs[0]);
			numBridges++;
		}

		if (numBridges < 2 && !bridges[1])
		{
			BuildIslandBridge(1, islandWidth, bridgeWidth, tilePrefabs[0]);
		}

		for (int row = zPos; row < zPos + height; row++)
		{
			for (int col = xPos; col < xPos + width; col++)
			{
				if (GenerateLevel.GetGridItem(row, col) == null)
				{
					if (row < zPos + height / 2 - islandWidth / 2 || row > zPos + height / 2 + islandWidth / 2 || col < xPos + width / 2 - islandWidth / 2 || col > xPos + width / 2 + islandWidth / 2)
					{
						if (pitOrWall == objectPrefabs[0])
						{
							GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
							GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().occupant = Instantiate(pitOrWall, new Vector3(col, pitOrWall.transform.position.y, row), Quaternion.Euler(pitOrWall.transform.rotation.eulerAngles.x, Random.Range(0f, 360f), 0), transform);
							tiles.Add(GenerateLevel.GetGridItem(row, col));
							GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().xPos = col;
							GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().zPos = row;
						}
						else
						{
							tiles.Remove(GenerateLevel.GetGridItem(row, col));
							GenerateLevel.SetGridItem(row, col, Instantiate(pitOrWall, new Vector3(col, pitOrWall.transform.position.y, row), Quaternion.identity, transform));
							tiles.Add(GenerateLevel.GetGridItem(row, col));
							GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().xPos = col;
							GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().zPos = row;
						}
					}
					else
					{
						GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
						tiles.Add(GenerateLevel.GetGridItem(row, col));
						GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().xPos = col;
						GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().zPos = row;
					}
				}
			}
		}
	}

	private void BuildTunnel(GameObject[] tilePrefabs, GameObject[] objectPrefabs, int islandWidth, int islandHeight)
	{
		GameObject pitOrWall = Random.Range(0f, 1f) < 0.5f ? tilePrefabs[2] : objectPrefabs[0];
		bool[] bridges = new bool[4];
		int numBridges = 1;
		int islandX = Random.Range(xPos + 1, xPos + width - islandWidth - 1);
		int islandZ = Random.Range(zPos + 1, zPos + height - islandHeight - 1);
		int direction = 0;

		if (zPos == 1)
		{
			islandWidth = 4;
			direction = 2;
		}
		else if (zPos == GenerateLevel.GridHeight - 1 - height)
		{
			islandWidth = 4;
			direction = 0;
		}

		if (xPos == 1)
		{
			islandHeight = 4;
			direction = 3;
		}
		else if(xPos == GenerateLevel.GridWidth - 1 - width)
		{
			islandHeight = 4;
			direction = 1;
		}

		BuildTunnelBridge(direction, islandWidth, islandHeight, islandX, islandZ, tilePrefabs[0]);
		bridges[direction] = true;

		for (int i = 0; i < 4; i++)
		{
			if (bridges[i]) continue;

			if (Random.Range(0f, 1f) < 0.5f)
			{
				bridges[i] = true;
				numBridges++;
				BuildTunnelBridge(i, islandWidth, islandHeight, islandX, islandZ, tilePrefabs[0]);
			}
		}

		if (numBridges < 2)
		{
			int randomDirection;
			do
			{
				randomDirection = Random.Range(0, 4);
			}
			while (bridges[randomDirection]);

			BuildTunnelBridge(randomDirection, islandWidth, islandHeight, islandX, islandZ, tilePrefabs[0]);
		}

		for (int row = zPos; row < zPos + height; row++)
		{
			for (int col = xPos; col < xPos + width; col++)
			{
				if (GenerateLevel.GetGridItem(row, col) == null)
				{
					if (row < islandZ || row > islandZ + islandHeight || col < islandX || col > islandX + islandWidth)
					{
						if (pitOrWall == objectPrefabs[0])
						{
							GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
							GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().occupant = Instantiate(pitOrWall, new Vector3(col, pitOrWall.transform.position.y, row), Quaternion.Euler(pitOrWall.transform.rotation.eulerAngles.x, Random.Range(0f, 360f), 0), transform);
							tiles.Add(GenerateLevel.GetGridItem(row, col));
							GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().xPos = col;
							GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().zPos = row;
						}
						else
						{
							GenerateLevel.SetGridItem(row, col, Instantiate(pitOrWall, new Vector3(col, pitOrWall.transform.position.y, row), Quaternion.identity, transform));
							tiles.Add(GenerateLevel.GetGridItem(row, col));
							GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().xPos = col;
							GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().zPos = row;
						}
					}
					else
					{
						GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
						tiles.Add(GenerateLevel.GetGridItem(row, col));
						GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().xPos = col;
						GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().zPos = row;
					}
				}
			}
		}
	}

	private void BuildMaze(GameObject[] tilePrefabs, GameObject[] objectPrefabs, int numWalls, int maxWallSize, float newWallOdds)
	{
		BuildField(tilePrefabs, objectPrefabs, numWalls, maxWallSize, newWallOdds);
	}

	/// <summary>
	/// Generates a bridge for an island
	/// </summary>
	/// <param name="direction">The direction to build the bridge in/param>
	/// <param name="islandWidth">The width of the island</param>
	/// <param name="tilePrefab">A basic tile prefab</param>
	private void BuildIslandBridge(int direction, int islandWidth, int bridgeWidth, GameObject tilePrefab)
	{
		Vector2Int bridge = new Vector2Int(0, 0);
		switch (direction)
		{
			case 0:
				bridge.x = Random.Range(xPos + width / 2 - islandWidth / 2, xPos + width / 2 + islandWidth / 2 - bridgeWidth + 1);
				bridge.y = zPos + height / 2 + islandWidth / 2;

				for (int row = bridge.y; row < zPos + height; row++)
				{
					for (int col = bridge.x; col < bridge.x + bridgeWidth; col++)
					{
						if (GenerateLevel.GetGridItem(row, col) == null)
						{
							GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefab, new Vector3(col, 0, row), tilePrefab.transform.rotation, transform));
							tiles.Add(GenerateLevel.GetGridItem(row, col));
						}
					}
				}
				break;
			case 1:
				bridge.x = xPos + width / 2 + islandWidth / 2;
				bridge.y = Random.Range(zPos + height / 2 - islandWidth / 2, zPos + height / 2 + islandWidth / 2 - bridgeWidth + 1);

				for (int row = bridge.y; row < bridge.y + bridgeWidth; row++)
				{
					for (int col = bridge.x; col < xPos + width; col++)
					{
						if (GenerateLevel.GetGridItem(row, col) == null)
						{
							GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefab, new Vector3(col, 0, row), tilePrefab.transform.rotation, transform));
							tiles.Add(GenerateLevel.GetGridItem(row, col));
						}
					}
				}
				break;
			case 2:
				bridge.x = Random.Range(xPos + width / 2 - islandWidth / 2, xPos + width / 2 + islandWidth / 2 - bridgeWidth + 1);
				bridge.y = zPos + height / 2 - islandWidth / 2;

				for (int row = zPos; row < bridge.y; row++)
				{
					for (int col = bridge.x; col < bridge.x + bridgeWidth; col++)
					{
						if (GenerateLevel.GetGridItem(row, col) == null)
						{
							GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefab, new Vector3(col, 0, row), tilePrefab.transform.rotation, transform));
							tiles.Add(GenerateLevel.GetGridItem(row, col));
						}
					}
				}
				break;
			default:
				bridge.x = xPos + width / 2 - islandWidth / 2;
				bridge.y = Random.Range(zPos + height / 2 - islandWidth / 2, zPos + height / 2 + islandWidth / 2 - bridgeWidth + 1);

				for (int row = bridge.y; row < bridge.y + bridgeWidth; row++)
				{
					for (int col = xPos; col < bridge.x; col++)
					{
						if (GenerateLevel.GetGridItem(row, col) == null)
						{
							GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefab, new Vector3(col, 0, row), tilePrefab.transform.rotation, transform));
							tiles.Add(GenerateLevel.GetGridItem(row, col));
						}
					}
				}
				break;
		}
	}

	/// <summary>
	/// Generates a bridge for an island
	/// </summary>
	/// <param name="direction">The direction to build the bridge in/param>
	/// <param name="islandWidth">The width of the island</param>
	/// <param name="islandHeight">The height of the island</param>
	/// <param name="islandX">The bottom left x-position of the island</param>
	/// <param name="islandZ">The bottom left z-position of the island</param>
	/// <param name="tilePrefab">A basic tile prefab</param>
	private void BuildTunnelBridge(int direction, int islandWidth, int islandHeight, int islandX, int islandZ, GameObject tilePrefab)
	{
		Vector2Int bridge = new Vector2Int(0, 0);
		switch (direction)
		{
			case 0:
				bridge.x = islandX;
				bridge.y = islandZ + islandHeight;

				for (int row = bridge.y; row < zPos + height; row++)
				{
					for (int col = bridge.x; col < bridge.x + islandWidth; col++)
					{
						if (GenerateLevel.GetGridItem(row, col) == null)
						{
							GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefab, new Vector3(col, 0, row), tilePrefab.transform.rotation, transform));
							tiles.Add(GenerateLevel.GetGridItem(row, col));
						}
					}
				}
				break;
			case 1:
				bridge.x = islandX + islandWidth;
				bridge.y = islandZ;

				for (int row = bridge.y; row < bridge.y + islandHeight; row++)
				{
					for (int col = bridge.x; col < xPos + width; col++)
					{
						if (GenerateLevel.GetGridItem(row, col) == null)
						{
							GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefab, new Vector3(col, 0, row), tilePrefab.transform.rotation, transform));
							tiles.Add(GenerateLevel.GetGridItem(row, col));
						}
					}
				}
				break;
			case 2:
				bridge.x = islandX;
				bridge.y = islandZ;

				for (int row = zPos; row < bridge.y; row++)
				{
					for (int col = bridge.x; col < bridge.x + islandWidth; col++)
					{
						if (GenerateLevel.GetGridItem(row, col) == null)
						{
							GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefab, new Vector3(col, 0, row), tilePrefab.transform.rotation, transform));
							tiles.Add(GenerateLevel.GetGridItem(row, col));
						}
					}
				}
				break;
			default:
				bridge.x = islandX;
				bridge.y = islandZ;

				for (int row = bridge.y; row < bridge.y + islandHeight; row++)
				{
					for (int col = xPos; col < bridge.x; col++)
					{
						if (GenerateLevel.GetGridItem(row, col) == null)
						{
							GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefab, new Vector3(col, 0, row), tilePrefab.transform.rotation, transform));
							tiles.Add(GenerateLevel.GetGridItem(row, col));
						}
					}
				}
				break;
		}
	}

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}
}
