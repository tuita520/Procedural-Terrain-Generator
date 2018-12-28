﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Terra.CoherentNoise;
using Terra.Graph.Generators;
using Terra.Structures;
using Terra.Terrain;
using UnityEngine;
using XNode;

namespace Terra.Graph.Biome {
	[CreateNodeMenu("Biomes/Biome")]
	public class BiomeNode: PreviewableNode {
		[Output]
		public BiomeNode Output;

		/// <summary>
		/// Name of the biome
		/// </summary>
		public string Name;

		public Color PreviewColor;

		/// <summary>
		/// Splats that this Biome will display
		/// </summary>
		[Input(ShowBackingValue.Never)] 
		public SplatObjectNode SplatObjects;

		[Input]
		public float Blend = 1f;

		[Input(ShowBackingValue.Never, ConnectionType.Override)] 
		public AbsGeneratorNode HeightmapGenerator;
		public bool UseHeightmap;
		public Vector2 HeightmapMinMaxMask = new Vector2(0, 1);

		[Input(ShowBackingValue.Never, ConnectionType.Override)] 
		public AbsGeneratorNode TemperatureGenerator;
		public bool UseTemperature;
		public Vector2 TemperatureMinMaxMask = new Vector2(0, 1);

		[Input(ShowBackingValue.Never, ConnectionType.Override)] 
		public AbsGeneratorNode MoistureGenerator;
		public bool UseMoisture;
		public Vector2 MoistureMinMaxMask = new Vector2(0, 1);

		private GeneratorSampler _heightmapSampler;
		private GeneratorSampler _temperatureSampler;
		private GeneratorSampler _moistureSampler;

		public override object GetValue(NodePort port) {
			return this;
		}

		public override Texture2D DidRequestTextureUpdate() {
			return Preview(PreviewTextureSize);
		}

		public SplatObjectNode[] GetSplatObjects() {
			return GetInputValues<SplatObjectNode>("SplatObjects", null);
		}

		/// <summary>
		/// Gets the heights for the height, temperature, and moisture maps. If 
		/// a map isn't used, 0 is set for its value. The returned float structure 
		/// is filled as follows:
		/// [heightmap val, temperature val, moisture val]
		/// </summary>
		/// <param name="x">X coordinate to sample</param>
		/// <param name="y">Y coordinate to sample</param>
		/// <param name="position">Position in Terra grid</param>
		/// <param name="resolution">Resolution of the map to sample for</param>
		/// <returns></returns>
		public float[] GetMapHeightsAt(int x, int y, GridPosition position, int resolution, float spread, int length) {
			float[] heights = new float[3];

			if (UseHeightmap && _heightmapSampler != null) {
				heights[0] = _heightmapSampler.GetValue(x, y, position, resolution, spread, length);
			}
			if (UseTemperature && _temperatureSampler != null) {
				heights[1] = _temperatureSampler.GetValue(x, y, position, resolution, spread, length);
			}
			if (UseMoisture && _moistureSampler != null) {
				heights[2] = _moistureSampler.GetValue(x, y, position, resolution, spread, length);
			}

			return heights;
		}
		
		/// <summary>
		/// Creates a map of weights where each weight represents "how much" of 
		/// this biome to show.
		/// </summary>
		/// <param name="values"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public float[,] GetWeightedValues(float[,,] values, float min, float max) {
			if (values == null) {
				return null;
			}

			//Constraints
			Constraint hc = new Constraint(HeightmapMinMaxMask.x, HeightmapMinMaxMask.y);
			Constraint tc = new Constraint(TemperatureMinMaxMask.x, TemperatureMinMaxMask.y);
			Constraint mc = new Constraint(MoistureMinMaxMask.x, MoistureMinMaxMask.y);

			int resolution = values.GetLength(0);
			float[,] map = new float[resolution, resolution];

