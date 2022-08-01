using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


[System.Serializable]
public class StageInfo
{
    public float spawnTime = 0;
    public int spawnCount;

    public int[] enemyKind;
}
public class EnemyManager : MonoBehaviour
{
    [SerializeField] private SoundManager SM;
    [SerializeField] private MultipleSpeed speedSet = null;

    [SerializeField] private GameObject ClearPanal = null;
    [SerializeField] private TextMeshProUGUI PlusCoin1 = null;
    [SerializeField] private GameObject FailPanal = null;
    [SerializeField] private TextMeshProUGUI PlusCoin2 = null;
    
    [SerializeField] private Transform water = null;

    [SerializeField] private GameObject unituiParent = null;

    //스테이지 정보
    //[SerializeField] private StageInfo[] stageDataroundData = null;

    //적이 죽거나 도착지에 도착했을 때, 골드획득이나 라이프 감소를 위한 플레이어 정보
    [SerializeField] private PlayerState playerstate = null;

    //enemy의 체력바와 공격시 데미지를 띄우는 UI정보를 위한 canvas
    [SerializeField] private Transform canvas = null;

    //소환되는 적에게 부여할 프리펩
    [SerializeField] private GameObject hpbar = null;
    [SerializeField] private GameObject damagenum = null;
    
    [SerializeField] private WeatherSetting weather =  null;

    //다음에 나올 적을 표현할 UI
    [SerializeField] private Image imagebar = null;
    [SerializeField] private Image[] imageEnemy = null;
    [SerializeField] private TextMeshProUGUI ShowBoss = null;

    private StageDataFrame stageData = null;

    public delegate void StageClear();
    public static StageClear stageclear;

    private float EnemyCoinRate = 0;
    
    private int StageNum = 0;
    public int GetStageNum => StageNum;

    //스테이지가 실행중인지 판단
    private bool gameongoing = false;

    //적 스폰이 끝났는지 여부
    private bool SpawnFinish = false;

    private Vector3[] waypoint;
    private Vector3 SpawnPos;

    //소환되는 적들의 정보를 담을 List
    //List<Enemy> EnemyCount = null;

    int EnemyCount = 0;

    int EnemyRemainCount = 0;

    private EnemyPooling Pooling = null;
 
    public int Getmaxstage => stageData.stageCount;
    public int Getcurrentstage => StageNum + 1;

    private void Awake()
    {
        stageData = GameManager.GetStageData();
        for(int i = 0; i < stageData.roundData.Length; i++)
        {
            Debug.Log("개수 : " + stageData.roundData[i].spawnCount);
        }
    }

    private void Start()
    {
        Pooling = this.GetComponent<EnemyPooling>();

        //EnemyCount = new List<Enemy>();
        MultipleSpeed.speedup += SpeedUP;
        ClearPanal.SetActive(false);
        FailPanal.SetActive(false);
        ShowEnemyImage(0);
    }

    private void SpeedUP(int x)
    {
        Time.timeScale = x;
    }


    //게임 시작 될 때 enemy의 루트와 스폰 위치를 받아서 게임 시작
    public void gameStartCourtain(Vector3[] _waypoint, Vector3 _SpawnPos,int spawnNum)
    {
        waypoint = _waypoint;
        SpawnPos = _SpawnPos;
        StartCoroutine(GameStart(_waypoint, _SpawnPos,spawnNum));
    }

