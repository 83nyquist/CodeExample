using UnityEngine;

namespace Core.Enumerations
{
    public class Directions : MonoBehaviour
    {
        public enum CardinalDirection
        {
            North,
            East,
            South,
            West,
        }
        public enum InterCardinalDirection
        {
            North,
            NorthEast,
            East,
            SouthEast,
            South,
            SouthWest,
            West,
            NorthWest
        }
        
        public enum Axial
        {
            East,      // +1,  0
            NorthEast, // +1, -1
            NorthWest, //  0, -1
            West,      // -1,  0
            SouthWest, // -1, +1
            SouthEast  //  0, +1
        }
    }
}
