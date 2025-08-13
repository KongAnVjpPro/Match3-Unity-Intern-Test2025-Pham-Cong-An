using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event Action<eStateGame> StateChangedAction = delegate { };

    public enum eLevelMode
    {
        TIMER,
        MOVES,
        AUTO_WIN,
        AUTO_LOSE
    }

    public enum eStateGame
    {
        SETUP,
        MAIN_MENU,
        GAME_STARTED,
        PAUSE,
        GAME_OVER,

    }

    private eStateGame m_state;
    public eStateGame State
    {
        get { return m_state; }
        private set
        {
            m_state = value;

            StateChangedAction(m_state);
        }
    }


    private GameSettings m_gameSettings;


    private BoardController m_boardController;

    private UIMainManager m_uiMenu;

    private LevelCondition m_levelCondition;

    public bool IsWin { get; private set; }

    private eLevelMode m_mode = eLevelMode.MOVES;//init
    public eLevelMode CurrentMode => m_mode;

    private void Awake()
    {
        State = eStateGame.SETUP;

        m_gameSettings = Resources.Load<GameSettings>(Constants.GAME_SETTINGS_PATH);

        m_uiMenu = FindObjectOfType<UIMainManager>();
        m_uiMenu.Setup(this);
    }

    void Start()
    {
        State = eStateGame.MAIN_MENU;
    }

    // Update is called once per frame
    void Update()
    {
        // if (m_boardController != null) m_boardController.Update();
    }


    internal void SetState(eStateGame state)
    {
        State = state;

        if (State == eStateGame.PAUSE)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
    }

    public void LoadLevel(eLevelMode mode)
    {
        m_mode = mode;
        m_boardController = new GameObject("BoardController").AddComponent<BoardController>();
        m_boardController.StartGame(this, m_gameSettings);



        if (mode == eLevelMode.MOVES)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelBar>();
            m_levelCondition.Setup(m_gameSettings.BoardSizeX * m_gameSettings.BoardSizeY, m_uiMenu.GetLevelConditionView(), m_boardController);
        }
        else if (mode == eLevelMode.TIMER)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelTime>();
            m_levelCondition.Setup(m_gameSettings.LevelTime, m_uiMenu.GetLevelConditionView(), this, m_boardController);
        }
        else if (mode == eLevelMode.AUTO_WIN)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelAutoPlay>();
            m_levelCondition.Setup(m_gameSettings.BoardSizeX * m_gameSettings.BoardSizeY, m_uiMenu.GetLevelConditionView(), m_boardController, true);
        }
        else if (mode == eLevelMode.AUTO_LOSE)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelAutoPlay>();
            m_levelCondition.Setup(m_gameSettings.BoardSizeX * m_gameSettings.BoardSizeY, m_uiMenu.GetLevelConditionView(), m_boardController, false);
        }



        m_levelCondition.ConditionCompleteEvent += GameOver;

        State = eStateGame.GAME_STARTED;
    }

    public void GameOver()
    {
        StartCoroutine(WaitBoardController());
    }

    internal void ClearLevel()
    {
        if (m_boardController)
        {
            m_boardController.Clear();
            Destroy(m_boardController.gameObject);
            m_boardController = null;
        }
    }

    private IEnumerator WaitBoardController()
    {
        while (m_boardController.IsBusy)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1f);


        if (m_mode == eLevelMode.TIMER)
        {
            IsWin = (!m_boardController.IsBarFull()) && (m_boardController.GetCurrentCellAmount() <= 0);
        }
        else
        {
            IsWin = !m_boardController.IsBarFull();
        }
        // IsWin = (m_boardController.GetCurrentCellAmount() <= 0);


        State = eStateGame.GAME_OVER;

        if (m_levelCondition != null)
        {
            m_levelCondition.ConditionCompleteEvent -= GameOver;

            Destroy(m_levelCondition);
            m_levelCondition = null;
        }
    }
}
