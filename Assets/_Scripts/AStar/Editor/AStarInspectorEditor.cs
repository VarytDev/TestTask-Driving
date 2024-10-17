using UnityEditor;
using UnityEngine;

namespace BS.Pathfinding
{
    [CustomEditor(typeof(AStarNavigation))]
    public class AStarInspectorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            AStarNavigation aStar = (AStarNavigation)target;

            if (GUILayout.Button("Refresh all nodes"))
            {
                aStar.RefreshAllNodes();
            }
        }
    } 
}
