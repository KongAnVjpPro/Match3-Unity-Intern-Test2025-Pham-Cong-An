using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
[Serializable]
public class BarController
{
    private List<Cell> m_barCells;
    // private List<Cell> m_barCellsPseudo;
    private List<GameObject> m_viewBar;
    private int m_minMatches;
    private int barSize;
    // private int cellExploded = 0;
    public BarController(int minMatches, int BarSize)
    {
        m_barCells = new List<Cell>();
        m_viewBar = new List<GameObject>();
        m_minMatches = minMatches;
        barSize = BarSize;
    }

    public bool IsBarFull()
    {
        // Debug.Log(m_barCells.Count +"  "+ m_viewBar.Count);
        return m_barCells.Count >= barSize;
    }


    public void AddCell(Cell cell)
    {
        if (cell == null) return;
        if (IsBarFull()) return;
        int id = FindIDCellSameType(cell);

        if (id == -1)
        {
            m_barCells.Add(cell);
            UpdateView();
        }
        else
        {
            m_barCells.Insert(id, cell);
            UpdateView();
            int sameCellCnt = FindAmountCellSameType(cell, id);

            if (sameCellCnt >= m_minMatches)//explode
            {

                List<Cell> tmpCells = new List<Cell>();
                for (int i = 0; i < sameCellCnt; i++)
                {
                    tmpCells.Add(m_barCells[i + id]);
                    m_barCells[i + id].EnableCollider(false);
                }

                m_barCells.RemoveRange(id, sameCellCnt);

                Sequence s = DOTween.Sequence();

                for (int i = 0; i < tmpCells.Count; i++)
                {
                    s.Join(tmpCells[i].transform.DOScale(0f, 0.15f));

                }
                s.OnComplete(() =>
                {
                    for (int i = 0; i < tmpCells.Count; i++)
                    {
                        tmpCells[i].ExplodeItem();
                        // cellExploded++;
                    }

                    UpdateView();
                });
            }

        }

    }

    public void RemoveCell(Cell cell)
    {
        if (cell == null) return;
        foreach (var ce in m_barCells)
        {
            if (ce == cell)
            {
                m_barCells.Remove(ce);
                break;
            }

        }
        UpdateView();
    }

    public void AddCellPseudo(Cell cell)//use for calculate path
    {
        if (cell == null) return;
        if (m_barCells.Count >= barSize) return;
        int id = FindIDCellSameType(cell);

        if (id == -1)
        {
            m_barCells.Add(cell);
        }
        else
        {
            m_barCells.Insert(id, cell);

            int sameCellCnt = FindAmountCellSameType(cell, id);

            if (sameCellCnt >= m_minMatches)
            {
                List<Cell> tmpCells = new List<Cell>();
                for (int i = 0; i < sameCellCnt; i++)
                {
                    tmpCells.Add(m_barCells[i + id]);
                }
                m_barCells.RemoveRange(id, sameCellCnt);

            }

        }

    }
    public void InitView(GameObject go)
    {
        if (go == null) return;
        m_viewBar.Add(go);
    }

    #region View
    void UpdateView()
    {
        if (m_viewBar.Count == 0) return;
        int sz = m_barCells.Count;
        if (sz > m_viewBar.Count)
        {
            sz = m_viewBar.Count;
        }
        for (int i = 0; i < sz; i++)
        {
            m_barCells[i].transform.position = m_viewBar[i].transform.position;
            m_barCells[i].ApplyItemMoveToPosition();
        }
    }
    #endregion

    internal int FindIDCellSameType(Cell c, bool isPseudo = false)
    {
        for (int i = 0; i < m_barCells.Count; i++)
        {
            if (m_barCells[i].IsSameType(c))
            {
                return i;
            }
        }
        return -1;
    }
    internal int FindAmountCellSameType(Cell c, int startId = -1, bool isPseudo = false)
    {
        if (startId == -1)
        {
            startId = FindIDCellSameType(c);
        }
        if (startId == -1)
        {
            return -1;
        }
        int cnt = 0;
        while (startId < m_barCells.Count && m_barCells[startId].IsSameType(c))
        {
            cnt++;
            startId++;
        }
        return cnt;
    }
}