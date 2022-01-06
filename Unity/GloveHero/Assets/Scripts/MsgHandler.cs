using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MsgHandler : MonoBehaviour
{
    private SpriteRenderer spriteRen;
    [SerializeField] private Sprite hitImage;
    [SerializeField] private Sprite missImage;

    private bool isActive = false;
    // Start is called before the first frame update
    void Start()
    {
        spriteRen = GetComponent<SpriteRenderer>();
        spriteRen.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator HitActivation()
    {
        spriteRen.sprite = hitImage; 
        spriteRen.enabled = true;
        isActive = true;
        yield return new WaitForSeconds(1);
        spriteRen.enabled = false;
        isActive = false;
    }

    public IEnumerator MissActivation()
    {
        spriteRen.sprite = missImage;
        spriteRen.enabled = true;
        yield return new WaitForSeconds(1);
        spriteRen.enabled = false;
    }
}
