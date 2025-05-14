using System.Collections.Generic;
using CustomArchitecture;
using UnityEngine;
using static PageHole;

namespace Comic
{
    public class PowerManager : BaseBehaviour
    {
        [SerializeField] private List<Power> m_allPowers;

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        { }
        public override void Init(params object[] parameters)
        {
            // should be done in late init
            ComicGameCore.Instance.MainGameMode.SubscribeToUnlockPower(OnUnlockPower);
            ComicGameCore.Instance.MainGameMode.SubscribeToLockPower(OnLockPower);

            foreach (var data in ComicGameCore.Instance.MainGameMode.GetUnlockChaptersData())
            {
                if (data.m_hasUnlockPower)
                {
                    PowerType powerType = ComicGameCore.Instance.MainGameMode.GetGameConfig().GetPowerByChapter(data.m_chapterType);
                    OnUnlockPower(powerType);
                }
            }
        }
        #endregion

        private void OnUnlockPower(PowerType powerType)
        {
            foreach (Power pow in m_allPowers)
            {
                if (pow.GetPowerType() == powerType)
                {
//                    ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetCurrentCharacter().AddPower(pow);
                }
            }
        }

        private void OnLockPower(PowerType powerType)
        {
            foreach (Power pow in m_allPowers)
            {
                if (pow.GetPowerType() == powerType)
                {
//                    ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetCurrentCharacter().RemovePower(pow);
                }
            }
        }
    }
}
