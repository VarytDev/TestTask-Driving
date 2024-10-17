using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BS.Pathfinding
{
    [CustomEditor(typeof(Node))]
    public class NodeInspectorEditor : Editor
    {
        private Node node;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            node = (Node)target;

            if (GUILayout.Button("Connect node"))
            {
                Selection.selectionChanged += ConnectNode;
            }
        }

        private void ConnectNode()
        {
            Selection.selectionChanged -= ConnectNode;

            GameObject activeObject = Selection.activeGameObject;

            if (activeObject == null || node == null)
            {
                return;
            }

            if(!activeObject.TryGetComponent(out Node targetNode) && activeObject.TryGetComponent(out ParkingSpot foundSpot))
            {
                targetNode = foundSpot.AssignedNode;
            }
            else if (targetNode == null)
            {
                return;
            }

            //This is editor script so performance doesn't matter, and i prefer to keep nodes as array so it's lighter
            List<Node> neighbors = node.Neighbors.ToList();
            neighbors.Add(targetNode);
            node.Neighbors = neighbors.ToArray();

            EditorUtility.SetDirty(node);
        }
    }
}
