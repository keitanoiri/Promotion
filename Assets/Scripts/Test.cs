using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Test : MonoBehaviour
{
    public float distance = 5f; // カメラからの距離
    public float additionalZRotation = 45f; // 追加のZ軸回転角度

    void Start()
    {
        // カメラの向きから得られる方向ベクトルを取得
        Vector3 cameraForward = Camera.main.transform.forward;

        // 移動先の位置を計算
        Vector3 targetPos = Camera.main.transform.position + cameraForward * distance;

        // GameObjectを移動させる
        transform.position = targetPos;

        // カメラの回転をGameObjectにコピーする
        transform.rotation = Camera.main.transform.rotation;

        // 追加のZ軸回転を適用する
        transform.Rotate(Vector3.forward, additionalZRotation);
    }

}




