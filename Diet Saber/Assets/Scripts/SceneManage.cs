using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneManage : MonoBehaviour
{
    public static SceneManage SceneManageinstance;
    [SerializeField] Dropdown Dropdown;
    List<AudioClip> Music;
    public AudioClip playmusic;
    public int SceneToLoad = 0;
    void Awake() 
    {
        SceneManageinstance = this;
        
        Dropdown.options.Clear();
        Music = Resources.LoadAll("Mp3",typeof(Object)).Cast<AudioClip>().ToList();
        playmusic = Music[0];
        foreach(AudioClip AudioClip in Music)
        {
            Dropdown.options.Add(new Dropdown.OptionData(AudioClip.name));
        }
        Dropdown.onValueChanged.AddListener(delegate {Dropdownmusicselected(Dropdown);});


    }
    public void SetLoadScene(int index)
    {
        SceneToLoad = index;
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
        SetLoadScene(2);
        SceneManager.LoadScene(1);
    }
}
