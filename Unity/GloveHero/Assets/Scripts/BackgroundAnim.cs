using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundAnim : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        for(int i=0; i<gameObject.transform.childCount; i++)
        {
            gameObject.transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Randomizer()
    {
        

        //change in a random position
        gameObject.transform.position = new Vector2(Random.Range(-7.5f, 7.5f), Random.Range(-3.5f, 3.5f));
        
        //change randomly the alpha value
        Color tmp = gameObject.GetComponent<SpriteRenderer>().color;
        tmp.a = Random.Range(0.3f,1f);
        gameObject.GetComponent<SpriteRenderer>().color = tmp;

        //change scale
        float scale = Random.Range(0.9f, 2f);
        gameObject.transform.localScale = new Vector3(scale, scale, 1);

    }

}
