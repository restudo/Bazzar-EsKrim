using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [Header("State Gameobject")]
    [SerializeField] private GameObject mainGame;
    [SerializeField] private GameObject miniGame;

    [Header("GameOver Panel")]
    [SerializeField] private GameObject gameOverWinUI;
    [SerializeField] private TextMeshProUGUI winDescription;
    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject gameOverLoseUI;

    [Header("Game Over Desc")]
    [TextArea][SerializeField] private string mainGameWinDesc;
    [TextArea][SerializeField] private string miniGameWinDesc;

    private void Awake()
    {
        switch (GameManager.Instance.gameStates)
        {
            case GameStates.MainGame:
                ActiveGame(main: true, mini: false);
                nextButton.onClick.AddListener(LoadToMiniGame);
                break;
            case GameStates.MiniGame:
                ActiveGame(main: false, mini: true);
                nextButton.onClick.AddListener(LoadToLevelSelection);
                break;
            default:
                Debug.LogError("Current Game State is " + GameManager.Instance.gameStates.ToString());

                if(mainGame.activeSelf)
                {
                    GameManager.Instance.gameStates = GameStates.MainGame;
                }
                else if(miniGame.activeSelf)
                {
                    GameManager.Instance.gameStates = GameStates.MiniGame;
                }

                Debug.LogWarning("Current Game State has changed to = " + GameManager.Instance.gameStates.ToString());
                break;
        }
    }

    private void ActiveGame(bool main, bool mini)
    {
        mainGame.SetActive(main);
        miniGame.SetActive(mini);
    }

    private IEnumerator WinAnim()
    {
        yield return new WaitForSeconds(1f);

        gameOverWinUI.transform.parent.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        gameOverWinUI.transform.parent.localScale = Vector3.zero;
        gameOverWinUI.transform.parent.gameObject.SetActive(true);
        gameOverWinUI.transform.parent.DOScale(1, 0.4f).SetEase(Ease.OutBounce).SetDelay(0.6f);
        gameOverWinUI.transform.parent.GetComponent<Image>().DOColor(new Color32(0, 0, 0, 150), 1.5f).SetDelay(1f);

        yield return new WaitForSeconds(1f);
        gameOverWinUI.transform.GetChild(gameOverWinUI.transform.childCount - 1).gameObject.SetActive(true);

        EventHandler.CallChaseCustomerEvent();

        Debug.Log("Game Over - Win");
    }

    private IEnumerator LoseAnim()
    {
        yield return new WaitForSeconds(1);

        gameOverLoseUI.transform.parent.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        gameOverLoseUI.transform.parent.localScale = Vector3.zero;
        gameOverLoseUI.transform.parent.gameObject.SetActive(true);
        gameOverLoseUI.transform.parent.DOScale(1, 0.4f).SetEase(Ease.OutBounce).SetDelay(0.6f);
        gameOverLoseUI.transform.parent.GetComponent<Image>().DOColor(new Color32(0, 0, 0, 150), 1.5f).SetDelay(1f);

        EventHandler.CallChaseCustomerEvent();

        Debug.Log("Game Over - Lose");
    }

    public void Win()
    {
        if (GameManager.Instance.gameStates == GameStates.MainGame)
        {
            winDescription.text = mainGameWinDesc;
        }
        else if (GameManager.Instance.gameStates == GameStates.MiniGame)
        {
            winDescription.text = miniGameWinDesc;
        }

        StartCoroutine(WinAnim());
    }

    public void Lose()
    {
        StartCoroutine(LoseAnim());
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadToLevelSelection()
    {
        GameManager.Instance.gameStates = GameStates.LevelSelection;

        SceneController.Instance.FadeAndLoadScene(Scenes.Menu);
    }

    public void LoadToMiniGame()
    {
        gameOverWinUI.transform.parent.gameObject.SetActive(false);
        gameOverLoseUI.transform.parent.gameObject.SetActive(false);

        GameManager.Instance.gameStates = GameStates.MiniGame;

        mainGame.SetActive(false);
        miniGame.SetActive(true);

        nextButton.onClick.AddListener(LoadToLevelSelection);
    }
}
