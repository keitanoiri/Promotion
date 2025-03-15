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

    [SerializeField] private AudioSource Audio;//AudioSource型の変数
    [SerializeField] private AudioClip SE_Serect;//音源
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
            MakeEnemyCursol(temp, UnitController.index);//敵の攻撃範囲表示
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

    public void MakeEnemyCursol(TYPE type, int[] index)//移動可能なマスを表示する関数
    {
        CursolPos.Clear();//listをクリア
        List<int[]> elementsToRemove = new List<int[]>();
        if (type != TYPE.TILE && type == TYPE.PAWN)
        {
            //PAWNの処理
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
            //knightの処理
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
            //ROOKの処理
            //x+方向の処理
            int[] searchleft = new int[] { index[0], index[1] };
            while (GameManager.CheckIndex(searchleft) == true)//範囲内か？
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
            //x-方向の処理
            int[] searchright = new int[] { index[0], index[1] };
            while (GameManager.CheckIndex(searchright) == true)//範囲内か？
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
            //y+方向の処理
            int[] searchup = new int[] { index[0], index[1] };
            while (GameManager.CheckIndex(searchup) == true)//範囲内か？
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
            //y-方向の処理
            int[] searchdown = new int[] { index[0], index[1] };
            while (GameManager.CheckIndex(searchdown) == true)//範囲内か？
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
            //Bishopの処理
            //x+ｙ+方向の処理
            int[] searchleftup = new int[] { index[0], index[1] };
            while (GameManager.CheckIndex(searchleftup) == true)//範囲内か？
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
            //x-y+方向の処理
            int[] searchrightup = new int[] { index[0], index[1] };
            while (GameManager.CheckIndex(searchrightup) == true)//範囲内か？
            {
                searchrightup[0]--;
                searchrightup[1]++;
                if (GameManager.CheckIndex(searchrightup) == false)
                {
                    break;
                }
                else//現在のマスがnull
                {
                    int[] adindex = new int[] { searchrightup[0], searchrightup[1] };
                    CursolPos.Add(adindex);
                }
            }
            //x+y-方向の処理
            int[] searchrightdown = new int[] { index[0], index[1] };
            while (GameManager.CheckIndex(searchrightdown) == true)//範囲内か？
            {
                searchrightdown[0]++;
                searchrightdown[1]--;
                if (GameManager.CheckIndex(searchrightdown) == false)
                {
                    break;
                }
                else//現在のマスがnull
                {
                    int[] adindex = new int[] { searchrightdown[0], searchrightdown[1] };
                    CursolPos.Add(adindex);
                }
            }
            //x-y-方向の処理
            int[] searchleftdown = new int[] { index[0], index[1] };
            while (GameManager.CheckIndex(searchleftdown) == true)//範囲内か？
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
            //Queenの処理=rook+bishop
            MakeEnemyCursol(TYPE.ROOK, index);
            MakeEnemyCursol(TYPE.BISHOP, index);
        }
        else if (type == TYPE.KING)
        {
            //Kingの処理
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

        CursolPos.RemoveAll(n => n == index);//自身のunitと同じ位置のインデックスは除く
        foreach (int[] search in CursolPos)//作成したrist全てに対しての処理
        {
            Vector3 pos = GameManager.ind_to_pos(search);
            pos.y = 1.1f;
            GameObject cursol = Instantiate(EnemyCursol, pos, Quaternion.identity);//カーソルのプレハブ作成
        }
        CursolPos.Clear();//立つ鳥跡を濁さず
    }

    public void DeleteTempCursol()
    {
        //カーソル削除
        GameObject[] Cursols = GameObject.FindGameObjectsWithTag("TempCursol");
        foreach (GameObject cursol in Cursols)
        {
            Destroy(cursol);
        }
    }
}
