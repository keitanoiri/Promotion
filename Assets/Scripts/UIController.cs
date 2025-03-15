using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class UIController : MonoBehaviour
{
    public GameObject obj;
    public GameObject textPrefab;
    public GameObject HPText;
    public GameObject APText;
    public GameObject TextWindow;

    public GameObject OptionWindow;

    public GameManager manager;
    public GameObject[] enemynumber;
    List<GameObject> enemynumbers;

    // Start is called before the first frame update
    void Start()
    {
        enemynumbers = new List<GameObject>();
        obj = GameObject.Find("GameManager");
        manager = obj.GetComponent<GameManager>();
    }

    public void ShowAP(int AP)
    {
        string stringtext = "AP:" + AP.ToString();
        APText.GetComponent<TMP_Text>().text = stringtext;
    }
    public void ShowHP(int HP)
    {
        string stringtext = "HP:" + HP.ToString();
        HPText.GetComponent<TMP_Text>().text = stringtext;
    }
    public void ChengeTextWindow(string stringtext)
    {
        TextWindow.GetComponent<TMP_Text>().text = stringtext;
    }


    public void TurnEnd()
    {
        manager.P_turn = false;
    }

    public void ShowOption()
    {
        GameObject Options = Instantiate(OptionWindow); 
    }
}
