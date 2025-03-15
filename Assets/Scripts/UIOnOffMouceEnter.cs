using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;

public class UIOnOffMouceEnter : MonoBehaviour
{
    [SerializeField] private GameObject UIUnitTag;
    private GameObject TagUI;
    private UnitController thisUnitcs;
    private HandController thisHandcs;
    [SerializeField] private float DistanceX;
    [SerializeField] private float DisranceY;

    private void Start()
    {
        if (gameObject.tag == "Hand")
        {
            thisHandcs = GetComponent<HandController>();
        }else
        {
            thisUnitcs = GetComponent<UnitController>();
        }
    }
    private void OnMouseEnter()
    {
        //UIê›íu
        GameObject canvas = GameObject.Find("Canvas");
        TagUI = Instantiate(UIUnitTag, canvas.transform);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        screenPos.x = screenPos.x + DistanceX;
        screenPos.y = screenPos.y + DisranceY;
        TagUI.transform.position = screenPos;
        if (gameObject.tag == "Hand")
        {
            TagUI.GetComponent<UITag>().SetTexts(thisHandcs.THISNAME, thisHandcs.THISTYPE);
        }
        else
        {
            TagUI.GetComponent<UITag>().SetTexts(thisUnitcs.THISNAME, thisUnitcs.THISTYPE);
        }
    }

    private void OnMouseExit()
    {
        Destroy(TagUI);
    }

    private void OnDestroy()
    {
        Destroy(TagUI);
    }

}