			//Normalize values
			for (int x = 0; x < resolution; x++) {
				for (int y = 0; y < resolution; y++) {
					float hv = (values[x, y, 0] - min) / (max - min);
					float tv = (values[x, y, 1] - min) / (max - min);
					float mv = (values[x, y, 2] - min) / (max - min);

					float val = 0;
					int count = 0;

					//Gather heights that fit set min/max
					if (UseHeightmap && hc.Fits(hv)) {
						val += hc.Weight(hv, Blend);
						count++;
					}
					if (UseTemperature && tc.Fits(tv)) {
						val += tc.Weight(tv, Blend);
						count++;
					}
					if (UseMoisture && mc.Fits(mv)) {
						val += mc.Weight(mv, Blend);
						count++;
					}

					val = count > 0 ? val / count : 0;
					map[x, y] = val;
				}
			}

			return map;
		}

		/// <summary>
		/// Create a map of weights each representing the weights of 
		/// the height, temperature, and moisture maps.
		/// </summary>
		/// <param name="position">Position in Terra grid units to poll the generators</param>
		/// <param name="resolution">Resolution of the map to create</param>
		/// <param name="spread">Optional spread value to poll the generator with</param>
		/// <param name="length">Optional length parameter that represents the length of a tile</param>
		/// <returns>
		/// A structure containing the weights of the height, temperature, and moisture maps.
		/// The returned structure is a 3D array where the first two indices are the x & y 
		/// coordinates while the last index is the weight of the height, temperature, and 
		/// moisture maps (in that order).
		/// </returns>
		public float[,,] GetMapValues(GridPosition position, int resolution, float spread = -1f, int length = -1) {
			float[,,] heights = new float[resolution, resolution, 3];

			SetHeightmapSampler();
			SetTemperatureSampler();
			SetMoistureSampler();

			//Fill heights structure and set min/max values
			for (int x = 0; x < resolution; x++) {
				for (int y = 0; y < resolution; y++) {
					spread = spread == -1f ? TerraConfig.Instance.Generator.Spread : spread;
					length = length == -1 ? TerraConfig.Instance.Generator.Length : length;

					float[] generated = GetMapHeightsAt(x, y, position, resolution, spread, length);

					for (int z = 0; z < 3; z++) {
						float height = generated[z];
						heights[x, y, z] = height;
					}
				}
			}

			return heights;
		}

		/// <summary>
		/// Creates a texture previewing this biome with the passed size used 
		/// for the width and height
		/// </summary>
		/// <param name="size">width & height</param>
		/// <returns></returns>
		public Texture2D Preview(int size) {
			Texture2D tex = new Texture2D(size, size);
			
			float[,,] biomeVals = GetMapValues(GridPosition.Zero, size, size, 1);

			//Calculate min/max
			float min = float.PositiveInfinity;
			float max = float.NegativeInfinity;

			for (int x = 0; x < size; x++) {
				for (int y = 0; y < size; y++) {
					for (int z = 0; z < 3; z++) {
						float val = biomeVals[x, y, z];
						
						if (val < min) {
							min = val;
						}
						if (val > max) {
							max = val;
						}
					}
				}
			}

			float[,] normalized = GetWeightedValues(biomeVals, min, max);

			//Set texture
			for (int x = 0; x < size; x++) {
				for (int y = 0; y < size; y++) {
					float val = normalized[x, y];
					tex.SetPixel(x, y, new Color(val, val, val, 1f));
				}
			}

			tex.Apply();
			return tex;
		}

		public void SetHeightmapSampler() {
			if (!UseHeightmap) {
				return;
			}

			AbsGeneratorNode gen = GetInputValue<AbsGeneratorNode>("HeightmapGenerator");
			_heightmapSampler = gen == null ? null : new GeneratorSampler(gen.GetGenerator());
		}

		public void SetTemperatureSampler() {
			if (!UseTemperature) {
				return;
			}

			AbsGeneratorNode gen = GetInputValue<AbsGeneratorNode>("TemperatureGenerator");
			_temperatureSampler = gen == null ? null : new GeneratorSampler(gen.GetGenerator());
		}

		public void SetMoistureSampler() {
			if (!UseMoisture) {
				return;
			}

			AbsGeneratorNode gen = GetInputValue<AbsGeneratorNode>("MoistureGenerator");
			_moistureSampler = gen == null ? null : new GeneratorSampler(gen.GetGenerator());
		}
	}
}