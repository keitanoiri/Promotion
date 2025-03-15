using System.Collections;
using UnityEngine;
using Enums;
using System;
using TMPro;
using System.Collections.Generic;



public class HandController : MonoBehaviour
{
    [SerializeField] private AudioSource Audio;//AudioSource型の変数aを宣言 使用するAudioSourceコンポーネントをアタッチ必要
    [SerializeField] private AudioClip SE_Serect;//AudioClip型の変数b2を宣言 使用するAudioClipをアタッチ必要 

    public List<TYPE> THISTYPE;
    public string THISNAME;
    public GameObject Canvas;
    private GameObject Player;
    bool Serecr_Sucssesed;
    private GameManager gameManager;
    private float distance = 10f; // カメラからの距離
    private float speed = 100.0f; // 移動速度
    private Vector3 targetPosition; // 移動先の目標地点
    private float additionalZRotation = 45f; // 追加のZ軸回転角度
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

    // マウスカーソルがオブジェクトから離れたときに呼び出されるメソッド
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

            // カメラの回転をGameObjectにコピーする
            transform.rotation = Camera.main.transform.rotation;
            // 追加のZ軸回転を適用する
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
                    // PAWNの処理
                    gameManager.AddStatus(STATUS.Skill_PAWN);
                    break;
                case TYPE.ROOK:
                    // ROOKの処理
                    gameManager.AddStatus(STATUS.Skill_ROOK);
                    break;
                case TYPE.KNIGHT:
                    // KNIGHTの処理
                    gameManager.AddStatus(STATUS.Skill_KNIGHT);
                    break;
                case TYPE.BISHOP:
                    // BISHPの処理
                    gameManager.AddStatus(STATUS.SKill_BISHP);
                    break;
                case TYPE.QUEEN:
                    // QUEENの処理
                    gameManager.AddStatus(STATUS.SKill_QUEEN);
                    break;
                case TYPE.KING:
                    // KINGの処理
                    gameManager.AddStatus(STATUS.SKill_KING);
                    break;
                case TYPE.CURSED:
                    // 呪い（HP-1)
                    gameManager.Life--;
                    break;
                default:
                    // 上記のいずれにも該当しない場合の処理
                    break;
            }
        }

        Serecr_Sucssesed = true;
    }

    public void OnClickMove()//typeを変更する
    {
        bool check = false;
        foreach (TYPE H_type in THISTYPE)
        {
            switch (H_type)
            {
                case TYPE.CURSED:
                    // 呪い（HP-1)
                    gameManager.Life--;
                    check = true;
                    break;
                default:
                    // 上記のいずれにも該当しない場合の処理
                    break;
            }
        }
        if(check==true)THISTYPE.Remove(TYPE.CURSED);

        UnitController playerController = Player.GetComponent<UnitController>();
        playerController.THISTYPE = THISTYPE;

        Serecr_Sucssesed = true;
    }

    IEnumerator MoveToTarget()//指定地点まで移動する
    {
        yield return null;

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        Vector3 cameraUp = Camera.main.transform.up;
        targetPosition = Camera.main.transform.position + cameraForward * distance;
        targetPosition = targetPosition + cameraRight * 1f;
        targetPosition = targetPosition - cameraUp * 1f;

        // 目標地点までの方向ベクトルを求める
        Vector3 direction = (targetPosition - transform.position).normalized;
        // 目標地点に到達するまでループ
        float deldistance = Vector3.Distance(transform.position, targetPosition)+1;
        while (true)
        {
            float olddis = Vector3.Distance(transform.position, targetPosition);
            // 指定した速度で移動する
            transform.position += direction * speed * Time.deltaTime;
            // 1フレーム待機する
            yield return null;
            float nowdis = Vector3.Distance(transform.position, targetPosition);
            if (olddis < nowdis)
            {
                break;
            }
        }

    }

    IEnumerator MoveToCancel()//指定地点まで移動する
    {
        yield return null;
        targetPosition = oldpos;
        transform.rotation = Quaternion.identity;
        // 目標地点までの方向ベクトルを求める
        Vector3 direction = (targetPosition - transform.position).normalized;
        // 目標地点に到達するまでループ
        while (true)
        {
            float olddis = Vector3.Distance(transform.position, targetPosition);
            // 指定した速度で移動する
            transform.position += direction * speed * Time.deltaTime;
            // 1フレーム待機する
            yield return null;
            float nowdis = Vector3.Distance(transform.position, targetPosition);
            if (olddis < nowdis)
            {
                break;
            }
        }

    }

}


