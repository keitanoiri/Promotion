using System.Collections.Generic;
using UnityEngine;
using Enums;
using System.Collections;

public class UnitController : MonoBehaviour
{
    public List<TYPE> THISTYPE;
    public string THISNAME;
    public int[] index;
    public int number;//タイルの場合は-1 プレイヤーも-1
    private GameManager GameManager;

    public float speed = 15.0f; // 移動速度
    public Vector3 targetPosition; // 移動先の目標地点

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

    //移動処理
    public void MoveUnit(int[] move_idx)
    {
        Vector3 pos = GameManager.ind_to_pos(move_idx);
        index = new int[] { move_idx[0], move_idx[1] };
        pos.y = 1;
        targetPosition = pos;
        // コルーチンを開始する
        StartCoroutine(MoveToTarget());
        GameManager.DeleteCursol();
    }

    IEnumerator MoveToTarget()//指定地点まで移動する
    {
        // 目標地点までの方向ベクトルを求める
        Vector3 direction = (targetPosition - transform.position).normalized;

        // 目標地点に到達するまでループ
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            // 指定した速度で移動する
            transform.position += direction * speed * Time.deltaTime;

            // 1フレーム待機する
            yield return null;
        }

    }


}