    IEnumerator GameStart(Vector3[] _wayPoint, Vector3 _spawnPos,int spawnNum)
    {
        EnemyRemainCount = stageData.roundData[StageNum].spawnCount;
        Debug.Log(EnemyRemainCount);
        GameManager.buttonOff();

        weather.GameStart();

       gameongoing = true;

        //적이 나올 개수
        //int count = GameManager.SetGameLevel == 3? (int)(stageinfo[StageNum ].spawnCount*0.7f): stageinfo[StageNum].spawnCount;
        int count = GameManager.SetGameLevel == 3 ? (int)(stageData.roundData[StageNum].spawnCount * 0.5f) : stageData.roundData[StageNum].spawnCount;

        int stagenum = StageNum;

        EnemyRemainCount = count;
        //적 종류
        for (int i = 0; i < count; i++)
        {
            int enemynum = 0;

            int num = Random.Range(0, stageData.roundData[StageNum].enemyKind.Length);

            enemynum = stageData.roundData[StageNum].enemyKind[num];

            var enemy = Pooling.GetEnemy(enemynum, _spawnPos);
            enemy.SetUpEnemy(this, _wayPoint, canvas,hpbar,damagenum, water);
            enemy.SetPooling(Pooling, enemynum);
            enemy.gameObject.layer = 6;
            enemy.StartMove();


            //소환되는 enemy를 list에 추가
            //EnemyCount.Add(enemy.GetComponent<Enemy>());
            //EnemyRemainCount++;

            yield return new WaitForSeconds(stageData.roundData[StageNum].spawnTime);
        }
        SpawnFinish = true;

        if (spawnNum == 1)
        {
            while (true)
            {
                if (SpawnFinish && EnemyRemainCount <= 0)
                {
                    StageNum++;

                    stageclear();

                    if (StageNum >= stageData.roundData.Length)
                    {
                        SM.TurnOnSound(4);
                        ClearPanal.SetActive(true);

                        speedSet.StopGame();

                        UserInformation.getMoney += (int)(GameManager.SetMoney * SkillSettings.PassiveValue("GetUserCoinUp"));

                        PlusCoin1.text = "획득코인 : " + (int)(GameManager.SetMoney * SkillSettings.PassiveValue("GetUserCoinUp"));

                        //별 개수에 따른 상금 얻기
                    }

                    ShowBoss.enabled = false;
                    gameongoing = false;
                    ShowEnemyImageReset();

                    if (StageNum < 20)
                    {
                        ShowEnemyImage(StageNum);
                    }
                    break;
                }
                yield return null;
            }
        }
    }

    public bool GetGameOnGoing => gameongoing;


    //출현한 적이 체력이 다 되서 죽을 때
    public void EnemyDie(Enemy enemy,int coin)
    {
        Pooling.GetCoin(0, enemy.transform.position);
        
        enemy.gameObject.layer = 0;
        playerstate.PlayerCoinUp(coin + Mathf.CeilToInt(coin * EnemyCoinRate));
        //EnemyCount.Remove(enemy);
        EnemyRemainCount--;
    }

    //출현한 적이 도착지에 도착했을 때
    public void EnemyArriveDestination(Enemy enemy)
    {
        Pooling.GetCoin(1, enemy.transform.position);
        if (!enemy.GetBoss())
        {
            playerstate.PlayerLifeDown(1);
        }
        else
        {
            playerstate.PlayerLifeDown(5);
        }
        //EnemyCount.Remove(enemy);
        EnemyRemainCount--;

        if (playerstate.GetPlayerLife <= 0)
        {
            SM.TurnOnSound(3);
            speedSet.StopGame();
            FailPanal.SetActive(true);
        }
    }

    public bool GameOnGoing
    {
        get
        {
            return gameongoing;
        }
    }

    Vector2 BossTextPos;
    private void ShowEnemyImage(int num)
    {
        int MaxCount = stageData.roundData[num].enemyKind.Length;

       

        for (int i =0;i< MaxCount; i++)
        {
            imageEnemy[i].gameObject.SetActive(true);


            Sprite enemy = Resources.Load<Sprite>("Image/EnemyImage/" + Pooling.GetName(stageData.roundData[num].enemyKind[i]));
            imageEnemy[i].sprite = enemy;

            imageEnemy[i].rectTransform.anchoredPosition = new Vector2((-70 * (stageData.roundData[num].enemyKind.Length - 1)) + (i * 140), 460);

            if (Pooling.GetBoss(stageData.roundData[num].enemyKind[i]))
            {
                ShowBoss.enabled = true;
            }

            ShowBoss.rectTransform.anchoredPosition = new Vector2((-70 * (stageData.roundData[num].enemyKind.Length - 1)) + (i * 140), 380);

        }

        float size = stageData.roundData[num].enemyKind.Length > 1 ? (Mathf.Abs(imageEnemy[0].rectTransform.rect.x) + Mathf.Abs(imageEnemy[stageData.roundData[num].enemyKind.Length - 1].rectTransform.rect.x)) : 0;

        float size2 = (Mathf.Abs(imageEnemy[0].rectTransform.anchoredPosition.x) + Mathf.Abs(imageEnemy[stageData.roundData[num].enemyKind.Length - 1].rectTransform.anchoredPosition.x));

        imagebar.rectTransform.sizeDelta = new Vector2(size2, 20);
    }

    private void ShowEnemyImageReset()
    {
        for(int i = 0; i < imageEnemy.Length; i++)
        {
            imageEnemy[i].gameObject.SetActive(false);
        }
    }




}
