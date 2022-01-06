using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private GameObject gameMan;

    private Text scoreTxt;
    private Text multiplierTxt;
    private Text streakTxt;

    // Start is called before the first frame update
    void Start()
    {
        scoreTxt = transform.GetChild(0).gameObject.GetComponent<Text>();
        streakTxt = transform.GetChild(1).gameObject.GetComponent<Text>();
        multiplierTxt = transform.GetChild(2).gameObject.GetComponent<Text>();
       
    }

    // Update is called once per frame
    void Update()
    {
        UpdateGUI();
    }

    private void UpdateGUI()
    {
        scoreTxt.text = PlayerPrefs.GetInt("Score").ToString();
        streakTxt.text = gameMan.GetComponent<GameManager>().GetStreak().ToString();
        multiplierTxt.text = gameMan.GetComponent<GameManager>().GetMultiplier().ToString() + "X";
    }

 
}
