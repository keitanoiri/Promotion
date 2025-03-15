using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIEndButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject TempTextWindow;
    private GameObject tempmessage;
    public void OnPointerEnter(PointerEventData eventData)
    {
        tempmessage = Instantiate(TempTextWindow, this.transform);
        tempmessage.GetComponent<RectTransform>().anchoredPosition = new Vector2(270, 0);
        UITempMessagePanel temp = tempmessage.GetComponent<UITempMessagePanel>();
        temp.SetTexts("ターンを終える","自ターンには移動と取得した駒を使用することができます", new Vector2(150, 55));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Destroy(tempmessage);
    }
}
