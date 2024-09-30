using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

public class SceneController : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float faderImageMaxScale = 12f;
    [SerializeField] private Canvas faderCanvas = null;
    [SerializeField] private Image faderImage = null;

    [SerializeField] private Sprite[] faderSprites;

    [Space(20)]
    [SerializeField] private RectTransform rollingDoor;

    private bool isFading;
    private Vector3 initialRollingDoorPos;

    public static SceneController Instance;

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

        RandomFader();
        faderImage.transform.localScale = new Vector3(faderImageMaxScale, faderImageMaxScale, faderImageMaxScale);
    }

    private IEnumerator FadeAndSwitchScenes(string sceneName)
    {
        RandomFader();

        isFading = true;

        faderCanvas.gameObject.SetActive(true);

        Sequence faderSequence = DOTween.Sequence();

        faderSequence.Append(faderImage.transform.DOScale(0, fadeDuration).SetEase(Ease.InOutExpo)).OnComplete(() =>
        {
            Debug.Log("FADE IN");
        });

        yield return faderSequence.WaitForCompletion();

        // Start loading the scene asynchronously
        yield return StartCoroutine(LoadSceneAndSetActive(sceneName));

        yield return new WaitForSeconds(fadeDuration / 2);

        RandomFader();

        // Second scaling animation
        faderSequence = DOTween.Sequence();

        faderSequence.Append(faderImage.transform.DOScale(faderImageMaxScale, fadeDuration).SetEase(Ease.InOutExpo)).OnComplete(() =>
        {
            FadeComplete();
            Debug.Log("FADE OUT");
        });

        yield return faderSequence.WaitForCompletion();

        isFading = false;
    }

    private IEnumerator TransitionAndSwitchScenes(string sceneName)
    {
        isFading = true;

        faderCanvas.gameObject.SetActive(true);

        Sequence faderSequence = DOTween.Sequence();

        // acnhor on bottom, y=0
        faderSequence.Append(rollingDoor.DOAnchorPosY(initialRollingDoorPos.y / 2, fadeDuration / 2).SetEase(Ease.OutBack));
        faderSequence.Append(rollingDoor.DOAnchorPosY(0, fadeDuration / 2).SetEase(Ease.InOutSine)).OnComplete(() =>
        {

        });

        yield return faderSequence.WaitForCompletion();

        Debug.Log("FADE IN");

        // Start loading the scene asynchronously
        yield return StartCoroutine(LoadSceneAndSetActive(sceneName));

        yield return new WaitForSeconds(fadeDuration / 2);

        // Second scaling animation
        faderSequence = DOTween.Sequence();

        faderSequence.Append(rollingDoor.DOAnchorPosY(initialRollingDoorPos.y, fadeDuration).SetEase(Ease.InBack)).OnComplete(() =>
        {

        });

        yield return faderSequence.WaitForCompletion();

        Debug.Log("FADE OUT");
        FadeComplete();

        isFading = false;
    }

    private IEnumerator TransitionAndSwitchGameobject(GameObject deactivate, GameObject activate)
    {
        isFading = true;

        faderCanvas.gameObject.SetActive(true);

        Sequence faderSequence = DOTween.Sequence();

        // acnhor on bottom, y=0
        faderSequence.Append(rollingDoor.DOAnchorPosY(initialRollingDoorPos.y / 2, fadeDuration / 1.5f).SetEase(Ease.OutBack));
        faderSequence.Append(rollingDoor.DOAnchorPosY(0, fadeDuration / 2).SetEase(Ease.InOutSine)).OnComplete(() =>
        {

        });

        yield return faderSequence.WaitForCompletion();

        Debug.Log("FADE IN");

        deactivate.SetActive(false);
        activate.SetActive(true);

        yield return new WaitForSeconds(fadeDuration / 4f);

        // Second scaling animation
        faderSequence = DOTween.Sequence();

        faderSequence.Append(rollingDoor.DOAnchorPosY(initialRollingDoorPos.y, fadeDuration).SetEase(Ease.InBack)).OnComplete(() =>
        {

        });

        yield return faderSequence.WaitForCompletion();

        Debug.Log("FADE OUT");
        FadeComplete();

        isFading = false;

        GameManager.Instance.isGameActive = true; //only for maingame to minigame
    }

    private IEnumerator LoadSceneAndSetActive(string sceneName)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
    }

    private void FadeComplete()
    {
        faderCanvas.gameObject.SetActive(false);
    }

    private void RandomFader()
    {
        faderImage.sprite = faderSprites[Random.Range(0, faderSprites.Length)];
        faderImage.SetNativeSize();
    }

    public void FadeAndLoadScene(Scenes sceneName)
    {
        if (!isFading)
        {
            DOTween.KillAll();

            // StartCoroutine(FadeAndSwitchScenes(sceneName.ToString()));

            StartCoroutine(TransitionAndSwitchScenes(sceneName.ToString()));
        }
    }

    public void FadeAndSetActiveGameobject(GameObject deActivate, GameObject activate)
    {
        if (!isFading)
        {
            DOTween.KillAll();

            StartCoroutine(TransitionAndSwitchGameobject(deActivate, activate));
        }
    }

    public void LoadScene(Scenes sceneName)
    {
        SceneManager.LoadScene(sceneName.ToString());
    }

    public void LoadScene(int sceneBuildIndex)
    {
        SceneManager.LoadScene(sceneBuildIndex);
    }
}
