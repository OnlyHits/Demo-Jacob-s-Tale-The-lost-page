using System.Collections.Generic;
using CustomArchitecture;
using UnityEngine;
using System.Collections;
using static Comic.Comic;
using System.Linq;
using Unity.VisualScripting;

namespace Comic
{
    public enum VfxName
    {
        Vfx_FootStep,
    }

    // 1) make a editor that list all addressable vfx with a system of label
    // 2) make the vfx loadable with a boolean in the editor
    // 3) make configurations to load a set of vfx

    public class VfxManager : BaseBehaviour
    {
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
        }
        #endregion
    }
}