using System.Collections.Generic;
using UnityEngine;

namespace BS.Pathfinding
{
    public static class AStarPathfinding
    {
        public static float GetPathLength(List<Node> path)
        {
            return GetPathLength(GetPathCoordinates(path));
        }

        public static float GetPathLength(Vector3[] coordinates)
        {
            float totalLength = 0f;

            for (int i = 0; i < coordinates.Length; i++)
            {
                if(i + 1 >=  coordinates.Length)
                {
                    break;
                }

                totalLength += Vector3.Distance(coordinates[i + 1], coordinates[i]);
            }

            return totalLength;
        }

        public static Vector3[] GetPathCoordinates(Node startNode, Node endNode)
        {
            return GetPathCoordinates(FindPath(startNode, endNode));
        }

        public static Vector3[] GetPathCoordinates(List<Node> path)
        {
            Vector3[] coordinates = new Vector3[path.Count];

            for (int i = 0; i < path.Count; i++)
            {
                coordinates[i] = path[i].transform.position;
            }

            return coordinates;
        }

        public static List<Node> FindPath(Node startNode, Node endNode)
        {
            List<Node> availableNodes = new List<Node>();
            List<Node> visitedNodes = new List<Node>();

            Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
            Dictionary<Node, float> gScore = new Dictionary<Node, float>();
            Dictionary<Node, float> fScore = new Dictionary<Node, float>();

            availableNodes.Add(startNode);
            gScore[startNode] = 0;
            fScore[startNode] = GetHeuristic(startNode, endNode);

            while (availableNodes.Count > 0)
            {
                Node currentNode = GetNodeWithLowestFScore(availableNodes, fScore);

                if (currentNode == endNode)
                {
                    return ReconstructPath(cameFrom, currentNode);
                }

                availableNodes.Remove(currentNode);
                visitedNodes.Add(currentNode);

                foreach (Node neighbor in currentNode.Neighbors)
                {
                    if (visitedNodes.Contains(neighbor)) continue;

                    float tentativeGScore = gScore[currentNode] + currentNode.CalculateCost(neighbor);

                    if (!visitedNodes.Contains(neighbor)) availableNodes.Add(neighbor);

                    if (tentativeGScore >= gScore.GetValueOrDefault(neighbor, float.MaxValue)) continue;

                    cameFrom[neighbor] = currentNode;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + GetHeuristic(neighbor, endNode);
                }
            }

            return null;
        }

        private static float GetHeuristic(Node a, Node b)
        {
            return Vector3.Distance(a.transform.position, b.transform.position);
        }

        private static Node GetNodeWithLowestFScore(List<Node> availableNodes, Dictionary<Node, float> fScore)
        {
            Node lowest = null;

            foreach (Node node in availableNodes)
            {
                if (lowest == null || fScore[node] < fScore[lowest])
                {
                    lowest = node;
                }
            }

            return lowest;
        }

        private static List<Node> ReconstructPath(Dictionary<Node, Node> cameFrom, Node currentNode)
        {
            List<Node> totalPath = new List<Node>();
            totalPath.Add(currentNode);

            while (cameFrom.ContainsKey(currentNode))
            {
                currentNode = cameFrom[currentNode];
                totalPath.Insert(0, currentNode);
            }

            return totalPath;
        }
    } 
}
