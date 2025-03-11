using System;
using UnityEngine;

namespace CustomArchitecture
{
    // todo : what the use case ?
    public class EventOnCollide2D : BaseBehaviour
    {
        public Action<Collision2D> onCollisionEnter;
        public Action<Collision2D> onCollisionStay;
        public Action<Collision2D> onCollisionExit;

        public Action<Collider2D> onTriggerEnter;
        public Action<Collider2D> onTriggerStay;
        public Action<Collider2D> onTriggerExit;

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
        { }
        #endregion

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision?.gameObject == null)
            {
                return;
            }
            onCollisionEnter?.Invoke(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision?.gameObject == null)
            {
                return;
            }
            onCollisionStay?.Invoke(collision);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision?.gameObject == null)
            {
                return;
            }
            onCollisionEnter?.Invoke(collision);
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider?.gameObject == null)
            {
                return;
            }
            onTriggerEnter?.Invoke(collider);
        }

        private void OnTriggerStay2D(Collider2D collider)
        {
            if (collider?.gameObject == null)
            {
                return;
            }
            onTriggerStay?.Invoke(collider);
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (collider?.gameObject == null)
            {
                return;
            }
            onTriggerExit?.Invoke(collider);
        }
    }
}
