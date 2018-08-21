using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;

public class wordDictionary : MonoBehaviour
{
    public TextAsset dict;
    public Dictionary<string,string> words = new Dictionary<string, string>();

    public InputField textInput;
    public Text textResult;
    public Text textWordsResult;
    public static wordDictionary data;
    void Start()
    {
        data = this;
        /* string[] separators = new[] { Environment.NewLine, "\t" }; */
        string[] separators = new[] { "\r\n", "\t" };
        string[] wordArray = dict.text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        int i = 0;
        foreach (string s in wordArray)
        {
            words.Add(i.ToString(), s);
            i++;
        }
		textWordsResult.text = i+" words loaded";
    }

    public void checkWord() {

        if (words.ContainsValue(textInput.text))
            textResult.text = textInput.text + " is CORRECT";
        else
            textResult.text = textInput.text + " is INCORRECT";
    }

    public bool hasWord(string word)
    {

        Debug.Log("checking "+word);
		word = word.ToLower();

        if (words.ContainsValue(word))
            return true;
        else
            return false;
    }
}