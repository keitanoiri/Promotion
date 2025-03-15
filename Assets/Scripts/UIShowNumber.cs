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
    public GameObject textPrefab; // �e�L�X�g�̃v���n�u
    private GameObject tempmessage;


    List<GameObject> instantiatedText;// ���������e�L�X�g�I�u�W�F�N�g
    public void Start()
    {
        instantiatedText = new List<GameObject>();
    }

        
    // �}�E�X��UI�I�u�W�F�N�g�ɓ������Ƃ��ɌĂяo����郁�\�b�h
    public void OnPointerEnter(PointerEventData eventData)
    {
        tempmessage = Instantiate(TempTextWindow, this.transform);
        tempmessage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50f);
        UITempMessagePanel temp = tempmessage.GetComponent<UITempMessagePanel>();
        temp.SetTexts("�s����","�G�͐��l���ɍs�����܂��B", new Vector2(150, 55));

        GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemys)
        {
            // �����ɃI�u�W�F�N�g���Ƃ̏������L�q
            // �V�����e�L�X�gUI�v�f�𐶐����ACanvas�̎q�v�f�Ƃ��Ĕz�u����
            GameObject newTextObject = Instantiate(textPrefab, transform);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(enemy.transform.position);
            newTextObject.transform.position = screenPos;

            instantiatedText.Add(newTextObject);
            // �V�����e�L�X�gUI�v�f�� Text �R���|�[�l���g�Ƀe�L�X�g��ݒ肷��
            string stringtext = Convert.ToString(enemy.GetComponent<UnitController>().number + 1);
            newTextObject.GetComponent<TMP_Text>().text = stringtext;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Destroy(tempmessage);
        foreach (GameObject text in instantiatedText)
        {
            Destroy(text);//���ׂĔj��
        }
        // �j���A���X�g���N���A����
        instantiatedText.Clear();
    }

}
