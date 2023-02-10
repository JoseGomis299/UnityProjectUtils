using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectUtils.GridSystem.PathFinding
{
    public class AStarPathFinding
    {
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;
        
        private Grid<PathNode> grid;
        private List<PathNode> openList;
        private List<PathNode> closedList;

        public static AStarPathFinding Instance { get; private set; }

        public AStarPathFinding(int width, int height)
        {
            Instance = this;
            grid = new Grid<PathNode>(width, height, 10f, Vector3.zero, (Grid<PathNode> g, int x, int y) => new PathNode(g,x,y));

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid.GetValue(x, y).SetNeighbours(GetNeighbourList(grid.GetValue(x, y)));
                }
            }
        }

        public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition)
        {
            grid.GetXY(startWorldPosition, out var startX, out var startY);
            grid.GetXY(endWorldPosition, out var endX, out var endY);
            List<PathNode> path = FindPath(startX, startY, endX, endY);
            if (path == null) return null;
            
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (var pathNode in path)
            {
                vectorPath.Add(new Vector3(pathNode.x, pathNode.y)*grid.GetCellSize()+Vector3.one*grid.GetCellSize()*.5f);
            }
            return vectorPath;
        }
        
        public List<PathNode> FindPath(int startX, int startY, int endX, int endY)
        {
            PathNode startNode = grid.GetValue(startX, startY);
            PathNode endNode = grid.GetValue(endX, endY);

            openList = new List<PathNode> { startNode };
            closedList = new List<PathNode>();

            startNode.gCost = 0;
            startNode.hCost = CalculateDistanceCost(startNode, endNode);
            startNode.CalculateFCost();

            while (openList.Count > 0)
            {
                PathNode currentNode = GetTheLowestFCostNode(openList);
                if (currentNode == endNode)
                {
                    return CalculatePath(endNode);
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                foreach (var neighbourNode in currentNode.neighbours.Where(neighbourNode => !closedList.Contains(neighbourNode)))
                {
                    if (!neighbourNode.isWalkable)
                    {
                        closedList.Add(neighbourNode);
                        continue;
                    }

                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                    if (tentativeGCost < neighbourNode.gCost)
                    {
                        neighbourNode.cameFromNode = currentNode;
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                        neighbourNode.CalculateFCost();

                        if (!openList.Contains(neighbourNode))
                        {
                            openList.Add(neighbourNode);
                        }
                    }
                }
            }

            return null;
        }
        
        private List<PathNode> CalculatePath(PathNode endNode)
        {
            List<PathNode> path = new List<PathNode>();
            path.Add(endNode);
            PathNode currentNode = endNode;
            while (currentNode.cameFromNode != null)
            {
                path.Add(currentNode.cameFromNode);
                currentNode = currentNode.cameFromNode;
            }
            path.Reverse();
            return path;
        }

        private PathNode GetTheLowestFCostNode(List<PathNode> pathNodeList)
        {
            PathNode lowestFCostNode = pathNodeList[0];
            foreach (var node in pathNodeList.Where(node => node.fCost < lowestFCostNode.fCost))
            {
                lowestFCostNode = node;
            }
            return lowestFCostNode;
        }

        private int CalculateDistanceCost(PathNode a, PathNode b)
        {
            int xDistance = Mathf.Abs(a.x - b.x);
            int yDistance = Mathf.Abs(a.y - b.y);
            int remaining = Mathf.Abs(xDistance - yDistance);
            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }
        
        private List<PathNode> GetNeighbourList(PathNode currentNode)
        {
            List<PathNode> neighbours = new List<PathNode>();

            if (currentNode.x - 1 >= 0)
            {
                // Left
                neighbours.Add(grid.GetValue(currentNode.x-1, currentNode.y));
                // Left Down
                if(currentNode.y - 1 >= 0) neighbours.Add(grid.GetValue(currentNode.x-1,currentNode.y-1));
                // Left Up
                if(currentNode.y + 1 < grid.GetHeight()) neighbours.Add(grid.GetValue(currentNode.x-1,currentNode.y+1));
            }
            if (currentNode.x + 1 < grid.GetWidth())
            {
                // Right
                neighbours.Add(grid.GetValue(currentNode.x+1, currentNode.y));
                // Right Down
                if(currentNode.y - 1 >= 0) neighbours.Add(grid.GetValue(currentNode.x+1,currentNode.y-1));
                // Right Up
                if(currentNode.y + 1 < grid.GetHeight()) neighbours.Add(grid.GetValue(currentNode.x+1,currentNode.y+1));
            }
            // Down
            if(currentNode.y - 1 >= 0) neighbours.Add(grid.GetValue(currentNode.x,currentNode.y-1));
            // Up
            if(currentNode.y + 1 < grid.GetHeight()) neighbours.Add(grid.GetValue(currentNode.x,currentNode.y+1));

            return neighbours;
        }
    }
}
