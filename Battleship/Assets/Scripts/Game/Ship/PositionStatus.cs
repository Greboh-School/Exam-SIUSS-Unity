using UnityEngine;

namespace Assets.Scripts.Game
{
    [System.Serializable]
    public class PositionStatus
    {
        [SerializeField]
        private Vector2 _position;
        [SerializeField]
        private bool _isHit;

        public Vector2 Position { get => _position; set => _position = value; }
        public bool IsHit { get => _isHit; set => _isHit = value; }
    }
}