using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Activator : MonoBehaviour
{
    public KeyCode key;//corresponding keyboard 
    public string gestureCode;
    private KeyCode pron = KeyCode.Space; //key for the pronation
    private string gesturePron = "S";
    private bool isActive = false; //is the note on the player's rig
    private GameObject note;
    private SpriteRenderer sr;
    private int hits = 0;


    [SerializeField] private bool createMode;
    [SerializeField] private GameObject n;
    [SerializeField] private GameObject gameMan;
    [SerializeField] private GameObject msgHandler;
    [SerializeField] private GameObject ioHandler;
    [SerializeField] private Sprite defaultImage;
    [SerializeField] private Sprite pressedImage;
    
    private GameManager.InputMode mode;

        // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        mode = gameMan.GetComponent<GameManager>().modality;
        if (mode == GameManager.InputMode.Arduino)
        {
            ioHandler.gameObject.SetActive(true);
        }
        
    }

    // Update is called once per frame
    void Update()
    {

        switch (mode)
        {
            case GameManager.InputMode.Keyboard:
                //the key is pressed 
                if (Input.GetKeyDown(key)
                    || Input.GetKeyDown(pron) && gameObject.tag != "Purple_Activator")//with pronation activate all buttons but the purple 
                {
                    //Animation for the pressed button
                    sr.sprite = pressedImage;

                    //create mode allows to create notes pattern for the song
                    if (createMode)
                    {
                        Instantiate(n, transform.position, Quaternion.identity);
                    }
                    else //logic of the game: press the activator when a note is on them
                    {
                        if (isActive) //hit
                        {
                            if (gameObject.tag == "Purple_Activator")
                            {
                                StartCoroutine(prolongedHit());
                                gameMan.GetComponent<GameManager>().AddScore();
                                hits++;
                            }
                            else
                            {
                                Destroy(note);
                                Hit();
                                isActive = false;
                            }

                        }
                        else if (!isActive) //miss
                        {
                            gameMan.GetComponent<GameManager>().ResetStreak();
                        }
                    }

                }

                if (Input.GetKeyUp(key) || Input.GetKeyUp(pron))
                {
                    sr.sprite = defaultImage;
                }

                break;

            case GameManager.InputMode.Arduino:

                
                //the code is read  
                if (ioHandler.GetComponent<IOHandler>().portString == gestureCode
                    || ioHandler.GetComponent<IOHandler>().portString == gesturePron && gameObject.tag != "Purple_Activator")//with pronation activate all buttons but the purple 
                {
                    //Animation for the pressed button
                    sr.sprite = pressedImage;

                    if (isActive) //hit
                    {
                        if (gameObject.tag == "Purple_Activator")
                        {
                            StartCoroutine(prolongedHit());
                            //gameMan.GetComponent<GameManager>().AddScore();
                            hits++;
                           // StartCoroutine(TriggerVibration());
                        }
                        else
                        {
                            Destroy(note);
                            Hit();
                            isActive = false;
                           // StartCoroutine(TriggerVibration());
                        }

                    }
                    else if (!isActive) //miss
                    {
                        //gameMan.GetComponent<GameManager>().ResetStreak();  //not useful when using arduino
                    }
                    
                }
                else
                {
                    sr.sprite = defaultImage;
                }


                break;
        }
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        isActive = true;
        if (collision.gameObject.tag == "NoteBlue" || collision.gameObject.tag == "NoteYellow" || collision.gameObject.tag == "NoteRed"
            || collision.gameObject.tag == "NoteGreen" || collision.gameObject.tag == "NoteLong") 
        {
            note = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        isActive = false;
    }

    private void Hit()
    {
        gameMan.GetComponent<GameManager>().AddScore();
        gameMan.GetComponent<GameManager>().AddStreak();
        StartCoroutine(msgHandler.GetComponent<MsgHandler>().HitActivation());
        GetComponent<ParticleSystem>().Play();
        
        //scoreMan.GetComponent<ScoreManager>().ScoreAnimation();
    }

    private IEnumerator prolongedHit()
    {
        gameMan.GetComponent<GameManager>().AddStreak();
        StartCoroutine(msgHandler.GetComponent<MsgHandler>().HitActivation());
        GetComponent<ParticleSystem>().Play();
        GetComponent<ParticleSystem>().loop = true;
        yield return new WaitUntil(() => !isActive);
        gameMan.GetComponent<GameManager>().AddScoreLong(hits/4);
        Destroy(note);
        //reset
        GetComponent<ParticleSystem>().loop = false;
        isActive = false;
        hits = 0;
    }

    private IEnumerator TriggerVibration()
    {
        ioHandler.GetComponent<IOHandler>().SerialWriting('V');
        Debug.Log("V");
        yield return new WaitForSecondsRealtime(0.01f);
        ioHandler.GetComponent<IOHandler>().KillWriting();
    }
}
