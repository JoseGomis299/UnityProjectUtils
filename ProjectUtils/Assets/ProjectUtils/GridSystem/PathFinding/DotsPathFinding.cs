using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectUtils.GridSystem.PathFinding
{
    public class DotsPathFinding : MonoBehaviour
    {
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;

        public static DotsPathFinding Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void FindPath(int2 startPosition, int2 endPosition)
        {
            FindPathJob findPathJob = new FindPathJob
            {
                startPosition = startPosition,
                endPosition = endPosition
            };

            JobHandle jobHandle = findPathJob.Schedule();
            jobHandle.Complete();
        }

        [BurstCompatible]
        private struct FindPathJob : IJob
        {
            public int2 startPosition;
            public int2 endPosition;

            public void Execute()
            {
                int2 gridSize = new int2(4, 4);

                NativeArray<PathNode> pathNodeArray =
                    new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.Temp);

                //Initialize Grid
                for (int x = 0; x < gridSize.x; x++)
                {
                    for (int y = 0; y < gridSize.y; y++)
                    {
                        PathNode pathNode = new PathNode();
                        pathNode.x = x;
                        pathNode.y = y;
                        pathNode.index = CalculateIndex(x, y, gridSize.x);

                        pathNode.gCost = int.MaxValue;
                        pathNode.hCost = CalculateDistanceCost(new int2(x, y), endPosition);
                        pathNode.CalculateFCost();

                        pathNode.isWalkable = true;
                        pathNode.cameFromNodeIndex = -1;

                        pathNodeArray[pathNode.index] = pathNode;
                    }
                }

                NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(8, Allocator.Temp);

                neighbourOffsetArray[0] = new int2(-1, 0); //Left
                neighbourOffsetArray[1] = new int2(+1, 0); //Right
                neighbourOffsetArray[2] = new int2(-1, +1); //Left Up
                neighbourOffsetArray[3] = new int2(-1, -1); //Left Down
                neighbourOffsetArray[4] = new int2(+1, +1); //Right Up
                neighbourOffsetArray[5] = new int2(+1, -1); //Right Down
                neighbourOffsetArray[6] = new int2(0, -1); //Down
                neighbourOffsetArray[7] = new int2(0, -1); //Up
                
                //Refresh startNode costs
                int endNodeIndex = CalculateIndex(endPosition.x, endPosition.y, gridSize.x);
                PathNode startNode = pathNodeArray[CalculateIndex(startPosition.x, startPosition.y, gridSize.x)];
                startNode.gCost = 0;
                startNode.CalculateFCost();
                pathNodeArray[startNode.index] = startNode;

                NativeList<int> openList = new NativeList<int>(Allocator.Temp);
                NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

                openList.Add(startNode.index);

                while (openList.Length > 0)
                {
                    int currentNodeIndex = GetTheLowestFCostNodeIndex(openList, pathNodeArray);
                    PathNode currentNode = pathNodeArray[currentNodeIndex];

                    if (currentNodeIndex == endNodeIndex)
                    {
                        break;
                    }

                    //Remove current node from openList
                    for (int i = 0; i < openList.Length; i++)
                    {
                        if (openList[i] == currentNodeIndex)
                        {
                            openList.RemoveAtSwapBack(i);
                            break;
                        }
                    }

                    closedList.Add(currentNodeIndex);

                    for (int i = 0; i < neighbourOffsetArray.Length; i++)
                    {
                        int2 neighbourOffset = neighbourOffsetArray[i];
                        int2 neighbourPosition = new int2(currentNode.x + neighbourOffset.x,
                            currentNode.y + neighbourOffset.y);

                        //Neighbour not valid position
                        if (!IsPositionInsideGrid(neighbourPosition, gridSize)) continue;

                        int neighbourNodeIndex = CalculateIndex(neighbourPosition.x, neighbourPosition.y, gridSize.x);

                        //Already searched this node
                        if (closedList.Contains(neighbourNodeIndex)) continue;

                        PathNode neighbourNode = pathNodeArray[neighbourNodeIndex];
                        //Not walkable
                        if (!neighbourNode.isWalkable) continue;

                        int2 currentNodePosition = new int2(currentNode.x, currentNode.y);

                        int tentativeGCost = currentNode.gCost +
                                             CalculateDistanceCost(currentNodePosition, neighbourPosition);
                        if (tentativeGCost < neighbourNode.gCost)
                        {
                            //Refresh neighbour costs
                            neighbourNode.cameFromNodeIndex = currentNodeIndex;
                            neighbourNode.gCost = tentativeGCost;
                            neighbourNode.CalculateFCost();
                            pathNodeArray[neighbourNodeIndex] = neighbourNode;
                            
                            if (!openList.Contains(neighbourNode.index))
                            {
                                openList.Add(neighbourNode.index);
                            }
                        }

                    }
                }

                PathNode endNode = pathNodeArray[endNodeIndex];
                NativeList<int2> path = CalculatePath(pathNodeArray, endNode);
                path.Dispose();

                openList.Dispose();
                neighbourOffsetArray.Dispose();
                closedList.Dispose();
                pathNodeArray.Dispose();

            }
        }

        private static NativeList<int2> CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode)
        {
            if (endNode.cameFromNodeIndex == -1)
            {
                //Didn't found a path
                return new NativeList<int2>(Allocator.Temp);
            }
            
            //Found a path
            // CalculatePath(pathNodeArray, endNode);
            NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
            path.Add(new int2(endNode.x, endNode.y));

            PathNode currentNode = endNode;
            while (currentNode.cameFromNodeIndex != -1)
            {
                PathNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                path.Add(new int2(cameFromNode.x, cameFromNode.y));
                currentNode = cameFromNode;
            }

            return path;
            
        }

        private static bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize)
        {
            return
                gridPosition.x >= 0 &&
                gridPosition.y >= 0 &&
                gridPosition.x < gridSize.x &&
                gridPosition.y < gridSize.y;
        }

        private static int CalculateIndex(int x, int y, int gridWidth)
        {
            return x + y * gridWidth;
        }

        private static int GetTheLowestFCostNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray)
        {
            PathNode lowestFCostNode = pathNodeArray[openList[0]];
            for (int i = 1; i < openList.Length; i++)
            {
                PathNode testPathNode = pathNodeArray[openList[i]];
                if (testPathNode.fCost < lowestFCostNode.fCost) lowestFCostNode = testPathNode;
            }

            return lowestFCostNode.index;
        }

        private static int CalculateDistanceCost(int2 a, int2 b)
        {
            int xDistance = Mathf.Abs(a.x - b.x);
            int yDistance = Mathf.Abs(a.y - b.y);
            int remaining = Mathf.Abs(xDistance - yDistance);
            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        private struct PathNode
        {
            public int x;
            public int y;

            public int index;

            public int gCost;
            public int hCost;
            public int fCost;

            public bool isWalkable;

            public int cameFromNodeIndex;

            public void CalculateFCost()
            {
                fCost = gCost + hCost;
            }

            public void SetWalkable(bool value)
            {
                isWalkable = value;
            }
        }
    }
}
