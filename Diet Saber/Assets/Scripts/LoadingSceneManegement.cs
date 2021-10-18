using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LoadingSceneManegement : MonoBehaviour
{
    void Start() 
    {
        StartCoroutine(LoadScene(SceneManage.SceneManageinstance.SceneToLoad));
    }

    IEnumerator LoadScene(int index)
    {
        SceneManager.LoadSceneAsync(index);
        yield return null;
    }
}
