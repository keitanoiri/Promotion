using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

using Enums;
using System.Linq;//���O��Ԃ��O������



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
    ///��
    [SerializeField] private AudioSource MainAudio;//AudioSource�^�̕ϐ�a��錾 �g�p����AudioSource�R���|�[�l���g���A�^�b�`�K�v
    [SerializeField] private AudioClip BGM;//AudioClip�^�̕ϐ�b1��錾 �g�p����AudioClip���A�^�b�`�K�v
    [SerializeField] private AudioClip SE_Serect;//AudioClip�^�̕ϐ�b2��錾 �g�p����AudioClip���A�^�b�`�K�v 


    // STATUS���ǉ����ꂽ�Ƃ��ɔ�������C�x���g
    public event Action<STATUS> OnStatusAdded;
    // STATUS����菜���ꂽ�Ƃ��ɔ�������C�x���g
    public event Action<STATUS> OnStatusRemoved;

    public List<STATUS> status;
    public List<int[]> AbleToMove;

    public const int TILE_X = 8;
    public const int TILE_Y = 8;
    public const int UNIT_NUM = 8;
    private int _life = 3;//���C�t�̏����l
    private int _ap = 1;//AP�̏����l

    public GameObject[] prefabTile;
    public GameObject[] prefabUnit;
    public GameObject[] prefabHand;
    public GameObject prefabStart;
    public GameObject prefabGoal;
    public GameObject prefabPlayer;
    public GameObject Cursol;
    public GameObject MoveLine;
    public GameObject gameOverPanel; // �Q�[���I�[�o�[��ʂ�UI�p�l��
    public GameObject gameClearPanel; // �Q�[���N���A��ʂ�UI�p�l��
    public GameObject MainCanvas;//���C���L�����o�X
    public GameObject SubCanvas;
    public GameObject ExtencionMark;//!ma-ku

    //�����f�[�^
    GameObject[,] tiles;�@//8*8�̃^�C�����
    GameObject[,] units;
    GameObject[] hands;
    GameObject Player;
    GameObject[] unitfornuber;
    List<TYPE> P_Default_Type;
    public�@bool P_turn;
    bool E_turn;
    bool Serect_Sucssesed;
    bool CanDiffence;
    bool Serect_Attack = false;
    bool Serect_Diffence = false;
    public bool Use_Skill = false;


    // Start is called before the first frame update
    void Start()
    {
        //�����f�[�^
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

        //��̃����_���z�u
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

        for (int i = 0; i < TILE_X; i++)//�^�C���̏����z�u
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

        for(int i = 0;i < TILE_X; i++)//�X�^�[�g�n�_�̔z�u
        {
            float x = (i - TILE_X / 2) * 2;

            Vector3 pos = new Vector3(x, 0, -10);

            GameObject start_tile = Instantiate(prefabStart, pos, Quaternion.identity);
        }

        for (int i = 0; i < TILE_X; i++)//�S�[���n�_�̔z�u
        {
            float x = (i - TILE_X / 2) * 2;

            Vector3 pos = new Vector3(x, 0, 10);

            GameObject goal_tile = Instantiate(prefabGoal, pos, Quaternion.identity);
        }
        MainCanvas.GetComponent<UIController>().ChengeTextWindow("�X�^�[�g�n�_��I�����Ă��������B");
    }

    // Update is called once per frame
    void Update()
    {
        if (P_turn == true && E_turn == false) //�����̃^�[���Ȃ�
        {
            if (Use_Skill==false && Input.GetMouseButtonDown(0)) // ���N���b�N�������ꂽ��
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (EventSystem.current.IsPointerOverGameObject()) return;//UI�Ƃ��Ԃ鎞���肵�Ȃ�
                if (Physics.Raycast(ray, out hit))
                {
                    GameObject targetObject = hit.collider.gameObject;
                    // �N���b�N���ꂽ�Ƃ��̏������L�q����
                    if (targetObject.tag == "Tile")//�^�C�����N���b�N�����Ƃ��E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E
                    {
                        if (status.Contains(STATUS.Serect_P))//�v���C���[�I����ԂȂ�����𖞂����΃v���C���[�ړ�
                        {
                            int[] hantei = pos_to_ind(targetObject.transform.position);
                            if (AP > 0)
                            {
                                if (AbleToMove.Any(arr => IsEqual(arr, hantei)))
                                {
                                    UnitController p_controller = Player.GetComponent<UnitController>();
                                    int[] idx = pos_to_ind(targetObject.transform.position);
                                    int[] p_oldindex = p_controller.index;
                                    if (CheckIndex(p_oldindex) == true)//�v���C���[�̃C���f�b�N�X���X�^�[�g�n�_�o�Ȃ����units���珜�O
                                    {
                                        units[p_oldindex[0], p_oldindex[1]] = null;
                                    }
                                    p_controller.MoveUnit(idx);//�ړ�
                                    units[idx[0], idx[1]] = Player;
                                    AbleToMove.Clear();
                                    p_controller.THISTYPE = P_Default_Type;//�ړ�������type���f�t�H���g�ɖ߂�
                                    //EnemyReaction
                                    StartCoroutine(EnemyReaction(p_oldindex, idx));

                                    //AP����
                                    AP--;
                                    status.Remove(STATUS.Serect_P);
                                }
                                else
                                {
                                    MainCanvas.GetComponent<UIController>().ChengeTextWindow("�ړ��\�ȃ}�X��I�����Ă�������");
                                }
                            }
                            else
                            {
                                MainCanvas.GetComponent<UIController>().ChengeTextWindow("AP������܂���");
                            }

                        }
                    }
                    else if (targetObject.tag == "Player")//�v���C���[���N���b�N�����Ƃ��E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E
                    {
                        if (status.Contains(STATUS.Serect_P))//�v���C���[�I����ԂȂ�
                        {
                            RemoveStatus(STATUS.Serect_P);
                            DeleteCursol();
                        }
                        else//�v���C���[���I������Ă��Ȃ��Ȃ�
                        {
                            MainAudio.PlayOneShot(SE_Serect);
                            AddStatus(STATUS.Serect_P);
                            //�ړ��\�ȃ}�X�𖾎�
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
                    else if (targetObject.tag == "Enemy")//�G���N���b�N�����Ƃ��E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E�E
                    {
                        if (status.Contains(STATUS.Serect_P))//�v���C���[�I����ԂȂ�G�̋�����
                        {
                            //����鏈��
                            int[] hantei = pos_to_ind(targetObject.transform.position);
                            if (AP > 0)
                            {
                                if (AbleToMove.Any(arr => IsEqual(arr, hantei)))
                                {
                                    UnitController p_controller = Player.GetComponent<UnitController>();
                                    int[] idx = pos_to_ind(targetObject.transform.position);
                                    int[] p_oldindex = p_controller.index;
                                    if (CheckIndex(p_oldindex) == true)//�v���C���[�̃C���f�b�N�X���X�^�[�g�n�_�o�Ȃ����units���珜�O
                                    {
                                        units[p_oldindex[0], p_oldindex[1]] = null;
                                    }



                                    p_controller.MoveUnit(idx);
                                    units[idx[0], idx[1]] = Player;

                                    AbleToMove.Clear();

                                    //hand��ǉ�
                                    MakeHand(targetObject.GetComponent<UnitController>().THISTYPE);
                                    //���j��
                                    Destroy(targetObject);

                                    //�G�̃��A�N�V����
                                    StartCoroutine(EnemyReaction(p_oldindex, idx));
                                    //�ړ������̂�AP����
                                    AP--;

                                    status.Remove(STATUS.Serect_P);
                                }
                                else
                                {
                                    MainCanvas.GetComponent<UIController>().ChengeTextWindow("�ړ��\�ȃ}�X��I�����Ă�������");
                                }
                            }
                            else
                            {
                                MainCanvas.GetComponent<UIController>().ChengeTextWindow("AP������܂���");
                            }
                        }
                        else if (status.Contains(STATUS.Serect_E))//Enemy�I����ԂȂ�
                        {
                            status.Remove(STATUS.Serect_E);//Enemy�I����ԉ���
                        }
                        else//�����I������Ă��Ȃ����
                        {
                            status.Add(STATUS.Serect_E);//Enemy�I����Ԃ�
                        }

                    }
                    else if (targetObject.tag == "Hand")
                    {
                        if (Player != null)
                        {
                            //�^�C�v�ύXor�������
                            HandController handController = targetObject.GetComponent<HandController>();
                            StartCoroutine(handController.OnSerected(Player));

                        }
                        else
                        {
                            Debug.Log("�X�^�[�g�n�_��I�����Ă�������");
                        }


                        //�^�C�v�ύXor�\�͎g�p
                    }
                    else if (targetObject.tag == "Start_tile")//Start�̏���
                    {
                        if (Player == null)
                        {
                            MainCanvas.GetComponent<UIController>().ChengeTextWindow("");
                            //�v���C���[�z�u
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
                                MainCanvas.GetComponent<UIController>().ChengeTextWindow("�S�[���ł��܂���");
                            }
                        }
                    }
                }
            }else if(Input.GetMouseButtonDown(1))
            {
                ClearStatus();//status���������i�L�����Z������
            }
        }

        //�G�̃^�[���̏���
        else if (P_turn == false&&E_turn==false)//����̃^�[��
        {
            E_turn = true;
            Debug.Log("�G�̃^�[��");
            if (Player != null)
            {
                StartCoroutine(EnemyTurn());
            }
            else
            {
                E_turn = false;
                MainCanvas.GetComponent<UIController>().ChengeTextWindow("�X�^�[�g�n�_��I�����Ă��������B");
            }
            AP = 1;//AP��
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
                        // array �ɑ΂��鏈�����s��
                        if (units[array[0], array[1]] == null && AttackCheck(array, E_idx, type) == true)//�G�̍U�����萬��
                        {
                            //!�}�[�N�쐬
                            Vector3 markpos = ind_to_pos(E_idx);
                            GameObject mark = Instantiate(ExtencionMark, markpos, Quaternion.identity);
                            //�G���ړ�
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
    // 2�̒n�_�̊Ԃ̃}�X�ڂ��擾���郁�\�b�h
    public static List<int[]> GetGridBetweenPoints(int[] start, int[] end)
    {
        List<int[]> gridBetweenPoints = new List<int[]>();

        // x �����̕ω��ʂ��v�Z
        int deltaX = end[0] - start[0];
        // y �����̕ω��ʂ��v�Z
        int deltaY = end[1] - start[1];

        // x ������ y �����̂ǂ��炪�傫�����𔻒肵�A�傫�����̕ω��ʂ� step �Ƃ���
        int step = Math.Max(Math.Abs(deltaX), Math.Abs(deltaY));

        // �X�e�b�v���ƂɃ}�X�ڂ��v�Z���A���ʂ� gridBetweenPoints �ɒǉ�
        for (int i = 1; i < step; i++)
        {
            float t = i / (float)step;
            int x = Mathf.RoundToInt(Mathf.Lerp(start[0], end[0], t));
            int y = Mathf.RoundToInt(Mathf.Lerp(start[1], end[1], t));
            gridBetweenPoints.Add(new int[] { x, y });
        }

        return gridBetweenPoints;
    }
    IEnumerator EnemyTurn()//�G�̃^�[���̓���
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
                        if (AttackCheckByObj(Player, unitfornuber[i], E_type) == true)//�G�̍U�����萬��
                        {
                            //!�}�[�N�ƌ��ʉ�
                            Vector3 pos = ind_to_pos(E_idx);
                            pos.y = 3f;
                            GameObject mark = Instantiate(ExtencionMark, pos, Quaternion.identity);

                            //�U�����j�b�g������
                            GameObject line = Instantiate(MoveLine);//line���쐬
                            Vector3 start = ind_to_pos(E_idx);
                            Vector3 end = ind_to_pos(P_idx);
                            line.GetComponent<CleateMoveLine>().CreateArchedLine(start, end, 1.0f);

                            if (AttackCheckByObj(Player, unitfornuber[i], P_type))//�����\���H
                            {
                                CanDiffence = true;
                            }
                            SubCanvas.SetActive(true);
                            // �{�^�����������܂őҋ@
                            while (Serect_Sucssesed == false)
                            {
                                yield return null;
                            }
                            // �{�^���������ꂽ��̏���
                            // Canvas ���\���ɂ���
                            Serect_Sucssesed = false;
                            SubCanvas.SetActive(false);
                            DeleteCursol();

                            if (Serect_Attack == true)////�m�[�肠�������
                            {
                                Serect_Attack = false;

                                //�v���C���[�ړ���
                                if (E_idx[0] == P_idx[0]) { move_idx[0] = P_idx[0]; }
                                else if (E_idx[0] < P_idx[0]) { move_idx[0] = P_idx[0] + 1; }
                                else { move_idx[0] = P_idx[0] - 1; }
                                if (E_idx[1] == P_idx[1]) { move_idx[1] = P_idx[1]; }
                                else if (E_idx[1] < P_idx[1]) { move_idx[1] = P_idx[1] + 1; }
                                else { move_idx[1] = P_idx[1] - 1; }

                                units[E_idx[0], E_idx[1]] = null;

                                if (CheckIndex(move_idx) == true && units[move_idx[0], move_idx[1]] == null)//�ړ��悪�͈͓��Ȃ�
                                {
                                    //�v���C���[�ړ�
                                    if (CheckIndex(P_idx) == true)//�v���C���[�̃C���f�b�N�X���X�^�[�g�n�_�o�Ȃ����units���珜�O
                                    {
                                        units[P_idx[0], P_idx[1]] = null;
                                    }
                                    //idx�Ɉړ���̃C���f�b�N�X���i�[
                                    p_controller.MoveUnit(move_idx);
                                    units[move_idx[0], move_idx[1]] = Player;
                                    AbleToMove.Clear();
                                }//�ǂ�G�ɂԂ���Ȃ�
                                else
                                {
                                    Life--;//�ǉ��Ń_���[�W
                                }
                                //hand��ǉ�
                                MakeHand(unitfornuber[i].GetComponent<UnitController>().THISTYPE);
                                //���j��
                                Destroy(unitfornuber[i]);
                                //LIFE����
                                Life--;
                            }
                            else if (Serect_Diffence == true)//���A�N�V����
                            {
                                Serect_Diffence = false;


                                //�v���C���[�ړ���
                                if (E_idx[0] == P_idx[0]) { move_idx[0] = P_idx[0]; }
                                else if (E_idx[0] < P_idx[0]) { move_idx[0] = P_idx[0] - 1; }
                                else { move_idx[0] = P_idx[0] + 1; }
                                if (E_idx[1] == P_idx[1]) { move_idx[1] = P_idx[1]; }
                                else if (E_idx[1] < P_idx[1]) { move_idx[1] = P_idx[1] - 1; }
                                else { move_idx[1] = P_idx[1] + 1; }

                                units[E_idx[0], E_idx[1]] = null;

                                if (CheckIndex(move_idx) == true && units[move_idx[0], move_idx[1]] == null)//�ړ���̂��͈͓��Ȃ�
                                {
                                    //�v���C���[�ړ�
                                    if (CheckIndex(P_idx) == true)//�v���C���[�̃C���f�b�N�X���X�^�[�g�n�_�o�Ȃ����units���珜�O
                                    {
                                        units[P_idx[0], P_idx[1]] = null;
                                    }
                                    //idx�Ɉړ���̃C���f�b�N�X���i�[
                                    p_controller.MoveUnit(move_idx);
                                    units[move_idx[0], move_idx[1]] = Player;
                                    AbleToMove.Clear();
                                }//�ǂ�G�ɂԂ���Ȃ�
                                else
                                {
                                    //��𓮂����Ȃ�
                                }
                                //hand��ǉ����Ȃ�
                                //���j��
                                Destroy(unitfornuber[i]);
                                //LIFE�������Ȃ�
                            }
                        }
                    }
                }
            }
        }
        p_controller.THISTYPE = P_Default_Type;
        E_turn=false;//�G�̃^�[�����I��
    }
    private bool IsEqual(int[] array1, int[] array2)//List�̒��g����v���邩�̃`�F�b�N
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
    public int[] pos_to_ind(Vector3 pos)//Vecter3����C���f�b�N�X�ɕϊ�����֐�
    {
        Vector2 ind_pos = new Vector2((pos.x / 2) + (TILE_X / 2), (pos.z / 2) + (TILE_Y / 2));
        int[] ind = new int[2];
        ind[0] = (int)ind_pos.x;
        ind[1] = (int)ind_pos.y;
        return ind;
    }
    public Vector3 ind_to_pos(int[] index)//�C���f�b�N�X����Vector�R�ɕϊ�����֐�
    {
        float x = (index[0] - TILE_X / 2) * 2;
        float y = (index[1] - TILE_Y / 2) * 2;
        Vector3 pos = new Vector3(x, 1, y);
        return pos;
    }
    private void MakeCursol(GameObject Unit)
    {
        Debug.Log("make�J�[�\��");
        AbleToMove.Clear();//list���N���A
        List<TYPE> Objtype;
        int[] index = null;

        Objtype = Unit.GetComponent<UnitController>().THISTYPE;
        index = Unit.GetComponent<UnitController>().index;
        


        List<int[]> elementsToRemove = new List<int[]>();
        foreach (TYPE type in Objtype)
        {
            if (type == TYPE.PAWN)
            {
                //PAWN�̏���
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
                //knight�̏���
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
                //ROOK�̏���
                //x+�����̏���
                int[] searchleft = new int[] { index[0], index[1] };
                while (CheckIndex(searchleft) == true)//�͈͓����H
                {
                    searchleft[0]++;
                    if (CheckIndex(searchleft) == false)
                    {
                        searchleft[0]--;
                        int[] adindex = new int[] { searchleft[0], searchleft[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchleft[0], searchleft[1]] != null)//�T�����̃}�X�ɋ����
                    {
                        int[] adindex = new int[] { searchleft[0], searchleft[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (Unit.tag!="Player")///�v���C���[�Ŗ�����Γ������ړ��\�ɂ���
                    {
                        int[] adindex = new int[] { searchleft[0], searchleft[1] };
                        AbleToMove.Add(adindex);
                    }
                }
                //x-�����̏���
                int[] searchright = new int[] { index[0], index[1] };
                while (CheckIndex(searchright) == true)//�͈͓����H
                {
                    searchright[0]--;
                    if (CheckIndex(searchright) == false)
                    {
                        searchright[0]++;
                        int[] adindex = new int[] { searchright[0], searchright[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchright[0], searchright[1]] != null)//���݂̃}�X��null
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
                //y+�����̏���
                int[] searchup = new int[] { index[0], index[1] };
                while (CheckIndex(searchup) == true)//�͈͓����H
                {
                    searchup[1]++;
                    if (CheckIndex(searchup) == false)
                    {
                        searchup[1]--;
                        int[] adindex = new int[] { searchup[0], searchup[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchup[0], searchup[1]] != null)//���݂̃}�X��null
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
                //y-�����̏���
                int[] searchdown = new int[] { index[0], index[1] };
                while (CheckIndex(searchdown) == true)//�͈͓����H
                {
                    searchdown[1]--;
                    if (CheckIndex(searchdown) == false)
                    {
                        searchdown[1]++;
                        int[] adindex = new int[] { searchdown[0], searchdown[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchdown[0], searchdown[1]] != null)//���݂̃}�X��null
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
                //Bishop�̏���
                //x+��+�����̏���
                int[] searchleftup = new int[] { index[0], index[1] };
                while (CheckIndex(searchleftup) == true)//�͈͓����H
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
                    else if (units[searchleftup[0], searchleftup[1]] != null)//���݂̃}�X��null
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
                //x-y+�����̏���
                int[] searchrightup = new int[] { index[0], index[1] };
                while (CheckIndex(searchrightup) == true)//�͈͓����H
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
                    else if (units[searchrightup[0], searchrightup[1]] != null)//���݂̃}�X��null
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
                //x+y-�����̏���
                int[] searchrightdown = new int[] { index[0], index[1] };
                while (CheckIndex(searchrightdown) == true)//�͈͓����H
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
                    else if (units[searchrightdown[0], searchrightdown[1]] != null)//���݂̃}�X��null
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
                //x-y-�����̏���
                int[] searchleftdown = new int[] { index[0], index[1] };
                while (CheckIndex(searchleftdown) == true)//�͈͓����H
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
                    else if (units[searchleftdown[0], searchleftdown[1]] != null)//���݂̃}�X��null
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
                //Queen�̏���=rook+bishop
                //ROOK�̏���
                //x+�����̏���
                int[] searchleft = new int[] { index[0], index[1] };
                while (CheckIndex(searchleft) == true)//�͈͓����H
                {
                    searchleft[0]++;
                    if (CheckIndex(searchleft) == false)
                    {
                        searchleft[0]--;
                        int[] adindex = new int[] { searchleft[0], searchleft[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchleft[0], searchleft[1]] != null)//���݂̃}�X��null
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
                //x-�����̏���
                int[] searchright = new int[] { index[0], index[1] };
                while (CheckIndex(searchright) == true)//�͈͓����H
                {
                    searchright[0]--;
                    if (CheckIndex(searchright) == false)
                    {
                        searchright[0]++;
                        int[] adindex = new int[] { searchright[0], searchright[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchright[0], searchright[1]] != null)//���݂̃}�X��null
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
                //y+�����̏���
                int[] searchup = new int[] { index[0], index[1] };
                while (CheckIndex(searchup) == true)//�͈͓����H
                {
                    searchup[1]++;
                    if (CheckIndex(searchup) == false)
                    {
                        searchup[1]--;
                        int[] adindex = new int[] { searchup[0], searchup[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchup[0], searchup[1]] != null)//���݂̃}�X��null
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
                //y-�����̏���
                int[] searchdown = new int[] { index[0], index[1] };
                while (CheckIndex(searchdown) == true)//�͈͓����H
                {
                    searchdown[1]--;
                    if (CheckIndex(searchdown) == false)
                    {
                        searchdown[1]++;
                        int[] adindex = new int[] { searchdown[0], searchdown[1] };
                        AbleToMove.Add(adindex);
                        break;
                    }
                    else if (units[searchdown[0], searchdown[1]] != null)//���݂̃}�X��null
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
                //Bishop�̏���
                //x+��+�����̏���
                int[] searchleftup = new int[] { index[0], index[1] };
                while (CheckIndex(searchleftup) == true)//�͈͓����H
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
                    else if (units[searchleftup[0], searchleftup[1]] != null)//���݂̃}�X��null
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
                //x-y+�����̏���
                int[] searchrightup = new int[] { index[0], index[1] };
                while (CheckIndex(searchrightup) == true)//�͈͓����H
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
                    else if (units[searchrightup[0], searchrightup[1]] != null)//���݂̃}�X��null
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
                //x+y-�����̏���
                int[] searchrightdown = new int[] { index[0], index[1] };
                while (CheckIndex(searchrightdown) == true)//�͈͓����H
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
                    else if (units[searchrightdown[0], searchrightdown[1]] != null)//���݂̃}�X��null
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
                //x-y-�����̏���
                int[] searchleftdown = new int[] { index[0], index[1] };
                while (CheckIndex(searchleftdown) == true)//�͈͓����H
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
                    else if (units[searchleftdown[0], searchleftdown[1]] != null)//���ݒT�����̃}�X��null�łȂ�
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
                //King�̏���
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
        ///////�v���C���[�łȂ��Ƃ��~�܂�}�X��Enemy�ł���Ȃ�I�������珜�O
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
        AbleToMove.RemoveAll(array => IsEqual(array,index));//���g��unit�Ɠ����ʒu�̃C���f�b�N�X�͏���


        foreach (int[] search in AbleToMove)//�쐬����rist�S�Ăɑ΂��Ă̏���
        {
            Vector3 pos = ind_to_pos(search);
            pos.y = 1.1f;
            GameObject cursol = Instantiate(Cursol, pos, Quaternion.identity);//�J�[�\���̃v���n�u�쐬
            if (Unit.tag == "Player")
            {
                GameObject line = Instantiate(MoveLine);//line���쐬
                Vector3 start = ind_to_pos(index);
                line.GetComponent<CleateMoveLine>().CreateArchedLine(start, pos, 2.0f);
            }
        }
    }
    private void MakeHand(List<TYPE> types)//��D�ɉ����郁�\�b�h//type�̈�ڂ��Q�Ƃ��ċ�����
    {
        TYPE type = types[0];//1�Ԗڂ��Q�Ƃ��ċ�����
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
        Debug.Log("���ׂĂ̎肪�g�p���ł��B");
    }
    public bool CheckIndex(int[] checkidx)//�͈͓������ׂ邾���̃��\�b�h
    {
        if (checkidx[0] >= 0 && checkidx[0] < TILE_X && checkidx[1] >= 0 && checkidx[1] < TILE_Y)//�͈͓����H
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
        //�J�[�\���폜
        GameObject[] Cursols = GameObject.FindGameObjectsWithTag("Cursol");
        foreach (GameObject cursol in Cursols)
        {
            Destroy(cursol);
        }
    }
    public bool AttackCheckByObj(GameObject Player, GameObject Enemy, TYPE type)
    {
        //�G�̏���
        int[] E_idx = Enemy.GetComponent<UnitController>().index;
        int[] P_idx = Player.GetComponent<UnitController>().index;
        return AttackCheck(P_idx, E_idx, type);
    }
    public bool AttackCheck(int[] P_idx, int[] E_idx, TYPE type)//player�������Enemy��type�̎��U���ł��邩
    {
        //�G�̏���
        List<int[]> attacklist = new List<int[]>();
        List<int[]> elementsToRemove = new List<int[]>();

        if (type == TYPE.PAWN)
        {
            //PAWN�̏���
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
            //knight�̏���
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
            //ROOK�̏���
            //x+�����̏���
            int[] searchleft = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchleft) == true)//�͈͓����H
            {
                searchleft[0]++;
                if (CheckIndex(searchleft) == false)
                {
                    break;
                }
                else if (units[searchleft[0], searchleft[1]] != null)//���݂̃}�X��null
                {
                    int[] adindex = new int[] { searchleft[0], searchleft[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //x-�����̏���
            int[] searchright = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchright) == true)//�͈͓����H
            {
                searchright[0]--;
                if (CheckIndex(searchright) == false)
                {
                    break;
                }
                else if (units[searchright[0], searchright[1]] != null)//���݂̃}�X��null
                {
                    int[] adindex = new int[] { searchright[0], searchright[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //y+�����̏���
            int[] searchup = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchup) == true)//�͈͓����H
            {
                searchup[1]++;
                if (CheckIndex(searchup) == false)
                {
                    break;
                }
                else if (units[searchup[0], searchup[1]] != null)//���݂̃}�X��null
                {
                    int[] adindex = new int[] { searchup[0], searchup[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //y-�����̏���
            int[] searchdown = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchdown) == true)//�͈͓����H
            {
                searchdown[1]--;
                if (CheckIndex(searchdown) == false)
                {
                    break;
                }
                else if (units[searchdown[0], searchdown[1]] != null)//���݂̃}�X��null
                {
                    int[] adindex = new int[] { searchdown[0], searchdown[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
        }
        else if (type == TYPE.BISHOP)
        {
            //Bishop�̏���
            //x+��+�����̏���
            int[] searchleftup = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchleftup) == true)//�͈͓����H
            {
                searchleftup[0]++;
                searchleftup[1]++;
                if (CheckIndex(searchleftup) == false)
                {
                    break;
                }
                else if (units[searchleftup[0], searchleftup[1]] != null)//���݂̃}�X��null
                {
                    int[] adindex = new int[] { searchleftup[0], searchleftup[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //x-y+�����̏���
            int[] searchrightup = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchrightup) == true)//�͈͓����H
            {
                searchrightup[0]--;
                searchrightup[1]++;
                if (CheckIndex(searchrightup) == false)
                {
                    break;
                }
                else if (units[searchrightup[0], searchrightup[1]] != null)//���݂̃}�X��null
                {
                    int[] adindex = new int[] { searchrightup[0], searchrightup[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //x+y-�����̏���
            int[] searchrightdown = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchrightdown) == true)//�͈͓����H
            {
                searchrightdown[0]++;
                searchrightdown[1]--;
                if (CheckIndex(searchrightdown) == false)
                {
                    break;
                }
                else if (units[searchrightdown[0], searchrightdown[1]] != null)//���݂̃}�X��null
                {
                    int[] adindex = new int[] { searchrightdown[0], searchrightdown[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //x-y-�����̏���
            int[] searchleftdown = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchleftdown) == true)//�͈͓����H
            {
                searchleftdown[0]--;
                searchleftdown[1]--;
                if (CheckIndex(searchleftdown) == false)
                {
                    break;
                }
                else if (units[searchleftdown[0], searchleftdown[1]] != null)//���݂̃}�X��null
                {
                    int[] adindex = new int[] { searchleftdown[0], searchleftdown[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
        }
        else if (type == TYPE.QUEEN)
        {
            //Queen�̏���=rook+bishop
            //ROOK�̏���
            //x+�����̏���
            int[] searchleft = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchleft) == true)//�͈͓����H
            {
                searchleft[0]++;
                if (CheckIndex(searchleft) == false)
                {
                    break;
                }
                else if (units[searchleft[0], searchleft[1]] != null)//���݂̃}�X��null
                {
                    int[] adindex = new int[] { searchleft[0], searchleft[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //x-�����̏���
            int[] searchright = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchright) == true)//�͈͓����H
            {
                searchright[0]--;
                if (CheckIndex(searchright) == false)
                {
                    break;
                }
                else if (units[searchright[0], searchright[1]] != null)//���݂̃}�X��null
                {
                    int[] adindex = new int[] { searchright[0], searchright[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //y+�����̏���
            int[] searchup = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchup) == true)//�͈͓����H
            {
                searchup[1]++;
                if (CheckIndex(searchup) == false)
                {
                    break;
                }
                else if (units[searchup[0], searchup[1]] != null)//���݂̃}�X��null
                {
                    int[] adindex = new int[] { searchup[0], searchup[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //y-�����̏���
            int[] searchdown = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchdown) == true)//�͈͓����H
            {
                searchdown[1]--;
                if (CheckIndex(searchdown) == false)
                {
                    break;
                }
                else if (units[searchdown[0], searchdown[1]] != null)//���݂̃}�X��null
                {
                    int[] adindex = new int[] { searchdown[0], searchdown[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //Bishop�̏���
            //x+��+�����̏���
            int[] searchleftup = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchleftup) == true)//�͈͓����H
            {
                searchleftup[0]++;
                searchleftup[1]++;
                if (CheckIndex(searchleftup) == false)
                {
                    break;
                }
                else if (units[searchleftup[0], searchleftup[1]] != null)//���݂̃}�X��null
                {
                    int[] adindex = new int[] { searchleftup[0], searchleftup[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //x-y+�����̏���
            int[] searchrightup = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchrightup) == true)//�͈͓����H
            {
                searchrightup[0]--;
                searchrightup[1]++;
                if (CheckIndex(searchrightup) == false)
                {
                    break;
                }
                else if (units[searchrightup[0], searchrightup[1]] != null)//���݂̃}�X��null
                {
                    int[] adindex = new int[] { searchrightup[0], searchrightup[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //x+y-�����̏���
            int[] searchrightdown = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchrightdown) == true)//�͈͓����H
            {
                searchrightdown[0]++;
                searchrightdown[1]--;
                if (CheckIndex(searchrightdown) == false)
                {
                    break;
                }
                else if (units[searchrightdown[0], searchrightdown[1]] != null)//���݂̃}�X��null
                {
                    int[] adindex = new int[] { searchrightdown[0], searchrightdown[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }
            //x-y-�����̏���
            int[] searchleftdown = new int[] { P_idx[0], P_idx[1] };
            while (CheckIndex(searchleftdown) == true)//�͈͓����H
            {
                searchleftdown[0]--;
                searchleftdown[1]--;
                if (CheckIndex(searchleftdown) == false)
                {
                    break;
                }
                else if (units[searchleftdown[0], searchleftdown[1]] != null)//���݂̃}�X��null
                {
                    int[] adindex = new int[] { searchleftdown[0], searchleftdown[1] };
                    attacklist.Add(adindex);
                    break;
                }
            }

        }
        else if (type == TYPE.KING)
        {
            //King�̏���
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

        attacklist.RemoveAll(n => n == P_idx);//���g��unit�Ɠ����ʒu�̃C���f�b�N�X�͏���

        //int[] hantei = Enemy.GetComponent<UnitController>().GetIndex();
        if (attacklist.Any(arr => IsEqual(arr, E_idx)))//�U�����萬��
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    public void OnClickAttack()//UIAttack�������ꂽ�Ƃ��̏���
    {
        Serect_Attack = true;
        Serect_Sucssesed = true;
    }
    public void OnClickDiffence()//UIDiffence�������ꂽ�Ƃ��̏���
    {
        //�I���\������

        if(CanDiffence == true){
            Serect_Diffence = true;
            Serect_Sucssesed = true;
        }
        else
        {
            MainCanvas.GetComponent<UIController>().ChengeTextWindow("�J�E���^�[�s�\");
        }
    }
    public void ClearStatus()
    {
        for (int i = status.Count - 1; i >= 0; i--)
        {
            RemoveStatus(status[i]);
        }
    }
    // STATUS��ǉ����郁�\�b�h
    public void AddStatus(STATUS newStatus)
    {
        status.Add(newStatus);
        OnStatusAdded?.Invoke(newStatus); // �C�x���g�𔭐�������
    }
    // STATUS���폜���郁�\�b�h
    public void RemoveStatus(STATUS newStatus)
    {
        status.Remove(newStatus);
        OnStatusRemoved?.Invoke(newStatus); // �C�x���g�𔭐�������
    }
    //status���ǉ����ꂽ�Ƃ��̏���
    void HandleStatusAdded(STATUS newStatus)
    {
        Debug.Log("Status added: " + newStatus);
        switch (newStatus)
        {
            case STATUS.Serect_P:
                // Serect_P �̏ꍇ�̏���
                Vector3 pos =Player.transform.position;
                pos.y = pos.y + 1;
                Player.transform.position= pos;
                break;
            case STATUS.Serect_E:
                // Serect_E �̏ꍇ�̏���
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
                // ��L�̂�����ɂ����Ă͂܂�Ȃ��ꍇ�̏���
                break;
        }
    }
    //status���폜���ꂽ���̏���
    void HandleStatusRemoved(STATUS removeStatus)
    {
        Debug.Log("Status Removed: " + removeStatus);
        switch (removeStatus)
        {
            case STATUS.Serect_P:
                // Serect_P �̏ꍇ�̏���
                Vector3 pos = Player.transform.position;
                pos.y = 1;
                Player.transform.position = pos;
                break;
            case STATUS.Serect_E:
                // Serect_E �̏ꍇ�̏���
                break;
            default:
                // ��L�̂�����ɂ����Ă͂܂�Ȃ��ꍇ�̏���
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
            MainCanvas.GetComponent<UIController>().ChengeTextWindow("�g�p�ł��܂���");
        }
        else
        {
            MainCanvas.GetComponent<UIController>().ChengeTextWindow("�Ώۂ�I�����Ă�������");
        }

        while (check == true)
        {
            yield return null;
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (!EventSystem.current.IsPointerOverGameObject())//UI�Ƃ��Ԃ鎞���肵�Ȃ�
                {
                    if (Physics.Raycast(ray, out hit))
                    {
                        GameObject targetObject = hit.collider.gameObject;
                        // �N���b�N���ꂽ�Ƃ��̏������L�q����
                        if (targetObject.tag == "Enemy")
                        {
                            MainCanvas.GetComponent<UIController>().ChengeTextWindow("");
                            MakeHand(targetObject.GetComponent<UnitController>().THISTYPE);
                            check = false;
                        }
                        else
                        {
                            MainCanvas.GetComponent<UIController>().ChengeTextWindow("�����ȑΏ�");
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
        MainCanvas.GetComponent<UIController>().ChengeTextWindow("�Ώۂ�I�����Ă�������(0/2)");
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
                if (!EventSystem.current.IsPointerOverGameObject())//UI�Ƃ��Ԃ鎞���肵�Ȃ�
                {
                    if (Physics.Raycast(ray, out hit))
                    {
                        GameObject targetObject = hit.collider.gameObject;
                        // �N���b�N���ꂽ�Ƃ��̏������L�q����
                        if (targetObject.tag == "Enemy")//�^�C�����N���b�N�����Ƃ��E�E�E�E�E�E�E�E�E
                        {
                            if(check1 == true)
                            {
                                MainCanvas.GetComponent<UIController>().ChengeTextWindow("�Ώۂ�I�����Ă�������(1/2)");
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
        MainCanvas.GetComponent<UIController>().ChengeTextWindow("�Ώۂ�I�����Ă�������(0/2)");
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
                if (!EventSystem.current.IsPointerOverGameObject())//UI�Ƃ��Ԃ鎞���肵�Ȃ�
                {
                    if (Physics.Raycast(ray, out hit))
                    {
                        GameObject targetObject = hit.collider.gameObject;
                        // �N���b�N���ꂽ�Ƃ��̏������L�q����
                        if (targetObject.tag == "Enemy")//�^�C�����N���b�N�����Ƃ��E�E�E�E�E�E�E�E�E
                        {
                            if (check1 == true)
                            {
                                MainCanvas.GetComponent<UIController>().ChengeTextWindow("�Ώۂ�I�����Ă�������(1/2)");
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
        MainCanvas.GetComponent<UIController>().ChengeTextWindow("�Ώۂ�I�����Ă�������");
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
                if (!EventSystem.current.IsPointerOverGameObject())//UI�Ƃ��Ԃ鎞���肵�Ȃ�
                {
                    if (Physics.Raycast(ray, out hit))
                    {
                        GameObject targetObject = hit.collider.gameObject;
                        // �N���b�N���ꂽ�Ƃ��̏������L�q����
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
        MainCanvas.GetComponent<UIController>().ChengeTextWindow("�ړ����I�����Ă�������");
        while (check == true)
        {
            yield return null;
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (!EventSystem.current.IsPointerOverGameObject())//UI�Ƃ��Ԃ鎞���肵�Ȃ�
                {
                    if (Physics.Raycast(ray, out hit))
                    {
                        GameObject targetObject = hit.collider.gameObject;
                        // �N���b�N���ꂽ�Ƃ��̏������L�q����
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
                                MainCanvas.GetComponent<UIController>().ChengeTextWindow("�ړ��s�\�Ȓn�_�ł�");
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
        MainCanvas.GetComponent<UIController>().ChengeTextWindow("�Ώۂ�I�����Ă�������");
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
                if (!EventSystem.current.IsPointerOverGameObject())//UI�Ƃ��Ԃ鎞���肵�Ȃ�
                {
                    if (Physics.Raycast(ray, out hit))
                    {
                    GameObject targetObject = hit.collider.gameObject;
                        // �N���b�N���ꂽ�Ƃ��̏������L�q����
                        if (targetObject.tag == "Enemy")//�^�C�����N���b�N�����Ƃ��E�E�E�E�E�E�E�E�E
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
    // ���C�t�v���p�e�B
    public int Life
    {
        get { return _life; }
        set
        {
            _life = value;
            MainCanvas.GetComponent<UIController>().ShowHP(value);
            // ���C�t��0�ɂȂ�����Q�[���I�[�o�[�������s��
            if (_life <= 0)
            {
                GameOver();
            }
        }
    }
    //AP�v���p�e�B
    public int AP
    {
        get { return _ap; }
        set
        {
            _ap = value;
            //AP�\����ύX
            MainCanvas.GetComponent<UIController>().ShowAP(value);
        }
    }
    // �Q�[���I�[�o�[����
    void GameOver()
    {
        // �Q�[���I�[�o�[��ʂ�\������
        gameOverPanel.SetActive(true);

        // �Q�[�����ꎞ��~����i�K�v�ɉ����āj
        //Time.timeScale = 0f;
    }
    // �Q�[���N���A����
    void GameClear()
    {
        // �Q�[���N���A��ʂ�\������
        gameClearPanel.SetActive(true);

        // �Q�[�����ꎞ��~����i�K�v�ɉ����āj
        //Time.timeScale = 0f;
    }
    public void OnClickRetry()
    {
        // MainScene�ֈړ�����
        SceneManager.LoadScene("MainScene");
    }
    public void Quit()
    {        
        // �Q�[�����I������
        Application.Quit();
    }
}
