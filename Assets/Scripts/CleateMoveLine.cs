using UnityEngine;

public class CleateMoveLine : MonoBehaviour
{
    public void CreateArchedLine(Vector3 start,Vector3 end, float archHeight)
    {

        archHeight = 1f;
        LineRenderer lineRenderer = this.GetComponent<LineRenderer>();

        // アーチの制御点を算出
        Vector3 controlPoint = (start + end) / 2f;
        controlPoint += Vector3.up * archHeight;

        // ベジェ曲線の制御点を設定
        lineRenderer.positionCount = 20; // 曲線の解像度
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            float t = i / (float)(lineRenderer.positionCount - 1);
            Vector3 position = CalculateBezierPoint(start, controlPoint, end, t);
            lineRenderer.SetPosition(i, position);
        }
    }

    // ベジェ曲線の点を計算する関数
    Vector3 CalculateBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;

        return p;
    }
}
