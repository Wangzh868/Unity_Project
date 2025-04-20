using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField] private Image Img_black;
    [SerializeField] private Image Img_red;
    private float current_likeability=0;
    // Update is called once per frame
    void Update()
    {
        if(CharaAnim.Likeability>=0 && CharaAnim.Likeability<=10&&current_likeability!= CharaAnim.Likeability)
        {
            current_likeability = CharaAnim.Likeability;
            Color color = Img_black.color;
            color.a = (float)((240-24*current_likeability)*0.01/3);
            Debug.Log(color.a);
            Img_black.color = color;
        }
        if(CharaAnim.Likeability<=0 &&current_likeability != CharaAnim.Likeability)
        {
            current_likeability = CharaAnim.Likeability;
            Color color = Img_red.color;
            color.a = (float)( -160*current_likeability/5/ 300);
            Debug.Log(color.a);
            Img_red.color = color;
        }
    }
}
