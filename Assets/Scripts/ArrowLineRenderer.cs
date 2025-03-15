using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ArrowLineRenderer : MonoBehaviour
{
    public Transform StartPoint; // ���̎n�_
    public Transform EndPoint;   // ���̏I�_
    public float ArrowSize = 0.2f; // ���̃T�C�Y

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

    // ��_���Ȃ����̌`�̐���`�悷�郁�\�b�h
    private void DrawArrowLine(Vector3 startPoint, Vector3 endPoint, float arrowSize)
    {
        Vector3 direction = endPoint - startPoint;

        // ���̐�[����
        Vector3 arrowEnd = endPoint - direction.normalized * arrowSize;

        // ���̑��ʂ̊p�x���v�Z
        Quaternion arrowRotation = Quaternion.LookRotation(direction);
        Vector3 arrowUp = arrowRotation * Vector3.up;
        Vector3 arrowRight = arrowRotation * Vector3.right;

        // ���̐�[�����K�܂ł̕����v�Z
        Vector3 arrowTip = arrowEnd + arrowUp * arrowSize / 2f;
        Vector3 arrowTail1 = arrowEnd - arrowRight * arrowSize / 2f;
        Vector3 arrowTail2 = arrowEnd + arrowRight * arrowSize / 2f;

        // ���̌`���LineRenderer�ɃZ�b�g
        lineRenderer.positionCount = 5;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, arrowTip);
        lineRenderer.SetPosition(2, arrowTail1);
        lineRenderer.SetPosition(3, endPoint);
        lineRenderer.SetPosition(4, arrowTail2);
    }
}

