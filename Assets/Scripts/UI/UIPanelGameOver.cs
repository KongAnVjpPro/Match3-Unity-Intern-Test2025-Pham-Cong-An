using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelGameOver : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnCloseLose;
    [SerializeField] private Button btnCloseWin;

    private UIMainManager m_mngr;
    [SerializeField] private GameObject winPanel, losePanel;

    private void Awake()
    {
        btnCloseLose.onClick.AddListener(OnClickClose);
        btnCloseWin.onClick.AddListener(OnClickClose);
    }

    private void OnDestroy()
    {
        if (btnCloseLose) btnCloseLose.onClick.RemoveAllListeners();
        if (btnCloseWin) btnCloseWin.onClick.RemoveAllListeners();
    }

    private void OnClickClose()
    {
        m_mngr.ShowMainMenu();
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    public void Show()
    {
        SetResult(m_mngr.IsWin);
        this.gameObject.SetActive(true);
    }

    public void SetResult(bool isWin)
    {
        winPanel.SetActive(isWin);
        losePanel.SetActive(!isWin);
    }
}
