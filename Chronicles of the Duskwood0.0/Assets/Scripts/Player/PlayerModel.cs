using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{


    [Header(" Ù–‘")]
    public int maxHp = 100;
    public int currentHP = 100;
    public int maxFuel = 100;
    public int currentFuel = 100;
    [Header("“∆∂Ø/Ã¯‘æ")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    //public BoxCollider2D collider;
    //public float colliderHeight;
    
    // Start is called before the first frame update
    void Start()
    {
       // collider = GetComponent<BoxCollider2D>();
       // colliderHeight = collider.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
