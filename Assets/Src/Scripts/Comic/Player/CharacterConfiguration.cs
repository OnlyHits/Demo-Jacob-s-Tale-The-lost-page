using UnityEngine;

namespace Comic
{
    public class CharacterConfiguration
    {
        public float speed;
        public float jumpSpeed;
    }

    public interface CharacterPhysicsProvider
    {
        public void StartMove(Vector2 v);
        public void Move(Vector2 v);
        public void StopMove(Vector2 v);
        public void TryJump();
    }
}