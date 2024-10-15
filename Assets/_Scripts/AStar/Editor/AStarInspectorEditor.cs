using UnityEditor;
using UnityEngine;

namespace BS.Pathfinding
{
    [CustomEditor(typeof(AStar))]
    public class AStarInspectorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            AStar aStar = (AStar)target;

            if (GUILayout.Button("Refresh all nodes"))
            {
                aStar.RefreshAllNodes();
            }
        }
    } 
}
