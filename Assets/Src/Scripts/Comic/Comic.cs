using UnityEngine;
using System.Collections.Generic;

namespace Comic
{
    public static class Comic
    {
        // Layers
        public static readonly string playerLayerName = "Player";
        public static readonly string caseColliderLayerName = "CaseCollider";


        // Sorting Layer. Is it relevant now ?
        public static readonly string frontLayerName = "SwitchPage";
        public static readonly string backLayerName = "NotSwitchPage";
        public static readonly string defaultLayerName = "Default";

        public static int frontLayerId => SortingLayer.NameToID(frontLayerName);
        public static int backLayerId => SortingLayer.NameToID(backLayerName);
        public static int defaultLayerId => SortingLayer.NameToID(defaultLayerName);
    
        public static List<string> characterPath = new()
        {
            "Characters/Jacob",
            "Characters/BestFriend",
            "Characters/Beloved",
            "Characters/Bully",
            "Characters/Boss"
        };

        public enum CharacterType
        {
            Character_Jacob,
            Character_BestFriend,
            Character_Bully,
            Character_Beloved,
            Character_Boss
        }
    }
}
