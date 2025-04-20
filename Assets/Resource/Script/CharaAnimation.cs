using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaAnimation : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Animator anim;
    [SerializeField] private BoxCollider2D face;
    [SerializeField] private float ShyColding = 60f; // 总计时时间（秒)
    [SerializeField] private float ShyDuration = 5f;
    [SerializeField] private float currentTime;    // 当前剩余时间
    [SerializeField] private ParticleSystem Love;
    void Start()
    {
        CharaAnim.Likeability = 10;
        CharaAnim.AnimIdel = AnimIdel.Idel;
        Love.Stop();
    }
    // Update is called once per frame
    void Update()
    {
        IsTouchingCheck();
        ShyTimmer();
        anim.SetFloat("Likeability", CharaAnim.Likeability);
        anim.SetFloat("AnimIdel", (float)CharaAnim.AnimIdel);
    }

    private void IsTouchingCheck()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorldPosition);

            if (hitCollider != null)
            {
                if (hitCollider==face && currentTime<=0)
                {
                    Love.Play();
                    currentTime = ShyColding;
                    CharaAnim.AnimIdel=AnimIdel.Is_shy;
                }
            }
        }
    }
    private void ShyTimmer()
    {
        if(currentTime>0)
        {
            if(currentTime<(ShyColding - ShyDuration) && CharaAnim.AnimIdel==AnimIdel.Is_shy)
            {
                CharaAnim.AnimIdel = AnimIdel.Idel;

                Love.Stop();
                Love.Clear();
            }

            currentTime -= Time.deltaTime;
        }
    }
}
