using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public static GameController data;
    public GameObject mainMenu;
    public GameObject GameUI;
    public GameObject GameOverUI;
    public GameObject UILettersPanel_p1, UILettersPanel_p2, UILettersPanel_p3, UILettersPanel_p4, ButtonsPanel;
    public List<GameObject> BoardSlots;
    public GameObject menuPanel;
    public GameObject SelectJokerLetter;
    public GameObject SwapBlock;
    public GameObject ErrorAlert;
    public GameObject confirmationDialog;
    public GameObject newPlayerTitle;
    public GameObject newScoreBlock;
    public Text letterLeftTxt;
    public Text currentPlayerTxt;
    public Text confirmationDialogTxt;
    public Text gameOverText;
    public InputField inputPlayersCount;
    public Toggle soundToggle;
    public bool dictChecking;
    public List<PlayerData> players;

    [HideInInspector]
    public bool uiTouched, letterDragging, canBePlant, paused, swapMode, pointerOverPanel;
    [HideInInspector]
    public List<GameObject> BoardTiles;

    private List<GameObject> UITileSlots;
    private List<GameObject> UITiles;
    private List<GameObject> boardTilesMatters;
    private GameObject targetBoardSlot;
    private GameObject tempJokerTile;
    private List<string> newWords;
    private List<string> addedWords;
    private List<int> newLetterIds;
    private string preApplyInfo;
    private string confirmationID;
    private int playersCount = 2;
    private int currentPlayer;
    private int currentScore;
    private int errorCode;
    private int skipCount;
    private float canvasWidth;

    [System.Serializable]
    public class PlayerData
    {
        public string name;
        public bool active;
        public bool complete;
        public int score;
        public Text scoreTxt;
        public GameObject UILettersPanel;
        public List<GameObject> UITileSlots;
        public List<GameObject> UITiles;
    }

    public void ClearPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("here");
    }

    void Start() {
        data = this;
        soundToggle.isOn = PlayerPrefs.GetInt("sound", 1) == 1 ? true : false;
        //StartGame();
    }

    public void StartGame()
    {
        ResetData();
        FitUIElements();
        FillUITiles();
        playersCount = int.Parse(inputPlayersCount.text);
        for (int i =0; i <= playersCount-1; i++)
        {
            players[i].active = true;
            players[i].scoreTxt.text = "0";
        }
        SwitchPlayer();
        UpdateTxts();
        mainMenu.SetActive(false);
        GameUI.SetActive(true);
        GameOverUI.SetActive(false);
    }

    void FitUIElements() {
        canvasWidth = GameObject.FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.width;
        float canvasHeight = GameObject.FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.height;
        float slotSize = canvasWidth / 7.0f;

        float ratio = (float)Screen.width / Screen.height;
        ButtonsPanel.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1.0f - ratio);
        menuPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(canvasWidth, canvasHeight);
        menuPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-canvasWidth, 0);
        menuPanel.SetActive(true);

        for (int i = 0; i <= playersCount - 1; i++)
        {
            float dx = -3 * slotSize;
            foreach (GameObject slot in players[i].UITileSlots)
            {
                RectTransform slotRT = slot.GetComponent<RectTransform>();
                slotRT.anchoredPosition = new Vector2(dx, 0);
                dx += slotSize;
                slotRT.sizeDelta = new Vector2(slotSize, slotSize);
                slot.GetComponent<UISlot>().UITile.GetComponent<RectTransform>().sizeDelta = new Vector2(slotSize - 2, slotSize - 2);
                slot.GetComponent<UISlot>().UITile.GetComponent<BoxCollider2D>().size = new Vector2(slotSize - 2, slotSize - 2);
                slot.GetComponent<UISlot>().UITile.GetComponent<RectTransform>().anchoredPosition = slot.GetComponent<RectTransform>().anchoredPosition;
                slot.GetComponent<UISlot>().UITile.GetComponent<UITile>().lastPosition = slot.GetComponent<RectTransform>().anchoredPosition;
            }

            players[i].UILettersPanel.SetActive(true);
            players[i].UILettersPanel.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1.0f - ratio);
        }
    }

    void FillUITiles()
    {
        for (int i = 0; i <= playersCount - 1; i++)
        {
            foreach (GameObject slot in players[i].UITileSlots)
            {
                players[i].UITiles.Add(slot.GetComponent<UISlot>().UITile);
            }

            foreach (GameObject tile in players[i].UITiles)
            {
                tile.SetActive(true);
                tile.GetComponent<UITile>().GetNewLetter();
            }
        }

    }

    void Update()
    {
        if (Input.GetKey("escape"))
            Application.Quit();
        if (Input.GetKey(KeyCode.R))
            SceneManager.LoadScene(0);
        if (paused)
            return;
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                uiTouched = true;
        }
