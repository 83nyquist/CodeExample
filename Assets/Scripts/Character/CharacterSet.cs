using System.Collections.Generic;
using UnityEngine;

namespace Character
{
    [CreateAssetMenu(fileName = "CharacterSet", menuName = "Data/CharacterSet")]
    public class CharacterSet : ScriptableObject
    {
        public List<CharacterItem> characters;
    }
}
