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

        public void Initialize(int index, Larva larva)
        {
            segmentIndex = index;
            parentLarva = larva;
        }
    }
}