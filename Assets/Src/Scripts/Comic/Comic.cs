using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

namespace Comic
{
    public static class Comic
    {
        public enum ComicScreenshot
        {
            Screenshot_Page_Left,
            Screenshot_Page_Right,
            Screenshot_Cover_Left,
            Screenshot_Cover_Right,
        }

        // this control where our comic z is (background, pages, panels) 
        public static readonly float COMIC_BASE_Z;
        
        public enum PanelSortingOrder : int
        {
            Panel_Background = 0,
            Panel_Props = 1,
            Panel_Foreground = 2,
            Panel_Outline = 3,
        }

        // Main game mode scene names
        public static readonly string HUD_SCENE_NAME = "HudScene";
        public static readonly string GAME_SCENE_NAME = "GameScene";

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
