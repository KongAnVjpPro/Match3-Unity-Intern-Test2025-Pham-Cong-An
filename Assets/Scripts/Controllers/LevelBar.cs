using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelBar : LevelCondition
{
    private int m_moves;

    private BoardController m_board;
    //move = so phan tu khi khoi tao, lose khi ban max
    public override void Setup(float value, Text txt, BoardController board)
    {
        base.Setup(value, txt, board);

        m_moves = (int)value;

        m_board = board;

        m_board.OnMoveEvent += OnMove;

        UpdateText();
    }

    private void OnMove()
    {
        if (m_conditionCompleted) return;

        m_moves--;

        UpdateText();

        if (m_moves <= 0)
        {
            OnConditionComplete();
        }
        if (m_board.IsBarFull())
        {
            OnConditionComplete();
        }
    }

    protected override void UpdateText()
    {
        m_txt.text = string.Format("MOVES:\n{0}", m_moves);
    }

    protected override void OnDestroy()
    {
        if (m_board != null) m_board.OnMoveEvent -= OnMove;

        base.OnDestroy();
    }
}
