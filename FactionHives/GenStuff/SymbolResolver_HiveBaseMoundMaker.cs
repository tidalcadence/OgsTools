﻿using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace ExtraHives.GenStuff
{
	// Token: 0x020010B0 RID: 4272 ExtraHives.GenStuff.SymbolResolver_HiveMountMaker
	public class SymbolResolver_HiveBaseMoundMaker : SymbolResolver
	{

		List<IntVec3> cells = new List<IntVec3>();
		List<IntVec3> cavecells = new List<IntVec3>();
		List<IntVec3> bigCaveCenters = new List<IntVec3>();
		List<IntVec3> smallCaveCenters = new List<IntVec3>();
		List<IntVec3> cellforbigcave = new List<IntVec3>();
		List<IntVec3> cellforlittlecave = new List<IntVec3>();
		List<IntVec3> entranceCaveCenters = new List<IntVec3>();
		// Token: 0x0600650C RID: 25868 RVA: 0x00233C80 File Offset: 0x00231E80
		public override void Resolve(ResolveParams rp)
		{
			cells.Clear();
			cavecells.Clear();
			bigCaveCenters.Clear();
			smallCaveCenters.Clear();
			cellforbigcave.Clear();
			cellforlittlecave.Clear();
			entranceCaveCenters.Clear();
			Map map = BaseGen.globalSettings.map;
			IntVec3 CenterCell = rp.rect.CenterCell;
			float dist = Math.Min(rp.rect.TopRight.DistanceTo(rp.rect.BottomLeft), GenRadial.MaxRadialPatternRadius-0.5f);
			cells = GenRadial.RadialCellsAround(CenterCell, dist, true).ToList();
			RoofGrid roofGrid = BaseGen.globalSettings.map.roofGrid;
			RoofDef def = rp.roofDef ?? RoofDefOf.RoofRockThick;
			int cavecountBig = Rand.RangeInclusive(1, 4);
			List<IntVec3> cellst = cells.Where(x => x.DistanceTo(CenterCell) > 15 && x.DistanceTo(CenterCell) < dist - 10).ToList();
			foreach (IntVec3 c in cells)
			{
				roofGrid.SetRoof(c, def);
				map.terrainGrid.SetTerrain(c, TerrainDefOf.Gravel);
				this.TrySpawnWall(c, rp);
			}
			cellforbigcave = cellst.Where(x => x.DistanceTo(CenterCell) > 15 && x.DistanceTo(CenterCell) < dist / 2).ToList();
			cellforlittlecave = cellst.Where(x => x.DistanceTo(CenterCell) > dist / 2 && x.DistanceTo(CenterCell) < dist - 10).ToList();
			float entranceChance = 1f;
			for (int i = 0; i < cavecountBig; i++)
			{
				float dist2 = Rand.RangeInclusive(7, 10);
				IntVec3 cell = cellforbigcave.Where(x=> !bigCaveCenters.Any(y=> x.DistanceTo(y)<20)).RandomElement();
				bigCaveCenters.Add(cell);
				List<IntVec3> ccells = GenRadial.RadialCellsAround(cell, dist2, true).ToList();

				if (this.NorthQuad(cell, CenterCell))
				{
					this.GenerateQuad(CenterCell, Rot4.North, cells.Where(x => x.DistanceTo(CenterCell) >= dist && this.NorthQuad(x, CenterCell)).ToList(),entranceChance,dist);
				}
				if (this.SouthQuad(cell, CenterCell))
				{
					this.GenerateQuad(CenterCell, Rot4.South, cells.Where(x => x.DistanceTo(CenterCell) >= dist && this.SouthQuad(x, CenterCell)).ToList(), entranceChance, dist);
				}
				if (this.EastQuad(cell, CenterCell))
				{
					this.GenerateQuad(CenterCell, Rot4.East, cells.Where(x => x.DistanceTo(CenterCell) >= dist && this.EastQuad(x, CenterCell)).ToList(), entranceChance, dist);
				}
				if (this.WestQuad(cell, CenterCell))
				{
					this.GenerateQuad(CenterCell, Rot4.West, cells.Where(x => x.DistanceTo(CenterCell) >= dist && this.WestQuad(x, CenterCell)).ToList(), entranceChance, dist);
				}

				cavecells.AddRange(ccells);
			}
		//	TrySpawnCave(CenterCell, cellst, dist, out List<IntVec3> cavecells2);
		//	cavecells.AddRange(cavecells2);
			//	cavecells.AddRange(GenRadial.RadialCellsAround(rp.rect.CenterCell, 10, true));
			//
			Log.Message("cavecells contains " + cavecells.Count);
			foreach (IntVec3 c in cavecells)
			{
				Thing b = c.GetFirstThing<Thing>(map);
				if (b != null) b.Destroy();
			}
			
		}
		public void GenerateQuad(IntVec3 CenterCell, Rot4 rot, List<IntVec3> QuadCells, float entranceChance, float radius)
		{
			Map map = BaseGen.globalSettings.map;
			Log.Message("Generatirng "+rot.ToStringHuman().CapitalizeFirst()+ " Quad Small Chambers");

			int cavecountSmall = Rand.RangeInclusive(5, 10);
			for (int i2 = 0; i2 < cavecountSmall; i2++)
			{
				float dist = Rand.RangeInclusive(3, 6);
				IntVec3 cell = cellforlittlecave.RandomElement();
				smallCaveCenters.Add(cell);
				List<IntVec3> ccells = GenRadial.RadialCellsAround(cell, dist, true).ToList();
				cavecells.AddRange(ccells);
			}
			
			if (Rand.Chance(entranceChance))
			{
				Log.Message("Generatirng " + rot.ToStringHuman().CapitalizeFirst() + " Quad Entrance");
				entranceChance -= 0.0f;
				float dist = Rand.RangeInclusive(3, 10);
				IntVec3 cell = cells.Where(x => x.DistanceTo(CenterCell) >= radius && QuadCells.Contains(x)).RandomElement();
				List<IntVec3> ccells = GenRadial.RadialCellsAround(cell, dist, true).ToList();
				cavecells.AddRange(ccells);
				entranceCaveCenters.Add(cell);
				List<IntVec3> tcells = new List<IntVec3>();
				tcells.AddRange(smallCaveCenters);
				tcells.AddRange(bigCaveCenters);

				List<IntVec3> ncells = new List<IntVec3>();
				ncells = tcells.Where(x => x.DistanceTo(CenterCell) < radius - 5 && !cell.WithinRegions(x, map, 10, TraverseParms.For(TraverseMode.ByPawn), RegionType.Set_Passable)).OrderBy(x => x.DistanceTo(cell)).ToList();
				Log.Message("Generatirng " + rot.ToStringHuman().CapitalizeFirst() + " Quad path "+ ncells.Count+" Nodes located");
				IntVec3 prevnode = cell;
				for (int i = 0; i < ncells.Count; i++)
				{
					IntVec3 node = ncells[i];
					IntVec3 offset = prevnode - node;
					float num = 0f;
					if ((prevnode.ToVector3() - node.ToVector3()).MagnitudeHorizontalSquared() > 0.001f)
					{
						num = (prevnode.ToVector3() - node.ToVector3()).AngleFlat();
					}
					num += 90f;
					int trX = prevnode.x < node.x ? prevnode.x : node.x;
					int trY = prevnode.z < node.z ? prevnode.z : node.z;
					CellRect rect = new CellRect(trX, trY, (int)node.DistanceTo(prevnode), (int)node.DistanceTo(prevnode));

					Log.Message((i == 0 ? "Entrace at " : "Prevous node at ") + prevnode + " next node at " + node + " distance: " + node.DistanceTo(prevnode) + " Angele: " + num + " Bottom Left: " + rect.BottomLeft + " Top Right: " + rect.TopRight);
					Dig(prevnode, num, 3, rect.ToList(), map, closed: false);
					prevnode = node;
				}
			}
			cellforlittlecave.RemoveAll(x => QuadCells.Contains(x));
			cellforbigcave.RemoveAll(x => QuadCells.Contains(x));
		}
		private void GeneratePathToCenter(List<IntVec3> group, Map map)
		{
			int a = GenMath.RoundRandom((float)group.Count * Rand.Range(0.9f, 1.1f) * 5.8f / 10000f);
			a = Mathf.Min(a, 3);
			if (a > 0)
			{
				a = Rand.RangeInclusive(1, a);
			}
			float num = TunnelsWidthPerRockCount.Evaluate(group.Count);
			for (int i = 0; i < a; i++)
			{
				IntVec3 start = IntVec3.Invalid;
				float num2 = -1f;
				float dir = -1f;
				float num3 = -1f;
				for (int j = 0; j < 10; j++)
				{
					IntVec3 intVec = FindRandomEdgeCellForTunnel(group, map);
					float distToCave = GetDistToCave(intVec, group, map, 40f, treatOpenSpaceAsCave: false);
					float dist;
					float num4 = FindBestInitialDir(intVec, group, out dist);
					if (!start.IsValid || distToCave > num2 || (distToCave == num2 && dist > num3))
					{
						start = intVec;
						num2 = distToCave;
						dir = num4;
						num3 = dist;
					}
				}
				float width = Rand.Range(num * 0.8f, num);
				Dig(start, dir, width, group, map, closed: false);
			}
		}
		public bool EastQuad(IntVec3 cell, IntVec3 CenterCell) 
		{

			IntVec3 offset = CenterCell - cell;
			if (offset.x < (offset.z > 0 ? offset.z - (offset.z * 2) : offset.z))
			{
				return true;
			}
			return false;
		}
		
		public bool WestQuad(IntVec3 cell, IntVec3 CenterCell)
		{

			IntVec3 offset = CenterCell - cell;
			if (offset.x > (offset.z < 0 ? offset.z - (offset.z * 2) : offset.z))
			{
				return true;
			}
			return false;
		}
		
		public bool SouthQuad(IntVec3 cell, IntVec3 CenterCell)
		{

			IntVec3 offset = CenterCell - cell;
			if (offset.z > (offset.x < 0 ? offset.x - (offset.x * 2) : offset.x))
			{
				return true;
			}
			return false;
		}
		
		public bool NorthQuad(IntVec3 cell, IntVec3 CenterCell)
		{

			IntVec3 offset = CenterCell - cell;
			if (offset.z < (offset.x > 0 ? offset.x - (offset.x * 2) : offset.x))
			{
				return true;
			}
			return false;
		}

		// Token: 0x0600650D RID: 25869 RVA: 0x00233CF0 File Offset: 0x00231EF0
		private Thing TrySpawnWall(IntVec3 c, ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{

				if (!thingList[i].def.destroyable)
				{
					return null;
				}
				/*
				if (thingList[i] is Building_Door)
				{
					return null;
				}
				*/
			}
			if (rp.rect.CenterCell.DistanceTo(c)<10)
			{
				return null;
			}
			for (int j = thingList.Count - 1; j >= 0; j--)
			{
				thingList[j].Destroy(DestroyMode.Vanish);
			}
			/*
			if (rp.chanceToSkipWallBlock != null && Rand.Chance(rp.chanceToSkipWallBlock.Value))
			{
				return null;
			}
			*/
			Thing thing = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("ExtraHive_Hive_Wall"), null);
			thing.SetFaction(rp.faction, null);
			return GenSpawn.Spawn(thing, c, map, WipeMode.Vanish);
		}

		/*
		public void TrySpawnCave(Map map)
		{
			directionNoise = new Perlin(0.0020500000100582838, 2.0, 0.5, 4, Rand.Int, QualityMode.Medium);
			BoolGrid visited = new BoolGrid(map);
			List<IntVec3> group = new List<IntVec3>();
			foreach (IntVec3 allCell in map.AllCells)
			{
				if (!visited[allCell] && IsRock(allCell, elevation, map))
				{
					group.Clear();
					map.floodFiller.FloodFill(allCell, (IntVec3 x) => IsRock(x, elevation, map), delegate (IntVec3 x)
					{
						visited[x] = true;
						group.Add(x);
					});
					Trim(group, map);
					RemoveSmallDisconnectedSubGroups(group, map);
					if (group.Count >= 300)
					{
						DoOpenTunnels(group, map);
						DoClosedTunnels(group, map);
					}
				}
			}
		}
		*/

		
		public void TrySpawnCave(IntVec3 c, List<IntVec3> cells, float dist, out List<IntVec3> CaveCells)
		{
			CaveCells = new List<IntVec3>();
			Map map = BaseGen.globalSettings.map;
			directionNoise = new Perlin(0.0020500000100582838, 2.0, 0.5, 4, Rand.Int, QualityMode.Medium);
			BoolGrid visited = new BoolGrid(map);
			MapGenFloatGrid elevation = new MapGenFloatGrid(map);
			List<IntVec3> group = new List<IntVec3>();
			foreach (IntVec3 allCell in CaveCells)
			{
				if (visited[allCell])
				{
					Log.Message("been to "+ allCell + " already");
				}
				if (!IsRock(allCell, elevation, map))
				{
					Log.Message(allCell + " is not rock");
				}
				if (allCell.DistanceTo(c) < dist && cells.Contains(c))
				{
					elevation[allCell] = 1f;
				}
				if (!visited[allCell] && IsRock(allCell, elevation, map) && cells.Contains(c))
				{
					group.Clear();
					map.floodFiller.FloodFill(allCell, (IntVec3 x) => IsRock(x, elevation, map), delegate (IntVec3 x)
					{
						visited[x] = true;
						group.Add(x);
					});
					Log.Message("TrySpawnCave at " + allCell + "containing " + group.Count + " cells");
					Trim(group, map);
					RemoveSmallDisconnectedSubGroups(group, map);
					if (group.Count >= 30)
					{
						Log.Message("TrySpawnCave starting tunnel at " + allCell + "containing "+ group.Count + " cells");
						DoOpenTunnels(group, map);
						DoClosedTunnels(group, map);
						CaveCells.AddRange(group);
					}
				}
			}
		}
		
		private void TrySpawnCave2(IntVec3 c, List<IntVec3> cells, float dist, out List<IntVec3> CaveCells)
		{
			CaveCells = new List<IntVec3>();
			Log.Message("checking " + cells.Count + " cells for tunnels"); ;
			MapGenFloatGrid elevation = MapGenerator.Elevation;
			Map map = BaseGen.globalSettings.map;
			BoolGrid visited = new BoolGrid(map);
			List<IntVec3> group = new List<IntVec3>();
			directionNoise = new Perlin(0.0020500000100582838, 2.0, 0.5, 4, Rand.Int, QualityMode.Medium);
			//	directionNoise = new Perlin(0.0020500000100582838, 1, 1, 6, Rand.Int, QualityMode.Medium);
			//  directionNoise = new Perlin(0.0070000002160668373, 2.0, 0.5, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
			foreach (IntVec3 allCell in cells)
			{
				if (allCell.DistanceTo(c)< dist)
				{
					elevation[allCell] = 1f;
				}
				if (!visited[allCell] && allCell.Filled(map))
				{
					//	Log.Message("checking " + allCell + " cells for tunnel");
					group.Clear();
					map.floodFiller.FloodFill(allCell, (IntVec3 x) => x.Filled(map), delegate (IntVec3 x)
					{
						visited[x] = true;
						group.Add(x);
					});
					//	Log.Message("found " + group.Count + " cells for tunnel group");
					Trim(group, map);
					RemoveSmallDisconnectedSubGroups(group, map);
						Log.Message("RemoveSmallDisconnectedSubGroups from group " + group.Count + " cells left");

					if (group.Count >= 300)
					{
						Log.Message("making " + group.Count + " tunnels");
						DoOpenTunnels(group, map);
						DoClosedTunnels(group, map);
						CaveCells.AddRange(group);
					}
				}
			}

			foreach (IntVec3 allCell in cells)
			{
				if (!CaveCells.Contains(allCell) && visited[allCell])
				{
					CaveCells.Add(allCell);
					Log.Message("adding " + allCell + "co CaveCells, now contains " + CaveCells.Count);
				}
			}
		}

		private void Trim(List<IntVec3> group, Map map)
		{
			GenMorphology.Open(group, 6, map);
		}

		private bool IsRock(IntVec3 c, MapGenFloatGrid elevation, Map map)
		{
			if (c.InBounds(map))
			{
				return elevation[c] > 0.7f;
			}
			return false;
		}

		private void DoOpenTunnels(List<IntVec3> group, Map map)
		{
			int a = GenMath.RoundRandom((float)group.Count * Rand.Range(0.9f, 1.1f) * 5.8f / 10000f);
			a = Mathf.Min(a, 3);
			if (a > 0)
			{
				a = Rand.RangeInclusive(1, a);
			}
			float num = TunnelsWidthPerRockCount.Evaluate(group.Count);
			for (int i = 0; i < a; i++)
			{
				IntVec3 start = IntVec3.Invalid;
				float num2 = -1f;
				float dir = -1f;
				float num3 = -1f;
				for (int j = 0; j < 10; j++)
				{
					IntVec3 intVec = FindRandomEdgeCellForTunnel(group, map);
					float distToCave = GetDistToCave(intVec, group, map, 40f, treatOpenSpaceAsCave: false);
					float dist;
					float num4 = FindBestInitialDir(intVec, group, out dist);
					if (!start.IsValid || distToCave > num2 || (distToCave == num2 && dist > num3))
					{
						start = intVec;
						num2 = distToCave;
						dir = num4;
						num3 = dist;
					}
				}
				float width = Rand.Range(num * 0.8f, num);
				Dig(start, dir, width, group, map, closed: false);
			}
		}

		private void DoClosedTunnels(List<IntVec3> group, Map map)
		{
			int a = GenMath.RoundRandom((float)group.Count * Rand.Range(0.9f, 1.1f) * 2.5f / 10000f);
			a = Mathf.Min(a, 1);
			if (a > 0)
			{
				a = Rand.RangeInclusive(0, a);
			}
			float num = TunnelsWidthPerRockCount.Evaluate(group.Count);
			for (int i = 0; i < a; i++)
			{
				IntVec3 start = IntVec3.Invalid;
				float num2 = -1f;
				for (int j = 0; j < 7; j++)
				{
					IntVec3 intVec = group.RandomElement();
					float distToCave = GetDistToCave(intVec, group, map, 30f, treatOpenSpaceAsCave: true);
					if (!start.IsValid || distToCave > num2)
					{
						start = intVec;
						num2 = distToCave;
					}
				}
				float width = Rand.Range(num * 0.8f, num);
				Dig(start, Rand.Range(0f, 360f), width, group, map, closed: true);
			}
		}

		private IntVec3 FindRandomEdgeCellForTunnel(List<IntVec3> group, Map map)
		{
			MapGenFloatGrid caves = MapGenerator.Caves;
			IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
			tmpCells.Clear();
			tmpGroupSet.Clear();
			tmpGroupSet.AddRange(group);
			for (int i = 0; i < group.Count; i++)
			{
				if (group[i].DistanceToEdge(map) < 3 || caves[group[i]] > 0f)
				{
					continue;
				}
				for (int j = 0; j < 4; j++)
				{
					IntVec3 item = group[i] + cardinalDirections[j];
					if (!tmpGroupSet.Contains(item))
					{
						tmpCells.Add(group[i]);
						break;
					}
				}
			}
			if (!tmpCells.Any())
			{
				Log.Warning("Could not find any valid edge cell.");
				return group.RandomElement();
			}
			return tmpCells.RandomElement();
		}

		private float FindBestInitialDir(IntVec3 start, List<IntVec3> group, out float dist)
		{
			float num = GetDistToNonRock(start, group, IntVec3.East, 40);
			float num2 = GetDistToNonRock(start, group, IntVec3.West, 40);
			float num3 = GetDistToNonRock(start, group, IntVec3.South, 40);
			float num4 = GetDistToNonRock(start, group, IntVec3.North, 40);
			float num5 = GetDistToNonRock(start, group, IntVec3.NorthWest, 40);
			float num6 = GetDistToNonRock(start, group, IntVec3.NorthEast, 40);
			float num7 = GetDistToNonRock(start, group, IntVec3.SouthWest, 40);
			float num8 = GetDistToNonRock(start, group, IntVec3.SouthEast, 40);
			dist = Mathf.Max(num, num2, num3, num4, num5, num6, num7, num8);
			return GenMath.MaxByRandomIfEqual(0f, num + num8 / 2f + num6 / 2f, 45f, num8 + num3 / 2f + num / 2f, 90f, num3 + num8 / 2f + num7 / 2f, 135f, num7 + num3 / 2f + num2 / 2f, 180f, num2 + num7 / 2f + num5 / 2f, 225f, num5 + num4 / 2f + num2 / 2f, 270f, num4 + num6 / 2f + num5 / 2f, 315f, num6 + num4 / 2f + num / 2f);
		}

		private void Dig(IntVec3 start, float dir, float width, List<IntVec3> group, Map map, bool closed, HashSet<IntVec3> visited = null)
		{
			Vector3 vect = start.ToVector3Shifted();
			float distcovered = 0f;
			IntVec3 intVec = start;
			float num = 0f;
			MapGenFloatGrid elevation = MapGenerator.Elevation;
			MapGenFloatGrid caves = MapGenerator.Caves;
			bool flag = false;
			bool flag2 = false;
			if (visited == null)
			{
				visited = new HashSet<IntVec3>();
			}
			tmpGroupSet.Clear();
			tmpGroupSet.AddRange(group);
			int num2 = 0;
			while (true)
			{
				if (closed)
				{
					int num3 = GenRadial.NumCellsInRadius(width / 2f + 1.5f);
					for (int i = 0; i < num3; i++)
					{
						IntVec3 intVec2 = intVec + GenRadial.RadialPattern[i];
						if (!visited.Contains(intVec2) && (!tmpGroupSet.Contains(intVec2)))
						{
							return;
						}
					}
				}
				if (num2 >= 15 && width > 1.4f + BranchedTunnelWidthOffset.max)
				{
					if (!flag && Rand.Chance(0.1f))
					{
						DigInBestDirection(intVec, dir, new FloatRange(40f, 90f), width - BranchedTunnelWidthOffset.RandomInRange, group, map, closed, visited);
						flag = true;
					}
					if (!flag2 && Rand.Chance(0.1f))
					{
						DigInBestDirection(intVec, dir, new FloatRange(-90f, -40f), width - BranchedTunnelWidthOffset.RandomInRange, group, map, closed, visited);
						flag2 = true;
					}
				}
				SetCaveAround(intVec, width, map, visited, out bool hitAnotherTunnel);
				if (hitAnotherTunnel)
				{
					Log.Message(intVec + " hitAnotherTunnel");
					break;
				}
				while (vect.ToIntVec3() == intVec)
				{
					vect += Vector3Utility.FromAngleFlat(dir) * 0.5f;
					num += 0.5f;
				}
				
				if (!tmpGroupSet.Contains(vect.ToIntVec3()))
				{
					Log.Message(vect.ToIntVec3() + " not in group");
					break;
				}
				IntVec3 intVec3 = new IntVec3(intVec.x, 0, vect.ToIntVec3().z);
				if (IsRock(intVec3, elevation, map))
				{
					caves[intVec3] = Mathf.Max(caves[intVec3], width);
					visited.Add(intVec3);
				}
				cavecells.Add(intVec);
			//	Log.Message(intVec + " added to cavecells, currently: "+ cavecells.Count);
				intVec = vect.ToIntVec3();
				/*
				Log.Message(intVec + " added to cavecells, currently: " + cavecells.Count);
				Log.Message("Randomize angel Original: "+ dir);
				Log.Message("Randomize angel num: " + num);
				Log.Message("Randomize angel start.x: " + start.x);
				Log.Message("Randomize angel start.z: " + start.z);
				*/
				if (directionNoise == null)
				{
					directionNoise = new Perlin(0.0020500000100582838, 2.0, 0.5, 4, Rand.Int, QualityMode.Medium);
				}
				dir += (float)directionNoise.GetValue(num * 60f, (float)start.x * 200f, (float)start.z * 200f) * 8f;
			//	Log.Message("angel = "+ dir);
				width -= 0.034f;
			//	Log.Message("Tunneling heading: " + dir + ",  current width: " + width);
				if (!(width < 1.4f))
				{
					num2++;
					continue;
				}
				distcovered = start.DistanceTo(intVec);
				break;
			}
			Log.Message("Tunneling from "+ start + " heading: " + dir +" Distance: " + distcovered + " complete");
		}

		private void DigInBestDirection(IntVec3 curIntVec, float curDir, FloatRange dirOffset, float width, List<IntVec3> group, Map map, bool closed, HashSet<IntVec3> visited = null)
		{
			int num = -1;
			float dir = -1f;
			for (int i = 0; i < 6; i++)
			{
				float num2 = curDir + dirOffset.RandomInRange;
				int distToNonRock = GetDistToNonRock(curIntVec, group, num2, 50);
				if (distToNonRock > num)
				{
					num = distToNonRock;
					dir = num2;
				}
			}
			if (num >= 18)
			{
				Dig(curIntVec, dir, width, group, map, closed, visited);
			}
		}

		private void SetCaveAround(IntVec3 around, float tunnelWidth, Map map, HashSet<IntVec3> visited, out bool hitAnotherTunnel)
		{
			hitAnotherTunnel = false;
			int num = GenRadial.NumCellsInRadius(tunnelWidth / 2f);
			MapGenFloatGrid elevation = MapGenerator.Elevation;
			MapGenFloatGrid caves = MapGenerator.Caves;
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = around + GenRadial.RadialPattern[i];
				if (IsRock(intVec, elevation, map))
				{
					if (caves[intVec] > 0f && !visited.Contains(intVec))
					{
						hitAnotherTunnel = true;
					}
					caves[intVec] = Mathf.Max(caves[intVec], tunnelWidth);
					visited.Add(intVec);
				}
				cavecells.Add(intVec);
			}
		}

		private int GetDistToNonRock(IntVec3 from, List<IntVec3> group, IntVec3 offset, int maxDist)
		{
			groupSet.Clear();
			groupSet.AddRange(group);
			for (int i = 0; i <= maxDist; i++)
			{
				IntVec3 item = from + offset * i;
				if (!groupSet.Contains(item))
				{
					return i;
				}
			}
			return maxDist;
		}

		private int GetDistToNonRock(IntVec3 from, List<IntVec3> group, float dir, int maxDist)
		{
			groupSet.Clear();
			groupSet.AddRange(group);
			Vector3 a = Vector3Utility.FromAngleFlat(dir);
			for (int i = 0; i <= maxDist; i++)
			{
				IntVec3 item = (from.ToVector3Shifted() + a * i).ToIntVec3();
				if (!groupSet.Contains(item))
				{
					return i;
				}
			}
			return maxDist;
		}

		private float GetDistToCave(IntVec3 cell, List<IntVec3> group, Map map, float maxDist, bool treatOpenSpaceAsCave)
		{
			MapGenFloatGrid caves = MapGenerator.Caves;
			tmpGroupSet.Clear();
			tmpGroupSet.AddRange(group);
			int num = GenRadial.NumCellsInRadius(maxDist);
			IntVec3[] radialPattern = GenRadial.RadialPattern;
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = cell + radialPattern[i];
				if ((treatOpenSpaceAsCave && !tmpGroupSet.Contains(intVec)) || (intVec.InBounds(map) && caves[intVec] > 0f))
				{
					return cell.DistanceTo(intVec);
				}
			}
			return maxDist;
		}

		private void RemoveSmallDisconnectedSubGroups(List<IntVec3> group, Map map)
		{
			groupSet.Clear();
			groupSet.AddRange(group);
			groupVisited.Clear();
			for (int i = 0; i < group.Count; i++)
			{
				if (groupVisited.Contains(group[i]) || !groupSet.Contains(group[i]))
				{
					continue;
				}
				subGroup.Clear();
				map.floodFiller.FloodFill(group[i], (IntVec3 x) => groupSet.Contains(x), delegate (IntVec3 x)
				{
					subGroup.Add(x);
					groupVisited.Add(x);
				});
				if (subGroup.Count < 300 || (float)subGroup.Count < 0.05f * (float)group.Count)
				{
					for (int j = 0; j < subGroup.Count; j++)
					{
						groupSet.Remove(subGroup[j]);
					}
				}
			}
			group.Clear();
			group.AddRange(groupSet);
		}
		private ModuleBase directionNoise;

		private static HashSet<IntVec3> tmpGroupSet = new HashSet<IntVec3>();

		private static readonly FloatRange BranchedTunnelWidthOffset = new FloatRange(0.2f, 0.4f);

		private static readonly SimpleCurve TunnelsWidthPerRockCount = new SimpleCurve
		{
			new CurvePoint(100f, 2f),
			new CurvePoint(300f, 4f),
			new CurvePoint(3000f, 5.5f)
		};

		private static List<IntVec3> tmpCells = new List<IntVec3>();

		private static HashSet<IntVec3> groupSet = new HashSet<IntVec3>();

		private static HashSet<IntVec3> groupVisited = new HashSet<IntVec3>();

		private static List<IntVec3> subGroup = new List<IntVec3>();

		public int SeedPart => 647814558;
	}
}
