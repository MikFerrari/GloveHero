using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject msgHandler;
    [SerializeField] private GameObject song;

    private int multiplier = 1;
    private int streak = 0;
    private int noteValue = 100;    
    private int streakInc = 6;

    public InputMode modality;

    public enum InputMode
    {
        Arduino,
        Keyboard
    };

    // Start is called before the first frame update
    void Start()
    {
        
        //at the start of each game set the scores to 0;
        PlayerPrefs.SetInt("Score", 0);
        PlayerPrefs.SetInt("HighStreak", 0);
        PlayerPrefs.SetInt("MissedBlue", 0);
        PlayerPrefs.SetInt("MissedYellow", 0);
        PlayerPrefs.SetInt("MissedRed", 0);
        PlayerPrefs.SetInt("MissedGreen", 0);
        PlayerPrefs.SetInt("MissedLong", 0);
        PlayerPrefs.SetInt("MissedSlap", 0);

        modality = (InputMode)PlayerPrefs.GetInt("Input");
        
    }

    // Update is called once per frame
    void Update()
    {
        //when the song ends go to gameover scene
        if (!song.GetComponent<SongManager>().audioSource.isPlaying && song.GetComponent<SongManager>().itStarted)
        {
            SceneManager.LoadScene(2);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if the note is purple wait to reset streak because is still being played, all the other cases reset the streak and activate Miss 
        if (collision.gameObject.tag != "NoteLong")
        {
            ResetStreak();
            StartCoroutine(msgHandler.GetComponent<MsgHandler>().MissActivation());
        }

        //update the respetive missed counter

        //for the slap, check if more than one note arrive at the same time
        
        //StartCoroutine(checkSlap());
        if (Physics2D.OverlapAreaAll(new Vector2(-4f, -4.25f), new Vector2(4f, -4.25f)).Length > 2)
        {
            PlayerPrefs.SetInt("MissedSlap", PlayerPrefs.GetInt("MissedSlap") + 1); 
        }

        else if (collision.gameObject.tag == "NoteBlue")
        {
            PlayerPrefs.SetInt("MissedBlue", PlayerPrefs.GetInt("MissedBlue") + 1);
        }
        else if (collision.gameObject.tag == "NoteYellow")
        {
            PlayerPrefs.SetInt("MissedYellow", PlayerPrefs.GetInt("MissedYellow") + 1);
        }
        else if (collision.gameObject.tag == "NoteRed")
        {
            PlayerPrefs.SetInt("MissedRed", PlayerPrefs.GetInt("MissedRed") + 1);
        }
        else if (collision.gameObject.tag == "NoteGreen")
        {
            PlayerPrefs.SetInt("MissedGreen", PlayerPrefs.GetInt("MissedGreen") + 1);
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // if the note is the long one we can reset the streak, activate the Miss and update the counter
        if (collision.gameObject.tag == "NoteLong")
        {
            PlayerPrefs.SetInt("MissedLong", PlayerPrefs.GetInt("MissedLong") + 1);
            ResetStreak();
            StartCoroutine(msgHandler.GetComponent<MsgHandler>().MissActivation());
        }

        // destroy all the notes that arrive here
        Destroy(collision.gameObject);

    }

    // obtaine the value of the current note according to the multipler
    public int GetPoints()
    {
        return noteValue * multiplier;
    }

    //update the streak and the multiplier
    public void AddStreak()
    {
        streak++;
        if(streak%streakInc == 0 && streak>0 && streak < streakInc*4)
        {
            multiplier++;
        }
    }

    //reset the streak and the multiplier
    public void ResetStreak()
    {
        if (PlayerPrefs.GetInt("HighStreak") < streak)
        {
            PlayerPrefs.SetInt("HighStreak", streak);
        }
        streak = 0;
        multiplier = 1;
    }

    //update the score
    public void AddScore()
    {
        PlayerPrefs.SetInt("Score", PlayerPrefs.GetInt("Score") + GetPoints());
    }

    //update the score for the long note
    public void AddScoreLong(int hits)
    {
        PlayerPrefs.SetInt("Score", PlayerPrefs.GetInt("Score") + GetPoints() * hits);
    }

    public int GetMultiplier()
    {
        return multiplier;
    }

    public int GetStreak()
    {
        return streak;
    }

    private IEnumerator checkSlap()
    {
        
        
        yield return null;
    }


}
