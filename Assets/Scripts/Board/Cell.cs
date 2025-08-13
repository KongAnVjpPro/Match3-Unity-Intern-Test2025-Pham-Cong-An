using System;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public int BoardX { get; private set; }

    public int BoardY { get; private set; }

    public Item Item { get; private set; }

    public Cell NeighbourUp { get; set; }

    public Cell NeighbourRight { get; set; }

    public Cell NeighbourBottom { get; set; }

    public Cell NeighbourLeft { get; set; }


    public bool IsEmpty => Item == null;
    #region new
    public bool CanClick { get; private set; }
    [SerializeField] private Collider2D selfCollider;
    [Header("Level Time Mode")]
    [SerializeField] private Vector3 originPos;
    [SerializeField] private bool canClickAgain = false;//init in time mode = true
    [SerializeField] private bool isOnBoard = true; // board or bar
    void Awake()
    {
        LoadComponents();
    }
    protected virtual void LoadComponents()
    {
        LoadCollider();
    }
    protected virtual void LoadCollider()
    {
        if (this.selfCollider != null) return;
        selfCollider = GetComponent<Collider2D>();

    }


    public void SetOriginPos(Vector3 origin)
    {
        this.originPos = origin;
    }
    public bool IsOnBarInTimeMode()
    {
        return !isOnBoard && canClickAgain;
    }
    public void OnClickHandle(Board board)
    {
        if (!canClickAgain)//click 1 time in normal mode
        {
            // selfCollider.enabled = false;
            EnableCollider(false);
            board.AddCellToBar(this);
            ApplyItemMoveToPosition();

            isOnBoard = false;
        }
        else // click multiple in time mode
        {
            if (isOnBoard)//go to bar
            {
                board.AddCellToBar(this);
                ApplyItemMoveToPosition();

                isOnBoard = false;
            }
            else //go to board
            {
                board.RemoveCellFromBar(this);
                ReturnToBoardPosition();

                isOnBoard = true;

            }
        }

    }



    #endregion
    public void Setup(int cellX, int cellY, bool isOnTimeMode = false)
    {
        this.BoardX = cellX;
        this.BoardY = cellY;
        this.canClickAgain = isOnTimeMode;
    }

    public bool IsNeighbour(Cell other)
    {
        return BoardX == other.BoardX && Mathf.Abs(BoardY - other.BoardY) == 1 ||
            BoardY == other.BoardY && Mathf.Abs(BoardX - other.BoardX) == 1;
    }


    public void Free()
    {
        Item = null;
    }

    public void Assign(Item item)
    {
        Item = item;
        Item.SetCell(this);
    }

    public void ApplyItemPosition(bool withAppearAnimation)
    {
        Item.SetViewPosition(this.transform.position);

        if (withAppearAnimation)
        {
            Item.ShowAppearAnimation();
        }
    }

    internal void Clear()
    {
        if (Item != null)
        {
            Item.Clear();
            Item = null;
        }
    }

    internal bool IsSameType(Cell other)
    {
        return Item != null && other.Item != null && Item.IsSameType(other.Item);
    }
    public void EnableCollider(bool enableValue)
    {
        this.selfCollider.enabled = enableValue;
    }
    internal void ExplodeItem()
    {
        if (Item == null) return;

        Item.ExplodeView();
        Item = null;
    }

    internal void AnimateItemForHint()
    {
        Item.AnimateForHint();
    }

    internal void StopHintAnimation()
    {
        Item.StopAnimateForHint();
    }

    internal void ApplyItemMoveToPosition()
    {
        Item.AnimationMoveToPosition();
    }
    internal void ReturnToBoardPosition()
    {
        transform.position = originPos;
        ApplyItemMoveToPosition();
    }
}
