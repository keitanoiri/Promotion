using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;

public class UIshowNumber : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    [SerializeField]private GameObject TempTextWindow;
    public GameObject textPrefab; // テキストのプレハブ
    private GameObject tempmessage;


    List<GameObject> instantiatedText;// 生成したテキストオブジェクト
    public void Start()
    {
        instantiatedText = new List<GameObject>();
    }

        
    // マウスがUIオブジェクトに入ったときに呼び出されるメソッド
    public void OnPointerEnter(PointerEventData eventData)
    {
        tempmessage = Instantiate(TempTextWindow, this.transform);
        tempmessage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50f);
        UITempMessagePanel temp = tempmessage.GetComponent<UITempMessagePanel>();
        temp.SetTexts("行動順","敵は数値順に行動します。", new Vector2(150, 55));

        GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemys)
        {
            // ここにオブジェクトごとの処理を記述
            // 新しいテキストUI要素を生成し、Canvasの子要素として配置する
            GameObject newTextObject = Instantiate(textPrefab, transform);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(enemy.transform.position);
            newTextObject.transform.position = screenPos;

            instantiatedText.Add(newTextObject);
            // 新しいテキストUI要素の Text コンポーネントにテキストを設定する
            string stringtext = Convert.ToString(enemy.GetComponent<UnitController>().number + 1);
            newTextObject.GetComponent<TMP_Text>().text = stringtext;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Destroy(tempmessage);
        foreach (GameObject text in instantiatedText)
        {
            Destroy(text);//すべて破壊
        }
        // 破壊後、リストをクリアする
        instantiatedText.Clear();
    }

}
