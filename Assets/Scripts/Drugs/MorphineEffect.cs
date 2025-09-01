using UnityEngine;

namespace Drugs
{
    /*
       Morphine induces a state of stupor. The larva's movements are extremely slow, deliberate, and intermittent. It often stops moving entirely for long periods and may adopt a characteristic curled posture.

       Behavioral Changes:

       Intermittent Motion: The larva will move for a few cycles and then pause for a variable amount of time. The isMoving state should be toggled on and off randomly.

       Slow, Drifting Movement: When it does move, it is very slow and smooth.

       Curling: The larva has a tendency to curl up on itself, especially during its long pauses.

       Lethality & Confusion:

       Deadliness: A high overdose is lethal, simulating respiratory failure. Death occurs after 5-7 minutes, often while the larva is in a motionless state.

       Confusion: Low. The larva is not confused, but rather unresponsive. It will drift into walls and stay there, not because of erratic direction changes, but due to a complete lack of response to its environment.
    */
    [CreateAssetMenu(fileName = "MorphineEffect", menuName = "Drugs/Morphine Effect")]
    public class MorphineEffect : DrugEffect
    {
    }
}