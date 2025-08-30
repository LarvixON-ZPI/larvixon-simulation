using UnityEditor;

namespace Drugs.Editor
{
    [CustomEditor(typeof(DrugEffect), true)]
    public class DrugEffectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var drugEffect = (DrugEffect)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Death Probability",
                drugEffect.DeathProbability.ToString("P1"));
        }
    }
}