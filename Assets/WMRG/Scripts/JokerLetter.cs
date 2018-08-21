using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JokerLetter : MonoBehaviour , IPointerClickHandler{

    private string _letter;
	void Start () {
        _letter = GetComponentInChildren<Text>().text;
    }


    public void OnPointerClick(PointerEventData data)
    {
        GameController.data.ApplyJokerTile(_letter);
    }
}
