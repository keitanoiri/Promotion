using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITempMessagePanel : MonoBehaviour
{
    [SerializeField]private GameObject MainTextObj;
    [SerializeField] private GameObject SubTextObj;
    public void SetTexts(string maintext,string subtext,Vector2 size)
    {
        GetComponent<RectTransform>().sizeDelta = size;
        Vector2 moveAP = GetComponent<RectTransform>().anchoredPosition;
        moveAP.y = moveAP.y - (size.y/2);
        GetComponent<RectTransform>().anchoredPosition=moveAP;
        MainTextObj.GetComponent<TMP_Text>().text = maintext;
        SubTextObj.GetComponent<TMP_Text>().text = subtext;
    }

    //�g����
    /*  
 �@ tempmessage = Instantiate(TempTextWindow, this.transform);
    tempmessage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50f);
    UITempMessagePanel temp = tempmessage.GetComponent<UITempMessagePanel>();
    temp.SetTexts("�\������������", new Vector2(400, 200));
    */
}
