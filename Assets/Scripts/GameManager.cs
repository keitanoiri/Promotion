using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

using Enums;
using System.Linq;//名前空間を外部から



namespace Enums
{    
    public enum TYPE
    {
        NONE = -1,
        PAWN = 0,
        ROOK = 1,
        KNIGHT = 2,
        BISHOP = 3,
        QUEEN = 4,
        KING = 5,
        TILE =6,
        CURSED=7,
    }
    public enum STATUS
    {
        Serect_P,
        Serect_E,
        Skill_PAWN,
        Skill_ROOK, 
        Skill_KNIGHT,
        SKill_BISHP,
        SKill_QUEEN,
        SKill_KING,
    }
}
public class GameManager : MonoBehaviour
{
    ///音
    [SerializeField] private AudioSource MainAudio;//AudioSource型の変数aを宣言 使用するAudioSourceコンポーネントをアタッチ必要
    [SerializeField] private AudioClip BGM;//AudioClip型の変数b1を宣言 使用するAudioClipをアタッチ必要
    [SerializeField] private AudioClip SE_Serect;//AudioClip型の変数b2を宣言 使用するAudioClipをアタッチ必要 


    // STATUSが追加されたときに発生するイベント
    public event Action<STATUS> OnStatusAdded;
    // STATUSが取り除かれたときに発生するイベント
    public event Action<STATUS> OnStatusRemoved;

    public List<STATUS> status;
    public List<int[]> AbleToMove;

    public const int TILE_X = 8;
    public const int TILE_Y = 8;
    public const int UNIT_NUM = 8;
    private int _life = 3;//ライフの初期値
    private int _ap = 1;//APの初期値

    public GameObject[] prefabTile;
    public GameObject[] prefabUnit;
    public GameObject[] prefabHand;
    public GameObject prefabStart;
    public GameObject prefabGoal;
    public GameObject prefabPlayer;
    public GameObject Cursol;
    public GameObject MoveLine;
    public GameObject gameOverPanel; // ゲームオーバー画面のUIパネル
    public GameObject gameClearPanel; // ゲームクリア画面のUIパネル
    public GameObject MainCanvas;//メインキャンバス
    public GameObject SubCanvas;
    public GameObject ExtencionMark;//!ma-ku

    //内部データ
    GameObject[,] tiles;　//8*8のタイル情報
    GameObject[,] units;
    GameObject[] hands;
    GameObject Player;
    GameObject[] unitfornuber;
    List<TYPE> P_Default_Type;
    public　bool P_turn;
    bool E_turn;
    bool Serect_Sucssesed;
    bool CanDiffence;
    bool Serect_Attack = false;
    bool Serect_Diffence = false;
    public bool Use_Skill = false;


    // Start is called before the first frame update
    void Start()
    {
        //内部データ
        tiles= new GameObject[TILE_X, TILE_Y];
        units= new GameObject[TILE_X, TILE_Y];
        hands = new GameObject[5];
        unitfornuber = new GameObject[UNIT_NUM];
        int random_Renge = TILE_X * TILE_Y-1;
        List<int> numbers = new List<int>();
        AbleToMove = new List<int[]>();
        P_turn = true;
        E_turn = false;
        CanDiffence = false;
        Serect_Sucssesed = false;
        Player = null;
        P_Default_Type = new List<TYPE> { TYPE.PAWN };
        SubCanvas.SetActive(false);

        OnStatusAdded += HandleStatusAdded;
        OnStatusRemoved += HandleStatusRemoved;

        //駒のランダム配置
        for (int i = 0; i <= random_Renge; i++)
        {
            numbers.Add(i);
        }
        int number = 0;
        int unitCount = UNIT_NUM;
        while (unitCount-- > 0)
        {
            int index = UnityEngine.Random.Range(0, numbers.Count);
            int ransu = numbers[index];

            numbers.RemoveAt(index);

            int idx_x = ransu % 8;
            int idx_y = ransu / 8;

            int[] ind = new int[2];
            ind[0] = idx_x;
            ind[1] = idx_y;

            float x = (idx_x - TILE_X / 2) * 2;
            float y = (idx_y - TILE_Y / 2) * 2;

            int idx = UnityEngine.Random.Range(0,prefabUnit.Length);

            Vector3 pos = new Vector3(x, 1, y);

            List<TYPE> E_types = new List<TYPE>();
            E_types.Add((TYPE)idx);
            GameObject unit = Instantiate(prefabUnit[idx],pos,Quaternion.Euler(0, -90, 0));
            unit.GetComponent<UnitController>().SetUnit(E_types,ind,number);//unit
            units[idx_x,idx_y] = unit;
            unitfornuber[number] = unit;
            number++;
        }////////////////////

        for (int i = 0; i < TILE_X; i++)//タイルの初期配置
        {
            for(int j = 0; j < TILE_Y; j++)
            {
                //tail&Unit Position
                float x = (i - TILE_X / 2)*2;
                float y= (j - TILE_Y / 2)*2;

                Vector3 pos= new Vector3(x, 0, y);

                //Create
                int idx = (i + j) % 2;
                GameObject tile = Instantiate(prefabTile[idx],pos,Quaternion.Euler(-90,-90,0));
            }
        }

        for(int i = 0;i < TILE_X; i++)//スタート地点の配置
        {
            float x = (i - TILE_X / 2) * 2;

            Vector3 pos = new Vector3(x, 0, -10);

            GameObject start_tile = Instantiate(prefabStart, pos, Quaternion.identity);
        }

        for (int i = 0; i < TILE_X; i++)//ゴール地点の配置
        {
            float x = (i - TILE_X / 2) * 2;

            Vector3 pos = new Vector3(x, 0, 10);

            GameObject goal_tile = Instantiate(prefabGoal, pos, Quaternion.identity);
        }
        MainCanvas.GetComponent<UIController>().ChengeTextWindow("スタート地点を選択してください。");
    }

