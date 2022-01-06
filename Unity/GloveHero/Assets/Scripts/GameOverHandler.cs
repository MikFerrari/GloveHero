using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class GameOverHandler : MonoBehaviour
{
    //class to handle UI in the gameover scene, both the text display and panel animation
    [SerializeField] private Text score;
    [SerializeField] private Text streak;
    [SerializeField] private Text missed;
    [SerializeField] private Text index;
    [SerializeField] private Text middle;
    [SerializeField] private Text ring;
    [SerializeField] private Text little;
    [SerializeField] private Text punch;
    [SerializeField] private Text slap;
    [SerializeField] private Text instr;

    [SerializeField] private GameObject panelMenu;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        // show the results
        score.text = PlayerPrefs.GetInt("Score").ToString();
        streak.text = PlayerPrefs.GetInt("HighStreak").ToString();
        missed.text = (PlayerPrefs.GetInt("MissedGreen")+ PlayerPrefs.GetInt("MissedRed")+ PlayerPrefs.GetInt("MissedYellow")+ PlayerPrefs.GetInt("MissedBlue")+ PlayerPrefs.GetInt("MissedLong")+ (PlayerPrefs.GetInt("MissedSlap") / 4)).ToString();
        index.text = PlayerPrefs.GetInt("MissedGreen").ToString();
        middle.text = PlayerPrefs.GetInt("MissedRed").ToString();
        ring.text = PlayerPrefs.GetInt("MissedYellow").ToString();
        little.text = PlayerPrefs.GetInt("MissedBlue").ToString();
        punch.text = PlayerPrefs.GetInt("MissedLong").ToString();
        slap.text = (PlayerPrefs.GetInt("MissedSlap")/4).ToString();

        //get component
        anim = panelMenu.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            instr.gameObject.SetActive(false);
            StartCoroutine(OpenMenu());
            
        }
        
    }

    private IEnumerator OpenMenu()
    {
        panelMenu.gameObject.SetActive(true);
        anim.SetBool("open", true);
        //wait till the nimation finishes
        yield return new WaitForSeconds(1f);
        
        //activate buttons
        panelMenu.transform.GetChild(0).gameObject.SetActive(true);
        panelMenu.transform.GetChild(1).gameObject.SetActive(true);

    }

    public void Retry()
    {
        SceneManager.LoadScene(1);
        anim.SetBool("open", false);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
        anim.SetBool("open", false);
    }
}
