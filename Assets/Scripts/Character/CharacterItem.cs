using UnityEngine;

namespace Character
{
    [CreateAssetMenu(fileName = "CharacterItem", menuName = "Data/CharacterItem")]
    public class CharacterItem : ScriptableObject
    {
        public GameObject gamePrefab;
        public Sprite profileImage;
    }
}
