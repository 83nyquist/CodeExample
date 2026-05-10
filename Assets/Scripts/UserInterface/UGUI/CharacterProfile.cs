using Character;
using Systems.EventBus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface.UGUI
{
    public class CharacterProfile : EventBusSubscriber
    {
        [SerializeField] private TextMeshProUGUI profileNameText;
        [SerializeField] private Image profileImage;
        
        /// <summary>
        /// Populates the UI elements with character data.
        /// </summary>
        public void SetCharacter(CharacterItem characterItem)
        {
            profileNameText.text = characterItem.name;
            profileImage.sprite = characterItem.profileImage;
        }
    }
}
