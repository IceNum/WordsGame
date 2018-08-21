using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UITile : MonoBehaviour
{
    public GameObject UISlot;
    public GameObject boardTilePrefab;
    public GameObject boardTile;
    public GameObject targetSlot;
    public Vector2 lastPosition;
    public Text letterString;
    public Text letterScore;
    public bool dragging;
    public bool needSwap;
    public bool finished;

    public Transform testObj;

    private Transformer transformer;
    private float uiZaxis;

    void Start()
    {
        transformer = GetComponent<Transformer>();
        uiZaxis = Camera.main.transform.position.z + FindObjectOfType<Canvas>().planeDistance;
    }

    public void GetNewLetter() {
        if (Alphabet.data.LettersFeed.Count > 0)
        {
            letterString.text = Alphabet.data.GetRandomLetter();
            int score = Alphabet.data.GetLetterScore(letterString.text);
            letterScore.text = score.ToString();
            CreateNewBoardTile(letterString.text, score);
        }
        else
        {
            finished = true;
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (GameController.data.paused)
            return;
        if (dragging)
        {
            Vector3 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            cursorPos.z = uiZaxis;
            transform.position = cursorPos;
            GameController.data.letterDragging = true;
            gameObject.transform.SetAsLastSibling();

            if (Input.GetMouseButtonUp(0))
            {
                OnMouseUp();
            }
        }
    }

    void OnMouseDrag()
    {
        if (GameController.data.paused)
            return;
        dragging = true;
                
    }

    void OnMouseDown() {
        if (GameController.data.paused)
            return;
        if (CheckifOverUISlot())
        {
            targetSlot.GetComponent<UISlot>().UITile = null;
            targetSlot = null;
        }
          
    }

    void OnMouseUp()
    {
        if (GameController.data.swapMode)
        {
            SetSwapState(!needSwap);

            return;
        }

        if (GameController.data.paused)
        {
            return;
        }

        dragging = GameController.data.letterDragging = false;

        if (letterScore.text == "0" && GameController.data.canBePlant)
        {
            GameController.data.OpenSelectJokerLetter(boardTile);
            gameObject.SetActive(false);
        }
        else if (GameController.data.canBePlant)
        {
            GameController.data.PlantTile(boardTile);
            gameObject.SetActive(false);
        }
        else {
            if (CheckifOverUISlot())
            {
                GoToSlot(targetSlot);
            }
            else {
                MoveToPos(lastPosition);
            }
            GameController.data.PreApply();
        }
    }

    public void ReCreateTile()
    {
        GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        GetNewLetter();
        GoToFreeSlot();
    }

    void CreateNewBoardTile(string letter, int score) {
        boardTile = (GameObject)Instantiate(boardTilePrefab, new Vector3(99, 0, 0), Quaternion.identity);
        boardTile.tag = "BoardTile";
        GameController.data.BoardTiles.Add(boardTile);
        boardTile.GetComponent<BoardTile>().UIclone = gameObject;
        boardTile.GetComponent<BoardTile>().letter = letter;
        boardTile.GetComponent<BoardTile>().score = score;
        TextMesh[] txts = boardTile.GetComponentsInChildren<TextMesh>();
        txts[0].text = letter;
        txts[1].text = score.ToString();

        boardTile.SetActive(false);
    }

    public void GoToSlot(GameObject slot)
    {
        MoveToPos(slot.GetComponent<RectTransform>().anchoredPosition);
        
        if (slot.GetComponent<UISlot>().UITile != null)
        {
            slot.GetComponent<UISlot>().UITile.GetComponent<UITile>().GoToFreeSlot();
        }

        slot.GetComponent<UISlot>().UITile = gameObject;
        UISlot = slot;
        lastPosition = slot.GetComponent<RectTransform>().anchoredPosition;
    }

    public void GoToFreeSlot()
    {
        GameObject freeUISlot = GameController.data.GetFreeUISlot();
        if (GameController.data.GetFreeUISlot() != null)
        {
            MoveToPos(freeUISlot.GetComponent<RectTransform>().anchoredPosition);
            freeUISlot.GetComponent<UISlot>().UITile = gameObject;
            UISlot = freeUISlot;
            lastPosition = freeUISlot.GetComponent<RectTransform>().anchoredPosition;
        }
    }

    public void CancelTile() {
        Vector3 tempPos = boardTile.transform.position;
        tempPos.z = uiZaxis;
        transform.position = tempPos;
        if (UISlot.GetComponent<UISlot>().UITile == null)
        {
            MoveToPos(lastPosition);
            UISlot.GetComponent<UISlot>().UITile = gameObject;
        }
        else
            GoToFreeSlot();

        boardTile.SetActive(false);
    }

    void MoveToPos(Vector3 toPos)
    {
        gameObject.transform.parent.SetAsLastSibling();
        transformer.MoveUI(toPos, 0.25f);
    }

    public bool CheckifOverUISlot()
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);

        if (raycastResults.Count > 0)
        {
            foreach (RaycastResult res in raycastResults)
            {

                if (res.gameObject.name == "ErrorAlert")
                {
                    return false;
                }

                if (res.gameObject.tag == "UISlot")
                {
                    targetSlot = res.gameObject;
                    return true;
                }
            }
        }
        targetSlot = null;
        return false;
    }

    public void SetSwapState(bool swapState)
    {
        needSwap = swapState;

        if (needSwap)
            MoveToPos(GetComponent<RectTransform>().anchoredPosition + new Vector2(0, 60));
        else
            MoveToPos(GetComponent<RectTransform>().anchoredPosition - new Vector2(0, 60));
    }

 }
