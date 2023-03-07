using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransitionController : MonoBehaviour
{
    [Header("HUD References")]
    [SerializeField] private Animator transition;
    [SerializeField] private float transitionTime = 1f;

    public void LoadScene(string sceneName)
    {
        StartCoroutine(TransitionToScene(sceneName));
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(sceneName);
    }
}
