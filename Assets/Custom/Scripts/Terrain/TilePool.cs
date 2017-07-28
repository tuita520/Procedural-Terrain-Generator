﻿using UnityEngine;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Contains a pool of Tiles that are can be placed and removed in the world asynchronously 
/// </summary>
public class TilePool: MonoBehaviour {
	public TerrainSettings Settings;

	private TileCache Cache = new TileCache(CACHE_SIZE);
	private int queuedTiles = 0;
	private const int CACHE_SIZE = 30;

	/// <summary>
	/// Updates tiles to update when the current queue of tiles 
	/// has finished generating.
	/// </summary>
	void Update() {
		if (queuedTiles < 1) {
			UpdateTiles();
		}
	}

	public static List<Vector2> GetTilePositionsFromRadius(int radius, Vector3 position, int length) {
		int xPos = Mathf.FloorToInt(position.x / length);
		int zPos = Mathf.FloorToInt(position.z / length);
		List<Vector2> result = new List<Vector2>(25);

		for (var zCircle = -radius; zCircle <= radius; zCircle++) {
			for (var xCircle = -radius; xCircle <= radius; xCircle++) {
				if (xCircle * xCircle + zCircle * zCircle < radius * radius)
					result.Add(new Vector2(xPos + xCircle, zPos + zCircle));
			}
		}

		return result;
	}

	/// <summary>
	/// Updates tiles that are surrounding the tracked GameObject 
	/// asynchronously
	/// </summary>
	public void UpdateTiles() {
		List<Vector2> nearbyPositions = GetTilePositionsFromRadius();
		List<Vector2> newPositions = Cache.GetNewTilePositions(nearbyPositions);

		//Remove old positions
		for (int i = 0; i < Cache.ActiveTiles.Count; i++) {
			bool found = false;

			foreach (Vector2 nearby in nearbyPositions) {
				if (Cache.ActiveTiles[i].Position == nearby) { //Position found, ignore
					found = true;
					break;
				}
			}

			if (!found) {
				Cache.CacheTile(Cache.ActiveTiles[i]);
				Cache.ActiveTiles.RemoveAt(i);
				i--;
			}
		}

		//Add new positions
		foreach (Vector2 pos in newPositions) {
			TerrainTile cached = Cache.GetCachedTileAtPosition(pos);

			//Attempt to pull from cache, generate if not available
			if (cached != null) {
				Cache.AddActiveTile(cached);
			} else {
				AddTileAsync(pos);
			}
		}
	}

	/// <summary>
	/// Takes the passed chunk position and returns all other chunk positions in <code>generationRadius</code>
	/// </summary>
	/// <returns>Tile x & z positions to add to world</returns>
	private List<Vector2> GetTilePositionsFromRadius() {
		return GetTilePositionsFromRadius(Settings.GenerationRadius, Settings.TrackedObject.transform.position, Settings.Length);
	}

	/// <summary>
	/// Adds a tile at the passed position asynchronously
	/// </summary>
	/// <param name="pos">Position to add tile at</param>
	private void AddTileAsync(Vector2 pos) {
		TerrainTile tile = new GameObject("Tile: " + pos).AddComponent<TerrainTile>();
		queuedTiles++;

		tile.CreateMesh(pos, null);
		tile.ApplySplatmap();
		Cache.AddActiveTile(tile);

		queuedTiles--;
	}
}