    // Update is called once per frame
    void Update()
    {
        if (P_turn == true && E_turn == false) //自分のターンなら
        {
            if (Use_Skill==false && Input.GetMouseButtonDown(0)) // 左クリックが押されたら
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (EventSystem.current.IsPointerOverGameObject()) return;//UIとかぶる時判定しない
                if (Physics.Raycast(ray, out hit))
                {
                    GameObject targetObject = hit.collider.gameObject;
                    // クリックされたときの処理を記述する
                    if (targetObject.tag == "Tile")//タイルをクリックしたとき・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・
                    {
                        if (status.Contains(STATUS.Serect_P))//プレイヤー選択状態なら条件を満たせばプレイヤー移動
                        {
                            int[] hantei = pos_to_ind(targetObject.transform.position);
                            if (AP > 0)
                            {
                                if (AbleToMove.Any(arr => IsEqual(arr, hantei)))
                                {
                                    UnitController p_controller = Player.GetComponent<UnitController>();
                                    int[] idx = pos_to_ind(targetObject.transform.position);
                                    int[] p_oldindex = p_controller.index;
                                    if (CheckIndex(p_oldindex) == true)//プレイヤーのインデックスがスタート地点出なければunitsから除外
                                    {
                                        units[p_oldindex[0], p_oldindex[1]] = null;
                                    }
                                    p_controller.MoveUnit(idx);//移動
                                    units[idx[0], idx[1]] = Player;
                                    AbleToMove.Clear();
                                    p_controller.THISTYPE = P_Default_Type;//移動したらtypeをデフォルトに戻す
                                    //EnemyReaction
                                    StartCoroutine(EnemyReaction(p_oldindex, idx));

                                    //AP減少
                                    AP--;
                                    status.Remove(STATUS.Serect_P);
                                }
                                else
                                {
                                    MainCanvas.GetComponent<UIController>().ChengeTextWindow("移動可能なマスを選択してください");
                                }
                            }
                            else
                            {
                                MainCanvas.GetComponent<UIController>().ChengeTextWindow("APが足りません");
                            }

                        }
                    }
                    else if (targetObject.tag == "Player")//プレイヤーをクリックしたとき・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・
                    {
                        if (status.Contains(STATUS.Serect_P))//プレイヤー選択状態なら
                        {
                            RemoveStatus(STATUS.Serect_P);
                            DeleteCursol();
                        }
                        else//プレイヤーが選択されていないなら
                        {
                            MainAudio.PlayOneShot(SE_Serect);
                            AddStatus(STATUS.Serect_P);
                            //移動可能なマスを明示
                            MakeCursol(targetObject);
                            if (AbleToMove.Count == 0 && Player.GetComponent<UnitController>().index[1]!=7)
                            {
                                List<TYPE> cursedType = new List<TYPE>();
                                cursedType.Add((TYPE)5); 
                                cursedType.Add((TYPE)7);
                                MakeHand(cursedType);
                            }
                        }
                    }
                    else if (targetObject.tag == "Enemy")//敵をクリックしたとき・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・・
                    {
                        if (status.Contains(STATUS.Serect_P))//プレイヤー選択状態なら敵の駒を取る
                        {
                            //取られる処理
                            int[] hantei = pos_to_ind(targetObject.transform.position);
                            if (AP > 0)
                            {
                                if (AbleToMove.Any(arr => IsEqual(arr, hantei)))
                                {
                                    UnitController p_controller = Player.GetComponent<UnitController>();
                                    int[] idx = pos_to_ind(targetObject.transform.position);
                                    int[] p_oldindex = p_controller.index;
                                    if (CheckIndex(p_oldindex) == true)//プレイヤーのインデックスがスタート地点出なければunitsから除外
                                    {
                                        units[p_oldindex[0], p_oldindex[1]] = null;
                                    }



                                    p_controller.MoveUnit(idx);
                                    units[idx[0], idx[1]] = Player;

                                    AbleToMove.Clear();

                                    //handを追加
                                    MakeHand(targetObject.GetComponent<UnitController>().THISTYPE);
                                    //駒を破壊
                                    Destroy(targetObject);

                                    //敵のリアクション
                                    StartCoroutine(EnemyReaction(p_oldindex, idx));
                                    //移動したのでAP減少
                                    AP--;

                                    status.Remove(STATUS.Serect_P);
                                }
                                else
                                {
                                    MainCanvas.GetComponent<UIController>().ChengeTextWindow("移動可能なマスを選択してください");
                                }
                            }
                            else
                            {
                                MainCanvas.GetComponent<UIController>().ChengeTextWindow("APがたりません");
                            }
                        }
                        else if (status.Contains(STATUS.Serect_E))//Enemy選択状態なら
                        {
                            status.Remove(STATUS.Serect_E);//Enemy選択状態解除
                        }
                        else//何も選択されていなければ
                        {
                            status.Add(STATUS.Serect_E);//Enemy選択状態へ
                        }

                    }
                    else if (targetObject.tag == "Hand")
                    {
                        if (Player != null)
                        {
                            //タイプ変更or特殊効果
                            HandController handController = targetObject.GetComponent<HandController>();
                            StartCoroutine(handController.OnSerected(Player));

                        }
                        else
                        {
                            Debug.Log("スタート地点を選択してください");
                        }


                        //タイプ変更or能力使用
                    }
                    else if (targetObject.tag == "Start_tile")//Startの処理
                    {
                        if (Player == null)
                        {
                            MainCanvas.GetComponent<UIController>().ChengeTextWindow("");
                            //プレイヤー配置
                            Vector3 pos = ind_to_pos(pos_to_ind(targetObject.transform.position));
                            Player = Instantiate(prefabPlayer, pos, Quaternion.identity);
                            Player.GetComponent<UnitController>().SetUnit(P_Default_Type, pos_to_ind(targetObject.transform.position),-1);
                        }
                    }
                    else if(targetObject.tag == "Goal_tile")
                    {
                        if(Player != null)
                        {
                            int[] P_ind = Player.GetComponent<UnitController>().index;
                            if (P_ind[1] == 7 && AP > 0)
                            {
                                GameClear();
                            }
                            else
                            {
                                MainCanvas.GetComponent<UIController>().ChengeTextWindow("ゴールできません");
                            }
                        }
                    }
                }
            }else if(Input.GetMouseButtonDown(1))
            {
                ClearStatus();//statusを初期化（キャンセル処理
            }
        }

