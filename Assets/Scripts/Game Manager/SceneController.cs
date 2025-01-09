using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;
using BazarEsKrim;

public class SceneController : MonoBehaviour
{
    [Header("Fader Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float faderImageMaxScale = 12f;
    [SerializeField] private Canvas faderCanvas = null;
    [SerializeField] private Image faderImage = null;
    [SerializeField] private Sprite[] faderSprites;

    [Header("Rolling Door Settings")]
    [SerializeField] private RectTransform rollingDoor;

    [Header("Audio")]
    [SerializeField] private AudioClip rollingDownSfx;
    [SerializeField] private AudioClip rollingUpSfx;

    private Vector3 initialRollingDoorPos;
    private bool isFading;

    public static SceneController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        initialRollingDoorPos = rollingDoor.anchoredPosition;
        rollingDoor.anchoredPosition = initialRollingDoorPos;
        faderCanvas.gameObject.SetActive(false);
        InitializeFader();
    }

    private void InitializeFader()
    {
        RandomizeFaderSprite();
        faderImage.transform.localScale = Vector3.one * faderImageMaxScale;
    }

    private IEnumerator PerformRollingDoorTransition(float targetPositionY, AudioClip audioClip)
    {
        AudioManager.Instance?.PlaySFX(audioClip);
        yield return rollingDoor.DOAnchorPosY(targetPositionY, fadeDuration).SetEase(Ease.InOutSine).WaitForCompletion();
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
    }

    private IEnumerator HandleTransition(GameObject deactivate, GameObject activate)
    {
        isFading = true;
        faderCanvas.gameObject.SetActive(true);

        yield return PerformRollingDoorTransition(0, rollingDownSfx);

        deactivate?.SetActive(false);
        activate?.SetActive(true);

        yield return new WaitForSeconds(fadeDuration / 4f);

        yield return PerformRollingDoorTransition(initialRollingDoorPos.y, rollingUpSfx);

        EndTransition();

        GameManager.Instance.isGameActive = true; //only for maingame to minigame
    }

    private void EndTransition()
    {
        faderCanvas.gameObject.SetActive(false);
        isFading = false;
    }

    private void RandomizeFaderSprite()
    {
        if (faderSprites.Length > 0)
        {
            faderImage.sprite = faderSprites[Random.Range(0, faderSprites.Length)];
            faderImage.SetNativeSize();
        }
    }

    public void FadeAndLoadScene(Scenes sceneName)
    {
        if (isFading) return;
        DOTween.KillAll();

        StartCoroutine(HandleSceneTransition(sceneName.ToString()));
    }

    private IEnumerator HandleSceneTransition(string sceneName)
    {
        isFading = true;
        faderCanvas.gameObject.SetActive(true);

        yield return PerformRollingDoorTransition(0, rollingDownSfx);
        yield return LoadSceneAsync(sceneName);
        yield return new WaitForSeconds(fadeDuration / 2);
        yield return PerformRollingDoorTransition(initialRollingDoorPos.y, rollingUpSfx);

        EndTransition();
    }

    public void FadeAndSetActiveGameobject(GameObject deactivate, GameObject activate)
    {
        if (isFading) return;
        DOTween.KillAll();

        StartCoroutine(HandleTransition(deactivate, activate));
    }

    public void LoadScene(Scenes sceneName) => SceneManager.LoadScene(sceneName.ToString());

    public void LoadScene(int sceneBuildIndex) => SceneManager.LoadScene(sceneBuildIndex);
}
