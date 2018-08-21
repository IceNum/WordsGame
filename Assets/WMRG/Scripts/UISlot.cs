using UnityEngine;
using System.Collections;

public class UISlot : MonoBehaviour {

    public GameObject UITile;

    void Start() {
        UITile.GetComponent<UITile>().UISlot = gameObject;
    }
}
