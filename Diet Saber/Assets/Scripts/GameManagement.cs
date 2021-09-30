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
    AudioSource AudioSource;
    void Start()
    {
        AudioSource = GetComponent<AudioSource>();
        StartCoroutine(Inti(waitfortimebeforeint));
    }
    void Update() 
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);
        }
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
