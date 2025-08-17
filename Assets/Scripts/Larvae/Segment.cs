using System;
using UnityEngine;

namespace Larvae
{
    public class Segment : MonoBehaviour
    {
        [SerializeField] private int segmentIndex;
        [SerializeField] private Larva parentLarva;

        public int SegmentIndex => segmentIndex;
        public Larva ParentLarva => parentLarva;

        public float Width => parentLarva != null ? parentLarva.pointWidths[segmentIndex] : 1f;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            var speed = collision.relativeVelocity.magnitude;
            var contactPoint = collision.GetContact(0).point;

            OnSegmentCollision?.Invoke(SegmentIndex, speed, contactPoint);
        }

        public event Action<int, float, Vector2> OnSegmentCollision;

        public void Initialize(int index, Larva larva)
        {
            segmentIndex = index;
            parentLarva = larva;
        }
    }
}