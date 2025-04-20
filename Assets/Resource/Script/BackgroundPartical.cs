using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundPartical : MonoBehaviour
{
    [SerializeField] private BoxCollider2D Fire;
    [SerializeField] private ParticleSystem FireParticle;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        FireController();
    }

    private void FireController()
    {
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorldPosition);
        if (hitCollider == Fire)
        {
            Debug.Log("CHECK1");
            if(!FireParticle.isPlaying) {
                FireParticle.Play();
            }
 

        }
        else
        {
            FireParticle.Stop();
            FireParticle.Clear();
        }
    }
}
