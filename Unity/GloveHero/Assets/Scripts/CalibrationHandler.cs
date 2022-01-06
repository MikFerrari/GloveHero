using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CalibrationHandler : MonoBehaviour
{
    [SerializeField] private GameObject ioHandler;
    private IOHandler io;

    private bool start = false;
    private float startingWaitingTime = 3f;
    private float waitingTimeBetween = 3f;
    private float executionTime = 2f;
    private int repetitions = 5;

    private object calibStart = "C";
    private object acquireData = "A";
    private object goNext = "N";

    // Start is called before the first frame update
    void Start()
    {
        io = ioHandler.GetComponent<IOHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        if (start)
        {
            StartCoroutine(CalibrationProcedure());

        }
    }

    public void StartCalibration()
    {
        start = true;
    }

    public IEnumerator CalibrationProcedure()
    {
        //deactivate button
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        
        //START THE GUIDED PROCEDURE //
        // waiting...
        gameObject.transform.GetChild(1).gameObject.SetActive(true);
        while(!gameObject.transform.GetChild(1).gameObject.GetComponent<TypeWriterEffect>().isFinished)
        {
            yield return null; //new WaitForSecondsRealtime(startingWaitingTime);
        }
        


        //Send starting signal (SEND SIGNAL)
        io.SerialWriting(calibStart);
        gameObject.transform.GetChild(1).gameObject.SetActive(false);
        
        // For every finger do the same procedure
        for(int i = 2; i<=5; i++)
        {
            //for the desired number of repetitions do the pinch acquire 
            for (int j = 0; j < repetitions; j++)
            {
                if (io.portString == i.ToString()) //if arduino is ready (LEGGI SEGNALE)
                {
                    io.KillWriting();//kill the previous writing

                    // write on the screen the gesture
                    gameObject.transform.GetChild(i).gameObject.SetActive(true);
                    while (!gameObject.transform.GetChild(i).gameObject.GetComponent<TypeWriterEffect>().isFinished)
                    {
                        yield return null; //new WaitForSecondsRealtime(startingWaitingTime);
                    }
                   // yield return new WaitForSecondsRealtime(executionTime);

                    //tell arduino to acquire the data INVIA SEGNALE ad arduino DI ACQUISIRE
                    io.SerialWriting(acquireData);

                    gameObject.transform.GetChild(i).gameObject.SetActive(false);
                    gameObject.transform.Find("Done").gameObject.SetActive(true); //it's done on display

                    while (!gameObject.transform.Find("Done").gameObject.GetComponent<TypeWriterEffect>().isFinished)
                    {
                        yield return null; //new WaitForSecondsRealtime(startingWaitingTime);
                    }
                    //yield return new WaitForSecondsRealtime(waitingTimeBetween);
                    gameObject.transform.Find("Done").gameObject.SetActive(false);
                    
                    io.KillWriting();
                    // send signal to start new acuisition if arduino is ready INVIA SEGNALE READY
                    if(io.portString == j.ToString())
                    {
                        io.SerialWriting(goNext);
                    }
                     
                }
            }

        }

        io.KillWriting();
        gameObject.transform.Find("Finish").gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(executionTime);
        TurnToMenu();
    }

    public IEnumerator InitialWaiter()
    {
        yield return new WaitForSecondsRealtime(startingWaitingTime);
    }

    public IEnumerator ExecutionWaiter()
    {
        yield return new WaitForSecondsRealtime(executionTime);
    }

    public IEnumerator InBetweenWaiter()
    {
        yield return new WaitForSecondsRealtime(waitingTimeBetween);
    }

    public void TurnToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
