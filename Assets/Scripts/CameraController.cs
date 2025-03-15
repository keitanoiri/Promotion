using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform[] rotationPoints; // ���[�e�[�V��������n�_�̔z��
    public float rotationSpeed; // ���[�e�[�V�����̑��x
    public float moveSpeed; // �ړ��̑��x

    private int currentIndex = 0; // ���݂̒n�_�̃C���f�b�N�X
    private bool isRotating = false; // ���[�e�[�V���������ǂ����̃t���O

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            if (!isRotating)
            {
                if (scroll > 0f) // �}�E�X�z�C�[������ɃX�N���[��
                {
                    // �O�̒n�_�ֈړ�
                    currentIndex--;
                    if (currentIndex < 0)
                        currentIndex = rotationPoints.Length - 1;
                }
                else // �}�E�X�z�C�[�������ɃX�N���[��
                {
                    // ���̒n�_�ֈړ�
                    currentIndex++;
                    if (currentIndex >= rotationPoints.Length)
                        currentIndex = 0;
                }

                // ���[�e�[�V�����ƈړ����J�n
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
