using UnityEngine;
using System.Collections;

public class BoardTile : MonoBehaviour {

    public bool completed;
    public GameObject UIclone;
    public BoardSlot currentslot;
    public string letter;
    public int score;

    void OnMouseDown()
    {
        if(currentslot != null && !completed)
        {
            currentslot.free = true;
            currentslot = null;
        }
            
    }

    void OnMouseDrag()
    {
        if (completed)
            return;
        Vector3 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorPos.z = 0;
        UIclone.SetActive(true);
        UIclone.GetComponent<UITile>().dragging = true;
        UIclone.transform.position = cursorPos;
        GameController.data.letterDragging = true;
        gameObject.SetActive(false);
    }
}
