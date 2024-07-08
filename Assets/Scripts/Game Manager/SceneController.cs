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

    private bool isFading;

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
        faderCanvas.gameObject.SetActive(false);
        faderImage.transform.localScale = new Vector3(faderImageMaxScale, faderImageMaxScale, faderImageMaxScale);
    }

    private IEnumerator FadeAndSwitchScenes(string sceneName)
    {

        isFading = true;

        faderCanvas.gameObject.SetActive(true);

        Sequence faderSequence = DOTween.Sequence();

        faderSequence.Append(faderImage.transform.DOScale(0, fadeDuration).SetEase(Ease.InCubic)).OnComplete(() =>
        {
            Debug.Log("FADE IN");
        });

        yield return faderSequence.WaitForCompletion();

        // Start loading the scene asynchronously
        yield return StartCoroutine(LoadSceneAndSetActive(sceneName));

        yield return new WaitForSeconds(fadeDuration / 2);

        // Second scaling animation
        faderSequence = DOTween.Sequence();

        faderSequence.Append(faderImage.transform.DOScale(faderImageMaxScale, fadeDuration).SetEase(Ease.OutCubic)).OnComplete(() =>
        {
            FadeComplete();
            Debug.Log("FADE OUT");
        });

        yield return faderSequence.WaitForCompletion();

        isFading = false;
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

    public void FadeAndLoadScene(Scenes sceneName)
    {
        if (!isFading)
        {
            DOTween.KillAll();
        
            StartCoroutine(FadeAndSwitchScenes(sceneName.ToString()));
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
