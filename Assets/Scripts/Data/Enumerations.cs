using UnityEngine;

namespace Data
{
    public class Enumerations : MonoBehaviour
    {
        /// <summary>
        /// Categorization for hex tiles.
        /// </summary>
        public enum TileType
        {
            PrimaryGround,
            SecondaryGround,
            Water,
            Forest,
            Mountain
        }
    }
}