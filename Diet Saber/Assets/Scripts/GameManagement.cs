using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagement : MonoBehaviour
{
    float waitfortimebeforeint = 10f;
    [SerializeField] SpawnKickCubes SpawnKickCubes;
    [SerializeField] SpawnSlashCubes SpawnSlashCubes;
    [SerializeField] Animator animator;
    [SerializeField] VNectModel VNectModel;
    [SerializeField] GameObject UIMenu;
    AudioSource AudioSource;
    void Awake()
    {
        
        AudioSource = GetComponent<AudioSource>();

        AudioSource.clip = SceneManage.SceneManageinstance.playmusic;
        StartCoroutine(Inti(waitfortimebeforeint));
    }
    void Update() 
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            OpenMenu();
        }
    }

    void OpenMenu()
    {
        if(Time.timeScale != 0)
        {
            Time.timeScale = 0;
            AudioSource.Pause();
        }

        UIMenu.SetActive(true);        
    }
    public void Backtogame()
    {
        Time.timeScale = 1;
        AudioSource.Play();
        UIMenu.SetActive(false);
    }

    public void Backtomenu()
    {
        UIMenu.SetActive(false);

        SceneManage.SceneManageinstance.SetLoadScene(0);
        SceneManager.LoadScene(1);

        Time.timeScale = 1;
    }

    IEnumerator Inti(float f)
    {
        yield return new WaitForSeconds(f);
        
        if(Time.time>6f)
        {
            VNectModel.SetState(VNectModel.GetModelState(2));
        }
        
        SpawnKickCubes.SetIsActive(true);
        SpawnSlashCubes.SetIsActive(true);
        AudioSource.Play();
        yield return new WaitForSeconds(AudioSource.clip.length);
        SpawnKickCubes.SetIsActive(false);
        SpawnSlashCubes.SetIsActive(false);

        yield return new WaitForSeconds(5f);
        animator.SetTrigger("Win");
    }

}
