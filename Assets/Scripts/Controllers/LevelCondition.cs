using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelCondition : MonoBehaviour
{
    public event Action ConditionCompleteEvent = delegate { };

    protected Text m_txt;

    protected bool m_conditionCompleted = false;

    public virtual void Setup(float value, Text txt)
    {
        m_txt = txt;
    }

    public virtual void Setup(float value, Text txt, GameManager mngr)//old
    {
        m_txt = txt;
    }
    public virtual void Setup(float value, Text txt, GameManager mngr, BoardController board)//time
    {
        m_txt = txt;
    }
    public virtual void Setup(float value, Text txt, BoardController board)//normal
    {
        m_txt = txt;
    }

    public virtual void Setup(float value, Text txt, BoardController board, bool AutoWin)//auto win or lose
    {
        m_txt = txt;
    }
    protected virtual void UpdateText() { }

    protected void OnConditionComplete()
    {
        m_conditionCompleted = true;

        ConditionCompleteEvent();
    }

    protected virtual void OnDestroy()
    {

    }
}
