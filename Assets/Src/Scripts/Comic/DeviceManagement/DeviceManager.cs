using System;
using CustomArchitecture;
using UnityEngine.InputSystem;
using static CustomArchitecture.CustomArchitecture;

namespace Comic
{
    // make it CustomArchitecture ?
    public class DeviceManager : ADeviceManager
    {
        #region SUB CALLBACKS
        public void SubscribeToDeviceChanged(Action<ControllerType> function)
        {
            onDeviceChanged -= function;
            onDeviceChanged += function;
        }

        #endregion SUB CALLBACKS

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void LateInit(params object[] parameters)
        { }
        public override void Init(params object[] parameters)
        {
            base.Init(parameters);

            InitStartingDevice();
        }
        #endregion

        #region INIT
        private void InitStartingDevice()
        {
            bool gamepadConnected = false;

            foreach (var device in InputSystem.devices)
            {
                //Debug.Log("Connected Device: " + device.name);

                if (device is Gamepad)
                {
                    gamepadConnected = true;
                    SelectGamepad();
                }
            }

            if (!gamepadConnected)
            {
                SelectKeyboard();
            }
        }
        #endregion INIT

        protected override void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            base.OnDeviceChange(device, change);

            switch (change)
            {
                case InputDeviceChange.Added:
                    //Debug.Log($"Device connected: {device.name}");
                    if (device is Gamepad)
                    {
                        SelectGamepad();
                    }
                    break;
                case InputDeviceChange.Removed:
                    //Debug.Log($"Device disconnected: {device.name}");
                    if (device is Gamepad)
                    {
                        SelectKeyboard();
                    }
                    break;
            }
        }
    }
}
