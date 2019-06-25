using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public int maxJumps = 0;
    public float moveSpeed = 5.0f;
    public float fallSpeed = 0.5f;
    public float jumpSpeed = 1.0f;
    public float jumpTime = 1.0f;
    public float groundedRaycastDistance = 0.02f;
    public float wallRaycastDistance = 0.02f;
    public float jumpForgiveness = 0.1f;

    private Rigidbody2D body;
    private BoxCollider2D hitbox;
    private float jumpLeft = 0.0f;
    private float offGroundTime = 0.0f;
    private int jumpsLeft = 0;
    private bool showEnding = false;

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<BoxCollider2D>();
    }

    private void OnGUI()
    {
        if (showEnding)
        {
            GUI.ModalWindow(0, new Rect(100, 100, 200, 100), windowFunc, "Game Over");
        }
    }

    private void windowFunc(int id)
    {
        GUI.Label(new Rect(10, 20, 180, 50), "You won the game. Item completion: " + (maxJumps * 100 / 3) + "%");
        if (GUI.Button(new Rect(50, 60, 100, 30), "OK"))
        {
            Application.Quit();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name.StartsWith("Jump Power Up"))
        {
            maxJumps++;
            jumpsLeft++;
            collision.gameObject.SetActive(false);
        }
        if (collision.gameObject.name.StartsWith("Exit"))
        {
            showEnding = true;
        }
    }

    void FixedUpdate()
    {
        float verticalVelocity = body.velocity.y;
        Vector2 raycastStart = body.position + hitbox.offset;

        if (showEnding)
        {
            if (Input.GetButtonDown("Jump"))
            {
                Application.Quit();
            }
            body.velocity = new Vector2(0, 0);
            return;
        }

        if (Input.GetButtonUp("Jump"))
        {
            jumpLeft = 0;
        }

        if (jumpLeft == 0)
        {
            // Check to see if you are on the ground
            Vector2[] m_RaycastPositions = new Vector2[3];

            Vector2 raycastDirection = Vector2.down;
            Vector2 raycastStartBottomCenter = raycastStart + Vector2.down * (hitbox.size.y * 0.5f);

            m_RaycastPositions[0] = raycastStartBottomCenter + Vector2.left * hitbox.size.x * 0.5f;
            m_RaycastPositions[1] = raycastStartBottomCenter;
            m_RaycastPositions[2] = raycastStartBottomCenter + Vector2.right * hitbox.size.x * 0.5f;

            bool onGround = false;
            for (int i = 0; i < m_RaycastPositions.Length; i++)
            {
                RaycastHit2D hit = Physics2D.Raycast(m_RaycastPositions[i], raycastDirection, groundedRaycastDistance);
                if (hit.collider != null)
                {
                    onGround = true;
                }
            }

            if (onGround)
            {
                // If you are on the ground, restore jumps
                jumpsLeft = maxJumps;
                offGroundTime = 0.0f;
            } else
            {
                // If you are not on the ground, give a little forgiveness, then remove the first jump
                offGroundTime += Time.deltaTime;
                if (offGroundTime > jumpForgiveness && jumpsLeft >= maxJumps)
                {
                    jumpsLeft = maxJumps - 1;
                }
            }

            if (jumpsLeft > 0 && Input.GetButtonDown("Jump") && jumpLeft == 0)
            {
                jumpLeft = jumpTime;
                jumpsLeft--;
            }
        }

        if (jumpLeft > 0)
        {
            jumpLeft -= Time.deltaTime;
            verticalVelocity = jumpSpeed;
        }
        else
        {
            jumpLeft = 0;
            verticalVelocity -= fallSpeed;
        }
        float horizontalVelocity = Input.GetAxis("Horizontal") * moveSpeed;
        if (horizontalVelocity < 0)
        {
            // Check to see if you are running into a wall to the left
            Vector2 raycastDirection = Vector2.left;
            Vector2 raycastStartLeftCenter = raycastStart + Vector2.left * (hitbox.size.x * 0.5f);
            Vector2[] m_RaycastPositions = new Vector2[3];

            m_RaycastPositions[0] = raycastStartLeftCenter + Vector2.up * hitbox.size.y * 0.5f;
            m_RaycastPositions[1] = raycastStartLeftCenter;
            m_RaycastPositions[2] = raycastStartLeftCenter + Vector2.down * hitbox.size.y * 0.5f;

            bool onWall = false;
            for (int i = 0; i < m_RaycastPositions.Length; i++)
            {
                RaycastHit2D hit = Physics2D.Raycast(m_RaycastPositions[i], raycastDirection, wallRaycastDistance);
                if (hit.collider != null)
                {
                    onWall = true;
                }
            }


            if (onWall)
            {
                horizontalVelocity = 0;
            }
        } else if (horizontalVelocity > 0)
        {
            // Check to see if you are running into a wall to the right
            Vector2 raycastDirection = Vector2.right;
            Vector2 raycastStartRightCenter = raycastStart + Vector2.right * (hitbox.size.x * 0.5f);
            Vector2[] m_RaycastPositions = new Vector2[3];

            m_RaycastPositions[0] = raycastStartRightCenter + Vector2.up * hitbox.size.y * 0.5f;
            m_RaycastPositions[1] = raycastStartRightCenter;
            m_RaycastPositions[2] = raycastStartRightCenter + Vector2.down * hitbox.size.y * 0.5f;

            bool onWall = false;
            for (int i = 0; i < m_RaycastPositions.Length; i++)
            {
                RaycastHit2D hit = Physics2D.Raycast(m_RaycastPositions[i], raycastDirection, wallRaycastDistance);
                if (hit.collider != null)
                {
                    onWall = true;
                }
            }


            if (onWall)
            {
                horizontalVelocity = 0;
            }
        }

        body.velocity = new Vector2(horizontalVelocity, verticalVelocity);
        //transform.rotation = Quaternion.identity;
    }
}
