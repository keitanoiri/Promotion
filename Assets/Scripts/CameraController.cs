using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform[] rotationPoints; // ローテーションする地点の配列
    public float rotationSpeed; // ローテーションの速度
    public float moveSpeed; // 移動の速度

    private int currentIndex = 0; // 現在の地点のインデックス
    private bool isRotating = false; // ローテーション中かどうかのフラグ

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            if (!isRotating)
            {
                if (scroll > 0f) // マウスホイールを上にスクロール
                {
                    // 前の地点へ移動
                    currentIndex--;
                    if (currentIndex < 0)
                        currentIndex = rotationPoints.Length - 1;
                }
                else // マウスホイールを下にスクロール
                {
                    // 次の地点へ移動
                    currentIndex++;
                    if (currentIndex >= rotationPoints.Length)
                        currentIndex = 0;
                }

                // ローテーションと移動を開始
                StartCoroutine(RotateAndMoveToPosition(rotationPoints[currentIndex].position, rotationPoints[currentIndex].rotation));
            }
        }
    }

    IEnumerator RotateAndMoveToPosition(Vector3 targetPosition, Quaternion targetRotation)
    {
        isRotating = true;
        Quaternion startRotation = transform.rotation;
        Vector3 startPosition = transform.position;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * rotationSpeed;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            transform.position = Vector3.Lerp(startPosition, targetPosition, t * moveSpeed);
            yield return null;
        }

        isRotating = false;
    }

}
