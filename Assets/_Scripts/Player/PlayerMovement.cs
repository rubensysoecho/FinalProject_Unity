﻿using System.Collections;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class PlayerMovement : MonoBehaviour {

    [Header("Deslizamiento en pared")]
    public bool isWallSliding;
    public float wallSlidingSpeed = 2f;

    [Header("Otros")]
    public LayerMask wallLayer;
    [SerializeField] private LayerMask m_WhatIsGround;
    [SerializeField] private float m_JumpForce = 0f;
    [SerializeField] private bool m_AirControl = false;
    [Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .05f;
    public bool m_FacingRight = true;
    public bool m_Grounded;
    public float horizontalInput = 0f;
    const float k_GroundedRadius = .0f;
    const float k_CeilingRadius = .2f;
    private Vector3 m_Velocity = Vector3.zero;

    [Header("Referencias")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform m_GroundCheck;
    public Rigidbody2D rb;
    public Transform wallCheck;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        bool wasGrounded = m_Grounded;
        m_Grounded = false;
        
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;
            }
        }

        WallSlide();
    }

    public void Move(float move) {
        if (m_Grounded || m_AirControl)
        {
            Vector3 targetVelocity = new Vector2(move * 10f, rb.velocity.y);
            rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

            if (move > 0 && !m_FacingRight)
            {
                Flip();
            }
            
            else if (move < 0 && m_FacingRight)
            {
                Flip();
            }
        }
    }
    
    private void Flip() {
        m_FacingRight = !m_FacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public void Jump() {
        //rb.velocity = Vector2.up * m_JumpForce;
        rb.velocity = new Vector2(rb.velocity.x, m_JumpForce);
    }

    private bool isWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    private void WallSlide()
    {
        if (isWalled() && !m_Grounded && horizontalInput != 0)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
            playerController.actualJumps = playerController.maxJumps;
        }
        else
        {
            isWallSliding = false;
        }
    }
}