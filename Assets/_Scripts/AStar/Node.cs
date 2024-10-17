using System;
using UnityEngine;

namespace BS.Pathfinding
{
	public class Node : MonoBehaviour
	{
		public Action OnRequestPlayer = default;
		public Node[] Neighbors = new Node[0];

		public float CalculateCost(Node targetNode)
		{
			return Vector3.Distance(transform.position, targetNode.transform.position);
		}

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
			Gizmos.DrawSphere(transform.position, 0.2f);

            Gizmos.color = Color.blue;

            foreach (var neighbor in Neighbors)
			{
				if(neighbor == null) 
				{
					continue;
				}

				Vector3 direction = (neighbor.transform.position - transform.position).normalized;

				//offset the line to right side of the road
				float offsetX = Mathf.Abs(direction.y) > float.Epsilon ? direction.y > 0 ? -0.2f : 0.2f : 0;
				float offsetY = Mathf.Abs(direction.x) > float.Epsilon ? direction.x > 0 ? -0.2f : 0.2f : 0;
				Vector3 offset = new Vector3(offsetX, offsetY, 0);

				Gizmos.DrawLine(transform.position + offset, neighbor.transform.position + offset);
			}
        }
    }
}
