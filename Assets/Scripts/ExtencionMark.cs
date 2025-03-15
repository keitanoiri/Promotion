using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtencionMark : MonoBehaviour
{
    [SerializeField] private AudioSource Audio;//AudioSource�^�̕ϐ�a��錾 �g�p����AudioSource�R���|�[�l���g���A�^�b�`�K�v

    [SerializeField] private AudioClip SE;//AudioClip�^�̕ϐ�b1��錾 �g�p����AudioClip���A�^�b�`�K�v
    IEnumerator Start()
    {
        Audio.PlayOneShot(SE);
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        // �J�����̉�]��GameObject�ɃR�s�[����
        transform.rotation = Camera.main.transform.rotation;
    }
}
