using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] Animator transitionAnim;

    public void LoadScene(int index)
    {
        StartCoroutine(Load(index));
    }

    IEnumerator Load(int index)
    {
        transitionAnim.SetTrigger("FadeOut");

        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(index);
    }

    public int GetSceneIndex()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }
}
