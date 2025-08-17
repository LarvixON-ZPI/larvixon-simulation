using System;
using UnityEngine;

namespace Larvae
{
    public class Segment : MonoBehaviour
    {
        [SerializeField] private int segmentIndex;

        [field: SerializeField]
        public float Width { get; private set; }

        [field: SerializeField]
        public Rigidbody2D Rigidbody { get; private set; }

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
            GetComponent<Collider2D>();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            var speed = collision.relativeVelocity.magnitude;
            var contactPoint = collision.GetContact(0).point;

            OnSegmentCollision?.Invoke(segmentIndex, speed, contactPoint);
        }

        public event Action<int, float, Vector2> OnSegmentCollision;

        public void Initialize(int index, float width)
        {
            segmentIndex = index;
            Width = width;
        }
    }
}