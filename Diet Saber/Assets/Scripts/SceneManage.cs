using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneManage : MonoBehaviour
{
    public static SceneManage instance;
    [SerializeField] Dropdown Dropdown;
    List<AudioClip> Music;
    public AudioClip playmusic;
    private void Awake() 
    {
        instance = this;
        
        Dropdown.options.Clear();
        Music = Resources.LoadAll("Mp3",typeof(Object)).Cast<AudioClip>().ToList();
        playmusic = Music[0];
        foreach(AudioClip AudioClip in Music)
        {
            Dropdown.options.Add(new Dropdown.OptionData(AudioClip.name));
        }
        Dropdown.onValueChanged.AddListener(delegate {Dropdownmusicselected(Dropdown);});
    }
    


    void Dropdownmusicselected(Dropdown dropdown)
    {
        playmusic = Music[dropdown.value];
    }

    public void Close()
    {
        Application.Quit();
    }
    
    public void ToPlayScene()
    {
        SceneManager.LoadScene(1);
    }
}
