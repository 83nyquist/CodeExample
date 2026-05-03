using Character;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface.UGUI
{
    public class CharacterProfile : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI profileNameText;
        [SerializeField] private Image profileImage;

        public void SetCharacter(CharacterItem characterItem)
        {
            profileNameText.text = characterItem.name;
            profileImage.sprite = characterItem.profileImage;
        }
    }
}
