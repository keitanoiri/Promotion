using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtencionMark : MonoBehaviour
{
    [SerializeField] private AudioSource Audio;//AudioSource型の変数aを宣言 使用するAudioSourceコンポーネントをアタッチ必要

    [SerializeField] private AudioClip SE;//AudioClip型の変数b1を宣言 使用するAudioClipをアタッチ必要
    IEnumerator Start()
    {
        Audio.PlayOneShot(SE);
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        // カメラの回転をGameObjectにコピーする
        transform.rotation = Camera.main.transform.rotation;
    }
}
