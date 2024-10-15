using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BS.Pathfinding
{
    public class AStar : MonoBehaviour
    {
        public List<Node> AllNodes = new List<Node>();

        public Node FindClosestNodeToPosition(Vector3 position)
        {
            float closestDistance = float.MaxValue;
            Node closestNode = null;

            foreach(Node node in AllNodes)
            {
                float magnitude = Vector3.SqrMagnitude(node.transform.position - position);

                if (magnitude < closestDistance)
                {
                    closestDistance = magnitude;
                    closestNode = node;
                }
            }

            return closestNode;
        }

#if UNITY_EDITOR
        public void RefreshAllNodes()
        {
            AllNodes.Clear();

            foreach (Node node in FindObjectsOfType<Node>())
            {
                AllNodes.Add(node);
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    } 
}
