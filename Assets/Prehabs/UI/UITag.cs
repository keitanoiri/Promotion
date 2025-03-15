using System.Collections;
using System.Collections.Generic;
using TMPro;
using Enums;
using UnityEngine;
using System;

public class UITag : MonoBehaviour
{
    [SerializeField] private GameObject MainText;
    [SerializeField] private GameObject TagText;
    [SerializeField] private GameObject TempMessage;
    public void SetTexts(string maintext,List<TYPE> tags)
    {
        MainText.GetComponent<TMP_Text>().text = maintext;
        string tagtext = "["+string.Join("][", tags.ConvertAll(x => x.ToString()).ToArray())+"]";
        TagText.GetComponent<TMP_Text>().text = tagtext;

        float ypos = 0;
        foreach(TYPE tag in tags)
        {
            GameObject tempmessage = Instantiate(TempMessage, this.transform);
            tempmessage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -ypos-30f);
            UITempMessagePanel temp = tempmessage.GetComponent<UITempMessagePanel>();
            string typetext = "[" + tag.ToString() + "]";
            temp.SetTexts(typetext, GetText2(tag),GetTextSize(tag));
            ypos += GetTextSize(tag).y;
        }
    }

    private string GetText2(TYPE tag) 
    {
        string rtext=null;
        switch (tag)
        {
            case TYPE.NONE:
                Debug.Log("NONEが選択されました");
                break;
            case TYPE.PAWN:
                rtext = "この駒は前方1マスにのみ移動できる\nこの駒は斜め前方にのみ攻撃できる";
                break;
            case TYPE.ROOK:
                rtext = "この駒は縦横任意の方向に移動、攻撃できる";
                break;
            case TYPE.KNIGHT:
                rtext = "この駒は2マス離れた8か所に移動、攻撃できる";
                break;
            case TYPE.BISHOP:
                rtext = "この駒は斜め任意の方向に移動、攻撃できる";
                break;
            case TYPE.QUEEN:
                rtext = "この駒は縦横斜め任意の方向に移動、攻撃できる";
                break;
            case TYPE.KING:
                rtext = "この駒は縦横斜め任意の方向に1マスのみ移動、攻撃できる";
                break;
            case TYPE.TILE:
                Debug.Log("TILEが選択されました");
                break;
            case TYPE.CURSED:
                rtext = "使用するとLIFEを1失う";
                break;
            default:
                Debug.Log("UNKNOWNが選択されました");
                break;
        }

        return rtext;
    }

    private Vector2 GetTextSize(TYPE tag)
    {
        Vector2 rsize;
        switch (tag)
        {
            case TYPE.PAWN:
                rsize = new Vector2(150, 85);
                break;
            case TYPE.KING:
                rsize = new Vector2(150, 70);
                break;
            default:
                rsize = new Vector2(150, 60);
                break;
        }

        return rsize;
    }
}
