using System.Collections.Generic;
using UnityEngine;
using Enums;
using System.Collections;

public class UnitController : MonoBehaviour
{
    public List<TYPE> THISTYPE;
    public string THISNAME;
    public int[] index;
    public int number;//�^�C���̏ꍇ��-1 �v���C���[��-1
    private GameManager GameManager;

    public float speed = 15.0f; // �ړ����x
    public Vector3 targetPosition; // �ړ���̖ڕW�n�_

    private void Start()
    {
        GameObject manager = GameObject.Find("GameManager");
        GameManager = manager.GetComponent<GameManager>();
    }
    // Start is called before the first frame update
    public void SetUnit(List<TYPE> type, int[] ind,int num)
    {
        THISTYPE = type;
        THISNAME = type[0].ToString();
        index = ind;
        number = num;
    }

    //�ړ�����
    public void MoveUnit(int[] move_idx)
    {
        Vector3 pos = GameManager.ind_to_pos(move_idx);
        index = new int[] { move_idx[0], move_idx[1] };
        pos.y = 1;
        targetPosition = pos;
        // �R���[�`�����J�n����
        StartCoroutine(MoveToTarget());
        GameManager.DeleteCursol();
    }

    IEnumerator MoveToTarget()//�w��n�_�܂ňړ�����
    {
        // �ڕW�n�_�܂ł̕����x�N�g�������߂�
        Vector3 direction = (targetPosition - transform.position).normalized;

        // �ڕW�n�_�ɓ��B����܂Ń��[�v
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            // �w�肵�����x�ňړ�����
            transform.position += direction * speed * Time.deltaTime;

            // 1�t���[���ҋ@����
            yield return null;
        }

    }


}
