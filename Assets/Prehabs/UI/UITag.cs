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
                Debug.Log("NONE���I������܂���");
                break;
            case TYPE.PAWN:
                rtext = "���̋�͑O��1�}�X�ɂ݈̂ړ��ł���\n���̋�͎΂ߑO���ɂ̂ݍU���ł���";
                break;
            case TYPE.ROOK:
                rtext = "���̋�͏c���C�ӂ̕����Ɉړ��A�U���ł���";
                break;
            case TYPE.KNIGHT:
                rtext = "���̋��2�}�X���ꂽ8�����Ɉړ��A�U���ł���";
                break;
            case TYPE.BISHOP:
                rtext = "���̋�͎΂ߔC�ӂ̕����Ɉړ��A�U���ł���";
                break;
            case TYPE.QUEEN:
                rtext = "���̋�͏c���΂ߔC�ӂ̕����Ɉړ��A�U���ł���";
                break;
            case TYPE.KING:
                rtext = "���̋�͏c���΂ߔC�ӂ̕�����1�}�X�݈̂ړ��A�U���ł���";
                break;
            case TYPE.TILE:
                Debug.Log("TILE���I������܂���");
                break;
            case TYPE.CURSED:
                rtext = "�g�p�����LIFE��1����";
                break;
            default:
                Debug.Log("UNKNOWN���I������܂���");
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
