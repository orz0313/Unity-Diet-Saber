using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManage : MonoBehaviour
{
    public void Close()
    {
        Application.Quit();
    }
    
    public void ToPlayScene()
    {
        SceneManager.LoadScene(1);
    }
}
