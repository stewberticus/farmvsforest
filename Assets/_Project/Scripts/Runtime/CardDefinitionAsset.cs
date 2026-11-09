using FarmVsForest.Core;
using UnityEngine;

namespace FarmVsForest.Runtime
{
    /// <summary>
    /// A card authored in the Unity editor. Create via Assets > Create > Farm vs Forest > Card.
    /// The rules data lives in <see cref="card"/>; Unity-only things like art live alongside it.
    /// </summary>
    [CreateAssetMenu(menuName = "Farm vs Forest/Card", fileName = "NewCard")]
    public class CardDefinitionAsset : ScriptableObject
    {
        public CardDefinition card = new CardDefinition();

        [Header("Presentation")]
        public Sprite art;

        private void OnValidate()
        {
            if (card == null) card = new CardDefinition();
            if (string.IsNullOrEmpty(card.id)) card.id = name;
            if (string.IsNullOrEmpty(card.displayName)) card.displayName = name;
        }
    }
}
