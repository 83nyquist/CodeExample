using UnityEngine;

namespace Systems.Decoration
{
    [CreateAssetMenu(fileName = "TileSet", menuName = "World Generation/TileSet")]
    public class TileSet : ScriptableObject
    {
        [Header("Tile Prefabs")]
        public GameObject groundTile;
        public GameObject waterTile;
        public GameObject forestTile;
        public GameObject fieldTile;
        public GameObject mountainTile;
        
        [Header("Tile Visual Variations")]
        public GameObject[] groundVariations;
        public GameObject[] waterVariations;
        public GameObject[] forestVariations;
        public GameObject[] fieldVariations;
        public GameObject[] mountainVariations;
        
        public GameObject GetTilePrefab(TileType type, int variationIndex = -1)
        {
            GameObject[] variations = GetVariationsForType(type);
            
            if (variations != null && variations.Length > 0 && variationIndex >= 0 && variationIndex < variations.Length)
            {
                return variations[variationIndex];
            }
            
            // Fallback to default tile
            return GetDefaultTileForType(type);
        }
        
        public int GetVariationCount(TileType type)
        {
            GameObject[] variations = GetVariationsForType(type);
            return variations?.Length ?? 0;
        }
        
        public bool HasVariations(TileType type)
        {
            return GetVariationCount(type) > 0;
        }
        
        private GameObject[] GetVariationsForType(TileType type)
        {
            return type switch
            {
                TileType.Ground => groundVariations,
                TileType.Water => waterVariations,
                TileType.Forest => forestVariations,
                TileType.Field => fieldVariations,
                TileType.Mountain => mountainVariations,
                _ => null
            };
        }
        
        private GameObject GetDefaultTileForType(TileType type)
        {
            return type switch
            {
                TileType.Ground => groundTile,
                TileType.Water => waterTile,
                TileType.Forest => forestTile,
                TileType.Field => fieldTile,
                TileType.Mountain => mountainTile,
                _ => groundTile
            };
        }
        
        // Helper method to validate tile assignments
        public bool IsValid()
        {
            // Check if all default tiles are assigned
            if (groundTile == null) return false;
            if (waterTile == null) return false;
            if (forestTile == null) return false;
            if (fieldTile == null) return false;
            if (mountainTile == null) return false;
            
            return true;
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Validate TileSet")]
        private void ValidateTileSet()
        {
            if (IsValid())
            {
                Debug.Log($"[TileSet] {name} is valid. All default tiles assigned.");
            }
            else
            {
                Debug.LogWarning($"[TileSet] {name} is missing some default tile assignments!");
            }
            
            // Log variation counts
            Debug.Log($"[TileSet] Variations - Ground: {GetVariationCount(TileType.Ground)}, " +
                     $"Water: {GetVariationCount(TileType.Water)}, " +
                     $"Forest: {GetVariationCount(TileType.Forest)}, " +
                     $"Field: {GetVariationCount(TileType.Field)}, " +
                     $"Mountain: {GetVariationCount(TileType.Mountain)}");
        }
        #endif
    }
    
    public enum TileType
    {
        Ground,
        Water,
        Forest,
        Field,
        Mountain
    }
}