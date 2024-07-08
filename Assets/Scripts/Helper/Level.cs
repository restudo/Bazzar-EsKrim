using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField] private LevelSelectionController levelSelectionController;

    private int level;

    private void Start()
    {
        level = 0;
    }

    private void OnMouseUp()
    {
        Debug.Log("Button " + level + " clicked.");
        levelSelectionController.LoadToLevel(level);
    }

    public void SetLevel(int level)
    {
        this.level = level;
    }
}
