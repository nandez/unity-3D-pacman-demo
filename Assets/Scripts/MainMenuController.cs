using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] protected LevelTransitionController levelTransitionCtrl;

    public void StartGame()
    {
        levelTransitionCtrl.LoadScene(Constants.SceneNames.GAME_SCENE);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }


}
