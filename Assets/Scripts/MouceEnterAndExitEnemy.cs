using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class MouceEnterAndExitEnemy : MonoBehaviour
{
    [SerializeField]private GameObject EnemyCursol;
    private List<int[]> CursolPos;
    private UnitController UnitController;
    private GameManager GameManager;

    [SerializeField] private AudioSource Audio;//AudioSource�^�̕ϐ�
    [SerializeField] private AudioClip SE_Serect;//����
    void Start()
    {
        UnitController = GetComponent<UnitController>();
        CursolPos = new List<int[]>();
        GameObject manager = GameObject.Find("GameManager");
        GameManager = manager.GetComponent<GameManager>();
    }
    private void OnMouseEnter()
    {
        Audio.PlayOneShot(SE_Serect);

        Vector3 pos = transform.position;
        pos.y = pos.y + 0.5f;
        transform.position = pos;
        foreach (TYPE temp in UnitController.THISTYPE)

        {
            MakeEnemyCursol(temp, UnitController.index);//�G�̍U���͈͕\��
        }
    }

    private void OnMouseExit()
    {
        DeleteTempCursol();
        Vector3 pos = transform.position;
        pos.y = pos.y - 0.5f;
        transform.position = pos;
    }

    private void OnDestroy()
    {
        DeleteTempCursol() ;
    }

    public void MakeEnemyCursol(TYPE type, int[] index)//�ړ��\�ȃ}�X��\������֐�
    {
        CursolPos.Clear();//list���N���A
        List<int[]> elementsToRemove = new List<int[]>();
        if (type != TYPE.TILE && type == TYPE.PAWN)
        {
            //PAWN�̏���
            CursolPos.Add(new int[] { index[0] + 1, index[1] - 1 });
            CursolPos.Add(new int[] { index[0] - 1, index[1] - 1 });

            foreach (int[] search in CursolPos)
            {
                if (GameManager.CheckIndex(search) == false)
                {
                    elementsToRemove.Add(search);
                }
            }

            foreach (int[] toRemove in elementsToRemove)
            {
                CursolPos.Remove(toRemove);
            }

        }
        else if (type == TYPE.KNIGHT)
        {
            //knight�̏���
            CursolPos.Add(new int[] { index[0] - 1, index[1] + 2 });
            CursolPos.Add(new int[] { index[0] + 1, index[1] + 2 });
            CursolPos.Add(new int[] { index[0] - 1, index[1] - 2 });
            CursolPos.Add(new int[] { index[0] + 1, index[1] - 2 });
            CursolPos.Add(new int[] { index[0] - 2, index[1] + 1 });
            CursolPos.Add(new int[] { index[0] + 2, index[1] + 1 });
            CursolPos.Add(new int[] { index[0] - 2, index[1] - 1 });
            CursolPos.Add(new int[] { index[0] + 2, index[1] - 1 });

            foreach (int[] search in CursolPos)
            {
                if (GameManager.CheckIndex(search) == false)
                {
                    elementsToRemove.Add(search);
                }
            }

            foreach (int[] toRemove in elementsToRemove)
            {
                CursolPos.Remove(toRemove);
            }
        }
        else if (type == TYPE.ROOK)
        {
            //ROOK�̏���
            //x+�����̏���
            int[] searchleft = new int[] { index[0], index[1] };
            while (GameManager.CheckIndex(searchleft) == true)//�͈͓����H
            {
                searchleft[0]++;
                if (GameManager.CheckIndex(searchleft) == false)
                {
                    break;
                }
                else
                {
                    int[] adindex = new int[] { searchleft[0], searchleft[1] };
                    CursolPos.Add(adindex);
                }
            }
            //x-�����̏���
            int[] searchright = new int[] { index[0], index[1] };
            while (GameManager.CheckIndex(searchright) == true)//�͈͓����H
            {
                searchright[0]--;
                if (GameManager.CheckIndex(searchright) == false)
                {
                    break;
                }
                else
                {
                    int[] adindex = new int[] { searchright[0], searchright[1] };
                    CursolPos.Add(adindex);
                }
            }
            //y+�����̏���
            int[] searchup = new int[] { index[0], index[1] };
            while (GameManager.CheckIndex(searchup) == true)//�͈͓����H
            {
                searchup[1]++;
                if (GameManager.CheckIndex(searchup) == false)
                {
                    break;
                }
                else
                {
                    int[] adindex = new int[] { searchup[0], searchup[1] };
                    CursolPos.Add(adindex);
                }

            }
            //y-�����̏���
            int[] searchdown = new int[] { index[0], index[1] };
            while (GameManager.CheckIndex(searchdown) == true)//�͈͓����H
            {
                searchdown[1]--;
                if (GameManager.CheckIndex(searchdown) == false)
                {
                    break;
                }
                else
                {
                    int[] adindex = new int[] { searchdown[0], searchdown[1] };
                    CursolPos.Add(adindex);
                }

            }
        }
        else if (type == TYPE.BISHOP)
        {
            //Bishop�̏���
            //x+��+�����̏���
            int[] searchleftup = new int[] { index[0], index[1] };
            while (GameManager.CheckIndex(searchleftup) == true)//�͈͓����H
            {
                searchleftup[0]++;
                searchleftup[1]++;
                if (GameManager.CheckIndex(searchleftup) == false)
                {
                    break;
                }
                else
                {
                    int[] adindex = new int[] { searchleftup[0], searchleftup[1] };
                    CursolPos.Add(adindex);
                }
            }
            //x-y+�����̏���
            int[] searchrightup = new int[] { index[0], index[1] };
            while (GameManager.CheckIndex(searchrightup) == true)//�͈͓����H
            {
                searchrightup[0]--;
                searchrightup[1]++;
                if (GameManager.CheckIndex(searchrightup) == false)
                {
                    break;
                }
                else//���݂̃}�X��null
                {
                    int[] adindex = new int[] { searchrightup[0], searchrightup[1] };
                    CursolPos.Add(adindex);
                }
            }
            //x+y-�����̏���
            int[] searchrightdown = new int[] { index[0], index[1] };
            while (GameManager.CheckIndex(searchrightdown) == true)//�͈͓����H
            {
                searchrightdown[0]++;
                searchrightdown[1]--;
                if (GameManager.CheckIndex(searchrightdown) == false)
                {
                    break;
                }
                else//���݂̃}�X��null
                {
                    int[] adindex = new int[] { searchrightdown[0], searchrightdown[1] };
                    CursolPos.Add(adindex);
                }
            }
            //x-y-�����̏���
            int[] searchleftdown = new int[] { index[0], index[1] };
            while (GameManager.CheckIndex(searchleftdown) == true)//�͈͓����H
            {
                searchleftdown[0]--;
                searchleftdown[1]--;
                if (GameManager.CheckIndex(searchleftdown) == false)
                {
                    break;
                }
                else
                {
                    int[] adindex = new int[] { searchleftdown[0], searchleftdown[1] };
                    CursolPos.Add(adindex);
                }

            }
        }
        else if (type == TYPE.QUEEN)
        {
            //Queen�̏���=rook+bishop
            MakeEnemyCursol(TYPE.ROOK, index);
            MakeEnemyCursol(TYPE.BISHOP, index);
        }
        else if (type == TYPE.KING)
        {
            //King�̏���
            CursolPos.Add(new int[] { index[0] - 1, index[1] + 0 });
            CursolPos.Add(new int[] { index[0] - 1, index[1] + 1 });
            CursolPos.Add(new int[] { index[0] + 0, index[1] + 1 });
            CursolPos.Add(new int[] { index[0] + 1, index[1] + 1 });
            CursolPos.Add(new int[] { index[0] + 1, index[1] + 0 });
            CursolPos.Add(new int[] { index[0] + 1, index[1] - 1 });
            CursolPos.Add(new int[] { index[0] + 0, index[1] - 1 });
            CursolPos.Add(new int[] { index[0] - 1, index[1] - 1 });

            foreach (int[] search in CursolPos)
            {
                if (GameManager.CheckIndex(search) == false)
                {
                    elementsToRemove.Add(search);
                }
            }

            foreach (int[] toRemove in elementsToRemove)
            {
                CursolPos.Remove(toRemove);
            }

        }

        CursolPos.RemoveAll(n => n == index);//���g��unit�Ɠ����ʒu�̃C���f�b�N�X�͏���
        foreach (int[] search in CursolPos)//�쐬����rist�S�Ăɑ΂��Ă̏���
        {
            Vector3 pos = GameManager.ind_to_pos(search);
            pos.y = 1.1f;
            GameObject cursol = Instantiate(EnemyCursol, pos, Quaternion.identity);//�J�[�\���̃v���n�u�쐬
        }
        CursolPos.Clear();//�����Ղ������
    }

    public void DeleteTempCursol()
    {
        //�J�[�\���폜
        GameObject[] Cursols = GameObject.FindGameObjectsWithTag("TempCursol");
        foreach (GameObject cursol in Cursols)
        {
            Destroy(cursol);
        }
    }
}
