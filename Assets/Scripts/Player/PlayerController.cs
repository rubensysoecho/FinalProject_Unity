using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rb;
    public bool hasFallen;
    public bool isAttacking;

    public float speed = 5f;
    public float runningSpeed = 1f;
    public PlayerMovement movement;

    public bool isAlive = true;
    public GameManager manager;

    public Animator anim;
    private float airIndex;

    public int actualJumps;
    public int maxJumps;

    public float jumpHeight;
    public float maxJumpHeight;
    public bool hardFall;

    void Start()
    {
        actualJumps = maxJumps;
        rb = movement.rb;
    }

    void Update()
    {
        airIndex = rb.velocity.y;

        anim.SetBool("Grounded", movement.m_Grounded);
        anim.SetBool("isAlive", isAlive);
        anim.SetBool("hardFall", hardFall);

        hasFallen = AnimationPlaying("fall");

        if (AnimationPlaying("attack01") || AnimationPlaying("attack02") || AnimationPlaying("attack03"))
        {
            isAttacking = true;
        }
        else
        {
            isAttacking = false;
        }

        // Salto del personaje
        Jump();
        
        if (movement.m_Grounded)
        {
            //actualJumps = maxJumps;
            jumpHeight = 0f;
            hardFall = false;
        }

        if (jumpHeight > maxJumpHeight)
        {
            hardFall = true;
        }

        // Asignar trigger caida salto
        if (airIndex <= 0 && !movement.m_Grounded)
        {
            anim.SetTrigger("Falling");
            jumpHeight += Time.deltaTime;
        }
        else
        {
            anim.ResetTrigger("Falling");
        }

        // Comprobar movimiento y determinar si est� corriendo
        if (movement.horizontalInput == 0)
        {
            anim.SetBool("Move", false);
        }
        else if (movement.horizontalInput != 0 && movement.m_Grounded)
        {
            anim.SetBool("Move", true);
            if (Input.GetKey(KeyCode.LeftShift))
            {
                anim.SetBool("Running", true);
            }
            else
            {
                anim.SetBool("Running", false);
            }
        }   
    }

    void FixedUpdate()
    {
        // Movimiento lateral, CANWALK ELIMINADO
        if (isAlive && !hasFallen && !isAttacking)
        {
            if (anim.GetBool("Running"))
            {
                movement.Move(movement.horizontalInput * (speed * runningSpeed) * Time.deltaTime);
            }
            else
            {
                movement.Move(movement.horizontalInput * speed * Time.deltaTime);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "PointProp":
                manager.levelPoints += 10;
                Destroy(collision.gameObject);
                break;
            case "HealthProp":
                if (manager.health < manager.maxHealth)
                {
                    manager.health += 20;
                }
                Destroy(collision.gameObject);
                break;
            case "CheckPoint":
                manager.spawnPoint = collision.gameObject.transform;
                Debug.Log("checkpoint");
                break;
            case "LevelEnd":
                manager.FinishLevel();
                switch (SceneManager.GetActiveScene().name)
                {
                    case "Level01":
                        LevelsCompleted.CompletarNivel(1);
                        break;
                    case "Level02":
                        LevelsCompleted.CompletarNivel(2);
                        break;
                    case "Level03":
                        LevelsCompleted.CompletarNivel(3);
                        break;
                    case "Level04":
                        LevelsCompleted.CompletarNivel(4);
                        break;
                }
                break;
            case "EndZone":
                if (manager.lives - 1 <= 0)
                {
                    manager.GameOver();
                }
                else
                {
                    manager.Spawn();
                    manager.lives--;
                }
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Spikes":
                anim.SetTrigger("Hit");
                Die();
                break;
            case "WeakPoint":
                Debug.Log("HIT");
                Destroy(collision.transform.parent.gameObject);
                break;
            case "Walker":
                MainEnemyController enemy = collision.gameObject.GetComponent<MainEnemyController>();
                if (manager.health - enemy.damage <= 0)
                {
                    Die();
                }
                else
                {
                    TakeDamage(enemy.damage);
                }
                break;
            case "Eagle":
                Debug.Log("IMPACTO " + collision.gameObject.tag);
                EagleController enemyEagle = collision.gameObject.GetComponent<EagleController>();
                if (manager.health - enemyEagle.damage <= 0)
                {
                    Die();
                }
                else
                {
                    TakeDamage(enemyEagle.damage);
                }
                break;
        }
    }

    private void Jump()
    {
        if (
            ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.UpArrow)) && isAlive && actualJumps > 0)
            ||
            ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.UpArrow)) && isAlive && movement.isWallSliding && manager.canWallJump)
           )
        {
            movement.Jump();
            anim.SetTrigger("Jump");
            actualJumps--;
        }
        else
        {
            anim.ResetTrigger("Jump");
        }

        if (movement.m_Grounded) 
        {
            actualJumps = maxJumps;
        }
    }
    
    public void Die()
    {
        manager.health = 0;
        rb.bodyType = RigidbodyType2D.Static;
        this.isAlive = false;
        Debug.Log("Has sido derrotado");
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    public void TakeDamage(int damage)
    {
        manager.health -= damage;
        Debug.Log("Te han tocado con " + damage);
    }

    private bool AnimationPlaying(string animationName)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            rb.velocity = Vector2.zero;
            return true;
        }
        return false;
    }
}
