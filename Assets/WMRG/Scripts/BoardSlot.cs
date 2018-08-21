using UnityEngine;
using System.Collections;

public class BoardSlot : MonoBehaviour {

    public bool free;
    public bool completed;
    public float letterFactor = 1;
    public float wordFactor = 1;

    void Start()
    {
        free = true;

        TextMesh text = gameObject.GetComponentInChildren<TextMesh>();
        if(text != null)
        {
            switch (text.text)
            {
                case "TW":
                    wordFactor = 3;
                    break;
                case "TL":
                    letterFactor = 3;
                    break;
                case "DW":
                    wordFactor = 2;
                    break;
                case "DL":
                    letterFactor = 2;
                    break;
            }
        }
    }
    
}
