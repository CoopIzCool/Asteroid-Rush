using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
				BuildField(tilePrefabs, objectPrefabs, Random.Range(2, 5), 2);
				break;
			case ZoneTypes.Pit:
				BuildPit(tilePrefabs, objectPrefabs, Random.Range(1, 4), Random.Range(0f, 1f) < 0.5f ? tilePrefabs[2] : objectPrefabs[0]);
				break;
			case ZoneTypes.Island:
				BuildIsland(tilePrefabs, objectPrefabs, Random.Range(4, width - 1), Random.Range(1, 3));
				break;
			case ZoneTypes.Tunnel:
				BuildTunnel(tilePrefabs, objectPrefabs, 2, 4);
				break;
			case ZoneTypes.Maze:
				BuildMaze(tilePrefabs, objectPrefabs, 2);
				break;
		}
	}

	private void BuildField(GameObject[] tilePrefabs, GameObject[] objectPrefabs, int numWalls, int maxWallSize)
	{
		for (int row = zPos; row < zPos + height; row++)
		{
			for (int col = xPos; col < xPos + width; col++)
			{
				if (GenerateLevel.GetGridItem(row, col) == null)
				{
					GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
					tiles.Add(GenerateLevel.GetGridItem(row, col));
				}
			}
		}

		GameObject pitOrWall = Random.Range(0f, 1f) < 0.5f ? tilePrefabs[2] : objectPrefabs[0];

		for (int i = 0; i < numWalls; i++)
		{
			int randomX;
			int randomZ;
			do
			{
				randomX = Random.Range(xPos, xPos + width);
				randomZ = Random.Range(zPos, zPos + height);
			} while (!GenerateLevel.IsTileWalkable(randomX, randomZ));

			int wallSize = Random.Range(1, maxWallSize + 1);

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

				wallSize--;
			} while (wallSize > 0);
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
					}
					else
					{
						if (obstacle == objectPrefabs[0])
						{
							GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
							GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().occupant = Instantiate(obstacle, new Vector3(col, obstacle.transform.position.y, row), Quaternion.Euler(obstacle.transform.rotation.eulerAngles.x, Random.Range(0f, 360f), 0), transform);
							tiles.Add(GenerateLevel.GetGridItem(row, col));
						}
						else
						{
							GenerateLevel.SetGridItem(row, col, Instantiate(obstacle, new Vector3(col, obstacle.transform.position.y, row), Quaternion.identity, transform));
							tiles.Add(GenerateLevel.GetGridItem(row, col));
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
		int direction = 0;

		if (zPos == 1)
		{
			direction = 2;
		}
		else if (zPos == GenerateLevel.GridHeight - 1 - height)
		{
			direction = 0;
		}

		if (xPos == 1)
		{
			direction = 3;
		}
		else if (xPos == GenerateLevel.GridWidth - 1 - width)
		{
			direction = 1;
		}

		BuildIslandBridge(direction, islandWidth, 4, tilePrefabs[0]);
		bridges[direction] = true;

		for (int i = 0; i < 4; i++)
		{
			if (Random.Range(0f, 1f) < 0.5f)
			{
				if (bridges[i]) continue;

				bridges[i] = true;
				numBridges++;
				BuildIslandBridge(i, islandWidth, bridgeWidth, tilePrefabs[0]);
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

			BuildIslandBridge(randomDirection, islandWidth, 4, tilePrefabs[0]);
		}

		for (int row = zPos; row < zPos + height; row++)
		{
			for (int col = xPos; col < xPos + width; col++)
			{
				if (GenerateLevel.GetGridItem(row, col) == null)
				{
					if (row < zPos + height / 2 - islandWidth / 2 || row >= zPos + height / 2 + islandWidth / 2 || col < xPos + width / 2 - islandWidth / 2 || col >= xPos + width / 2 + islandWidth / 2)
					{
						if (pitOrWall == objectPrefabs[0])
						{
							GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
							GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().occupant = Instantiate(pitOrWall, new Vector3(col, pitOrWall.transform.position.y, row), Quaternion.Euler(pitOrWall.transform.rotation.eulerAngles.x, Random.Range(0f, 360f), 0), transform);
							tiles.Add(GenerateLevel.GetGridItem(row, col));
						}
						else
						{
							tiles.Remove(GenerateLevel.GetGridItem(row, col));
							GenerateLevel.SetGridItem(row, col, Instantiate(pitOrWall, new Vector3(col, pitOrWall.transform.position.y, row), Quaternion.identity, transform));
							tiles.Add(GenerateLevel.GetGridItem(row, col));
						}
					}
					else
					{
						GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
						tiles.Add(GenerateLevel.GetGridItem(row, col));
					}
				}
			}
		}
	}

	private void BuildTunnel(GameObject[] tilePrefabs, GameObject[] objectPrefabs, int minTunnelWidth, int maxTunnelWidth)
	{
		GameObject pitOrWall = Random.Range(0f, 1f) < 0.5f ? tilePrefabs[2] : objectPrefabs[0];

		#region Set Up Enemy Spawns
		int bottomTunnelWidth = Random.Range(minTunnelWidth, maxTunnelWidth + 1);
		int randomBottom = zPos == 1 ? -1 : Random.Range(0f, 1f) < 0.5f ? Random.Range(xPos + 1, xPos + width - bottomTunnelWidth) : xPos - 1;

		int leftTunnelWidth = Random.Range(minTunnelWidth, maxTunnelWidth + 1);
		int randomLeft = xPos == 1 ? -1 : Random.Range(0f, 1f) < 0.5f ? Random.Range(zPos + 1, zPos + height - leftTunnelWidth) : zPos - 1;

		int topTunnelWidth = Random.Range(minTunnelWidth, maxTunnelWidth + 1);
		int randomTop = zPos == GenerateLevel.GridHeight - 1 - height ? -1 : Random.Range(0f, 1f) < 0.5f ? Random.Range(xPos + 1, xPos + width - topTunnelWidth) : xPos - 1;

		int rightTunnelWidth = Random.Range(minTunnelWidth, maxTunnelWidth + 1);
		int randomRight = xPos == GenerateLevel.GridWidth - 1 - width ? -1 : Random.Range(0f, 1f) < 0.5f ? Random.Range(zPos + 1, zPos + height - rightTunnelWidth) : zPos - 1;

		if ((randomBottom == -1 || randomBottom == xPos - 1) && (randomLeft == -1 || randomLeft == zPos - 1) && (randomTop == -1 || randomTop == xPos - 1) && (randomRight == -1 || randomRight == zPos - 1))
		{
			if (randomBottom == -1 || randomTop == -1)
			{
				if (Random.Range(0f, 1f) < 0.5f)
				{
					randomLeft = Random.Range(zPos + 1, zPos + height - leftTunnelWidth);
				}
				else
				{
					randomRight = Random.Range(zPos + 1, zPos + height - rightTunnelWidth);
				}
			}
			else if (randomLeft == -1 || randomRight == -1)
			{
				if (Random.Range(0f, 1f) < 0.5f)
				{
					randomTop = Random.Range(xPos + 1, xPos + width - topTunnelWidth);
				}
				else
				{
					randomBottom = Random.Range(xPos + 1, xPos + width - bottomTunnelWidth);
				}
			}
		}

		if (randomBottom == -1)
		{
			for (int col = xPos; col < xPos + width; col++)
			{
				if (col < xPos + width / 2 - 2 || col >= xPos + width / 2 + 2)
				{
					tiles.Remove(GenerateLevel.GetGridItem(zPos, col));
					if (pitOrWall == objectPrefabs[0])
					{
						GenerateLevel.SetGridItem(zPos, col, Instantiate(tilePrefabs[0], new Vector3(col, tilePrefabs[0].transform.position.y, zPos), tilePrefabs[0].transform.rotation, transform));
						GenerateLevel.GetGridItem(zPos, col).GetComponent<Tile>().occupant = Instantiate(pitOrWall, new Vector3(col, pitOrWall.transform.position.y, zPos), Quaternion.Euler(pitOrWall.transform.rotation.eulerAngles.x, Random.Range(0f, 360f), 0), transform);
					}
					else
					{
						GenerateLevel.SetGridItem(zPos, col, Instantiate(pitOrWall, new Vector3(col, pitOrWall.transform.position.y, zPos), Quaternion.identity, transform));
					}

					tiles.Add(GenerateLevel.GetGridItem(zPos, col));
					GenerateLevel.GetGridItem(zPos, col).GetComponent<Tile>().xPos = col;
					GenerateLevel.GetGridItem(zPos, col).GetComponent<Tile>().zPos = zPos;
				}
				else
				{
					tiles.Remove(GenerateLevel.GetGridItem(zPos, col));
					GenerateLevel.SetGridItem(zPos, col, Instantiate(tilePrefabs[0], new Vector3(col, tilePrefabs[0].transform.position.y, zPos), tilePrefabs[0].transform.rotation, transform));
					tiles.Add(GenerateLevel.GetGridItem(zPos, col));
					GenerateLevel.GetGridItem(zPos, col).GetComponent<Tile>().xPos = col;
					GenerateLevel.GetGridItem(zPos, col).GetComponent<Tile>().zPos = zPos;
				}
			}
		}
		else if (randomLeft == -1)
		{
			for (int row = zPos; row < zPos + height; row++)
			{
				if (row < zPos + height / 2 - 2 || row >= zPos + height / 2 + 2)
				{
					tiles.Remove(GenerateLevel.GetGridItem(row, xPos));
					if (pitOrWall == objectPrefabs[0])
					{
						GenerateLevel.SetGridItem(row, xPos, Instantiate(tilePrefabs[0], new Vector3(xPos, tilePrefabs[0].transform.position.y, row), tilePrefabs[0].transform.rotation, transform));
						GenerateLevel.GetGridItem(row, xPos).GetComponent<Tile>().occupant = Instantiate(pitOrWall, new Vector3(xPos, pitOrWall.transform.position.y, row), Quaternion.Euler(pitOrWall.transform.rotation.eulerAngles.x, Random.Range(0f, 360f), 0), transform);
					}
					else
					{
						GenerateLevel.SetGridItem(row, xPos, Instantiate(pitOrWall, new Vector3(xPos, pitOrWall.transform.position.y, row), Quaternion.identity, transform));
					}

					tiles.Add(GenerateLevel.GetGridItem(row, xPos));
					GenerateLevel.GetGridItem(row, xPos).GetComponent<Tile>().xPos = xPos;
					GenerateLevel.GetGridItem(row, xPos).GetComponent<Tile>().zPos = row;
				}
				else
				{
					tiles.Remove(GenerateLevel.GetGridItem(row, xPos));
					GenerateLevel.SetGridItem(row, xPos, Instantiate(tilePrefabs[0], new Vector3(xPos, tilePrefabs[0].transform.position.y, row), tilePrefabs[0].transform.rotation, transform));
					tiles.Add(GenerateLevel.GetGridItem(row, xPos));
					GenerateLevel.GetGridItem(row, xPos).GetComponent<Tile>().xPos = xPos;
					GenerateLevel.GetGridItem(row, xPos).GetComponent<Tile>().zPos = row;
				}
			}
		}
		else if (randomTop == -1)
		{
			for (int col = xPos; col < xPos + width; col++)
			{
				if (col < xPos + width / 2 - 2 || col >= xPos + width / 2 + 2)
				{
					tiles.Remove(GenerateLevel.GetGridItem(zPos + height - 1, col));
					if (pitOrWall == objectPrefabs[0])
					{
						GenerateLevel.SetGridItem(zPos + height - 1, col, Instantiate(tilePrefabs[0], new Vector3(col, tilePrefabs[0].transform.position.y, zPos + height - 1), tilePrefabs[0].transform.rotation, transform));
						GenerateLevel.GetGridItem(zPos + height - 1, col).GetComponent<Tile>().occupant = Instantiate(pitOrWall, new Vector3(col, pitOrWall.transform.position.y, zPos + height - 1), Quaternion.Euler(pitOrWall.transform.rotation.eulerAngles.x, Random.Range(0f, 360f), 0), transform);
					}
					else
					{
						GenerateLevel.SetGridItem(zPos + height - 1, col, Instantiate(pitOrWall, new Vector3(col, pitOrWall.transform.position.y, zPos + height - 1), Quaternion.identity, transform));
					}

					tiles.Add(GenerateLevel.GetGridItem(zPos + height - 1, col));
					GenerateLevel.GetGridItem(zPos + height - 1, col).GetComponent<Tile>().xPos = col;
					GenerateLevel.GetGridItem(zPos + height - 1, col).GetComponent<Tile>().zPos = zPos + height - 1;
				}
				else
				{
					tiles.Remove(GenerateLevel.GetGridItem(zPos + height - 1, col));
					GenerateLevel.SetGridItem(zPos + height - 1, col, Instantiate(tilePrefabs[0], new Vector3(col, tilePrefabs[0].transform.position.y, zPos + height - 1), tilePrefabs[0].transform.rotation, transform));
					tiles.Add(GenerateLevel.GetGridItem(zPos + height - 1, col));
					GenerateLevel.GetGridItem(zPos + height - 1, col).GetComponent<Tile>().xPos = col;
					GenerateLevel.GetGridItem(zPos + height - 1, col).GetComponent<Tile>().zPos = zPos + height - 1;
				}
			}
		}
		else
		{
			for (int row = zPos; row < zPos + height; row++)
			{
				if (row < zPos + height / 2 - 2 || row >= zPos + height / 2 + 2)
				{
					tiles.Remove(GenerateLevel.GetGridItem(row, xPos + width));
					if (pitOrWall == objectPrefabs[0])
					{
						GenerateLevel.SetGridItem(row, xPos + width - 1, Instantiate(tilePrefabs[0], new Vector3(xPos + width - 1, tilePrefabs[0].transform.position.y, row), tilePrefabs[0].transform.rotation, transform));
						GenerateLevel.GetGridItem(row, xPos + width - 1).GetComponent<Tile>().occupant = Instantiate(pitOrWall, new Vector3(xPos + width - 1, pitOrWall.transform.position.y, row), Quaternion.Euler(pitOrWall.transform.rotation.eulerAngles.x, Random.Range(0f, 360f), 0), transform);
					}
					else
					{
						GenerateLevel.SetGridItem(row, xPos + width - 1, Instantiate(pitOrWall, new Vector3(xPos + width - 1, pitOrWall.transform.position.y, row), Quaternion.identity, transform));
					}

					tiles.Add(GenerateLevel.GetGridItem(row, xPos + width - 1));
					GenerateLevel.GetGridItem(row, xPos + width - 1).GetComponent<Tile>().xPos = xPos + width - 1;
					GenerateLevel.GetGridItem(row, xPos + width - 1).GetComponent<Tile>().zPos = row;
				}
				else
				{
					tiles.Remove(GenerateLevel.GetGridItem(row, xPos + width - 1));
					GenerateLevel.SetGridItem(row, xPos + width - 1, Instantiate(tilePrefabs[0], new Vector3(xPos + width - 1, tilePrefabs[0].transform.position.y, row), tilePrefabs[0].transform.rotation, transform));
					tiles.Add(GenerateLevel.GetGridItem(row, xPos + width - 1));
					GenerateLevel.GetGridItem(row, xPos + width - 1).GetComponent<Tile>().xPos = xPos + width - 1;
					GenerateLevel.GetGridItem(row, xPos + width - 1).GetComponent<Tile>().zPos = row;
				}
			}
		}
		#endregion

		#region Build Bridges
		if (randomTop == -1 || randomBottom == -1)
		{
			int rightOrLeft = Random.Range(0, 3);
			int startX;
			int endX;
			int startZ;

			if (rightOrLeft == 0)
			{
				startX = xPos + width / 2 - 2;
				endX = xPos + width;
				startZ = Random.Range(zPos + 1, zPos + height - rightTunnelWidth);
				for (int row = startZ; row < startZ + rightTunnelWidth; row++)
				{
					for (int col = xPos + width / 2 - 2; col < xPos + width; col++)
					{
						GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
						tiles.Add(GenerateLevel.GetGridItem(row, col));
					}
				}
			}
			else if (rightOrLeft == 1)
			{
				startX = xPos;
				endX = xPos + width / 2 + 2;
				startZ = Random.Range(zPos + 1, zPos + height - leftTunnelWidth);
				for (int row = startZ; row < startZ + leftTunnelWidth; row++)
				{
					for (int col = xPos; col < xPos + width / 2 + 2; col++)
					{
						GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
						tiles.Add(GenerateLevel.GetGridItem(row, col));
					}
				}
			}
			else
			{
				startX = xPos;
				endX = xPos + width;
				startZ = Random.Range(zPos + 1, zPos + height - leftTunnelWidth);
				for (int row = startZ; row < startZ + leftTunnelWidth; row++)
				{
					for (int col = xPos; col < xPos + width; col++)
					{
						GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
						tiles.Add(GenerateLevel.GetGridItem(row, col));
					}
				}
			}

			if (randomTop == -1)
			{
				int tunnelWidth = rightOrLeft == 0 ? rightTunnelWidth : leftTunnelWidth;
				for (int row = startZ + tunnelWidth; row < zPos + height; row++)
				{
					for (int col = xPos + width / 2 - 2; col < xPos + width / 2 + 2; col++)
					{
						if (GenerateLevel.GetGridItem(row, col) == null)
						{
							GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
							tiles.Add(GenerateLevel.GetGridItem(row, col));
						}
					}
				}

				if(Random.Range(0f, 1f) < 0.5f)
				{
					tunnelWidth = Random.Range(minTunnelWidth, maxTunnelWidth + 1);
					int bridgeX = Random.Range(startX, endX - tunnelWidth);
					if (bridgeX == xPos) bridgeX++;
					for (int row = zPos; row < startZ; row++)
					{
						for (int col = bridgeX; col < bridgeX + tunnelWidth; col++)
						{
							if (GenerateLevel.GetGridItem(row, col) == null)
							{
								GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
								tiles.Add(GenerateLevel.GetGridItem(row, col));
							}
						}
					}
				}
			}
			else
			{
				for (int row = zPos + 1; row < startZ; row++)
				{
					for (int col = xPos + width / 2 - 2; col < xPos + width / 2 + 2; col++)
					{
						if (GenerateLevel.GetGridItem(row, col) == null)
						{
							GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
							tiles.Add(GenerateLevel.GetGridItem(row, col));
						}
					}
				}

				if (Random.Range(0f, 1f) < 0.5f)
				{
					int tunnelWidth = rightOrLeft == 0 ? rightTunnelWidth : leftTunnelWidth;
					int shipTunnelWidth = Random.Range(minTunnelWidth, maxTunnelWidth + 1);
					int bridgeX = Random.Range(startX, endX - tunnelWidth);
					if (bridgeX == xPos) bridgeX++;
					for (int row = startZ + tunnelWidth; row < zPos + height; row++)
					{
						for (int col = bridgeX; col < bridgeX + shipTunnelWidth; col++)
						{
							if (GenerateLevel.GetGridItem(row, col) == null)
							{
								GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
								tiles.Add(GenerateLevel.GetGridItem(row, col));
							}
						}
					}
				}
			}
		}
		else
		{
			int topOrBottom = Random.Range(0, 3);
			int startX;
			int startZ;
			int endZ;

			if(topOrBottom == 0)
			{
				startX = Random.Range(xPos + 1, xPos + width - topTunnelWidth);
				startZ = zPos + height / 2 - 2;
				endZ = zPos + height;
				for (int row = zPos + height / 2 - 2; row < zPos + height; row++)
				{
					for(int col = startX; col < startX + topTunnelWidth; col++)
					{
						GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
						tiles.Add(GenerateLevel.GetGridItem(row, col));
					}
				}
			}
			else if(topOrBottom == 1)
			{
				startX = Random.Range(xPos + 1, xPos + width - bottomTunnelWidth);
				startZ = zPos;
				endZ = zPos + height / 2 + 2;
				for (int row = zPos; row < zPos + height / 2 + 2; row++)
				{
					for (int col = startX; col < startX + bottomTunnelWidth; col++)
					{
						GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
						tiles.Add(GenerateLevel.GetGridItem(row, col));
					}
				}
			}
			else
			{
				startX = Random.Range(xPos + 1, xPos + width - bottomTunnelWidth);
				startZ = zPos;
				endZ = zPos + height;
				for (int row = zPos; row < zPos + height; row++)
				{
					for (int col = startX; col < startX + bottomTunnelWidth; col++)
					{
						GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
						tiles.Add(GenerateLevel.GetGridItem(row, col));
					}
				}
			}

			if (randomRight == -1)
			{
				int tunnelWidth = topOrBottom == 0 ? topTunnelWidth : bottomTunnelWidth;
				for (int row = zPos + height / 2 - 2; row < zPos + height / 2 + 2; row++)
				{
					for (int col = startX + tunnelWidth; col < xPos + width; col++)
					{
						if (GenerateLevel.GetGridItem(row, col) == null)
						{
							GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
							tiles.Add(GenerateLevel.GetGridItem(row, col));
						}
					}
				}

				if (Random.Range(0f, 1f) < 0.5f)
				{
					tunnelWidth = Random.Range(minTunnelWidth, maxTunnelWidth + 1);
					int bridgeZ = Random.Range(startZ, endZ - tunnelWidth);
					if (bridgeZ == zPos) bridgeZ++;
					for (int row = bridgeZ; row < bridgeZ + tunnelWidth; row++)
					{
						for (int col = xPos; col < startX; col++)
						{
							if (GenerateLevel.GetGridItem(row, col) == null)
							{
								GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
								tiles.Add(GenerateLevel.GetGridItem(row, col));
							}
						}
					}
				}
			}
			else
			{
				for (int row = zPos + height / 2 - 2; row < zPos + height / 2 + 2; row++)
				{
					for (int col = xPos + 1; col < startX; col++)
					{
						if (GenerateLevel.GetGridItem(row, col) == null)
						{
							GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
							tiles.Add(GenerateLevel.GetGridItem(row, col));
						}
					}
				}

				if (Random.Range(0f, 1f) < 0.5f)
				{
					int tunnelWidth = topOrBottom == 0 ? topTunnelWidth : bottomTunnelWidth;
					int shipTunnelWidth = Random.Range(minTunnelWidth, maxTunnelWidth + 1);
					int bridgeZ = Random.Range(startZ, endZ - tunnelWidth);
					if (bridgeZ == zPos) bridgeZ++;
					for (int row = bridgeZ; row < bridgeZ + shipTunnelWidth; row++)
					{
						for (int col = startX + tunnelWidth; col < xPos + width; col++)
						{
							if (GenerateLevel.GetGridItem(row, col) == null)
							{
								GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
								tiles.Add(GenerateLevel.GetGridItem(row, col));
							}
						}
					}
				}
			}
		}
		#endregion

		#region Fill Empty Spaces
		for (int row = zPos; row < zPos + height; row++)
		{
			for (int col = xPos; col < xPos + width; col++)
			{
				if (GenerateLevel.GetGridItem(row, col) == null)
				{
					if (pitOrWall == objectPrefabs[0])
					{
						GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, 0, row), tilePrefabs[0].transform.rotation, transform));
						GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().occupant = Instantiate(pitOrWall, new Vector3(col, pitOrWall.transform.position.y, row), Quaternion.Euler(pitOrWall.transform.rotation.eulerAngles.x, Random.Range(0f, 360f), 0), transform);
						tiles.Add(GenerateLevel.GetGridItem(row, col));
					}
					else
					{
						GenerateLevel.SetGridItem(row, col, Instantiate(pitOrWall, new Vector3(col, pitOrWall.transform.position.y, row), Quaternion.identity, transform));
						tiles.Add(GenerateLevel.GetGridItem(row, col));
					}
				}
			}
		}
		#endregion
	}

	private void BuildMaze(GameObject[] tilePrefabs, GameObject[] objectPrefabs, int numWalls)
	{
		GameObject pitOrWall = Random.Range(0f, 1f) < 0.5f ? tilePrefabs[2] : objectPrefabs[0];

		#region Set Up Enemy Spawns
		int randomBottom = zPos == 1 ? -1 : Random.Range(0f, 1f) < 0.5f ? Random.Range(xPos + 1, xPos + width - 1) : xPos - 1;
		int randomLeft = xPos == 1 ? -1 : Random.Range(0f, 1f) < 0.5f ? Random.Range(zPos + 1, zPos + height - 1) : zPos - 1;
		int randomTop = zPos == GenerateLevel.GridHeight - 1 - height ? -1 : Random.Range(0f, 1f) < 0.5f ? Random.Range(xPos + 1, xPos + width - 1) : xPos - 1;
		int randomRight = xPos == GenerateLevel.GridWidth - 1 - width ? -1 : Random.Range(0f, 1f) < 0.5f ? Random.Range(zPos + 1, zPos + height - 1) : zPos - 1;

		if ((randomBottom == -1 || randomBottom == xPos - 1) && (randomLeft == -1 || randomLeft == zPos - 1) && (randomTop == -1 || randomTop == xPos - 1) && (randomRight == -1 || randomRight == zPos - 1))
		{
			if (randomBottom == -1) randomTop = Random.Range(xPos + 1, xPos + width - 1);
			else if (randomLeft == -1) randomRight = Random.Range(zPos + 1, zPos + height - 1);
			else if (randomTop == -1) randomBottom = Random.Range(xPos + 1, xPos + width - 1);
			else if (randomRight == -1) randomLeft = Random.Range(zPos + 1, zPos + height - 1);
		}

		if (randomBottom == -1)
		{
			for (int col = xPos; col < xPos + width; col++)
			{
				if (col < xPos + width / 2 - 2 || col >= xPos + width / 2 + 2)
				{
					tiles.Remove(GenerateLevel.GetGridItem(zPos, col));
					if (pitOrWall == objectPrefabs[0])
					{
						GenerateLevel.SetGridItem(zPos, col, Instantiate(tilePrefabs[0], new Vector3(col, tilePrefabs[0].transform.position.y, zPos), tilePrefabs[0].transform.rotation, transform));
						GenerateLevel.GetGridItem(zPos, col).GetComponent<Tile>().occupant = Instantiate(pitOrWall, new Vector3(col, pitOrWall.transform.position.y, zPos), Quaternion.Euler(pitOrWall.transform.rotation.eulerAngles.x, Random.Range(0f, 360f), 0), transform);
					}
					else
					{
						GenerateLevel.SetGridItem(zPos, col, Instantiate(pitOrWall, new Vector3(col, pitOrWall.transform.position.y, zPos), Quaternion.identity, transform));
					}

					tiles.Add(GenerateLevel.GetGridItem(zPos, col));
					GenerateLevel.GetGridItem(zPos, col).GetComponent<Tile>().xPos = col;
					GenerateLevel.GetGridItem(zPos, col).GetComponent<Tile>().zPos = zPos;
				}
				else
				{
					tiles.Remove(GenerateLevel.GetGridItem(zPos, col));
					GenerateLevel.SetGridItem(zPos, col, Instantiate(tilePrefabs[0], new Vector3(col, tilePrefabs[0].transform.position.y, zPos), tilePrefabs[0].transform.rotation, transform));
					tiles.Add(GenerateLevel.GetGridItem(zPos, col));
					GenerateLevel.GetGridItem(zPos, col).GetComponent<Tile>().xPos = col;
					GenerateLevel.GetGridItem(zPos, col).GetComponent<Tile>().zPos = zPos;
				}
			}
		}
		else if (randomLeft == -1)
		{
			for (int row = zPos; row < zPos + height; row++)
			{
				if (row < zPos + height / 2 - 2 || row >= zPos + height / 2 + 2)
				{
					tiles.Remove(GenerateLevel.GetGridItem(row, xPos));
					if (pitOrWall == objectPrefabs[0])
					{
						GenerateLevel.SetGridItem(row, xPos, Instantiate(tilePrefabs[0], new Vector3(xPos, tilePrefabs[0].transform.position.y, row), tilePrefabs[0].transform.rotation, transform));
						GenerateLevel.GetGridItem(row, xPos).GetComponent<Tile>().occupant = Instantiate(pitOrWall, new Vector3(xPos, pitOrWall.transform.position.y, row), Quaternion.Euler(pitOrWall.transform.rotation.eulerAngles.x, Random.Range(0f, 360f), 0), transform);
					}
					else
					{
						GenerateLevel.SetGridItem(row, xPos, Instantiate(pitOrWall, new Vector3(xPos, pitOrWall.transform.position.y, row), Quaternion.identity, transform));
					}

					tiles.Add(GenerateLevel.GetGridItem(row, xPos));
					GenerateLevel.GetGridItem(row, xPos).GetComponent<Tile>().xPos = xPos;
					GenerateLevel.GetGridItem(row, xPos).GetComponent<Tile>().zPos = row;
				}
				else
				{
					tiles.Remove(GenerateLevel.GetGridItem(row, xPos));
					GenerateLevel.SetGridItem(row, xPos, Instantiate(tilePrefabs[0], new Vector3(xPos, tilePrefabs[0].transform.position.y, row), tilePrefabs[0].transform.rotation, transform));
					tiles.Add(GenerateLevel.GetGridItem(row, xPos));
					GenerateLevel.GetGridItem(row, xPos).GetComponent<Tile>().xPos = xPos;
					GenerateLevel.GetGridItem(row, xPos).GetComponent<Tile>().zPos = row;
				}
			}
		}
		else if (randomTop == -1)
		{
			for (int col = xPos; col < xPos + width; col++)
			{
				if (col < xPos + width / 2 - 2 || col >= xPos + width / 2 + 2)
				{
					tiles.Remove(GenerateLevel.GetGridItem(zPos + height - 1, col));
					if (pitOrWall == objectPrefabs[0])
					{
						GenerateLevel.SetGridItem(zPos + height - 1, col, Instantiate(tilePrefabs[0], new Vector3(col, tilePrefabs[0].transform.position.y, zPos + height - 1), tilePrefabs[0].transform.rotation, transform));
						GenerateLevel.GetGridItem(zPos + height - 1, col).GetComponent<Tile>().occupant = Instantiate(pitOrWall, new Vector3(col, pitOrWall.transform.position.y, zPos + height - 1), Quaternion.Euler(pitOrWall.transform.rotation.eulerAngles.x, Random.Range(0f, 360f), 0), transform);
					}
					else
					{
						GenerateLevel.SetGridItem(zPos + height - 1, col, Instantiate(pitOrWall, new Vector3(col, pitOrWall.transform.position.y, zPos + height - 1), Quaternion.identity, transform));
					}

					tiles.Add(GenerateLevel.GetGridItem(zPos + height - 1, col));
					GenerateLevel.GetGridItem(zPos + height - 1, col).GetComponent<Tile>().xPos = col;
					GenerateLevel.GetGridItem(zPos + height - 1, col).GetComponent<Tile>().zPos = zPos + height - 1;
				}
				else
				{
					tiles.Remove(GenerateLevel.GetGridItem(zPos + height - 1, col));
					GenerateLevel.SetGridItem(zPos + height - 1, col, Instantiate(tilePrefabs[0], new Vector3(col, tilePrefabs[0].transform.position.y, zPos + height - 1), tilePrefabs[0].transform.rotation, transform));
					tiles.Add(GenerateLevel.GetGridItem(zPos + height - 1, col));
					GenerateLevel.GetGridItem(zPos + height - 1, col).GetComponent<Tile>().xPos = col;
					GenerateLevel.GetGridItem(zPos + height - 1, col).GetComponent<Tile>().zPos = zPos + height - 1;
				}
			}
		}
		else
		{
			for (int row = zPos; row < zPos + height; row++)
			{
				if (row < zPos + height / 2 - 2 || row >= zPos + height / 2 + 2)
				{
					tiles.Remove(GenerateLevel.GetGridItem(row, xPos + width));
					if (pitOrWall == objectPrefabs[0])
					{
						GenerateLevel.SetGridItem(row, xPos + width - 1, Instantiate(tilePrefabs[0], new Vector3(xPos + width - 1, tilePrefabs[0].transform.position.y, row), tilePrefabs[0].transform.rotation, transform));
						GenerateLevel.GetGridItem(row, xPos + width - 1).GetComponent<Tile>().occupant = Instantiate(pitOrWall, new Vector3(xPos + width - 1, pitOrWall.transform.position.y, row), Quaternion.Euler(pitOrWall.transform.rotation.eulerAngles.x, Random.Range(0f, 360f), 0), transform);
					}
					else
					{
						GenerateLevel.SetGridItem(row, xPos + width - 1, Instantiate(pitOrWall, new Vector3(xPos + width - 1, pitOrWall.transform.position.y, row), Quaternion.identity, transform));
					}

					tiles.Add(GenerateLevel.GetGridItem(row, xPos + width - 1));
					GenerateLevel.GetGridItem(row, xPos + width - 1).GetComponent<Tile>().xPos = xPos + width - 1;
					GenerateLevel.GetGridItem(row, xPos + width - 1).GetComponent<Tile>().zPos = row;
				}
				else
				{
					tiles.Remove(GenerateLevel.GetGridItem(row, xPos + width - 1));
					GenerateLevel.SetGridItem(row, xPos + width - 1, Instantiate(tilePrefabs[0], new Vector3(xPos + width - 1, tilePrefabs[0].transform.position.y, row), tilePrefabs[0].transform.rotation, transform));
					tiles.Add(GenerateLevel.GetGridItem(row, xPos + width - 1));
					GenerateLevel.GetGridItem(row, xPos + width - 1).GetComponent<Tile>().xPos = xPos + width - 1;
					GenerateLevel.GetGridItem(row, xPos + width - 1).GetComponent<Tile>().zPos = row;
				}
			}
		}
		#endregion

		#region Set Up Zone Border
		for (int row = zPos; row < zPos + height; row++)
		{
			for (int col = xPos; col < xPos + width; col++)
			{
				if (GenerateLevel.GetGridItem(row, col) == null)
				{
					GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, tilePrefabs[0].transform.position.y, row), tilePrefabs[0].transform.rotation, transform));
					tiles.Add(GenerateLevel.GetGridItem(row, col));

					if (row == zPos || row == zPos + height - 1 || col == xPos || col == xPos + width - 1)
					{
						if ((row == randomLeft && col == xPos) || (row == randomRight && col == xPos + width - 1) || (row == zPos && col == randomBottom) || (row == zPos + height - 1 && col == randomTop))
						{
							tiles.Remove(GenerateLevel.GetGridItem(row, col));
							GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, tilePrefabs[0].transform.position.y, row), tilePrefabs[0].transform.rotation, transform));
							tiles.Add(GenerateLevel.GetGridItem(row, col));
						}
						else
						{
							tiles.Remove(GenerateLevel.GetGridItem(row, col));
							if (pitOrWall == objectPrefabs[0])
							{
								GenerateLevel.SetGridItem(row, col, Instantiate(tilePrefabs[0], new Vector3(col, tilePrefabs[0].transform.position.y, row), tilePrefabs[0].transform.rotation, transform));
								GenerateLevel.GetGridItem(row, col).GetComponent<Tile>().occupant = Instantiate(pitOrWall, new Vector3(col, pitOrWall.transform.position.y, row), Quaternion.Euler(pitOrWall.transform.rotation.eulerAngles.x, Random.Range(0f, 360f), 0), transform);
							}
							else
							{
								GenerateLevel.SetGridItem(row, col, Instantiate(pitOrWall, new Vector3(col, pitOrWall.transform.position.y, row), Quaternion.identity, transform));
							}

							tiles.Add(GenerateLevel.GetGridItem(row, col));
						}
					}
				}
			}
		}
		#endregion

		#region Set Up Obstacle Area
		List<Vector2Int> validObstaclePositions = new List<Vector2Int>();
		for(int row = zPos + 2; row < zPos + height - 2; row++)
		{
			for(int col = xPos + 2; col < xPos + width - 2; col++)
			{
				validObstaclePositions.Add(new Vector2Int(row, col));
			}
		}

		for (int i = 0; i < numWalls; i++)
		{
			int randomX;
			int randomZ;
			do
			{
				randomX = Random.Range(xPos + 2, xPos + width - 2);
				randomZ = Random.Range(zPos + 2, zPos + height - 2);
			} while (!validObstaclePositions.Contains(new Vector2Int(randomZ, randomX)));

			int wallSize = 2;

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
				}

				if (validObstaclePositions.Contains(new Vector2Int(randomZ, randomX - 1))) validPositions.Add(new Vector2Int(randomX - 1, randomZ));
				if (validObstaclePositions.Contains(new Vector2Int(randomZ, randomX + 1))) validPositions.Add(new Vector2Int(randomX + 1, randomZ));
				if (validObstaclePositions.Contains(new Vector2Int(randomZ - 1, randomX))) validPositions.Add(new Vector2Int(randomX, randomZ - 1));
				if (validObstaclePositions.Contains(new Vector2Int(randomZ + 1, randomX))) validPositions.Add(new Vector2Int(randomX, randomZ + 1));

				for(int row = randomZ - 1; row <= randomZ + 1; row++)
				{
					for(int col = randomX - 1; col <= randomX + 1; col++)
					{
						validObstaclePositions.Remove(new Vector2Int(row, col));
					}
				}

				if (validPositions.Count > 0)
				{
					Vector2Int chosenPosition = validPositions[Random.Range(0, validPositions.Count)];
					randomX = chosenPosition.x;
					randomZ = chosenPosition.y;
				}

				wallSize--;
			} while (wallSize > 0);
		}
		#endregion
	}

	private bool TileExists(int x, int z)
	{
		return GenerateLevel.GetGridItem(z, x) != null;
	}

	private bool TileExists(int x, int z, int width, int height)
	{
		for(int row = z; row < z + height; row++)
		{
			for(int col = x; col < x + width; col++)
			{
				if (TileExists(col, row)) return true;
			}
		}

		return false;
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
			// Build up from top of island
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

			// Build right from right side of island
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

			// Build down from bottom of island
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

			// Build left from left side of island
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
