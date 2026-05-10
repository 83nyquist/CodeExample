using System;
using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Systems.Decoration.Components
{
    [CreateAssetMenu(fileName = "TileSet", menuName = "Data/TileSet")]
    public class TileSet : ScriptableObject
    {
        [Serializable]
        public struct TileTypeGroup
        {
            public Enumerations.TileType type;
            public List<GameObject> prefabs;
            public GameObject shroudedPrefab;
        }

        [SerializeField] private List<TileTypeGroup> tileGroups = new List<TileTypeGroup>();

        /// <summary>
        /// Retrieves a specific prefab variant for a tile type based on the provided index.
        /// </summary>
        public GameObject GetTilePrefab(Enumerations.TileType type, int index)
        {
            var group = tileGroups.Find(g => g.type == type);
            if (group.prefabs == null || group.prefabs.Count == 0) return null;

            int safeIndex = Mathf.Clamp(index < 0 ? 0 : index, 0, group.prefabs.Count - 1);
            return group.prefabs[safeIndex];
        }

        /// <summary>
        /// Retrieves the shrouded visual representation for a tile type.
        /// </summary>
        public GameObject GetShroudedPrefab(Enumerations.TileType type)
        {
            var group = tileGroups.Find(g => g.type == type);
            return group.shroudedPrefab;
        }

        /// <summary>
        /// Returns the number of available prefab variants for a given tile type.
        /// </summary>
        public int GetVariationCount(Enumerations.TileType type)
        {
            var group = tileGroups.Find(g => g.type == type);
            return group.prefabs?.Count ?? 0;
        }
    }
}