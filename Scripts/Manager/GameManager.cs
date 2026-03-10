using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject[] stagePrefab;
    [SerializeField] private TextMeshProUGUI countdownUI;
    [SerializeField] private GameObject countdownPanelUI;
    [SerializeField] private Image[] limitUIs;
    [SerializeField] private GameObject limitPanelUI;
    [SerializeField] private Color limitColor;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject gameOverUI;

    private GameObject stage;
    private const int LIMIT_COUNT = 8;
    private const float LIMIT_DURATION = 48;
    private const float LIMIT_GRACE = 3;
    private float limitTimer;
    private bool isStage;
    private int currentLimit;

    private void Start()
    {
        Initialize();
        Countdown().Forget();
    }

    private void Initialize()
    {
        isStage = false;
        limitPanelUI.SetActive(false);
        if (stage != null)
            Destroy(stage);
    }

    private async UniTask Countdown(float wait = 0)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(wait));
        countdownPanelUI.SetActive(true);
        countdownUI.text = "3";
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        countdownUI.text = "2";
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        countdownUI.text = "1";
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        countdownUI.text = "スタート!";
        SetupStage();
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        countdownPanelUI.SetActive(false);
    }

    private void SetupStage()
    {
        isStage = true;
        stage = Instantiate(stagePrefab[0]);
        limitTimer = LIMIT_DURATION;
        limitPanelUI.SetActive(true);
        currentLimit = LIMIT_COUNT;
    }

    private void Update()
    {
        if (isStage)
        {
            LimitHandler();
        }
        
        if (player.position.y <= 0)
        {
            GameOver();
        }
    }

    private void LimitHandler()
    {
        limitTimer -= Time.deltaTime;
        if (limitTimer + LIMIT_GRACE <= 0)
        {
            Initialize();
            Countdown(3f).Forget();
            return;
        }

        for (int i = 0; i < LIMIT_COUNT; i++)
        {
            if (limitTimer < i * LIMIT_DURATION / LIMIT_COUNT)
            {
                limitUIs[i].color = limitColor;
                if (currentLimit > i)
                {
                    currentLimit = i;
                    float pitch = 1.5f - i * 0.05f;
                    SoundManager.Instance.PlayPitchSE("limit", pitch);
                }
            }
            else
                limitUIs[i].color = GetSignalColor();
        }
    }

    private Color GetSignalColor()
    {
        float step = limitTimer / LIMIT_DURATION;
        return Color.Lerp(Color.red, Color.green, step);
    }

    private void GameOver()
    {
        isStage = false;
        gameOverUI.SetActive(true);
        Camera.main.transform.SetParent(null);
        Camera.main.transform.rotation = Quaternion.Euler(-90f, 0, 0);
        Load().Forget();
    }

    private async UniTask Load()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(3f));
        SceneManager.LoadScene("Title");
    }
}