#endif

#if UNITY_ANDROID
        if (Input.touchCount == 1)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                uiTouched = true;
        }
#endif
        if (Input.GetMouseButtonUp(0))
            uiTouched = false;


        if (letterDragging)
        {
            if (newScoreBlock.activeInHierarchy)
                newScoreBlock.SetActive(false);
            CheckifPointerOverPanel();
            if (pointerOverPanel)
            {
                canBePlant = false;
                return;
            }
                
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, 1 << 9);
            if (hit.collider != null)
            {
                targetBoardSlot = hit.collider.gameObject;
                if (targetBoardSlot.GetComponent<BoardSlot>().free)
                    canBePlant = true;
                else
                    canBePlant = false;
            }
            else
            {
                canBePlant = false;
            }
        }
    }

    public void CheckifPointerOverPanel()
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);
        pointerOverPanel = false;

        if (raycastResults.Count > 0)
        {
            foreach (RaycastResult res in raycastResults)
            {
                if (res.gameObject.tag == "UIPanel")
                    pointerOverPanel = true;
            }
        }
    }

    public void PlantTile(GameObject tile)
    {
        tile.transform.parent = targetBoardSlot.transform;
        tile.transform.localPosition = new Vector3(0,0,-0.1f);
        tile.SetActive(true);
        tile.GetComponent<BoardTile>().currentslot = targetBoardSlot.GetComponent<BoardSlot>();
        targetBoardSlot.GetComponent<BoardSlot>().free = false;
        Camera.main.BroadcastMessage("ZoomIn", targetBoardSlot.transform.position);
        PreApply();
        SoundController.data.playTap();
    }

    public GameObject GetFreeUISlot()
    {
        foreach(GameObject slot in UITileSlots)
        {
            if (slot.GetComponent<UISlot>().UITile == null)
                return slot;
        }
        return null;
    }

    public void CancelLetters()
    {
        foreach (GameObject tile in UITiles)
        {
            if (!tile.activeInHierarchy && !tile.GetComponent<UITile>().finished)
            {
                tile.SetActive(true);
                tile.GetComponent<UITile>().CancelTile();
            }
        }

        foreach (GameObject bs in BoardSlots) {
            if(bs.GetComponent<BoardSlot>().completed != true)
                bs.GetComponent<BoardSlot>().free = true;
        }

        GameController.data.PreApply();
    }

    public void ShuffleUITiles()
    {
        foreach (GameObject slot in UITileSlots)
        {
            slot.GetComponent<UISlot>().UITile = null;
        }

        for (int i = 0; i < UITiles.Count; i++)
        {
            if (UITiles[i] != null)
            {
                GameObject tempObj = UITiles[i];
                int randomIndex = UnityEngine.Random.Range(i, UITiles.Count);
                UITiles[i] = UITiles[randomIndex];
                UITiles[randomIndex] = tempObj;
            }
        }

        for (int i = 0; i < UITiles.Count; i++)
        {
            UITiles[i].GetComponent<UITile>().GoToSlot(UITileSlots[i]);
        }
    }

    public void OpenSelectJokerLetter(GameObject jt)
    {
        paused = true;
        tempJokerTile = jt;
        SelectJokerLetter.SetActive(true);
    }

    public void ApplyJokerTile(string letter)
    {
        tempJokerTile.GetComponent<BoardTile>().letter = letter;
        tempJokerTile.GetComponentInChildren<TextMesh>().text = letter;
        PlantTile(tempJokerTile);
        SelectJokerLetter.SetActive(false);
        paused = false;
    }

    public void PreApply()
    {
        errorCode = 0;
        preApplyInfo = "";
        newScoreBlock.SetActive(false);
        newLetterIds = new List<int>();
        newWords = new List<string>();
        boardTilesMatters = new List<GameObject>();
        bool firstWord = true;
        //Checking if tiles are not alone
        for (int i = 0; i < BoardSlots.Count; i++)
        {
            if (!BoardSlots[i].GetComponent<BoardSlot>().free && !BoardSlots[i].GetComponent<BoardSlot>().completed)
            {
                if (!CheckIfNewTileConnected(i))
                {
                    errorCode = 1;
                    return;
                }
                newLetterIds.Add(i);
            }

            if (BoardSlots[i].GetComponent<BoardSlot>().completed && firstWord)
                firstWord = false;
        }

        if (newLetterIds.Count == 0)
            return;

        //Check if first word intersects center tile
        
        if (firstWord)
        {
            bool correct = false;
            foreach (int id in newLetterIds)
            {
                if (id == 112)
                    correct = true;
            }

            if (!correct)
            {
                errorCode = 2;
                return;
            }
        }

        //Checking if new tiles are at one line
        int prevX = 0;
        int prevY = 0;
        bool horizontal = false;
        bool vertical = false;

        foreach (int id in newLetterIds)
        {
            int x = id / 15 + 1;
            int y = (id + 1) % 15 == 0 ? 15 : (id + 1) % 15;

            if (prevX == 0 && prevY == 0)
            {
                prevX = x;
                prevY = y;
            }
            else
            {
                if (x == prevX && !vertical)
                {
                    horizontal = true;
                }
                else if (y == prevY && !horizontal)
                {
                    vertical = true;
                }
                else
                {
                    errorCode = 3;
                    return;
                }
            }
        }

        //Checking if a free space between letters
        int firstNewId = newLetterIds[0];
        if (horizontal)
        {
            for (int i = firstNewId; i < newLetterIds[newLetterIds.Count - 1]; i++)
            {
                if (BoardSlots[i].GetComponent<BoardSlot>().free)
                {
                    errorCode = 4;
                    return;
                }
            }
        }

        //Check if new tile contact old tile
        bool haveConnect = false;

        foreach (int id in newLetterIds)
        {
            if (CheckIfNewTileConnectsOld(id))
                haveConnect = true;
        }

        if (!haveConnect && !firstWord)
        {
            errorCode = 5;
            return;
        }



        //Buildig words and scores
        currentScore = 0;
        newWords = new List<string>();
        foreach (int id in newLetterIds)
        {
            int i;
            for (i = id; i > 0; i -= 15)
            {
                GameObject topSlot = GetАdjacentSlot(i, "top");
                if (!topSlot || topSlot.GetComponent<BoardSlot>().free)
                    break;
            }
            if (VerticalWord(i).Length > 1 && !newWords.Contains(VerticalWord(i)))
            {
                newWords.Add(VerticalWord(i));
                currentScore += GetVerticalScore(i);
            }
            

            int y;
            for (y = id; y > 0; y--)
            {
                GameObject leftSlot = GetАdjacentSlot(y, "left");
                if (!leftSlot || leftSlot.GetComponent<BoardSlot>().free)
                    break;
            }
            if (HorizontalWord(y).Length > 1 && !newWords.Contains(HorizontalWord(y)))
            {
                newWords.Add(HorizontalWord(y));
                currentScore += GetHorizontalScore(y);
            }
                
        }
        string newWordsList = "";
        foreach(string word in newWords)
        {
            newWordsList += word + ", ";
        }
        newWordsList = newWordsList.Remove(newWordsList.Length - 2);
        preApplyInfo = "APPLY '" + newWordsList + "' for " + currentScore + " scores?";

        newScoreBlock.SetActive(true);
        newScoreBlock.transform.position = boardTilesMatters[boardTilesMatters.Count - 1].transform.position + new Vector3(0.5f,-0.5f,-0.4f);
        newScoreBlock.GetComponentInChildren<TextMesh>().text = currentScore.ToString();
        newScoreBlock.GetComponent<Transformer>().ScaleImpulse(new Vector3(0.6f, 0.6f, 1), 0.15f, 1);
        //Debug.Log(currentScore);
    }

    public void ApplyTurn() {
        string info = "";

        if (newWords.Count == 0 && errorCode == 0)
            errorCode = 7;
        
        newWords = newWords.Distinct().ToList();

        //CHECKING NEW WORD WITH DICTIONARY
        if (dictChecking)
        {
            info += "\r\n";
            foreach (string word in newWords)
            {

                if (wordDictionary.data != null && !wordDictionary.data.hasWord(word))
                {
                    info += word + " ";
                    errorCode = 8;
                }

                if (addedWords.Contains(word))
                {
                    info = word;
                    errorCode = 9;
                }
            }
        }

        if (errorCode != 0)
        {
            ShowErrorAlert(errorCode, info);
            return;
        } else
        {
            foreach (string word in newWords)
                addedWords.Add(word);
        }

        //APPLYING WORD
        foreach (int id in newLetterIds)
        {
            BoardSlots[id].GetComponent<BoardSlot>().completed = true;
            BoardSlots[id].GetComponentInChildren<BoardTile>().completed = true;
        }

        foreach(GameObject bTile in boardTilesMatters)
        {
            bTile.GetComponent<Transformer>().ScaleImpulse(new Vector3(1.2f, 1.2f, 1), 0.125f, 1);
        }

        foreach (GameObject tile in UITiles)
        {
            if (!tile.activeInHierarchy)
            {
                tile.SetActive(true);
                tile.GetComponent<UITile>().ReCreateTile();
            }
        }

        int tilesLeft = 0;
        foreach (GameObject uiTile in UITiles)
        {
            if (uiTile.activeInHierarchy)
                tilesLeft++;
        }

        if (tilesLeft == 0)
            players[currentPlayer - 1].complete = true;

        players[currentPlayer-1].score += currentScore;
        skipCount = 0;
        UpdateTxts();

        Invoke("SwitchPlayer", 0.35f);
        SoundController.data.playApply();
    }

    public void ShowApplyConfirmDialog()
    {
        if (errorCode == 0 && preApplyInfo.Length > 0)
            ShowConfirmationDialog("ApplyTurn");
        else
            ApplyTurn();
    }

    bool CheckIfNewTileConnected(int tileId) {
        GameObject topTile = GetАdjacentSlot(tileId, "top");
        if (topTile != null && !topTile.GetComponent<BoardSlot>().free)
            return true;
        GameObject rightTile = GetАdjacentSlot(tileId, "right");
        if (rightTile != null && !rightTile.GetComponent<BoardSlot>().free)
            return true;
        GameObject bottomTile = GetАdjacentSlot(tileId, "bottom");
        if (bottomTile != null && !bottomTile.GetComponent<BoardSlot>().free)
            return true;
        GameObject leftTile = GetАdjacentSlot(tileId, "left");
        if (leftTile != null && !leftTile.GetComponent<BoardSlot>().free)
            return true;

        return false;
    }

    bool CheckIfNewTileConnectsOld(int tileId)
    {
        GameObject topTile = GetАdjacentSlot(tileId, "top");
        if (topTile != null && topTile.GetComponent<BoardSlot>().completed)
            return true;
        GameObject rightTile = GetАdjacentSlot(tileId, "right");
        if (rightTile != null && rightTile.GetComponent<BoardSlot>().completed)
            return true;
        GameObject bottomTile = GetАdjacentSlot(tileId, "bottom");
        if (bottomTile != null && bottomTile.GetComponent<BoardSlot>().completed)
            return true;
        GameObject leftTile = GetАdjacentSlot(tileId, "left");
        if (leftTile != null && leftTile.GetComponent<BoardSlot>().completed)
            return true;

        return false;
    }

    public GameObject GetАdjacentSlot(int tileId, string pos)
    {
        switch (pos)
        {
            case "top":
                int topTileID = tileId - 15;
                if (topTileID + 1 > 0)
                {
                    return BoardSlots[topTileID];
                }
                break;
            case "right":
                int rightTileId = tileId + 1;
                if ((rightTileId) % 15 != 0)
                {
                    return BoardSlots[rightTileId];
                }
                break;
            case "bottom":
                int bottomTileId = tileId + 15;
                if (bottomTileId + 1 < 226)
                {
                    return BoardSlots[bottomTileId];
                }
                break;
            case "left":
                int leftTileId = tileId - 1;
                if ((leftTileId+1) % 15 != 0)
                {
                    return BoardSlots[leftTileId];
                }
                break;
        }
        return null;
    }

    public string VerticalWord(int firstId)
    {
        string word = "";
        for(int i = firstId; i < 225; i += 15)
        {
            if (BoardSlots[i] && !BoardSlots[i].GetComponent<BoardSlot>().free)
            {
                word += BoardSlots[i].GetComponentInChildren<BoardTile>().letter;
                if (!boardTilesMatters.Contains(BoardSlots[i].GetComponentInChildren<BoardTile>().gameObject))
                    boardTilesMatters.Add(BoardSlots[i].GetComponentInChildren<BoardTile>().gameObject);
                else
                    boardTilesMatters.Remove(BoardSlots[i].GetComponentInChildren<BoardTile>().gameObject); boardTilesMatters.Add(BoardSlots[i].GetComponentInChildren<BoardTile>().gameObject);
                if (i + 15 > 224)
                    return word;
            }
            else
            {
                return word;
            }
        }
        return "";
    }

    public string HorizontalWord(int firstId)
    {
        string word = "";
        for (int i = firstId; i < 225; i++)
        {
            if (BoardSlots[i] && !BoardSlots[i].GetComponent<BoardSlot>().free)
            {
                word += BoardSlots[i].GetComponentInChildren<BoardTile>().letter;
                if (!boardTilesMatters.Contains(BoardSlots[i].GetComponentInChildren<BoardTile>().gameObject))
                    boardTilesMatters.Add(BoardSlots[i].GetComponentInChildren<BoardTile>().gameObject);
                else
                    boardTilesMatters.Remove(BoardSlots[i].GetComponentInChildren<BoardTile>().gameObject); boardTilesMatters.Add(BoardSlots[i].GetComponentInChildren<BoardTile>().gameObject);
                if ((i + 1) % 15 == 0)
                    return word;
            }
            else
            {
                return word;
            }
        }
        return word;
    }

    public int GetVerticalScore(int firstId)
    {
        int score = 0;
        int wordFactor = 1;
        for (int i = firstId; i < 225; i += 15)
        {
            BoardSlot boardSlot = BoardSlots[i].GetComponent<BoardSlot>();

            if (BoardSlots[i] && !boardSlot.free)
            {
                if (!boardSlot.completed)
                {
                    //score += Alphabet.data.GetLetterScore(BoardSlots[i].GetComponentInChildren<BoardTile>().letter) * (int)boardSlot.letterFactor;
                    score += BoardSlots[i].GetComponentInChildren<BoardTile>().score * (int)boardSlot.letterFactor;
                    if ((int)boardSlot.wordFactor > 1)
                        wordFactor *= (int)boardSlot.wordFactor;
                } else
                {
                    //score += Alphabet.data.GetLetterScore(BoardSlots[i].GetComponentInChildren<BoardTile>().letter);
                    score += BoardSlots[i].GetComponentInChildren<BoardTile>().score;
                }

                if (i + 15 > 224)
                    break;
            }
            else
            {
                break;
            }
        }
        return score * wordFactor;
    }

    public int GetHorizontalScore(int firstId)
    {
        int score = 0;
        int wordFactor = 1;
        for (int i = firstId; i < 225; i++)
        {
            BoardSlot boardSlot = BoardSlots[i].GetComponent<BoardSlot>();

            if (!BoardSlots[i].GetComponent<BoardSlot>().free)
            {

                if (!boardSlot.completed)
                {
                    //score += Alphabet.data.GetLetterScore(BoardSlots[i].GetComponentInChildren<BoardTile>().letter) * (int)boardSlot.letterFactor;
                    score += BoardSlots[i].GetComponentInChildren<BoardTile>().score * (int)boardSlot.letterFactor;
                if ((int)BoardSlots[i].GetComponent<BoardSlot>().wordFactor > 1)
                    wordFactor *= (int)BoardSlots[i].GetComponent<BoardSlot>().wordFactor;
                }
                else
                {
                    score += BoardSlots[i].GetComponentInChildren<BoardTile>().score;
                }
                if ((i + 1) % 15 == 0)
                    break;
            }
            else
            {
                break;
            }
        }
        return score * wordFactor;
    }

    public void ShowErrorAlert(int code, string info) {
        paused = true;
        string errorText = "";
        switch (code)
        {
            case 1:
                errorText = "TILES SHOULD BE CONNECTED!";
                break;
            case 2:
                errorText = "FIRST WORD SHOULD INTERSECT CENTER TILE!";
                break;
            case 3:
                errorText = "TILES SHOULD BE IN 1 LINE!";
                break;
            case 4:
                errorText = "TILES SHOULD NOT HAVE SPACES!";
                break;
            case 5:
                errorText = "NO CONNECTION WITH OLD TILES!";
                break;
            case 6:
                errorText = "NOT ENOUGH FREE TILES";
                break;
            case 7:
                errorText = "YOU NEED TO PLACE TILES FIRST!";
                break;
            case 8:
                errorText = "INCORRECT WORDS: " + info;
                break;
            case 9:
                errorText = "ALREADY USED WORD: " + info;
                break;
        }
        ErrorAlert.SetActive(true);
        ErrorAlert.GetComponentInChildren<Text>().text = errorText;
    }

    public void CloseErrorAlert()
    {
        ErrorAlert.SetActive(false);
        paused = false;
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    public void EnableSwapMode()
    {
        paused = swapMode =  true;
        CancelLetters();
        SwapBlock.SetActive(true);
    }

    public void ApplySwap()
    {
        List<GameObject> tilesToSwap = new List<GameObject>();
        List<string> oldLetters = new List<string>();

        foreach (GameObject uiTile in UITiles)
        {
            if (uiTile.GetComponent<UITile>().needSwap)
            {
                tilesToSwap.Add(uiTile);
                oldLetters.Add(uiTile.GetComponent<UITile>().letterString.text);
            }
        }

        if (tilesToSwap.Count == 0)
        {
            DisableSwapMode();
            return;
        }

        if (tilesToSwap.Count > Alphabet.data.LettersFeed.Count)
        {
            DisableSwapMode();
            ShowErrorAlert(6, "");
            return;
        }


            foreach (GameObject uiTile in tilesToSwap)
        {
            uiTile.GetComponent<UITile>().GetNewLetter();
        }

        foreach (string letter in oldLetters)
        {
            Alphabet.data.LettersFeed.Add(letter);
        }

        Invoke("DisableSwapMode", 0.15f);
        Invoke("SwitchPlayer", 0.5f);
        skipCount = 0;
    }

    public void DisableSwapMode()
    {
        paused = swapMode = false;
        SwapBlock.SetActive(false);

        foreach (GameObject uiTile in UITiles)
        {
            if (uiTile.GetComponent<UITile>().needSwap)
            {
                uiTile.GetComponent<UITile>().SetSwapState(false);
            }
        }
    }

    void UpdateTxts()
    {
        players[currentPlayer - 1].scoreTxt.text = players[currentPlayer - 1].score.ToString();
        letterLeftTxt.text = Alphabet.data.LettersFeed.Count.ToString() + " LETTERS LEFT";
    }

    public void SwitchPlayer()
    {
        if (CheckForGameOver())
        {
            GameOver();
            return;
        }
            

        currentPlayer = currentPlayer + 1 <= 4 ? currentPlayer + 1 : 1;
        if (!players[currentPlayer - 1].active || players[currentPlayer - 1].complete)
        {
            SwitchPlayer();
            return;
        }

        for (int i = 1; i <= players.Count; i++)
        {
            if (i == currentPlayer)
            {
                players[i - 1].UILettersPanel.SetActive(true);
                players[i - 1].scoreTxt.text += "←";
            }
            else
            {
                players[i - 1].UILettersPanel.SetActive(false);
                players[i - 1].scoreTxt.text.Trim((new System.Char[] { '←' }));
            }
        }


        UITileSlots = players[currentPlayer - 1].UITileSlots;
        UITiles = players[currentPlayer - 1].UITiles;

        currentPlayerTxt.text = "Player " + currentPlayer;
        PreApply();
        //Showing Title
        newPlayerTitle.GetComponentInChildren<Text>().text = "PLAYER'S " + currentPlayer + " MOVE";
        newPlayerTitle.GetComponent<Transformer>().MoveUIImpulse(Vector2.zero, 1, 1);

        UpdateTxts();
    }

    public void SkipTurn()
    {
        CancelLetters();
        Invoke("SwitchPlayer", 0.35f);
    }

    public void GiveUp()
    {
        Debug.Log("Player "+currentPlayer +" gived up!");

        foreach (GameObject uiTile in UITiles)
            Alphabet.data.LettersFeed.Add(uiTile.GetComponent<UITile>().letterString.text);

        SkipTurn();
        playersCount -= 1;
        players[currentPlayer - 1].active = false;
    }

    public void ShowConfirmationDialog(string confirmationID)
    {
        this.confirmationID = confirmationID;
        switch (confirmationID)
        {
            case "ApplyTurn":
                confirmationDialogTxt.text = preApplyInfo;
                break;
            case "SkipTurn":
                confirmationDialogTxt.text = "Are you sure to skip turn?";
                break;
            case "GiveUp":
                confirmationDialogTxt.text = "Give up?";
                break;
        }
        confirmationDialog.SetActive(true);
        switchMenu(false);
    }

    public void ConfirmDialog()
    {
        switch (confirmationID)
        {
            case "ApplyTurn":
                ApplyTurn();
                break;
            case "SkipTurn":
                skipCount++;
                SkipTurn();
                break;
            case "GiveUp":
                Invoke("GiveUp", 0.35f);
                break;
        }
        confirmationDialog.SetActive(false);
    }

    public void switchMenu(bool state)
    {
        if(state)
            menuPanel.GetComponent<Transformer>().MoveUI(new Vector2(0,0), 0.5f);
        else
            menuPanel.GetComponent<Transformer>().MoveUI(new Vector2(-canvasWidth, 0), 0.5f);
    }

    public void switchSound()
    {
        PlayerPrefs.SetInt("sound", PlayerPrefs.GetInt("sound", 1) == 1 ? 0 : 1);
        soundToggle.isOn = PlayerPrefs.GetInt("sound", 1) == 1 ? true : false;
        AudioListener.volume = PlayerPrefs.GetInt("sound", 1);
        //Debug.Log("volume set to "+ AudioListener.volume);
    }

    public void ChangePlayerCount(int delta)
    {
        if (playersCount + delta > 4)
            playersCount = 2;
        else if (playersCount + delta < 2)
            playersCount = 4;
        else
            playersCount += delta;
        inputPlayersCount.text = playersCount.ToString();
    }

    public void ResetData()
    {
        foreach (GameObject go in BoardTiles)
            Destroy(go);
        BoardTiles = new List<GameObject>();

        foreach (GameObject go in BoardSlots)
        {
            go.GetComponentInChildren<BoardSlot>().free = true;
            go.GetComponentInChildren<BoardSlot>().completed = false;
        }

        for (int i = 0; i <= 3; i++)
        {
            players[i].score = 0;
            players[i].active = false;
            players[i].complete = false;
            players[i].scoreTxt.text = "-";
            players[i].UITiles = new List<GameObject>();
        }

        currentPlayer = 0;
        addedWords = new List<string>();
        Alphabet.data.ResetFeed();
    }

    public void GoToMainMenu()
    {
        mainMenu.SetActive(true);
        GameOverUI.SetActive(false);
    }

    public void GameOver()
    {
        SoundController.data.playFinish();
        if (playersCount == 1)
        {
            GameOverUI.SetActive(true);
            gameOverText.text = "PLAYER " + currentPlayer + " WINS!";
            return;
        } else
        {
            int winnerPlayer = 0;
            int maxScore = 0;
            for (int i = 0; i <= 3; i++)
            {
                if(players[i].active && players[i].score > maxScore)
                {
                    maxScore = players[i].score;
                    winnerPlayer = i + 1;
                }

            }
            GameOverUI.SetActive(true);
            gameOverText.text = "PLAYER " + winnerPlayer + " WINS!";
        }

    }

    public bool CheckForGameOver()
    {
        foreach (PlayerData pd in players)
        {
            if (pd.complete)
                return true;
        }
        if (playersCount == 1 || skipCount == playersCount*2)
            return true;
        else
            return false;
    }
}
