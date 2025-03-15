using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ArrowLineRenderer : MonoBehaviour
{
    public Transform StartPoint; // 矢印の始点
    public Transform EndPoint;   // 矢印の終点
    public float ArrowSize = 0.2f; // 矢印のサイズ

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        if (StartPoint != null && EndPoint != null)
        {
            DrawArrowLine(StartPoint.position, EndPoint.position, ArrowSize);
        }
    }

    // 二点をつなぐ矢印の形の線を描画するメソッド
    private void DrawArrowLine(Vector3 startPoint, Vector3 endPoint, float arrowSize)
    {
        Vector3 direction = endPoint - startPoint;

        // 矢印の先端部分
        Vector3 arrowEnd = endPoint - direction.normalized * arrowSize;

        // 矢印の側面の角度を計算
        Quaternion arrowRotation = Quaternion.LookRotation(direction);
        Vector3 arrowUp = arrowRotation * Vector3.up;
        Vector3 arrowRight = arrowRotation * Vector3.right;

        // 矢印の先端から矢尻までの幅を計算
        Vector3 arrowTip = arrowEnd + arrowUp * arrowSize / 2f;
        Vector3 arrowTail1 = arrowEnd - arrowRight * arrowSize / 2f;
        Vector3 arrowTail2 = arrowEnd + arrowRight * arrowSize / 2f;

        // 矢印の形状をLineRendererにセット
        lineRenderer.positionCount = 5;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, arrowTip);
        lineRenderer.SetPosition(2, arrowTail1);
        lineRenderer.SetPosition(3, endPoint);
        lineRenderer.SetPosition(4, arrowTail2);
    }
}

