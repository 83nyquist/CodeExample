using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.Decoration.Components
{
    [CreateAssetMenu(fileName = "TileSet", menuName = "Data/TileSet")]
    public class TileSet : ScriptableObject
    {
        [Serializable]
        public struct TileTypeGroup
        {
            public TileType type;
            public List<GameObject> prefabs;
            public GameObject shroudedPrefab;
        }

        [SerializeField] private List<TileTypeGroup> tileGroups = new List<TileTypeGroup>();

        public GameObject GetTilePrefab(TileType type, int index)
        {
            var group = tileGroups.Find(g => g.type == type);
            if (group.prefabs == null || group.prefabs.Count == 0) return null;

            // Use the specific index provided, clamped to existing variants
            int safeIndex = Mathf.Clamp(index < 0 ? 0 : index, 0, group.prefabs.Count - 1);
            return group.prefabs[safeIndex];
        }

        public GameObject GetShroudedPrefab(TileType type)
        {
            var group = tileGroups.Find(g => g.type == type);
            return group.shroudedPrefab;
        }

        public int GetVariationCount(TileType type)
        {
            var group = tileGroups.Find(g => g.type == type);
            return group.prefabs?.Count ?? 0;
        }
    }
    
    public enum TileType
    {
        PrimaryGround,
        SecondaryGround,
        Water,
        Forest,
        Mountain
    }
}