using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board
{
    public enum eMatchDirection
    {
        NONE,
        HORIZONTAL,
        VERTICAL,
        ALL
    }

    private int boardSizeX;

    private int boardSizeY;

    private int barSize;

    private Cell[,] m_cells;

    private Transform m_root;

    private int m_matchMin;
    private BarController m_barController;

    public Board(Transform transform, GameSettings gameSettings, bool isOnTimeMode = false)
    {
        m_root = transform;

        m_matchMin = gameSettings.MatchesMin;

        this.boardSizeX = gameSettings.BoardSizeX;
        this.boardSizeY = gameSettings.BoardSizeY;
        this.barSize = gameSettings.BarSize;

        m_cells = new Cell[boardSizeX, boardSizeY];
        m_barController = new BarController(gameSettings.MatchesMin, barSize);
        CreateBoard(isOnTimeMode);
    }
    public void AutoMoveNext()
    {
        if (winOrder.Count == 0) return;
        winOrder[0].OnClickHandle(this);
        winOrder.RemoveAt(0);
    }
    private void CreateBoard(bool isOnTimeMode = false)
    {
        Vector3 origin = new Vector3(-boardSizeX * 0.5f + 0.5f, -boardSizeY * 0.5f + 0.5f, 0f);
        Vector3 originBar = new Vector3(-barSize * 0.5f + 0.5f, -boardSizeY * 0.5f - 1f, 0f);

        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
        GameObject prefabBar = Resources.Load<GameObject>(Constants.PREFAB_BAR_BACKGROUND);
        //gen cell

        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                GameObject go = GameObject.Instantiate(prefabBG);
                go.transform.position = origin + new Vector3(x, y, 0f);
                go.transform.SetParent(m_root);

                Cell cell = go.GetComponent<Cell>();
                cell.Setup(x, y, isOnTimeMode);

                m_cells[x, y] = cell;
            }
        }
        //gen bar
        for (int i = 0; i < barSize; i++)
        {
            GameObject ba = GameObject.Instantiate(prefabBar);
            ba.transform.position = originBar + new Vector3(i, 0f, 0f);
            ba.transform.SetParent(m_root);

            m_barController.InitView(ba);
            // BarCell bCell = ba.GetComponent<BarCell>();
            // barController.AddCell(bCell);
        }

        //set neighbours
        // for (int x = 0; x < boardSizeX; x++)
        // {
        //     for (int y = 0; y < boardSizeY; y++)
        //     {
        //         if (y + 1 < boardSizeY) m_cells[x, y].NeighbourUp = m_cells[x, y + 1];
        //         if (x + 1 < boardSizeX) m_cells[x, y].NeighbourRight = m_cells[x + 1, y];
        //         if (y > 0) m_cells[x, y].NeighbourBottom = m_cells[x, y - 1];
        //         if (x > 0) m_cells[x, y].NeighbourLeft = m_cells[x - 1, y];
        //     }
        // }

    }
    #region New logic
    internal void AddCellToBar(Cell c)
    {
        if (c == null) return;
        m_barController.AddCell(c);
    }

    internal void RemoveCellFromBar(Cell c)
    {
        if (c == null) return;
        m_barController.RemoveCell(c);
    }

    public bool IsBarFull()
    {
        return m_barController.IsBarFull();
    }
    public int GetCurrentCellAmount()
    {
        int cellAmount = 0;
        foreach (var cell in m_cells)
        {
            if (cell.Item == null) continue;
            cellAmount++;
        }
        return cellAmount;
    }
    private List<Cell> winOrder = new List<Cell>();
    private List<Cell> loseOrder = new List<Cell>();

    //run after fill
    public void AutoWin(bool value)
    {
        // winOrder = new List<Cell>();

        autoWin = value;
        if (autoWin) return;
        winOrder = loseOrder;

    }
    private bool autoWin = false;
    private List<bool> visitedLoseOrder = new List<bool>();
    private List<int> loseOrderId = new List<int>();
    #endregion

    internal void Fill()
    {

        List<NormalItem.eNormalType> types = new List<NormalItem.eNormalType>();
        types = Enum.GetValues(typeof(NormalItem.eNormalType)).Cast<NormalItem.eNormalType>().ToList();

        int totalCell = boardSizeX * boardSizeY;
        int baseCellCount = (totalCell / (types.Count * m_matchMin)) * m_matchMin;
        int remainCell = totalCell - baseCellCount * types.Count;

        // Debug.Log("" + totalCell + " " + baseCellCount + " " + remainCell);

        List<NormalItem.eNormalType> itemToFill = new List<NormalItem.eNormalType>();
        foreach (NormalItem.eNormalType type in types)
        {
            for (int i = 0; i < baseCellCount; i++)
            {
                itemToFill.Add(type);
                visitedLoseOrder.Add(false);
            }
        }

        if (remainCell > 0)
        {
            int cntItem = 0;
            int idItem = -1;
            // Unity.Random rd = new System.Random();
            while (remainCell > 0)
            {
                if (idItem != -1 && cntItem < m_matchMin)
                {
                    itemToFill.Add(types[idItem]);
                    visitedLoseOrder.Add(false);

                    remainCell--;
                    cntItem++;
                }
                else
                {
                    idItem = UnityEngine.Random.Range(0, types.Count);
                    cntItem = 0;
                }
            }
        }

        //auto win solution

        // Debug.Log(itemToFill.Count);
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                NormalItem item = new NormalItem();

                // int itemId = UnityEngine.Random.Range(0, itemToFill.Count);
                int itemId = x * boardSizeY + y;

                item.SetType(itemToFill[itemId]);
                item.SetView();
                item.SetViewRoot(m_root);

                cell.Assign(item);
                cell.ApplyItemPosition(false);
                winOrder.Add(cell);
                // itemToFill.RemoveAt(itemId);
            }
        }

        //shuffle
        Shuffle();
        // if (autoWin) return;

        //gen lose path
        GenerateLosePath();
    }
    void GenerateLosePath()
    {
        if (winOrder.Count == 0) return;
        BarController tmp_bar = new BarController(m_matchMin, barSize);
        int step = m_cells.Length;
        while (!tmp_bar.IsBarFull() && step > 0)
        {
            NextWorstMove(tmp_bar);
            step--;
        }

    }
    void NextWorstMove(BarController tmp_bar)
    {
        int idItem = -1;
        int tmpCount = m_matchMin + 1;
        for (int i = 0; i < visitedLoseOrder.Count; i++)
        {
            if (visitedLoseOrder[i]) continue;
            int cntItem = tmp_bar.FindAmountCellSameType(winOrder[i]);
            if (cntItem == -1)
            {
                idItem = i;
                break;
            }
            if (tmpCount >= cntItem)
            {
                tmpCount = cntItem;
                idItem = i;
            }
            //check trong so
        }
        if (idItem != -1)
        {
            tmp_bar.AddCellPseudo(winOrder[idItem]);
            visitedLoseOrder[idItem] = true;
            loseOrder.Add(winOrder[idItem]);
        }
        else //visitedFull
        {

        }
    }
    private void ReOrder(int step)
    {
        // m_barController
        if (m_barController.IsBarFull())
        {
            return;
        }
    }

    internal void Shuffle()
    {
        Vector3 origin = new Vector3(-boardSizeX * 0.5f + 0.5f, -boardSizeY * 0.5f + 0.5f, 0f);
        // List<Cell> list = new List<Cell>();
        // for (int x = 0; x < boardSizeX; x++)
        // {
        //     for (int y = 0; y < boardSizeY; y++)
        //     {
        //         list.Add(m_cells[x, y]);
        //         // m_cells[x, y].Free();
        //     }
        // }
        //  go.transform.position = origin + new Vector3(x, y, 0f);

        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                // int rnd = UnityEngine.Random.Range(0, list.Count);
                int rndX = UnityEngine.Random.Range(0, boardSizeX);
                int rndY = UnityEngine.Random.Range(0, boardSizeY);
                // m_cells[x, y].Assign(list[rnd]);
                // m_cells[x, y].ApplyItemMoveToPosition();
                // list.RemoveAt(rnd);
                Swap(m_cells[x, y], m_cells[rndX, rndY]);
            }
        }
    }


    internal void FillGapsWithNewItems()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                if (!cell.IsEmpty) continue;

                NormalItem item = new NormalItem();

                item.SetType(Utils.GetRandomNormalType());
                item.SetView();
                item.SetViewRoot(m_root);

                cell.Assign(item);
                cell.ApplyItemPosition(true);
            }
        }
    }

    internal void ExplodeAllItems()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                cell.ExplodeItem();
            }
        }
    }

    public void Swap(Cell cell1, Cell cell2, Action callback)
    {
        Item item = cell1.Item;
        cell1.Free();
        Item item2 = cell2.Item;
        cell1.Assign(item2);
        cell2.Free();
        cell2.Assign(item);

        item.View.DOMove(cell2.transform.position, 0.3f);
        item2.View.DOMove(cell1.transform.position, 0.3f).OnComplete(() => { if (callback != null) callback(); });
    }
    public void Swap(Cell cell1, Cell cell2)
    {
        // Sequence s = DOTween.Sequence();
        Vector3 cell1Pos = cell1.transform.position;
        Vector3 cell2Pos = cell2.transform.position;

        cell1.transform.position = cell2Pos;
        cell2.transform.position = cell1Pos;

        cell1.ApplyItemMoveToPosition();
        cell2.ApplyItemMoveToPosition();

        cell1.SetOriginPos(cell2Pos);
        cell2.SetOriginPos(cell1Pos);
        // s.Join(cell1.transform.DOMove(cell2Pos, 0.3f));
        // s.Join(cell2.transform.DOMove(cell1Pos, 0.3f));
    }
    public List<Cell> GetHorizontalMatches(Cell cell)
    {
        List<Cell> list = new List<Cell>();
        list.Add(cell);

        //check horizontal match
        Cell newcell = cell;
        while (true)
        {
            Cell neib = newcell.NeighbourRight;
            if (neib == null) break;

            if (neib.IsSameType(cell))
            {
                list.Add(neib);
                newcell = neib;
            }
            else break;
        }

        newcell = cell;
        while (true)
        {
            Cell neib = newcell.NeighbourLeft;
            if (neib == null) break;

            if (neib.IsSameType(cell))
            {
                list.Add(neib);
                newcell = neib;
            }
            else break;
        }

        return list;
    }


    public List<Cell> GetVerticalMatches(Cell cell)
    {
        List<Cell> list = new List<Cell>();
        list.Add(cell);

        Cell newcell = cell;
        while (true)
        {
            Cell neib = newcell.NeighbourUp;
            if (neib == null) break;

            if (neib.IsSameType(cell))
            {
                list.Add(neib);
                newcell = neib;
            }
            else break;
        }

        newcell = cell;
        while (true)
        {
            Cell neib = newcell.NeighbourBottom;
            if (neib == null) break;

            if (neib.IsSameType(cell))
            {
                list.Add(neib);
                newcell = neib;
            }
            else break;
        }

        return list;
    }

    internal void ConvertNormalToBonus(List<Cell> matches, Cell cellToConvert)
    {
        eMatchDirection dir = GetMatchDirection(matches);

        BonusItem item = new BonusItem();
        switch (dir)
        {
            case eMatchDirection.ALL:
                item.SetType(BonusItem.eBonusType.ALL);
                break;
            case eMatchDirection.HORIZONTAL:
                item.SetType(BonusItem.eBonusType.HORIZONTAL);
                break;
            case eMatchDirection.VERTICAL:
                item.SetType(BonusItem.eBonusType.VERTICAL);
                break;
        }

        if (item != null)
        {
            if (cellToConvert == null)
            {
                int rnd = UnityEngine.Random.Range(0, matches.Count);
                cellToConvert = matches[rnd];
            }

            item.SetView();
            item.SetViewRoot(m_root);

            cellToConvert.Free();
            cellToConvert.Assign(item);
            cellToConvert.ApplyItemPosition(true);
        }
    }


    internal eMatchDirection GetMatchDirection(List<Cell> matches)
    {
        if (matches == null || matches.Count < m_matchMin) return eMatchDirection.NONE;

        var listH = matches.Where(x => x.BoardX == matches[0].BoardX).ToList();
        if (listH.Count == matches.Count)
        {
            return eMatchDirection.VERTICAL;
        }

        var listV = matches.Where(x => x.BoardY == matches[0].BoardY).ToList();
        if (listV.Count == matches.Count)
        {
            return eMatchDirection.HORIZONTAL;
        }

        if (matches.Count > 5)
        {
            return eMatchDirection.ALL;
        }

        return eMatchDirection.NONE;
    }

    internal List<Cell> FindFirstMatch()
    {
        List<Cell> list = new List<Cell>();

        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];

                var listhor = GetHorizontalMatches(cell);
                if (listhor.Count >= m_matchMin)
                {
                    list = listhor;
                    break;
                }

                var listvert = GetVerticalMatches(cell);
                if (listvert.Count >= m_matchMin)
                {
                    list = listvert;
                    break;
                }
            }
        }

        return list;
    }

    public List<Cell> CheckBonusIfCompatible(List<Cell> matches)
    {
        var dir = GetMatchDirection(matches);

        var bonus = matches.Where(x => x.Item is BonusItem).FirstOrDefault();
        if (bonus == null)
        {
            return matches;
        }

        List<Cell> result = new List<Cell>();
        switch (dir)
        {
            case eMatchDirection.HORIZONTAL:
                foreach (var cell in matches)
                {
                    BonusItem item = cell.Item as BonusItem;
                    if (item == null || item.ItemType == BonusItem.eBonusType.HORIZONTAL)
                    {
                        result.Add(cell);
                    }
                }
                break;
            case eMatchDirection.VERTICAL:
                foreach (var cell in matches)
                {
                    BonusItem item = cell.Item as BonusItem;
                    if (item == null || item.ItemType == BonusItem.eBonusType.VERTICAL)
                    {
                        result.Add(cell);
                    }
                }
                break;
            case eMatchDirection.ALL:
                foreach (var cell in matches)
                {
                    BonusItem item = cell.Item as BonusItem;
                    if (item == null || item.ItemType == BonusItem.eBonusType.ALL)
                    {
                        result.Add(cell);
                    }
                }
                break;
        }

        return result;
    }

    internal List<Cell> GetPotentialMatches()
    {
        List<Cell> result = new List<Cell>();
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];

                //check right
                /* example *\
                  * * * * *
                  * * * * *
                  * * * ? *
                  * & & * ?
                  * * * ? *
                \* example  */

                if (cell.NeighbourRight != null)
                {
                    result = GetPotentialMatch(cell, cell.NeighbourRight, cell.NeighbourRight.NeighbourRight);
                    if (result.Count > 0)
                    {
                        break;
                    }
                }

                //check up
                /* example *\
                  * ? * * *
                  ? * ? * *
                  * & * * *
                  * & * * *
                  * * * * *
                \* example  */
                if (cell.NeighbourUp != null)
                {
                    result = GetPotentialMatch(cell, cell.NeighbourUp, cell.NeighbourUp.NeighbourUp);
                    if (result.Count > 0)
                    {
                        break;
                    }
                }

                //check bottom
                /* example *\
                  * * * * *
                  * & * * *
                  * & * * *
                  ? * ? * *
                  * ? * * *
                \* example  */
                if (cell.NeighbourBottom != null)
                {
                    result = GetPotentialMatch(cell, cell.NeighbourBottom, cell.NeighbourBottom.NeighbourBottom);
                    if (result.Count > 0)
                    {
                        break;
                    }
                }

                //check left
                /* example *\
                  * * * * *
                  * * * * *
                  * ? * * *
                  ? * & & *
                  * ? * * *
                \* example  */
                if (cell.NeighbourLeft != null)
                {
                    result = GetPotentialMatch(cell, cell.NeighbourLeft, cell.NeighbourLeft.NeighbourLeft);
                    if (result.Count > 0)
                    {
                        break;
                    }
                }

                /* example *\
                  * * * * *
                  * * * * *
                  * * ? * *
                  * & * & *
                  * * ? * *
                \* example  */
                Cell neib = cell.NeighbourRight;
                if (neib != null && neib.NeighbourRight != null && neib.NeighbourRight.IsSameType(cell))
                {
                    Cell second = LookForTheSecondCellVertical(neib, cell);
                    if (second != null)
                    {
                        result.Add(cell);
                        result.Add(neib.NeighbourRight);
                        result.Add(second);
                        break;
                    }
                }

                /* example *\
                  * * * * *
                  * & * * *
                  ? * ? * *
                  * & * * *
                  * * * * *
                \* example  */
                neib = null;
                neib = cell.NeighbourUp;
                if (neib != null && neib.NeighbourUp != null && neib.NeighbourUp.IsSameType(cell))
                {
                    Cell second = LookForTheSecondCellHorizontal(neib, cell);
                    if (second != null)
                    {
                        result.Add(cell);
                        result.Add(neib.NeighbourUp);
                        result.Add(second);
                        break;
                    }
                }
            }

            if (result.Count > 0) break;
        }

        return result;
    }

    private List<Cell> GetPotentialMatch(Cell cell, Cell neighbour, Cell target)
    {
        List<Cell> result = new List<Cell>();

        if (neighbour != null && neighbour.IsSameType(cell))
        {
            Cell third = LookForTheThirdCell(target, neighbour);
            if (third != null)
            {
                result.Add(cell);
                result.Add(neighbour);
                result.Add(third);
            }
        }

        return result;
    }

    private Cell LookForTheSecondCellHorizontal(Cell target, Cell main)
    {
        if (target == null) return null;
        if (target.IsSameType(main)) return null;

        //look right
        Cell second = null;
        second = target.NeighbourRight;
        if (second != null && second.IsSameType(main))
        {
            return second;
        }

        //look left
        second = null;
        second = target.NeighbourLeft;
        if (second != null && second.IsSameType(main))
        {
            return second;
        }

        return null;
    }

    private Cell LookForTheSecondCellVertical(Cell target, Cell main)
    {
        if (target == null) return null;
        if (target.IsSameType(main)) return null;

        //look up        
        Cell second = target.NeighbourUp;
        if (second != null && second.IsSameType(main))
        {
            return second;
        }

        //look bottom
        second = null;
        second = target.NeighbourBottom;
        if (second != null && second.IsSameType(main))
        {
            return second;
        }

        return null;
    }

    private Cell LookForTheThirdCell(Cell target, Cell main)
    {
        if (target == null) return null;
        if (target.IsSameType(main)) return null;

        //look up
        Cell third = CheckThirdCell(target.NeighbourUp, main);
        if (third != null)
        {
            return third;
        }

        //look right
        third = null;
        third = CheckThirdCell(target.NeighbourRight, main);
        if (third != null)
        {
            return third;
        }

        //look bottom
        third = null;
        third = CheckThirdCell(target.NeighbourBottom, main);
        if (third != null)
        {
            return third;
        }

        //look left
        third = null;
        third = CheckThirdCell(target.NeighbourLeft, main); ;
        if (third != null)
        {
            return third;
        }

        return null;
    }

    private Cell CheckThirdCell(Cell target, Cell main)
    {
        if (target != null && target != main && target.IsSameType(main))
        {
            return target;
        }

        return null;
    }

    internal void ShiftDownItems()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            int shifts = 0;
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                if (cell.IsEmpty)
                {
                    shifts++;
                    continue;
                }

                if (shifts == 0) continue;

                Cell holder = m_cells[x, y - shifts];

                Item item = cell.Item;
                cell.Free();

                holder.Assign(item);
                item.View.DOMove(holder.transform.position, 0.3f);
            }
        }
    }

    public void Clear()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                cell.Clear();

                GameObject.Destroy(cell.gameObject);
                m_cells[x, y] = null;
            }
        }
    }
}
