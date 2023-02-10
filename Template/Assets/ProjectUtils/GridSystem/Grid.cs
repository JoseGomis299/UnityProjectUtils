using System;
using Unity.VisualScripting;
using UnityEngine;

namespace ProjectUtils.GridSystem
{

    public class Grid<TGridObject>
    {
        private int width;
        private int height;
        private float cellSize;
        private TGridObject[,] gridArray;
        private Vector3 originPosition;

        public Grid(int width, int height, float cellSize, Vector3 originPosition, Func<Grid<TGridObject>, int, int, TGridObject> createGridObject)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;

            gridArray = new TGridObject[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    gridArray[x, y] = createGridObject(this, x, y);
                    
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                }
            }

            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);
        }

        private Vector3 GetWorldPosition(int x, int y)
        {
            return new Vector3(x, y) * cellSize + originPosition;
        }

        public void GetXY(Vector3 worldPosition, out int x, out int y)
        {
            x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
            y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
        }

        public void SetValue(int x, int y, TGridObject value)
        {
            if (x >= 0 && y >= 0 && x < width && y < height)
            {
                gridArray[x, y] = value;
            }
        }

        public void SetValue(Vector3 worldPosition, TGridObject value)
        {
            GetXY(worldPosition, out var x, out var y);
            SetValue(x, y, value);
        }

        public TGridObject GetValue(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < width && y < height)
            {
                return gridArray[x, y];
            }
            else
            {
                return default(TGridObject);
            }
        }

        public TGridObject GetValue(Vector3 worldPosition)
        {
            GetXY(worldPosition, out var x, out var y);
            return GetValue(x, y);
        }

        public Vector3 GetCellCenter(int x, int y)
        {
            return GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * 0.5f;
        }

        public Vector3 GetCellCenter(Vector3 worldPosition)
        {
            GetXY(worldPosition, out var x, out var y);
            return GetCellCenter(x, y);
        }

        public int GetWidth()
        {
            return width;
        }

        public int GetHeight()
        {
            return height;
        }

        public float GetCellSize()
        {
            return cellSize;
        }
    }

}
