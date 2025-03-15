using System.Collections;
using UnityEngine;
using Enums;
using System;
using TMPro;
using System.Collections.Generic;



public class HandController : MonoBehaviour
{
    [SerializeField] private AudioSource Audio;//AudioSource�^�̕ϐ�a��錾 �g�p����AudioSource�R���|�[�l���g���A�^�b�`�K�v
    [SerializeField] private AudioClip SE_Serect;//AudioClip�^�̕ϐ�b2��錾 �g�p����AudioClip���A�^�b�`�K�v 

    public List<TYPE> THISTYPE;
    public string THISNAME;
    public GameObject Canvas;
    private GameObject Player;
    bool Serecr_Sucssesed;
    private GameManager gameManager;
    private float distance = 10f; // �J��������̋���
    private float speed = 100.0f; // �ړ����x
    private Vector3 targetPosition; // �ړ���̖ڕW�n�_
    private float additionalZRotation = 45f; // �ǉ���Z����]�p�x
    private Vector3 oldpos;

    // Start is called before the first frame update
    void Start()
    {
        THISNAME = THISTYPE[0].ToString();
    }

    private void Awake()
    {
        oldpos = transform.position;
        Canvas.SetActive(false);
        Serecr_Sucssesed = false;
        GameObject gamemanager = GameObject.Find("GameManager");
        gameManager= gamemanager.GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseEnter()
    {
        if (!THISTYPE.Contains(TYPE.TILE))
        {
            Audio.PlayOneShot(SE_Serect);
            Vector3 pos = transform.position;
            pos.y = pos.y + 0.5f;
            transform.position = pos;
        }
    }

    // �}�E�X�J�[�\�����I�u�W�F�N�g���痣�ꂽ�Ƃ��ɌĂяo����郁�\�b�h
    private void OnMouseExit()
    {
        if (!THISTYPE.Contains(TYPE.TILE))
        {
            Vector3 pos = transform.position;
            pos.y = pos.y - 0.5f;
            transform.position = pos;
        }
    }

    public IEnumerator OnSerected(GameObject player)
    {
        GetComponent<BoxCollider>().enabled = false;
        yield return StartCoroutine(MoveToTarget());

        Player = player;
        Canvas.SetActive(true);
        while (Serecr_Sucssesed == false)
        {
            yield return null;

            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraRight = Camera.main.transform.right;
            Vector3 cameraUp = Camera.main.transform.up;
            targetPosition = Camera.main.transform.position + cameraForward * distance;
            targetPosition = targetPosition + cameraRight * 0.5f;
            targetPosition = targetPosition - cameraUp * 0.5f;
            transform.position = targetPosition;

            // �J�����̉�]��GameObject�ɃR�s�[����
            transform.rotation = Camera.main.transform.rotation;
            // �ǉ���Z����]��K�p����
            transform.Rotate(Vector3.forward, additionalZRotation);

            if (Input.GetMouseButtonDown(1))
            {
                Canvas.SetActive(false);
                yield return StartCoroutine(MoveToCancel());
                GetComponent<BoxCollider>().enabled = true;

                break;
            }
        }
        if(Serecr_Sucssesed==true)
        { 
            Destroy(gameObject);
        }

    }

    public void OnClickAbility()
    {
        gameManager.Use_Skill=true;
        foreach (TYPE H_type in THISTYPE)
        {
            switch (H_type)
            {
                case TYPE.PAWN:
                    // PAWN�̏���
                    gameManager.AddStatus(STATUS.Skill_PAWN);
                    break;
                case TYPE.ROOK:
                    // ROOK�̏���
                    gameManager.AddStatus(STATUS.Skill_ROOK);
                    break;
                case TYPE.KNIGHT:
                    // KNIGHT�̏���
                    gameManager.AddStatus(STATUS.Skill_KNIGHT);
                    break;
                case TYPE.BISHOP:
                    // BISHP�̏���
                    gameManager.AddStatus(STATUS.SKill_BISHP);
                    break;
                case TYPE.QUEEN:
                    // QUEEN�̏���
                    gameManager.AddStatus(STATUS.SKill_QUEEN);
                    break;
                case TYPE.KING:
                    // KING�̏���
                    gameManager.AddStatus(STATUS.SKill_KING);
                    break;
                case TYPE.CURSED:
                    // �􂢁iHP-1)
                    gameManager.Life--;
                    break;
                default:
                    // ��L�̂�����ɂ��Y�����Ȃ��ꍇ�̏���
                    break;
            }
        }

        Serecr_Sucssesed = true;
    }

    public void OnClickMove()//type��ύX����
    {
        bool check = false;
        foreach (TYPE H_type in THISTYPE)
        {
            switch (H_type)
            {
                case TYPE.CURSED:
                    // �􂢁iHP-1)
                    gameManager.Life--;
                    check = true;
                    break;
                default:
                    // ��L�̂�����ɂ��Y�����Ȃ��ꍇ�̏���
                    break;
            }
        }
        if(check==true)THISTYPE.Remove(TYPE.CURSED);

        UnitController playerController = Player.GetComponent<UnitController>();
        playerController.THISTYPE = THISTYPE;

        Serecr_Sucssesed = true;
    }

    IEnumerator MoveToTarget()//�w��n�_�܂ňړ�����
    {
        yield return null;

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        Vector3 cameraUp = Camera.main.transform.up;
        targetPosition = Camera.main.transform.position + cameraForward * distance;
        targetPosition = targetPosition + cameraRight * 1f;
        targetPosition = targetPosition - cameraUp * 1f;

        // �ڕW�n�_�܂ł̕����x�N�g�������߂�
        Vector3 direction = (targetPosition - transform.position).normalized;
        // �ڕW�n�_�ɓ��B����܂Ń��[�v
        float deldistance = Vector3.Distance(transform.position, targetPosition)+1;
        while (true)
        {
            float olddis = Vector3.Distance(transform.position, targetPosition);
            // �w�肵�����x�ňړ�����
            transform.position += direction * speed * Time.deltaTime;
            // 1�t���[���ҋ@����
            yield return null;
            float nowdis = Vector3.Distance(transform.position, targetPosition);
            if (olddis < nowdis)
            {
                break;
            }
        }

    }

    IEnumerator MoveToCancel()//�w��n�_�܂ňړ�����
    {
        yield return null;
        targetPosition = oldpos;
        transform.rotation = Quaternion.identity;
        // �ڕW�n�_�܂ł̕����x�N�g�������߂�
        Vector3 direction = (targetPosition - transform.position).normalized;
        // �ڕW�n�_�ɓ��B����܂Ń��[�v
        while (true)
        {
            float olddis = Vector3.Distance(transform.position, targetPosition);
            // �w�肵�����x�ňړ�����
            transform.position += direction * speed * Time.deltaTime;
            // 1�t���[���ҋ@����
            yield return null;
            float nowdis = Vector3.Distance(transform.position, targetPosition);
            if (olddis < nowdis)
            {
                break;
            }
        }

    }

}


