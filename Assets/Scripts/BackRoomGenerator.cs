using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BackRoomGenerator : MonoBehaviour
{
    [SerializeField] private BackRoomCell backRoomCellPrefab; // BackRoom房間單元的預製體
    [SerializeField] private GameObject initialRoom;
    [SerializeField] private GameObject spawnPointEffect;
    [SerializeField] private GameObject exitPointEffect;
    [SerializeField] private GameObject exitTrigger;
    
    [Header("Back Room 設置")]
    [SerializeField] private int backRoomWidth;  // 迷宮寬度（X軸方向的房間數量）
    [SerializeField] private int backRoomDepth;  // 迷宮深度（Z軸方向的房間數量）
    [SerializeField] private int backRoomScale;
    
    [Header("特殊區域設置")]
    [SerializeField, Range(0f, 1f)] public float brightness = 0.5f;  // 亮度
    [SerializeField, Range(0f, 1f)] public float bigRoomProbability = 0.05f;  // 大房間出現概率
    [SerializeField] public int bigRoomMinSize = 3;                // 大房間最小尺寸
    [SerializeField] public int bigRoomMaxSize = 5;                // 大房間最大尺寸
    [SerializeField, Range(0f, 1f)] public float openSpaceProbability = 0.08f; // 空地出現概率
    [SerializeField] public int openSpaceMinSize = 2;              // 空地最小尺寸
    [SerializeField] public int openSpaceMaxSize = 4;             // 空地最大尺寸
    
    private BackRoomCell[,] backRoomGrid; // 二維陣列，儲存所有房間單元的引用
    private BackRoomCell lastVisitedCell; // 記錄最後被訪問的房間（終點）
    private bool[,] isSpecialArea; // 標記哪些區域屬於特殊區域（大房間或空地）
    private List<SpecialArea> specialAreas; // 儲存所有特殊區域的資訊
    
    //特殊區域類型
    public enum SpecialAreaType
    {
        BigRoom,    // 大房間
        OpenSpace   // 空地
    }
    

    [System.Serializable]
    public class SpecialArea
    {
        public SpecialAreaType type;
        public int startX, startZ, width, height;
        public BackRoomCell entryCell; // 進入該區域的入口cell
        
        public SpecialArea(SpecialAreaType type, int startX, int startZ, int width, int height)
        {
            this.type = type;
            this.startX = startX;
            this.startZ = startZ;
            this.width = width;
            this.height = height;
        }
        
        public bool ContainsPosition(int x, int z)
        {
            return x >= startX && x < startX + width && z >= startZ && z < startZ + height;
        }
    }

    // [Header("Player Initial UI")]
    // [SerializeField] private GameObject initialUI;
    // [SerializeField] private Slider brightnessSlider;
    // [SerializeField] private Slider roomSizeSlider;
    // void Start() {
    //     brightnessSlider.minValue = 0;
    //     brightnessSlider.maxValue = 1;
    //     roomSizeSlider.minValue = 20;
    //     roomSizeSlider.maxValue = 150;
    // }
    // public void StartGame()
    // {
    //     initialRoom.SetActive(false);
    //     initialUI.SetActive(false);
    //     brightness = brightnessSlider.value;
    //     backRoomWidth = backRoomDepth = (int)roomSizeSlider.value;
    //     StartGenerate();
    // }

    public void InitializeRoomData()
    {
        for (int x = 0; x < backRoomWidth; x++)
            for (int y = 0; y < backRoomDepth; y++)
                if (backRoomGrid[x, y] != null)
                    Destroy(backRoomGrid[x, y].gameObject);
        
        backRoomGrid = new BackRoomCell[backRoomWidth, backRoomDepth];
        isSpecialArea = new bool[backRoomWidth, backRoomDepth];
        lastVisitedCell = null;
        visitCnt = 0;
        specialAreas.Clear();
    }

    public void StartGenerate(float brightnessVal, float backRoomSizeVal)
    {
        // 初始化資料結構
        backRoomGrid = new BackRoomCell[backRoomWidth, backRoomDepth];
        isSpecialArea = new bool[backRoomWidth, backRoomDepth];
        specialAreas = new List<SpecialArea>();
        visitCnt = 0; targetCnt = backRoomDepth * backRoomWidth / 2;
        brightness = brightnessVal;
        backRoomWidth = backRoomDepth = (int)backRoomSizeVal;

        // 生成特殊區域
        GenerateSpecialAreas();

        // 雙重迴圈生成所有房間單元
        for (int i = 0; i < backRoomDepth; i += 2)      // 遍歷深度（Z軸）
        {
            for (int j = 0; j < backRoomWidth; j += 2)  // 遍歷寬度（X軸）
            {
                // 在對應位置實例化房間單元，位置為(j, 0, i)
                BackRoomCell newCell = Instantiate(backRoomCellPrefab, new Vector3(j, 0, i), Quaternion.identity);
                newCell.Initial(Random.value < brightness, (j!=0 || i!=0) && Random.value < 0.2f);
                newCell.gameObject.name = "( " + j + ", " + i + " )";
                backRoomGrid[j, i] = newCell;
            }
        }

        // 從左上角(0,0)開始生成迷宮，preCell設為null表示起始點
        GenerateBackRoom(null, backRoomGrid[0, 0]);

        // 設置特殊區域的牆壁
        SetupSpecialAreas();

        // 迷宮生成完成後，可以使用 lastVisitedCell 作為終點
        // Debug.Log($"起點位置: (0, 0)");
        // Debug.Log($"終點位置: ({lastVisitedCell.transform.position.x}, {lastVisitedCell.transform.position.z})");
        // Debug.Log($"生成了 {specialAreas.Count} 個特殊區域");
        Instantiate(spawnPointEffect, backRoomGrid[0, 0].transform);
        Instantiate(exitPointEffect, lastVisitedCell.transform);
        Instantiate(exitTrigger, lastVisitedCell.transform);
    }
    
    //生成特殊區域（大房間和空地）
    private void GenerateSpecialAreas()
    {
        // 嘗試生成大房間、空地
        for (int attempt = 0; attempt < 300; attempt++)
        {
            if (Random.value < bigRoomProbability)
            {
                TryCreateSpecialArea(SpecialAreaType.BigRoom);
            }

            if (Random.value < openSpaceProbability)
            {
                TryCreateSpecialArea(SpecialAreaType.OpenSpace);
            }
        }
    }
    
    //嘗試創建特殊區域
    private void TryCreateSpecialArea(SpecialAreaType type)
    {
        int minSize = type == SpecialAreaType.BigRoom ? bigRoomMinSize : openSpaceMinSize;
        int maxSize = type == SpecialAreaType.BigRoom ? bigRoomMaxSize : openSpaceMaxSize;
        
        int width = Random.Range(minSize / 2, (maxSize + 1) / 2) * 2;
        int height = Random.Range(minSize / 2, (maxSize + 1) / 2) * 2;
        
        // 隨機選擇位置，避免邊界和起點
        int startX = Random.Range(1, (backRoomWidth - width - 1) / 2) * 2;
        int startZ = Random.Range(1, (backRoomDepth - height - 1) / 2) * 2;
        
        // 檢查是否與現有特殊區域重疊
        if (CanPlaceSpecialArea(startX, startZ, width, height))
        {
            SpecialArea newArea = new(type, startX, startZ, width, height);
            specialAreas.Add(newArea);
            
            // 標記這些位置為特殊區域
            for (int x = startX; x < startX + width; x++)
            {
                for (int z = startZ; z < startZ + height; z++)
                {
                    isSpecialArea[x, z] = true;
                }
            }
        }
    }
    
    //檢查是否可以在指定位置放置特殊區域
    private bool CanPlaceSpecialArea(int startX, int startZ, int width, int height)
    {
        // 檢查邊界
        if (startX + width >= backRoomWidth || startZ + height >= backRoomDepth)
            return false;
            
        // 檢查是否包含起點
        if (startX <= 0 && startZ <= 0 && startX + width > 0 && startZ + height > 0)
            return false;
        
        // 檢查是否與現有特殊區域重疊（包含緩衝區）
        for (int x = startX - 2; x < startX + width + 2; x++)
        {
            for (int z = startZ - 2; z < startZ + height + 2; z++)
            {
                if (x >= 0 && x < backRoomWidth && z >= 0 && z < backRoomDepth)
                {
                    if (isSpecialArea[x, z])
                        return false;
                }
            }
        }
        
        return true;
    }

    private int visitCnt = 0, targetCnt;
    private void GenerateBackRoom(BackRoomCell preCell, BackRoomCell curCell)
    {
        // 標記當前房間為已訪問
        curCell.Visit();
        visitCnt++;
        
        if (visitCnt == backRoomDepth)
            lastVisitedCell = curCell;
        
        // 清除當前房間與前一個房間之間的牆壁，建立通道
        ClearWalls(preCell, curCell);

        BackRoomCell nextCell;
        do
        {
            // 獲取下一個未訪問的相鄰房間
            nextCell = GetNextUnvisitedCell(curCell);
            
            // 如果找到未訪問的房間，遞迴處理該房間
            if (nextCell != null)
            {
                GenerateBackRoom(curCell, nextCell);
            }
        } while (nextCell != null); // 直到沒有未訪問的相鄰房間為止
    }

    // 隨機選擇一個未訪問的相鄰房間
    private BackRoomCell GetNextUnvisitedCell(BackRoomCell curCell)
    {
        // 獲取所有未訪問的相鄰房間
        var unvisitedCells = GetUnvisitedCell(curCell);
        
        // 使用隨機排序選擇一個房間，FirstOrDefault()返回第一個元素或null
        return unvisitedCells.OrderBy(_ => Random.Range(1, 10)).FirstOrDefault();
    }

    // 獲取指定房間的所有未訪問相鄰房間
    //考慮特殊區域的處理
    private IEnumerable<BackRoomCell> GetUnvisitedCell(BackRoomCell curCell)
    {
        // 獲取當前房間的座標位置
        int x = (int)curCell.transform.position.x;
        int z = (int)curCell.transform.position.z;
        
        // 檢查右邊的房間（X+2方向）
        if (x + 2 < backRoomWidth)
        {
            var cellToRight = backRoomGrid[x + 2, z];
            if (ShouldVisitCell(cellToRight, x + 2, z))
            {
                yield return cellToRight;
            }
        }

        // 檢查左邊的房間（X-2方向）
        if (x - 2 >= 0)
        {
            var cellToLeft = backRoomGrid[x - 2, z];
            if (ShouldVisitCell(cellToLeft, x - 2, z))
            {
                yield return cellToLeft;
            }
        }

        // 檢查前面的房間（Z+2方向）
        if (z + 2 < backRoomDepth)
        {
            var cellToFront = backRoomGrid[x, z + 2];
            if (ShouldVisitCell(cellToFront, x, z + 2))
            {
                yield return cellToFront;
            }
        }

        // 檢查後面的房間（Z-2方向）
        if (z - 2 >= 0)
        {
            var cellToBack = backRoomGrid[x, z - 2];
            if (ShouldVisitCell(cellToBack, x, z - 2))
            {
                yield return cellToBack;
            }
        }
    }
    
    // 判斷是否應該訪問指定的房間
    // 考慮特殊區域的邊界處理
    private bool ShouldVisitCell(BackRoomCell cell, int x, int z)
    {
        if (cell.isVisited) return false;
        
        // 如果目標位置是特殊區域，需要特殊處理
        if (isSpecialArea[x, z])
        {
            // 找到包含此位置的特殊區域
            SpecialArea area = specialAreas.FirstOrDefault(a => a.ContainsPosition(x, z));
            if (area != null && area.entryCell != null)
            {
                // 已有入口，不再訪問其他位置（除非是大房間內部連通）
                return area.type == SpecialAreaType.OpenSpace;
            }
        }
        
        return true;
    }

    // 設置特殊區域
    [SerializeField] private GameObject flagPrefab, flashLightPrefab;
    private void SetupSpecialAreas()
    {
        foreach (SpecialArea area in specialAreas)
        {
            if (area.type == SpecialAreaType.BigRoom)
                SetupBigRoom(area);
            else if (area.type == SpecialAreaType.OpenSpace)
                SetupOpenSpace(area);
        }
    }
    
    // 設置大房間：只有一個小門，其他都被圍起來
    private void SetupBigRoom(SpecialArea area)
    {
        
        for (int x = area.startX; x < area.startX + area.width; x += 2)
        {
            for (int z = area.startZ; z < area.startZ + area.height; z += 2)
            {
                BackRoomCell cell = backRoomGrid[x, z];
                cell.Visit();
                // 如果不是入口cell，清除所有內部牆壁
                if (cell != area.entryCell)
                {
                    // 清除與同一大房間內其他cell的牆壁
                    if (x > area.startX) // 不是左邊界
                        cell.ClearLeftWall();
                    if (x < area.startX + area.width - 2) // 不是右邊界
                        cell.ClearRightWall();
                    if (z > area.startZ) // 不是後邊界
                        cell.ClearBackWall();
                    if (z < area.startZ + area.height - 2) // 不是前邊界
                        cell.ClearFrontWall();
                }

                // 封閉與外部的連接（除了入口）
                if (cell != area.entryCell)
                {
                    // 檢查邊界並封閉外部連接
                    if (x == area.startX && x > 0) // 左邊界
                        backRoomGrid[x - 2, z].ClearRightWall(); // 這行可能需要調整
                    if (x == area.startX + area.width - 2 && x < backRoomWidth - 2) // 右邊界
                        backRoomGrid[x + 2, z].ClearLeftWall(); // 這行可能需要調整
                    if (z == area.startZ && z > 0) // 後邊界
                        backRoomGrid[x, z - 2].ClearFrontWall(); // 這行可能需要調整
                    if (z == area.startZ + area.height - 2 && z < backRoomDepth - 2) // 前邊界
                        backRoomGrid[x, z + 2].ClearBackWall(); // 這行可能需要調整
                }

                if (x == area.startX + area.width / 2 && z == area.startZ + area.width / 2)
                {
                    Instantiate(flagPrefab, cell.transform);
                    if (Random.value < 0.3)
                    {
                        Instantiate(flashLightPrefab, cell.transform);
                        if (Random.value < 0.7)
                        {
                            Instantiate(flashLightPrefab, cell.transform);
                        }
                    }
                }
            }
        }
    }
    
    // 設置空地：清除區域內所有牆壁
    private void SetupOpenSpace(SpecialArea area)
    {
        for (int x = area.startX; x < area.startX + area.width; x++)
        {
            for (int z = area.startZ; z < area.startZ + area.height; z++)
            {
                BackRoomCell cell = backRoomGrid[x, z];

                if (cell != null)
                {
                    cell.ClearLeftWall();
                    cell.ClearRightWall();
                    cell.ClearFrontWall();
                    cell.ClearBackWall();
                    cell.Visit();
                    if (x == area.startX + area.width / 2 && z == area.startZ + area.width / 2)
                    {
                        Instantiate(flagPrefab, cell.transform);
                        if (Random.value < 0.7)
                        {
                            Instantiate(flashLightPrefab, cell.transform);
                        }
                    }
                }
            }
        }
    }

    // 清除兩個相鄰房間之間的牆壁，建立通道
    // 考慮特殊區域的處理
    private void ClearWalls(BackRoomCell preCell, BackRoomCell curCell)
    {
        // 如果前一個房間為null（起始房間），則不需要清除牆壁
        if (preCell == null) return;
        
        int preX = (int)preCell.transform.position.x;
        int preZ = (int)preCell.transform.position.z;
        int curX = (int)curCell.transform.position.x;
        int curZ = (int)curCell.transform.position.z;
        
        // 檢查是否跨越特殊區域邊界
        bool preInSpecial = isSpecialArea[preX, preZ];
        bool curInSpecial = isSpecialArea[curX, curZ];
        
        bool isBigRoomEntry = false;
        if (curInSpecial)
        {
            SpecialArea curArea = specialAreas.FirstOrDefault(a => a.ContainsPosition(curX, curZ));
            if (curArea.type == SpecialAreaType.BigRoom)
            {
                curArea.entryCell = curCell;
                isBigRoomEntry = true;
            }
        }
        
        // 如果兩個都在同一個特殊區域內，特殊處理會在SetupSpecialAreas中進行
        if (preInSpecial && curInSpecial)
        {
            SpecialArea preArea = specialAreas.FirstOrDefault(a => a.ContainsPosition(preX, preZ));
            SpecialArea curArea = specialAreas.FirstOrDefault(a => a.ContainsPosition(curX, curZ));
            if (preArea == curArea) return; // 同一特殊區域，稍後處理
        }
        
        // 前一個房間在當前房間的左邊（X座標相差2）
        if (preCell.transform.position.x < curCell.transform.position.x)
        {
            // 清除中間的牆壁，創造2倍寬的通道
            preCell.ClearRightWall();  // 清除前房間的右牆
            curCell.ClearLeftWall();   // 清除當前房間的左牆
            if (isBigRoomEntry) curCell.ClearRightWall();
            return;
        }

        // 前一個房間在當前房間的右邊（X座標相差2）
        if (preCell.transform.position.x > curCell.transform.position.x)
        {
            preCell.ClearLeftWall();   // 清除前房間的左牆
            curCell.ClearRightWall();  // 清除當前房間的右牆
            if (isBigRoomEntry) curCell.ClearLeftWall();
            return;
        }

        // 前一個房間在當前房間的後面（Z座標相差2）
        if (preCell.transform.position.z < curCell.transform.position.z)
        {
            preCell.ClearFrontWall();  // 清除前房間的前牆
            curCell.ClearBackWall();   // 清除當前房間的後牆
            if (isBigRoomEntry) curCell.ClearFrontWall();
            return;
        }

        // 前一個房間在當前房間的前面（Z座標相差2）
        if (preCell.transform.position.z > curCell.transform.position.z)
        {
            preCell.ClearBackWall();   // 清除前房間的後牆
            curCell.ClearFrontWall();  // 清除當前房間的前牆
            if (isBigRoomEntry) curCell.ClearBackWall();
            return;
        }
    }
}