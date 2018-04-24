using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField]
    private LayerMask _unwalkableMask;
    [SerializeField]
    Vector2 _gridWorldSize;
    [SerializeField]
    float _nodeRadius;

    [SerializeField]
    private bool _displayGridGizmos;
    [SerializeField]
    private TerrainType[] _walkableRegions;

    [SerializeField] private int _obstacleProxmittPenalty = 10;
    private LayerMask _walkableMask;
    Dictionary<int,int> _walkableRegionDictionary= new Dictionary<int, int>();
    Node[,] _grid;
    private float _nodeDiameter;
    private Coordinate _gridSize;

    private int _penaltyMin = int.MaxValue;
    private int _penaltyMax = int.MinValue;
    void Awake()
    {
        _gridSize = new Coordinate();
        _nodeDiameter = _nodeRadius * 2;
        _gridSize.X = Mathf.RoundToInt(_gridWorldSize.x / _nodeDiameter);
        _gridSize.Y = Mathf.RoundToInt(_gridWorldSize.y / _nodeDiameter);

        foreach (var region in _walkableRegions)
        {
            _walkableMask.value |= region.TerrainMask.value;
            _walkableRegionDictionary.Add(Mathf.RoundToInt(Mathf.Log(region.TerrainMask.value,2)),region.TerrainPenalty);
        }

        CreateGrid();
    }

    public int MaxSize
    {
        get { return _gridSize.X * _gridSize.Y; }
    }
    private void CreateGrid()
    {
        _grid = new Node[_gridSize.X, _gridSize.Y];
        Vector3 worldBottomLeft = transform.position - Vector3.right * _gridWorldSize.x / 2 - Vector3.forward * _gridWorldSize.y / 2;
        for (int x = 0; x < _gridSize.X; x++)
        {
            for (int y = 0; y < _gridSize.Y; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * _nodeDiameter + _nodeRadius) +
                                     Vector3.forward * (y * _nodeDiameter + _nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, _nodeRadius, _unwalkableMask));
                int movementPenalty = 0;
                    Ray ray = new Ray(worldPoint + Vector3.up*50,Vector3.down);
                    RaycastHit hit;
                    if (Physics.Raycast(ray,out hit,100,_walkableMask))
                    {
                        _walkableRegionDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                    }
                if (!walkable)
                {
                    movementPenalty += _obstacleProxmittPenalty;
                }
                _grid[x, y] = new Node(walkable, worldPoint, new Coordinate { X = x, Y = y }, movementPenalty);
            }
        }
        BlurPenaltyMap(3);
    }

    void BlurPenaltyMap(int blurSize)
    {
        int kernelSize = blurSize*2 + 1;
        int kernelExtents = (kernelSize-1)/2;

        int[,] penaltiesHorizontalPass = new int[_gridSize.X,_gridSize.Y];
        int[,] penaltiesVerticalPass = new int[_gridSize.X, _gridSize.Y];

        for (int y = 0; y < _gridSize.Y; y++)
        {
            for (int x = -kernelExtents; x <= kernelExtents; x++)
            {
                int sampleX = Mathf.Clamp(x, 0, kernelExtents);
                penaltiesHorizontalPass[0, y] += _grid[sampleX, y].MovementPenalty;
            }
            for (int x = 1; x < _gridSize.X; x++)
            {
                int removeIndex = Mathf.Clamp( x - kernelExtents - 1,0,_gridSize.X);
                int addIndex = Mathf.Clamp(x + kernelExtents, 0, _gridSize.X - 1);
                penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] -
                                                _grid[removeIndex, y].MovementPenalty +
                                                _grid[addIndex, y].MovementPenalty;
            }
        }

        for (int x = 0; x < _gridSize.X; x++)
        {
            for (int y = -kernelExtents; y <= kernelExtents; y++)
            {
                int sampleY = Mathf.Clamp(y, 0, kernelExtents);
                penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x,sampleY];
            }
            int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
            _grid[x, 0].MovementPenalty = blurredPenalty;
            for (int y = 1; y < _gridSize.Y; y++)
            {
                int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, _gridSize.Y);
                int addIndex = Mathf.Clamp(y + kernelExtents, 0, _gridSize.Y - 1);
                penaltiesVerticalPass[x,y] = penaltiesVerticalPass[x,y - 1] -
                                                penaltiesHorizontalPass[x,removeIndex] +
                                                penaltiesHorizontalPass[x,addIndex];
                blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y]/(kernelSize*kernelSize));
                _grid[x, y].MovementPenalty = blurredPenalty;

                if (blurredPenalty>_penaltyMax)
                {
                    _penaltyMax = blurredPenalty;
                }
                if (blurredPenalty<_penaltyMin)
                {
                    _penaltyMin = blurredPenalty;
                }
            }
        }
    }
    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                int checkX = node.Grid.X + x;
                int checkY = node.Grid.Y + y;
                if (checkX >= 0 && checkX < _gridSize.X && checkY >= 0 && checkY < _gridSize.Y)
                {
                    neighbours.Add(_grid[checkX, checkY]);
                }
            }
        }
        return neighbours;
    }
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + _gridWorldSize.x / 2) / _gridWorldSize.x;
        float percentY = (worldPosition.z + _gridWorldSize.y / 2) / _gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);
        int x = Mathf.RoundToInt((_gridSize.X - 1) * percentX);
        int y = Mathf.RoundToInt((_gridSize.Y - 1) * percentY);
        return _grid[x, y];
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(_gridWorldSize.x, 1, _gridWorldSize.y));
        if (_grid != null && _displayGridGizmos)
        {
            foreach (var node in _grid)
            {
                Gizmos.color = Color.Lerp(Color.white, Color.black,
                    Mathf.InverseLerp(_penaltyMin, _penaltyMax, node.MovementPenalty));
                Gizmos.color = node.Walkable ? Gizmos.color : Color.red;
                Gizmos.DrawCube(node.WorldPosition, Vector3.one * (_nodeDiameter));
            }
        }
    }
    [System.Serializable]
    public class TerrainType
    {
        public LayerMask TerrainMask;
        public int TerrainPenalty;
    }
}

public struct Coordinate
{
    public int X { get; set; }
    public int Y { get; set; }
}
