using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    public class CellData
    {
        public bool Passable;
        public CellObject ContainedObject;
    }

    [Header("Configurações Base (Nível 1)")]
    public int BaseWidth = 8;
    public int BaseHeight = 8;

    [HideInInspector] public int Width;
    [HideInInspector] public int Height;

    public Tile[] GroundTiles;
    public Tile[] WallTiles;

    [Header("Cell Objects Settings")]
    public WallObject[] WallPrefabs;
    public FoodObject[] FoodPrefabs;
    public StatBoostObject[] StatBoostPrefabs; // Array para os novos itens de atributos
    public ExitCellObject ExitCellPrefab;
    public GameObject[] EnemyPrefabs;

    public int MinFoodCount = 2;
    public int MaxFoodCount = 6;

    private CellData[,] m_BoardData;
    private Tilemap m_Tilemap;
    private Grid m_Grid;
    private List<Vector2Int> m_EmptyCellsList;

    public void Init()
    {
        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_Grid = GetComponentInChildren<Grid>();

        int currentLevel = 1;
        if (GameManager.Instance != null)
        {
            currentLevel = GameManager.Instance.CurrentLevel;
        }

        // CORRIGIDO: Cálculo correto do tamanho com base no nível atual
        Width = BaseWidth + ((currentLevel - 1) * 2);
        Height = BaseHeight + ((currentLevel - 1) * 2);

        m_EmptyCellsList = new List<Vector2Int>();
        m_BoardData = new CellData[Width, Height];

        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                Tile tile;
                m_BoardData[x, y] = new CellData();

                if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                {
                    tile = WallTiles[Random.Range(0, WallTiles.Length)];
                    m_BoardData[x, y].Passable = false;
                }
                else
                {
                    tile = GroundTiles[Random.Range(0, GroundTiles.Length)];
                    m_BoardData[x, y].Passable = true;
                    m_EmptyCellsList.Add(new Vector2Int(x, y));
                }

                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        m_EmptyCellsList.Remove(new Vector2Int(1, 1));

        Vector2Int endCoord = new Vector2Int(Width - 2, Height - 2);
        AddObject(Instantiate(ExitCellPrefab), endCoord);
        m_EmptyCellsList.Remove(endCoord);

        GenerateWall(currentLevel);
        GenerateFood();
        GenerateStatBoosts(); // Gera os novos itens/poções de atributos
        GenerateEnemy(currentLevel);
    }

    public void Clean()
    {
        if (m_BoardData == null)
            return;

        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                var cellData = m_BoardData[x, y];
                if (cellData != null && cellData.ContainedObject != null)
                {
                    Destroy(cellData.ContainedObject.gameObject);
                }
                SetCellTile(new Vector2Int(x, y), null);
            }
        }
        m_Tilemap.ClearAllTiles();
    }

    void AddObject(CellObject obj, Vector2Int coord)
    {
        CellData data = m_BoardData[coord.x, coord.y];
        obj.transform.position = CellToWorld(coord);
        data.ContainedObject = obj;
        obj.Init(coord);
    }

    void GenerateWall(int level)
    {
        if (WallPrefabs == null || WallPrefabs.Length == 0) return;

        // CORRIGIDO: Aumento de paredes proporcional ao nível
        int wallCount = Random.Range(6, 10) + (level - 1);
        for (int i = 0; i < wallCount; ++i)
        {
            if (m_EmptyCellsList.Count == 0) break;

            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);

            WallObject selectedPrefab = WallPrefabs[Random.Range(0, WallPrefabs.Length)];
            WallObject newWall = Instantiate(selectedPrefab);
            AddObject(newWall, coord);
        }
    }

    void GenerateFood()
    {
        if (FoodPrefabs == null || FoodPrefabs.Length == 0) return;

        int currentLevel = 1;
        if (GameManager.Instance != null)
        {
            currentLevel = GameManager.Instance.CurrentLevel;
        }

        // Aumenta a quantidade de comida gerada conforme o nível da sala avança
        int minFood = MinFoodCount + (currentLevel - 1);
        int maxFood = MaxFoodCount + (currentLevel - 1);
        int foodCount = Random.Range(minFood, maxFood + 1);

        for (int i = 0; i < foodCount; ++i)
        {
            if (m_EmptyCellsList.Count == 0) break;

            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);

            FoodObject selectedPrefab = FoodPrefabs[Random.Range(0, FoodPrefabs.Length)];
            FoodObject newFood = Instantiate(selectedPrefab);
            AddObject(newFood, coord);
        }
    }

    void GenerateStatBoosts()
    {
        if (StatBoostPrefabs == null || StatBoostPrefabs.Length == 0) return;

        int currentLevel = 1;
        if (GameManager.Instance != null)
        {
            currentLevel = GameManager.Instance.CurrentLevel;
        }

        // Aumenta a quantidade de poções/itens especiais gerados nas salas mais altas
        int boostCount = Random.Range(1, 3) + ((currentLevel - 1) / 2); // Adiciona mais poções a cada 2 níveis

        for (int i = 0; i < boostCount; ++i)
        {
            if (m_EmptyCellsList.Count == 0) break;

            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);

            StatBoostObject selectedPrefab = StatBoostPrefabs[Random.Range(0, StatBoostPrefabs.Length)];
            StatBoostObject newBoost = Instantiate(selectedPrefab);
            AddObject(newBoost, coord);
        }
    }

    void GenerateEnemy(int level)
    {
        if (EnemyPrefabs == null || EnemyPrefabs.Length == 0)
        {
            Debug.LogWarning("Nenhum EnemyPrefab foi atribuído no BoardManager!");
            return;
        }

        // CORRIGIDO: Aumento de inimigos proporcional ao nível
        int enemyCount = 1 + (level - 1);

        for (int i = 0; i < enemyCount; ++i)
        {
            if (m_EmptyCellsList.Count == 0) break;

            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);

            var enemyInstance = Instantiate(EnemyPrefabs[Random.Range(0, EnemyPrefabs.Length)]);
            var enemy = enemyInstance.GetComponent<Enemy>();

            AddObject(enemy, coord);
        }
    }

    public void SetCellTile(Vector2Int cellIndex, Tile tile)
    {
        m_Tilemap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
    }

    public Tile GetCellTile(Vector2Int cellIndex)
    {
        return m_Tilemap.GetTile<Tile>(new Vector3Int(cellIndex.x, cellIndex.y, 0));
    }

    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return m_Grid.GetCellCenterWorld(new Vector3Int(cellIndex.x, cellIndex.y, 0));
    }

    public CellData GetCellData(Vector2Int cellIndex)
    {
        if (cellIndex.x < 0 || cellIndex.x >= Width || cellIndex.y < 0 || cellIndex.y >= Height)
        {
            return null;
        }

        return m_BoardData[cellIndex.x, cellIndex.y];
    }
}