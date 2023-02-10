using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUtils.GridSystem.PathFinding
{
    public class PathNode
    {
        private Grid<PathNode> grid;
        public List<PathNode> neighbours { get; private set; }
        public int x { get; private set; }
        public int y { get; private set; }

        public int gCost;
        public int hCost;
        public int fCost;

        public bool isWalkable;
        public PathNode cameFromNode;
        
        public PathNode(Grid<PathNode> grid, int x, int y)
        {
            this.grid = grid;
            this.x = x;
            this.y = y;
            isWalkable = true;
            
            gCost = int.MaxValue;
            CalculateFCost();
            cameFromNode = null;
        }

        public void SetNeighbours(List<PathNode> neighbours)
        {
            this.neighbours = neighbours;
        }

        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }
    }
}
