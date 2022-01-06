using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class UIHandler : MonoBehaviour
{
    [SerializeField] private GameObject panelMenu;
    private Animator anim;

    public AudioMixer mixer;
    public GameObject cal;

    public enum InputMode
    {
        Keyboard,
        Arduino
    };

    public enum Gestures
    {
        p,
        pp,
        pps
    };

    // Start is called before the first frame update
    void Start()
    {
        //get component
        anim = panelMenu.GetComponent<Animator>();

        PlayerPrefs.SetInt("Input", 0);
        PlayerPrefs.SetString("SelectedGesture", "p");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NewGame()
    {
        SceneManager.LoadScene(1);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void Menu()
    {
        StartCoroutine(OpenMenu());
    }

    public void TurnBack()
    {
        StartCoroutine(CloseMenu());
    }

    public void StartCalibration()
    {
        SceneManager.LoadScene(3);
    }

    public void SetGestures(int selection)
    {
        PlayerPrefs.SetString("SelectedGesture", ((Gestures)selection).ToString());
    }

    public void SetInput(int selection)
    {
        PlayerPrefs.SetInt("Input", selection);
        Debug.Log(PlayerPrefs.GetInt("Input"));
    }

    public void SetCOM(string port)
    {
        PlayerPrefs.SetString("COM", port);
    }

    public void SetVolume(float volume)
    {
        mixer.SetFloat("volume", volume);
    }

    private IEnumerator OpenMenu()
    {
        panelMenu.gameObject.SetActive(true);
        anim.SetBool("open", true);
        //wait till the nimation finishes
        yield return new WaitForSeconds(1f);

        //activate buttons
        for (int i=0;  i < panelMenu.transform.childCount; i++)
        {
            panelMenu.transform.GetChild(i).gameObject.SetActive(true);
        }

    }

    private IEnumerator CloseMenu()
    {
        //Deactivate buttons
        for (int i = 0; i < panelMenu.transform.childCount; i++)
        {
            panelMenu.transform.GetChild(i).gameObject.SetActive(false);
        }

        anim.SetBool("open", false);
        //wait till the nimation finishes
        yield return new WaitForSeconds(1f);

        panelMenu.gameObject.SetActive(false);
    }

}