        //敵のターンの処理
        else if (P_turn == false&&E_turn==false)//相手のターン
        {
            E_turn = true;
            Debug.Log("敵のターン");
            if (Player != null)
            {
                StartCoroutine(EnemyTurn());
            }
            else
            {
                E_turn = false;
                MainCanvas.GetComponent<UIController>().ChengeTextWindow("スタート地点を選択してください。");
            }
            AP = 1;//AP回復
            P_turn = true;
            //E_turn =false;
        }
    }
    
    IEnumerator EnemyReaction(int[] start, int[] end)
    {
        List<int[]> BetweenGrid = GetGridBetweenPoints(start,end);

        for (int i = 0; i<UNIT_NUM; i++)
        {
            if (unitfornuber[i]!=null)
            {
                UnitController EnemyController = unitfornuber[i].GetComponent<UnitController>();
                int[] E_idx = EnemyController.index;
                List<TYPE> E_type = EnemyController.THISTYPE;


                bool tempcheck = false;
                foreach (int[] array in BetweenGrid)
                {
                    foreach (TYPE type in E_type)
                    {
                        // array に対する処理を行う
                        if (units[array[0], array[1]] == null && AttackCheck(array, E_idx, type) == true)//敵の攻撃判定成功
                        {
                            //!マーク作成
                            Vector3 markpos = ind_to_pos(E_idx);
                            GameObject mark = Instantiate(ExtencionMark, markpos, Quaternion.identity);
                            //敵を移動
                            EnemyController.MoveUnit(array);
                            units[E_idx[0], E_idx[1]] = null;
                            units[array[0], array[1]] = unitfornuber[i];
                            EnemyController.index = new int[] { array[0], array[1] };
                            tempcheck = true;
                            break;
                        }
                        if (tempcheck == true) { break; }
                    }
                }

            }
            yield return null;
        }
    }
    // 2つの地点の間のマス目を取得するメソッド
    public static List<int[]> GetGridBetweenPoints(int[] start, int[] end)
    {
        List<int[]> gridBetweenPoints = new List<int[]>();

        // x 方向の変化量を計算
        int deltaX = end[0] - start[0];
        // y 方向の変化量を計算
        int deltaY = end[1] - start[1];

        // x 方向と y 方向のどちらが大きいかを判定し、大きい方の変化量を step とする
        int step = Math.Max(Math.Abs(deltaX), Math.Abs(deltaY));

        // ステップごとにマス目を計算し、結果を gridBetweenPoints に追加
        for (int i = 1; i < step; i++)
        {
            float t = i / (float)step;
            int x = Mathf.RoundToInt(Mathf.Lerp(start[0], end[0], t));
            int y = Mathf.RoundToInt(Mathf.Lerp(start[1], end[1], t));
            gridBetweenPoints.Add(new int[] { x, y });
        }

        return gridBetweenPoints;
    }
    IEnumerator EnemyTurn()//敵のターンの統括
    {
        UnitController p_controller = Player.GetComponent<UnitController>();
        for (int i = 0; i < UNIT_NUM; i++)
        {
            if (unitfornuber[i] != null)
            {
                CanDiffence=false;
                int[] move_idx = new int[2];
                int[] P_idx = p_controller.index;
                List<TYPE> P_types = p_controller.THISTYPE;
                int[] E_idx = unitfornuber[i].GetComponent<UnitController>().index;
                List<TYPE> E_types = unitfornuber[i].GetComponent<UnitController>().THISTYPE;
                List<int[]> elementsToRemove = new List<int[]>();

                foreach (TYPE E_type in E_types)
                {
                    foreach (TYPE P_type in P_types)
                    {
                        if (AttackCheckByObj(Player, unitfornuber[i], E_type) == true)//敵の攻撃判定成功
                        {
                            //!マークと効果音
                            Vector3 pos = ind_to_pos(E_idx);
                            pos.y = 3f;
                            GameObject mark = Instantiate(ExtencionMark, pos, Quaternion.identity);

                            //攻撃ユニットを強調
                            GameObject line = Instantiate(MoveLine);//lineを作成
                            Vector3 start = ind_to_pos(E_idx);
                            Vector3 end = ind_to_pos(P_idx);
                            line.GetComponent<CleateMoveLine>().CreateArchedLine(start, end, 1.0f);

                            if (AttackCheckByObj(Player, unitfornuber[i], P_type))//反撃可能か？
                            {
                                CanDiffence = true;
                            }
                            SubCanvas.SetActive(true);
                            // ボタンが押されるまで待機
                            while (Serect_Sucssesed == false)
                            {
                                yield return null;
                            }
                            // ボタンが押された後の処理
                            // Canvas を非表示にする
                            Serect_Sucssesed = false;
                            SubCanvas.SetActive(false);
                            DeleteCursol();

                            if (Serect_Attack == true)////ノーりあくじょん
                            {
                                Serect_Attack = false;

                                //プレイヤー移動先
                                if (E_idx[0] == P_idx[0]) { move_idx[0] = P_idx[0]; }
                                else if (E_idx[0] < P_idx[0]) { move_idx[0] = P_idx[0] + 1; }
                                else { move_idx[0] = P_idx[0] - 1; }
                                if (E_idx[1] == P_idx[1]) { move_idx[1] = P_idx[1]; }
                                else if (E_idx[1] < P_idx[1]) { move_idx[1] = P_idx[1] + 1; }
                                else { move_idx[1] = P_idx[1] - 1; }

                                units[E_idx[0], E_idx[1]] = null;

                                if (CheckIndex(move_idx) == true && units[move_idx[0], move_idx[1]] == null)//移動先が範囲内なら
                                {
                                    //プレイヤー移動
                                    if (CheckIndex(P_idx) == true)//プレイヤーのインデックスがスタート地点出なければunitsから除外
                                    {
                                        units[P_idx[0], P_idx[1]] = null;
                                    }
                                    //idxに移動先のインデックスを格納
                                    p_controller.MoveUnit(move_idx);
                                    units[move_idx[0], move_idx[1]] = Player;
                                    AbleToMove.Clear();
                                }//壁や敵にぶつかるなら
                                else
                                {
                                    Life--;//追加でダメージ
                                }
                                //handを追加
                                MakeHand(unitfornuber[i].GetComponent<UnitController>().THISTYPE);
                                //駒を破壊
                                Destroy(unitfornuber[i]);
                                //LIFE減少
                                Life--;
                            }
                            else if (Serect_Diffence == true)//リアクション
                            {
                                Serect_Diffence = false;


                                //プレイヤー移動先
                                if (E_idx[0] == P_idx[0]) { move_idx[0] = P_idx[0]; }
                                else if (E_idx[0] < P_idx[0]) { move_idx[0] = P_idx[0] - 1; }
                                else { move_idx[0] = P_idx[0] + 1; }
                                if (E_idx[1] == P_idx[1]) { move_idx[1] = P_idx[1]; }
                                else if (E_idx[1] < P_idx[1]) { move_idx[1] = P_idx[1] - 1; }
                                else { move_idx[1] = P_idx[1] + 1; }

                                units[E_idx[0], E_idx[1]] = null;

                                if (CheckIndex(move_idx) == true && units[move_idx[0], move_idx[1]] == null)//移動先のが範囲内なら
                                {
                                    //プレイヤー移動
                                    if (CheckIndex(P_idx) == true)//プレイヤーのインデックスがスタート地点出なければunitsから除外
                                    {
                                        units[P_idx[0], P_idx[1]] = null;
                                    }
                                    //idxに移動先のインデックスを格納
                                    p_controller.MoveUnit(move_idx);
                                    units[move_idx[0], move_idx[1]] = Player;
                                    AbleToMove.Clear();
                                }//壁や敵にぶつかるなら
                                else
                                {
                                    //駒を動かさない
                                }
                                //handを追加しない
                                //駒を破壊
                                Destroy(unitfornuber[i]);
                                //LIFE減少しない
                            }
                        }
                    }
                }
            }
        }
        p_controller.THISTYPE = P_Default_Type;
        E_turn=false;//敵のターンを終了
    }
    private bool IsEqual(int[] array1, int[] array2)//Listの中身が一致するかのチェック
    {
        if (array1.Length != array2.Length)
            return false;

        for (int i = 0; i < array1.Length; i++)
        {
            if (array1[i] != array2[i])
                return false;
        }

        return true;
    }
    public int[] pos_to_ind(Vector3 pos)//Vecter3からインデックスに変換する関数
    {
        Vector2 ind_pos = new Vector2((pos.x / 2) + (TILE_X / 2), (pos.z / 2) + (TILE_Y / 2));
        int[] ind = new int[2];
        ind[0] = (int)ind_pos.x;
        ind[1] = (int)ind_pos.y;
        return ind;
    }
    public Vector3 ind_to_pos(int[] index)//インデックスからVector３に変換する関数
    {
        float x = (index[0] - TILE_X / 2) * 2;
        float y = (index[1] - TILE_Y / 2) * 2;
        Vector3 pos = new Vector3(x, 1, y);
        return pos;
    }
    private void MakeCursol(GameObject Unit)
    {
        Debug.Log("makeカーソル");
        AbleToMove.Clear();//listをクリア
        List<TYPE> Objtype;
        int[] index = null;

        Objtype = Unit.GetComponent<UnitController>().THISTYPE;
        index = Unit.GetComponent<UnitController>().index;
        


        List<int[]> elementsToRemove = new List<int[]>();
        foreach (TYPE type in Objtype)
        {
            if (type == TYPE.PAWN)
            {
                //PAWNの処理
                if (Unit.tag == "Player")
                {
                    if (CheckIndex(new int[] { index[0], index[1] + 1 }) == true && units[index[0], index[1] + 1] == null)
                    {
                        AbleToMove.Add(new int[] { index[0], index[1] + 1 });
                    }
                    if (CheckIndex(new int[] { index[0] + 1, index[1] + 1 }) == true && units[index[0] + 1, index[1] + 1] != null && units[index[0] + 1, index[1] + 1].tag == "Enemy")
                    {
                        AbleToMove.Add(new int[] { index[0] + 1, index[1] + 1 });
                    }
                    if (CheckIndex(new int[] { index[0] - 1, index[1] + 1 }) == true && units[index[0] - 1, index[1] + 1] != null && units[index[0] - 1, index[1] + 1].tag == "Enemy")
                    {
                        AbleToMove.Add(new int[] { index[0] - 1, index[1] + 1 });
                    }
                }
                else
                {
                    if (CheckIndex(new int[] { index[0], index[1] - 1 }) == true && units[index[0], index[1] + 1] == null)
                    {
                        AbleToMove.Add(new int[] { index[0], index[1] - 1 });
                    }

                }

            }
            else if (type == TYPE.KNIGHT)
            {
                //knightの処理
                AbleToMove.Add(new int[] { index[0] - 1, index[1] + 2 });
                AbleToMove.Add(new int[] { index[0] + 1, index[1] + 2 });
                AbleToMove.Add(new int[] { index[0] - 1, index[1] - 2 });
                AbleToMove.Add(new int[] { index[0] + 1, index[1] - 2 });
                AbleToMove.Add(new int[] { index[0] - 2, index[1] + 1 });
                AbleToMove.Add(new int[] { index[0] + 2, index[1] + 1 });
                AbleToMove.Add(new int[] { index[0] - 2, index[1] - 1 });
                AbleToMove.Add(new int[] { index[0] + 2, index[1] - 1 });


                foreach (int[] search in AbleToMove)
                {
                    if (CheckIndex(search) == false)
                    {
                        elementsToRemove.Add(search);
                    }
                }


                foreach (int[] toRemove in elementsToRemove)
                {
                    AbleToMove.Remove(toRemove);
                }
            }
            else if (type == TYPE.ROOK)
            {
                //ROOKの処理
                //x+方向の処理
                int[] searchleft = new int[] { index[0], index[1] };
                while (CheckIndex(searchleft) == true)//範囲内か？
                {
                    searchleft[0]++;
                    if (CheckIndex(searchleft) == false)
                    {
                        searchleft[0]--;
                        int[] adindex = new int[] { searchleft[0], searchleft[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchleft[0], searchleft[1]] != null)//探索中のマスに駒がある
                    {
                        int[] adindex = new int[] { searchleft[0], searchleft[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (Unit.tag!="Player")///プレイヤーで無ければ道中も移動可能にする
                    {
                        int[] adindex = new int[] { searchleft[0], searchleft[1] };
                        AbleToMove.Add(adindex);
                    }
                }
                //x-方向の処理
                int[] searchright = new int[] { index[0], index[1] };
                while (CheckIndex(searchright) == true)//範囲内か？
                {
                    searchright[0]--;
                    if (CheckIndex(searchright) == false)
                    {
                        searchright[0]++;
                        int[] adindex = new int[] { searchright[0], searchright[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchright[0], searchright[1]] != null)//現在のマスがnull
                    {
                        int[] adindex = new int[] { searchright[0], searchright[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (Unit.tag != "Player")
                    {
                        int[] adindex = new int[] { searchright[0], searchright[1] };
                        AbleToMove.Add(adindex);
                    }

                }
                //y+方向の処理
                int[] searchup = new int[] { index[0], index[1] };
                while (CheckIndex(searchup) == true)//範囲内か？
                {
                    searchup[1]++;
                    if (CheckIndex(searchup) == false)
                    {
                        searchup[1]--;
                        int[] adindex = new int[] { searchup[0], searchup[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchup[0], searchup[1]] != null)//現在のマスがnull
                    {
                        int[] adindex = new int[] { searchup[0], searchup[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (Unit.tag != "Player")
                    {
                        int[] adindex = new int[] { searchup[0], searchup[1] };
                        AbleToMove.Add(adindex);
                    }
                }
                //y-方向の処理
                int[] searchdown = new int[] { index[0], index[1] };
                while (CheckIndex(searchdown) == true)//範囲内か？
                {
                    searchdown[1]--;
                    if (CheckIndex(searchdown) == false)
                    {
                        searchdown[1]++;
                        int[] adindex = new int[] { searchdown[0], searchdown[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchdown[0], searchdown[1]] != null)//現在のマスがnull
                    {
                        int[] adindex = new int[] { searchdown[0], searchdown[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (Unit.tag != "Player")
                    {
                        int[] adindex = new int[] { searchdown[0], searchdown[1] };
                        AbleToMove.Add(adindex);
                    }
                }
            }
            else if (type == TYPE.BISHOP)
            {
                //Bishopの処理
                //x+ｙ+方向の処理
                int[] searchleftup = new int[] { index[0], index[1] };
                while (CheckIndex(searchleftup) == true)//範囲内か？
                {
                    searchleftup[0]++;
                    searchleftup[1]++;
                    if (CheckIndex(searchleftup) == false)
                    {
                        searchleftup[0]--;
                        searchleftup[1]--;
                        int[] adindex = new int[] { searchleftup[0], searchleftup[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchleftup[0], searchleftup[1]] != null)//現在のマスがnull
                    {
                        int[] adindex = new int[] { searchleftup[0], searchleftup[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (Unit.tag != "Player")
                    {
                        int[] adindex = new int[] { searchleftup[0], searchleftup[1] };
                        AbleToMove.Add(adindex);
                    }
                }
                //x-y+方向の処理
                int[] searchrightup = new int[] { index[0], index[1] };
                while (CheckIndex(searchrightup) == true)//範囲内か？
                {
                    searchrightup[0]--;
                    searchrightup[1]++;
                    if (CheckIndex(searchrightup) == false)
                    {
                        searchrightup[0]++;
                        searchrightup[1]--;
                        int[] adindex = new int[] { searchrightup[0], searchrightup[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchrightup[0], searchrightup[1]] != null)//現在のマスがnull
                    {
                        int[] adindex = new int[] { searchrightup[0], searchrightup[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (Unit.tag != "Player")
                    {
                        int[] adindex = new int[] { searchrightup[0], searchrightup[1] };
                        AbleToMove.Add(adindex);
                    }
                }
                //x+y-方向の処理
                int[] searchrightdown = new int[] { index[0], index[1] };
                while (CheckIndex(searchrightdown) == true)//範囲内か？
                {
                    searchrightdown[0]++;
                    searchrightdown[1]--;
                    if (CheckIndex(searchrightdown) == false)
                    {
                        searchrightdown[0]--;
                        searchrightdown[1]++;
                        int[] adindex = new int[] { searchrightdown[0], searchrightdown[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchrightdown[0], searchrightdown[1]] != null)//現在のマスがnull
                    {
                        int[] adindex = new int[] { searchrightdown[0], searchrightdown[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (Unit.tag != "Player")
                    {
                        int[] adindex = new int[] { searchrightdown[0], searchrightdown[1] };
                        AbleToMove.Add(adindex);
                    }
                }
                //x-y-方向の処理
                int[] searchleftdown = new int[] { index[0], index[1] };
                while (CheckIndex(searchleftdown) == true)//範囲内か？
                {
                    searchleftdown[0]--;
                    searchleftdown[1]--;
                    if (CheckIndex(searchleftdown) == false)
                    {
                        searchleftdown[0]++;
                        searchleftdown[1]++;
                        int[] adindex = new int[] { searchleftdown[0], searchleftdown[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchleftdown[0], searchleftdown[1]] != null)//現在のマスがnull
                    {
                        int[] adindex = new int[] { searchleftdown[0], searchleftdown[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (Unit.tag != "Player")
                    {
                        int[] adindex = new int[] { searchleftdown[0], searchleftdown[1] };
                        AbleToMove.Add(adindex);
                    }
                }
            }
            else if (type == TYPE.QUEEN)
            {
                //Queenの処理=rook+bishop
                //ROOKの処理
                //x+方向の処理
                int[] searchleft = new int[] { index[0], index[1] };
                while (CheckIndex(searchleft) == true)//範囲内か？
                {
                    searchleft[0]++;
                    if (CheckIndex(searchleft) == false)
                    {
                        searchleft[0]--;
                        int[] adindex = new int[] { searchleft[0], searchleft[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchleft[0], searchleft[1]] != null)//現在のマスがnull
                    {
                        int[] adindex = new int[] { searchleft[0], searchleft[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (Unit.tag != "Player")
                    {
                        int[] adindex = new int[] { searchleft[0], searchleft[1] };
                        AbleToMove.Add(adindex);
                    }
                }
                //x-方向の処理
                int[] searchright = new int[] { index[0], index[1] };
                while (CheckIndex(searchright) == true)//範囲内か？
                {
                    searchright[0]--;
                    if (CheckIndex(searchright) == false)
                    {
                        searchright[0]++;
                        int[] adindex = new int[] { searchright[0], searchright[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchright[0], searchright[1]] != null)//現在のマスがnull
                    {
                        int[] adindex = new int[] { searchright[0], searchright[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (Unit.tag != "Player")
                    {
                        int[] adindex = new int[] { searchright[0], searchright[1] };
                        AbleToMove.Add(adindex);
                    }
                }
                //y+方向の処理
                int[] searchup = new int[] { index[0], index[1] };
                while (CheckIndex(searchup) == true)//範囲内か？
                {
                    searchup[1]++;
                    if (CheckIndex(searchup) == false)
                    {
                        searchup[1]--;
                        int[] adindex = new int[] { searchup[0], searchup[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchup[0], searchup[1]] != null)//現在のマスがnull
                    {
                        int[] adindex = new int[] { searchup[0], searchup[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (Unit.tag != "Player")
                    {
                        int[] adindex = new int[] { searchup[0], searchup[1] };
                        AbleToMove.Add(adindex);
                    }
                }
                //y-方向の処理
                int[] searchdown = new int[] { index[0], index[1] };
                while (CheckIndex(searchdown) == true)//範囲内か？
                {
                    searchdown[1]--;
                    if (CheckIndex(searchdown) == false)
                    {
                        searchdown[1]++;
                        int[] adindex = new int[] { searchdown[0], searchdown[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchdown[0], searchdown[1]] != null)//現在のマスがnull
                    {
                        int[] adindex = new int[] { searchdown[0], searchdown[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (Unit.tag != "Player")
                    {
                        int[] adindex = new int[] { searchdown[0], searchdown[1] };
                        AbleToMove.Add(adindex);
                    }

                }
                //Bishopの処理
                //x+ｙ+方向の処理
                int[] searchleftup = new int[] { index[0], index[1] };
                while (CheckIndex(searchleftup) == true)//範囲内か？
                {
                    searchleftup[0]++;
                    searchleftup[1]++;
                    if (CheckIndex(searchleftup) == false)
                    {
                        searchleftup[0]--;
                        searchleftup[1]--;
                        int[] adindex = new int[] { searchleftup[0], searchleftup[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchleftup[0], searchleftup[1]] != null)//現在のマスがnull
                    {
                        int[] adindex = new int[] { searchleftup[0], searchleftup[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (Unit.tag != "Player")
                    {
                        int[] adindex = new int[] { searchleftup[0], searchleftup[1] };
                        AbleToMove.Add(adindex);
                    }
                }
                //x-y+方向の処理
                int[] searchrightup = new int[] { index[0], index[1] };
                while (CheckIndex(searchrightup) == true)//範囲内か？
                {
                    searchrightup[0]--;
                    searchrightup[1]++;
                    if (CheckIndex(searchrightup) == false)
                    {
                        searchrightup[0]++;
                        searchrightup[1]--;
                        int[] adindex = new int[] { searchrightup[0], searchrightup[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchrightup[0], searchrightup[1]] != null)//現在のマスがnull
                    {
                        int[] adindex = new int[] { searchrightup[0], searchrightup[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (Unit.tag != "Player")
                    {
                        int[] adindex = new int[] { searchrightup[0], searchrightup[1] };
                        AbleToMove.Add(adindex);
                    }
                }
                //x+y-方向の処理
                int[] searchrightdown = new int[] { index[0], index[1] };
                while (CheckIndex(searchrightdown) == true)//範囲内か？
                {
                    searchrightdown[0]++;
                    searchrightdown[1]--;
                    if (CheckIndex(searchrightdown) == false)
                    {
                        searchrightdown[0]--;
                        searchrightdown[1]++;
                        int[] adindex = new int[] { searchrightdown[0], searchrightdown[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchrightdown[0], searchrightdown[1]] != null)//現在のマスがnull
                    {
                        int[] adindex = new int[] { searchrightdown[0], searchrightdown[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (Unit.tag != "Player")
                    {
                        int[] adindex = new int[] { searchrightdown[0], searchrightdown[1] };
                        AbleToMove.Add(adindex);
                    }
                }
                //x-y-方向の処理
                int[] searchleftdown = new int[] { index[0], index[1] };
                while (CheckIndex(searchleftdown) == true)//範囲内か？
                {
                    searchleftdown[0]--;
                    searchleftdown[1]--;
                    if (CheckIndex(searchleftdown) == false)
                    {
                        searchleftdown[0]++;
                        searchleftdown[1]++;
                        int[] adindex = new int[] { searchleftdown[0], searchleftdown[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchleftdown[0], searchleftdown[1]] != null)//現在探索中のマスがnullでない
                    {
                        int[] adindex = new int[] { searchleftdown[0], searchleftdown[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (Unit.tag != "Player")
                    {
                        int[] adindex = new int[] { searchleftdown[0], searchleftdown[1] };
                        AbleToMove.Add(adindex);
                    }
                }

            }
            else if (type == TYPE.KING)
            {
                //Kingの処理
                AbleToMove.Add(new int[] { index[0] - 1, index[1] + 0 });
                AbleToMove.Add(new int[] { index[0] - 1, index[1] + 1 });
                AbleToMove.Add(new int[] { index[0] + 0, index[1] + 1 });
                AbleToMove.Add(new int[] { index[0] + 1, index[1] + 1 });
                AbleToMove.Add(new int[] { index[0] + 1, index[1] + 0 });
                AbleToMove.Add(new int[] { index[0] + 1, index[1] - 1 });
                AbleToMove.Add(new int[] { index[0] + 0, index[1] - 1 });
                AbleToMove.Add(new int[] { index[0] - 1, index[1] - 1 });

                foreach (int[] search in AbleToMove)
                {
                    if (CheckIndex(search) == false)
                    {
                        elementsToRemove.Add(search);
                    }
                }

                foreach (int[] toRemove in elementsToRemove)
                {
                    AbleToMove.Remove(toRemove);
                }

            }
        }
        ///////プレイヤーでないとき止まるマスがEnemyであるなら選択肢から除外
        if (Unit.tag != "Player")
        {
            foreach (int[] search in AbleToMove)
            {
                if (units[search[0], search[1]].tag=="Enemy")
                {
                    elementsToRemove.Add(search);
                }
            }
            foreach (int[] toRemove in elementsToRemove)
            {
                AbleToMove.Remove(toRemove);
            }
        }
        ///////
        AbleToMove.RemoveAll(array => IsEqual(array,index));//自身のunitと同じ位置のインデックスは除く


        foreach (int[] search in AbleToMove)//作成したrist全てに対しての処理
        {
            Vector3 pos = ind_to_pos(search);
            pos.y = 1.1f;
            GameObject cursol = Instantiate(Cursol, pos, Quaternion.identity);//カーソルのプレハブ作成
            if (Unit.tag == "Player")
            {
                GameObject line = Instantiate(MoveLine);//lineを作成
                Vector3 start = ind_to_pos(index);
                line.GetComponent<CleateMoveLine>().CreateArchedLine(start, pos, 2.0f);
            }
        }
    }
    private void MakeHand(List<TYPE> types)//手札に加えるメソッド//typeの一つ目を参照して駒を作る
    {
        TYPE type = types[0];//1番目を参照して駒を作る
        for(int i = 0; i < 5; i++)
        {
            if (hands[i] == null)
            {
                Enums.TYPE[] values = (Enums.TYPE[])Enum.GetValues(typeof(Enums.TYPE));
                int index = Array.IndexOf(values, type);
                Vector3 hand_pos = new Vector3(8, 1, i*2-8);
                GameObject hand = Instantiate(prefabHand[index], hand_pos, Quaternion.identity);
                hands[i] = hand;
                hand.GetComponent<HandController>().THISTYPE=types;
                hand.GetComponent<HandController>().THISNAME = types[0].ToString();
                return;
            }
        }
        Debug.Log("すべての手が使用中です。");
    }
    public bool CheckIndex(int[] checkidx)//範囲内か調べるだけのメソッド
    {
        if (checkidx[0] >= 0 && checkidx[0] < TILE_X && checkidx[1] >= 0 && checkidx[1] < TILE_Y)//範囲内か？
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void DeleteCursol()
    {
        //カーソル削除
        GameObject[] Cursols = GameObject.FindGameObjectsWithTag("Cursol");
        foreach (GameObject cursol in Cursols)
        {
            Destroy(cursol);
        }
    }
    public bool AttackCheckByObj(GameObject Player, GameObject Enemy, TYPE type)
    {
        //敵の処理
        int[] E_idx = Enemy.GetComponent<UnitController>().index;
        int[] P_idx = Player.GetComponent<UnitController>().index;
        return AttackCheck(P_idx, E_idx, type);
    }
    public bool AttackCheck(int[] P_idx, int[] E_idx, TYPE type)//playerが特定のEnemyにtypeの時攻撃できるか
    {
        //敵の処理
        List<int[]> attacklist = new List<int[]>();
        List<int[]> elementsToRemove = new List<int[]>();

        if (type == TYPE.PAWN)
        {
            //PAWNの処理
            if (CheckIndex(new int[] { P_idx[0], P_idx[1] + 1 }) == true && units[P_idx[0], P_idx[1] + 1] == null)
            {
                attacklist.Add(new int[] { P_idx[0], P_idx[1] + 1 });
            }
            if (CheckIndex(new int[] { P_idx[0] + 1, P_idx[1] + 1 }) == true && units[P_idx[0] + 1, P_idx[1] + 1] != null && units[P_idx[0] + 1, P_idx[1] + 1].tag == "Enemy")
            {
                attacklist.Add(new int[] { P_idx[0] + 1, P_idx[1] + 1 });
            }
            if (CheckIndex(new int[] { P_idx[0] - 1, P_idx[1] + 1 }) == true && units[P_idx[0] - 1, P_idx[1] + 1] != null && units[P_idx[0] - 1, P_idx[1] + 1].tag == "Enemy")
            {
                attacklist.Add(new int[] { P_idx[0] - 1, P_idx[1] + 1 });
            }
        }
        else if (type == TYPE.KNIGHT)
        {
            //knightの処理
            attacklist.Add(new int[] { P_idx[0] - 1, P_idx[1] + 2 });
            attacklist.Add(new int[] { P_idx[0] + 1, P_idx[1] + 2 });
            attacklist.Add(new int[] { P_idx[0] - 1, P_idx[1] - 2 });
            attacklist.Add(new int[] { P_idx[0] + 1, P_idx[1] - 2 });
            attacklist.Add(new int[] { P_idx[0] - 2, P_idx[1] + 1 });
            attacklist.Add(new int[] { P_idx[0] + 2, P_idx[1] + 1 });
            attacklist.Add(new int[] { P_idx[0] - 2, P_idx[1] - 1 });
            attacklist.Add(new int[] { P_idx[0] + 2, P_idx[1] - 1 });

            foreach (int[] search in attacklist)
            {
                if (CheckIndex(search) == false)
                {
                    elementsToRemove.Add(search);
                }
            }

            foreach (int[] toRemove in elementsToRemove)
            {
                attacklist.Remove(toRemove);
            }
        }
        else if (type == TYPE.ROOK)
        {
            //ROOKの処理
            //x+方向の処理
            int[] searchleft = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchleft) == true)//範囲内か？
            {
                searchleft[0]++;
                if (CheckIndex(searchleft) == false)
                {
                    break;
                }
                else if (units[searchleft[0], searchleft[1]] != null)//現在のマスがnull
                {
                    int[] adindex = new int[] { searchleft[0], searchleft[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //x-方向の処理
            int[] searchright = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchright) == true)//範囲内か？
            {
                searchright[0]--;
                if (CheckIndex(searchright) == false)
                {
                    break;
                }
                else if (units[searchright[0], searchright[1]] != null)//現在のマスがnull
                {
                    int[] adindex = new int[] { searchright[0], searchright[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //y+方向の処理
            int[] searchup = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchup) == true)//範囲内か？
            {
                searchup[1]++;
                if (CheckIndex(searchup) == false)
                {
                    break;
                }
                else if (units[searchup[0], searchup[1]] != null)//現在のマスがnull
                {
                    int[] adindex = new int[] { searchup[0], searchup[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //y-方向の処理
            int[] searchdown = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchdown) == true)//範囲内か？
            {
                searchdown[1]--;
                if (CheckIndex(searchdown) == false)
                {
                    break;
                }
                else if (units[searchdown[0], searchdown[1]] != null)//現在のマスがnull
                {
                    int[] adindex = new int[] { searchdown[0], searchdown[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
        }
        else if (type == TYPE.BISHOP)
        {
            //Bishopの処理
            //x+ｙ+方向の処理
            int[] searchleftup = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchleftup) == true)//範囲内か？
            {
                searchleftup[0]++;
                searchleftup[1]++;
                if (CheckIndex(searchleftup) == false)
                {
                    break;
                }
                else if (units[searchleftup[0], searchleftup[1]] != null)//現在のマスがnull
                {
                    int[] adindex = new int[] { searchleftup[0], searchleftup[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //x-y+方向の処理
            int[] searchrightup = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchrightup) == true)//範囲内か？
            {
                searchrightup[0]--;
                searchrightup[1]++;
                if (CheckIndex(searchrightup) == false)
                {
                    break;
                }
                else if (units[searchrightup[0], searchrightup[1]] != null)//現在のマスがnull
                {
                    int[] adindex = new int[] { searchrightup[0], searchrightup[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //x+y-方向の処理
            int[] searchrightdown = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchrightdown) == true)//範囲内か？
            {
                searchrightdown[0]++;
                searchrightdown[1]--;
                if (CheckIndex(searchrightdown) == false)
                {
                    break;
                }
                else if (units[searchrightdown[0], searchrightdown[1]] != null)//現在のマスがnull
                {
                    int[] adindex = new int[] { searchrightdown[0], searchrightdown[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //x-y-方向の処理
            int[] searchleftdown = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchleftdown) == true)//範囲内か？
            {
                searchleftdown[0]--;
                searchleftdown[1]--;
                if (CheckIndex(searchleftdown) == false)
                {
                    break;
                }
                else if (units[searchleftdown[0], searchleftdown[1]] != null)//現在のマスがnull
                {
                    int[] adindex = new int[] { searchleftdown[0], searchleftdown[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
        }
        else if (type == TYPE.QUEEN)
        {
            //Queenの処理=rook+bishop
            //ROOKの処理
            //x+方向の処理
            int[] searchleft = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchleft) == true)//範囲内か？
            {
                searchleft[0]++;
                if (CheckIndex(searchleft) == false)
                {
                    break;
                }
                else if (units[searchleft[0], searchleft[1]] != null)//現在のマスがnull
                {
                    int[] adindex = new int[] { searchleft[0], searchleft[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //x-方向の処理
            int[] searchright = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchright) == true)//範囲内か？
            {
                searchright[0]--;
                if (CheckIndex(searchright) == false)
                {
                    break;
                }
                else if (units[searchright[0], searchright[1]] != null)//現在のマスがnull
                {
                    int[] adindex = new int[] { searchright[0], searchright[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //y+方向の処理
            int[] searchup = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchup) == true)//範囲内か？
            {
                searchup[1]++;
                if (CheckIndex(searchup) == false)
                {
                    break;
                }
                else if (units[searchup[0], searchup[1]] != null)//現在のマスがnull
                {
                    int[] adindex = new int[] { searchup[0], searchup[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //y-方向の処理
            int[] searchdown = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchdown) == true)//範囲内か？
            {
                searchdown[1]--;
                if (CheckIndex(searchdown) == false)
                {
                    break;
                }
                else if (units[searchdown[0], searchdown[1]] != null)//現在のマスがnull
                {
                    int[] adindex = new int[] { searchdown[0], searchdown[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //Bishopの処理
            //x+ｙ+方向の処理
            int[] searchleftup = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchleftup) == true)//範囲内か？
            {
                searchleftup[0]++;
                searchleftup[1]++;
                if (CheckIndex(searchleftup) == false)
                {
                    break;
                }
                else if (units[searchleftup[0], searchleftup[1]] != null)//現在のマスがnull
                {
                    int[] adindex = new int[] { searchleftup[0], searchleftup[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //x-y+方向の処理
            int[] searchrightup = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchrightup) == true)//範囲内か？
            {
                searchrightup[0]--;
                searchrightup[1]++;
                if (CheckIndex(searchrightup) == false)
                {
                    break;
                }
                else if (units[searchrightup[0], searchrightup[1]] != null)//現在のマスがnull
                {
                    int[] adindex = new int[] { searchrightup[0], searchrightup[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //x+y-方向の処理
            int[] searchrightdown = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchrightdown) == true)//範囲内か？
            {
                searchrightdown[0]++;
                searchrightdown[1]--;
                if (CheckIndex(searchrightdown) == false)
                {
                    break;
                }
                else if (units[searchrightdown[0], searchrightdown[1]] != null)//現在のマスがnull
                {
                    int[] adindex = new int[] { searchrightdown[0], searchrightdown[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //x-y-方向の処理
            int[] searchleftdown = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchleftdown) == true)//範囲内か？
            {
                searchleftdown[0]--;
                searchleftdown[1]--;
                if (CheckIndex(searchleftdown) == false)
                {
                    break;
                }
                else if (units[searchleftdown[0], searchleftdown[1]] != null)//現在のマスがnull
                {
                    int[] adindex = new int[] { searchleftdown[0], searchleftdown[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }

        }
        else if (type == TYPE.KING)
        {
            //Kingの処理
            attacklist.Add(new int[] { P_idx[0] - 1, P_idx[1] + 0 });
            attacklist.Add(new int[] { P_idx[0] - 1, P_idx[1] + 1 });
            attacklist.Add(new int[] { P_idx[0] + 0, P_idx[1] + 1 });
            attacklist.Add(new int[] { P_idx[0] + 1, P_idx[1] + 1 });
            attacklist.Add(new int[] { P_idx[0] + 1, P_idx[1] + 0 });
            attacklist.Add(new int[] { P_idx[0] + 1, P_idx[1] - 1 });
            attacklist.Add(new int[] { P_idx[0] + 0, P_idx[1] - 1 });
            attacklist.Add(new int[] { P_idx[0] - 1, P_idx[1] - 1 });

            foreach (int[] search in attacklist)
            {
                if (CheckIndex(search) == false)
                {
                    elementsToRemove.Add(search);
                }
            }

            foreach (int[] toRemove in elementsToRemove)
            {
                attacklist.Remove(toRemove);
            }

        }

        attacklist.RemoveAll(n => n == P_idx);//自身のunitと同じ位置のインデックスは除く

        //int[] hantei = Enemy.GetComponent<UnitController>().GetIndex();
        if (attacklist.Any(arr => IsEqual(arr, E_idx)))//攻撃判定成功
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    public void OnClickAttack()//UIAttackが押されたときの処理
    {
        Serect_Attack = true;
        Serect_Sucssesed = true;
    }
    public void OnClickDiffence()//UIDiffenceが押されたときの処理
    {
        //選択可能か判定

        if(CanDiffence == true){
            Serect_Diffence = true;
            Serect_Sucssesed = true;
        }
        else
        {
            MainCanvas.GetComponent<UIController>().ChengeTextWindow("カウンター不能");
        }
    }
    public void ClearStatus()
    {
        for (int i = status.Count - 1; i >= 0; i--)
        {
            RemoveStatus(status[i]);
        }
    }
    // STATUSを追加するメソッド
    public void AddStatus(STATUS newStatus)
    {
        status.Add(newStatus);
        OnStatusAdded?.Invoke(newStatus); // イベントを発生させる
    }
    // STATUSを削除するメソッド
    public void RemoveStatus(STATUS newStatus)
    {
        status.Remove(newStatus);
        OnStatusRemoved?.Invoke(newStatus); // イベントを発生させる
    }
    //statusが追加されたときの処理
    void HandleStatusAdded(STATUS newStatus)
    {
        Debug.Log("Status added: " + newStatus);
        switch (newStatus)
        {
            case STATUS.Serect_P:
                // Serect_P の場合の処理
                Vector3 pos =Player.transform.position;
                pos.y = pos.y + 1;
                Player.transform.position= pos;
                break;
            case STATUS.Serect_E:
                // Serect_E の場合の処理
                break;
            case STATUS.Skill_PAWN:
                StartCoroutine(skill_PAWN());
                break;
            case STATUS.SKill_BISHP:
                StartCoroutine(skill_BISHOP());
                break;
            case STATUS.Skill_KNIGHT:
                StartCoroutine(skill_KNIGHT());
                break;
            case STATUS.Skill_ROOK:
                StartCoroutine(skill_ROOK());
                break;
            case STATUS.SKill_KING:
                StartCoroutine(skill_KING());
                break;
            case STATUS.SKill_QUEEN:
                StartCoroutine(skill_QUEEN());
                break;
            default:
                // 上記のいずれにも当てはまらない場合の処理
                break;
        }
    }
    //statusが削除された時の処理
    void HandleStatusRemoved(STATUS removeStatus)
    {
        Debug.Log("Status Removed: " + removeStatus);
        switch (removeStatus)
        {
            case STATUS.Serect_P:
                // Serect_P の場合の処理
                Vector3 pos = Player.transform.position;
                pos.y = 1;
                Player.transform.position = pos;
                break;
            case STATUS.Serect_E:
                // Serect_E の場合の処理
                break;
            default:
                // 上記のいずれにも当てはまらない場合の処理
                break;
        }
    }
    IEnumerator skill_PAWN()
    {

        bool check = true;
        Debug.Log(Player.GetComponent<UnitController>().index[1]);
        if (Player.GetComponent<UnitController>().index[1]!=7) 
        {
            check = false;
            MainCanvas.GetComponent<UIController>().ChengeTextWindow("使用できません");
        }
        else
        {
            MainCanvas.GetComponent<UIController>().ChengeTextWindow("対象を選択してください");
        }

        while (check == true)
        {
            yield return null;
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (!EventSystem.current.IsPointerOverGameObject())//UIとかぶる時判定しない
                {
                    if (Physics.Raycast(ray, out hit))
                    {
                        GameObject targetObject = hit.collider.gameObject;
                        // クリックされたときの処理を記述する
                        if (targetObject.tag == "Enemy")
                        {
                            MainCanvas.GetComponent<UIController>().ChengeTextWindow("");
                            MakeHand(targetObject.GetComponent<UnitController>().THISTYPE);
                            check = false;
                        }
                        else
                        {
                            MainCanvas.GetComponent<UIController>().ChengeTextWindow("無効な対象");
                        }
                    }
                }
            }
        }
        MainCanvas.GetComponent<UIController>().ChengeTextWindow("");
        RemoveStatus(STATUS.Skill_PAWN);
        Use_Skill = false;
    }
    IEnumerator skill_KNIGHT()
    {
        MainCanvas.GetComponent<UIController>().ChengeTextWindow("対象を選択してください(0/2)");
        Use_Skill = true;
        GameObject target1 = null;
        GameObject target2 = null;
        bool check1 = true;
        bool check2 = true;
        while (check1 == true || check2 == true)
        {
            yield return null;
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (!EventSystem.current.IsPointerOverGameObject())//UIとかぶる時判定しない
                {
                    if (Physics.Raycast(ray, out hit))
                    {
                        GameObject targetObject = hit.collider.gameObject;
                        // クリックされたときの処理を記述する
                        if (targetObject.tag == "Enemy")//タイルをクリックしたとき・・・・・・・・・
                        {
                            if(check1 == true)
                            {
                                MainCanvas.GetComponent<UIController>().ChengeTextWindow("対象を選択してください(1/2)");
                                target1 = targetObject;
                                check1 = false;
                            }
                            else
                            {
                                target2 = targetObject;
                                check2 = false;
                            }
                        }
                    }
                }
            }
        }
        int tmp = target1.GetComponent<UnitController>().number;
        target1.GetComponent<UnitController>().number = target2.GetComponent<UnitController>().number;
        target2.GetComponent<UnitController>().number = tmp;

        unitfornuber[target1.GetComponent<UnitController>().number] = target1;
        unitfornuber[target2.GetComponent<UnitController>().number] = target2;

        MainCanvas.GetComponent<UIController>().ChengeTextWindow("");
        RemoveStatus(STATUS.Skill_KNIGHT);
        Use_Skill = false;//sukirusyuuryou?
    }
    IEnumerator skill_ROOK()
    {
        MainCanvas.GetComponent<UIController>().ChengeTextWindow("対象を選択してください(0/2)");
        Use_Skill = true;
        GameObject target1 = null;
        GameObject target2 = null;
        bool check1 = true;
        bool check2 = true;
        while (check1 == true || check2 == true)
        {
            yield return null;
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (!EventSystem.current.IsPointerOverGameObject())//UIとかぶる時判定しない
                {
                    if (Physics.Raycast(ray, out hit))
                    {
                        GameObject targetObject = hit.collider.gameObject;
                        // クリックされたときの処理を記述する
                        if (targetObject.tag == "Enemy")//タイルをクリックしたとき・・・・・・・・・
                        {
                            if (check1 == true)
                            {
                                MainCanvas.GetComponent<UIController>().ChengeTextWindow("対象を選択してください(1/2)");
                                target1 = targetObject;
                                check1 = false;
                            }
                            else
                            {
                                target2 = targetObject;
                                check2 = false;
                            }
                        }
                    }
                }
            }
        }
        int[] tmp = target1.GetComponent<UnitController>().index;
        target1.GetComponent<UnitController>().index = target2.GetComponent<UnitController>().index;
        target2.GetComponent<UnitController>().index = tmp;

        units[target1.GetComponent<UnitController>().index[0], target1.GetComponent<UnitController>().index[1]] = target1;
        units[target2.GetComponent<UnitController>().index[0], target2.GetComponent<UnitController>().index[1]] = target2;
        target1.transform.position = ind_to_pos(target1.GetComponent<UnitController>().index);
        target2.transform.position = ind_to_pos(target2.GetComponent<UnitController>().index);

        MainCanvas.GetComponent<UIController>().ChengeTextWindow("");
        RemoveStatus(STATUS.Skill_ROOK);
        Use_Skill = false;//sukirusyuuryou?
    }
    IEnumerator skill_BISHOP()
    {
        MainCanvas.GetComponent<UIController>().ChengeTextWindow("対象を選択してください");
        Use_Skill = true;
        GameObject target = null;
        bool check = true;
        while (check == true)
        {
            yield return null;
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (!EventSystem.current.IsPointerOverGameObject())//UIとかぶる時判定しない
                {
                    if (Physics.Raycast(ray, out hit))
                    {
                        GameObject targetObject = hit.collider.gameObject;
                        // クリックされたときの処理を記述する
                        if (targetObject.tag == "Enemy")
                        {
                            target = targetObject;
                            MakeCursol(target);
                            check = false;
                        }
                    }
                }
            }
        }
        check = true;
        MainCanvas.GetComponent<UIController>().ChengeTextWindow("移動先を選択してください");
        while (check == true)
        {
            yield return null;
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (!EventSystem.current.IsPointerOverGameObject())//UIとかぶる時判定しない
                {
                    if (Physics.Raycast(ray, out hit))
                    {
                        GameObject targetObject = hit.collider.gameObject;
                        // クリックされたときの処理を記述する
                        if (targetObject.tag == "Tile")
                        {
                            int[] hantei = targetObject.GetComponent<UnitController>().index;
                            if(AbleToMove.Any(arr => IsEqual(arr, hantei)))
                            {
                                units[target.GetComponent<UnitController>().index[0], target.GetComponent<UnitController>().index[0]] = null;
                                target.GetComponent<UnitController>().MoveUnit(targetObject.GetComponent<UnitController>().index);
                                units[target.GetComponent<UnitController>().index[0], target.GetComponent<UnitController>().index[0]] = target;
                                check = false;
                            }
                            else
                            {
                                MainCanvas.GetComponent<UIController>().ChengeTextWindow("移動不能な地点です");
                            }
                            
                        }
                    }
                }
            }
        }

        MainCanvas.GetComponent<UIController>().ChengeTextWindow("");
        RemoveStatus(STATUS.SKill_BISHP);
        Use_Skill = false;//sukirusyuuryou?
    }
    IEnumerator skill_KING()
    {
        Use_Skill=true;
        yield return null;
        Life++;
        RemoveStatus(STATUS.SKill_KING);
        Use_Skill=false;
    }
    IEnumerator skill_QUEEN()
    {
        MainCanvas.GetComponent<UIController>().ChengeTextWindow("対象を選択してください");
        Use_Skill = true;
        GameObject target = null;
        bool check = true;
        while (check == true)
        {
            yield return null;
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (!EventSystem.current.IsPointerOverGameObject())//UIとかぶる時判定しない
                {
                    if (Physics.Raycast(ray, out hit))
                    {
                    GameObject targetObject = hit.collider.gameObject;
                        // クリックされたときの処理を記述する
                        if (targetObject.tag == "Enemy")//タイルをクリックしたとき・・・・・・・・・
                        {
                            target = targetObject;
                            check = false;
                        }
                    }
                }
            }
        }
        Destroy(target);

        MainCanvas.GetComponent<UIController>().ChengeTextWindow("");
        RemoveStatus(STATUS.SKill_QUEEN);
        Use_Skill = false;//sukirusyuuryou?
    }
    // ライフプロパティ
    public int Life
    {
        get { return _life; }
        set
        {
            _life = value;
            MainCanvas.GetComponent<UIController>().ShowHP(value);
            // ライフが0になったらゲームオーバー処理を行う
            if (_life <= 0)
            {
                GameOver();
            }
        }
    }
    //APプロパティ
    public int AP
    {
        get { return _ap; }
        set
        {
            _ap = value;
            //AP表示を変更
            MainCanvas.GetComponent<UIController>().ShowAP(value);
        }
    }
    // ゲームオーバー処理
    void GameOver()
    {
        // ゲームオーバー画面を表示する
        gameOverPanel.SetActive(true);

        // ゲームを一時停止する（必要に応じて）
        //Time.timeScale = 0f;
    }
    // ゲームクリア処理
    void GameClear()
    {
        // ゲームクリア画面を表示する
        gameClearPanel.SetActive(true);

        // ゲームを一時停止する（必要に応じて）
        //Time.timeScale = 0f;
    }
    public void OnClickRetry()
    {
        // MainSceneへ移動する
        SceneManager.LoadScene("MainScene");
    }
    public void Quit()
    {        
        // ゲームを終了する
        Application.Quit();
    }
}
