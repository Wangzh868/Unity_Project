using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pincontroller : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject Pin;
    [SerializeField] private float x=0;
    [SerializeField] private float y=0;
    
    void Start()
    {
        
    }

    // Update is called once per frame

    private void OnTriggerStay2D(Collider2D other)
    {

        if (other.CompareTag("Assignment") && !Input.GetMouseButton(0))
        {

            Vector3 newPosition = other.transform.position;
            newPosition.x = Pin.transform.position.x + x;
            newPosition.y = Pin.transform.position.y + y;
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            rb.MovePosition(newPosition); 
           
           
      
        }
    }

}
