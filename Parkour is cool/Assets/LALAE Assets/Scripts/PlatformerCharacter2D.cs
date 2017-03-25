using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;
using System.Collections.Generic;


namespace UnityStandardAssets._2D
{

    public class PlatformerCharacter2D : MonoBehaviour
    {
        [SerializeField]
        private float m_MaxSpeed = 10f;                    // The fastest the player can travel in the x axis.
        [SerializeField]
        private float m_JumpForce = 400f;                  // Amount of force added when the player jumps.
        [Range(0, 1)]
        [SerializeField]
        private float m_CrouchSpeed = .36f;  // Amount of maxSpeed applied to crouching movement. 1 = 100%
        [SerializeField]
        private bool m_AirControl = false;                 // Whether or not a player can steer while jumping;
        [SerializeField]
        private LayerMask m_WhatIsGround;                  // A mask determining what is ground to the character
        [SerializeField]
        private float m_RunMultiplier = 1.5f;

        private Transform m_GroundCheck;    // A position marking where to check if the player is grounded.
        const float k_GroundedRadius = .4f; // Radius of the overlap circle to determine if grounded
        private bool m_Grounded;            // Whether or not the player is grounded.
        private Transform m_CeilingCheck;   // A position marking where to check for ceilings
        const float k_CeilingRadius = .01f; // Radius of the overlap circle to determine if the player can stand up
        private Animator m_Anim;            // Reference to the player's animator component.
        private Rigidbody2D m_Rigidbody2D;
        private bool m_FacingRight = true;  // For determining which way the player is currently facing.

        //part about tricks
        private Trick m_CurrentTrick;
        private Collider2D m_TrickCollider;         //the Challenge collider of the current trick
        public Collider2D TrickCollider
        {
            get
            {
                return m_TrickCollider;
            }
        }
        private GameObject m_InTrickGameObject;       //GameObject tracking Guy's position in trick
        private float k_MaxTrickCooldown = 0.0f;    //time left just after the trick in milliseconds
        private float m_TrickCooldown = 0.0f;       //time left to perform another trick for the score boost in milliseconds
        private int m_TrickMultipliyer = 1;         //bonus xp multiplier
        private int m_LevelXP = 0;                  //XP got on this level
        public bool m_AutoRun;
        public float m_FlipVelocityThreshold;       //less than what vertical speed enables to flip
        private float m_Direction;                  //if auto run then detects direction
        private bool m_InTrick = false;
        private GameObject trickFollow;

        public Trick m_FlipTrick;
        public Trick m_MonkeyVaultTrick;

        private void Awake()
        {
            // Setting up references.
            m_GroundCheck = transform.Find("GroundCheck");
            m_CeilingCheck = transform.Find("CeilingCheck");
            m_Anim = GetComponent<Animator>();
            m_Rigidbody2D = GetComponent<Rigidbody2D>();
            trickFollow = GameObject.Find("TrickFollow");
        }


        private void FixedUpdate()
        {
            m_Grounded = false;

            // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
            // This can be done using layers instead but Sample Assets will not overwrite your project settings.
            Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject && colliders[i].gameObject != GameObject.Find("TrickFollow"))
                    m_Grounded = true;
            }
            m_Anim.SetBool("Ground", m_Grounded);

            // Set the vertical animation
            m_Anim.SetFloat("vSpeed", m_Rigidbody2D.velocity.y);
            if (m_Anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.95 && m_Anim.GetCurrentAnimatorStateInfo(0).IsName("Trick"))
            {
                FinishTrick();

            }
        }


        public void Move(float move, bool crouch, bool jump, bool run)
        {
            // If crouching, check to see if the character can stand up
            /*if (!crouch && m_Anim.GetBool("Crouch"))
            {
                // If the character has a ceiling preventing them from standing up, keep them crouching
                if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
                {
                    crouch = true;
                }
            }
            
            // Set whether or not the character is crouching in the animator
            m_Anim.SetBool("Crouch", crouch);*/

            //only control the player if grounded or airControl is turned on
            if (m_AutoRun && !m_InTrick)
            {
                if (Math.Abs(move)>0)
                {
                    m_Direction = Math.Sign(move);
                }
                else
                {
                    move = m_Direction;
                }
            }
            if ((m_Grounded || m_AirControl) && !m_InTrick)
            {
                // Reduce the speed if crouching by the crouchSpeed multiplier
               // move = (crouch ? move * m_CrouchSpeed : move);

                // The Speed animator parameter is set to the absolute value of the horizontal input.
                m_Anim.SetFloat("Speed", Mathf.Abs(move));
                m_Anim.SetBool("Run", run);
                // Move the character
                float speed = m_MaxSpeed;
                if (run) speed *= m_RunMultiplier;
                m_Rigidbody2D.velocity = new Vector2(move * speed, m_Rigidbody2D.velocity.y);

                // If the input is moving the player right and the player is facing left...
                if (move > 0 && !m_FacingRight)
                {
                    // ... flip the player.
                    Flip();
                }
                // Otherwise if the input is moving the player left and the player is facing right...
                else if (move < 0 && m_FacingRight)
                {
                    // ... flip the player.
                    Flip();
                }
            }
            // If the player should jump...
            if (m_Grounded && jump && m_Anim.GetBool("Ground"))
            {
                // Add a vertical force to the player.
                m_Grounded = false;
                m_Anim.SetBool("Ground", false);
                m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
                m_Anim.SetTrigger("Jump");
            }
            float x = CrossPlatformInputManager.GetAxis("Horizontal");
            if (!m_Grounded && Math.Abs(x) > 0 && Math.Sign(x) == m_Direction &&
                !m_Anim.GetCurrentAnimatorStateInfo(0).IsName("Trick") && m_Rigidbody2D.velocity.y < m_FlipVelocityThreshold && m_Rigidbody2D.velocity.y>0 && !m_InTrick)
            {
                //Trick[] tricks = Resources.FindObjectsOfTypeAll<Trick>();
                PerformTrick(m_FlipTrick);
                //Debug.Log("Start flip");
            }
            if (m_TrickCooldown > 0) //trick bonus xp cooldown timer
            {
                m_TrickCooldown -= Time.deltaTime;
            }
            else
            {
                m_TrickMultipliyer = 1;
            }
           
        }

        public void OnTriggerEnter2D(Collider2D collision)
        {
            float x = CrossPlatformInputManager.GetAxis("Horizontal");
            string coliderTag = collision.transform.tag;
            //Debug.Log(coliderTag);
            switch (coliderTag)
            {
                case "StandardBlock":
                    if (m_AutoRun && Math.Abs(x) > 0 && Math.Sign(x) == m_Direction && !m_Anim.GetCurrentAnimatorStateInfo(0).IsName("Trick"))
                    {
                        //Trick[] tricks = Resources.FindObjectsOfTypeAll<Trick>();
                        m_TrickCollider = collision;
                        transform.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
                        PerformTrick(m_MonkeyVaultTrick);
                    }
                    break;
            }
        }

        public void OnTriggerStay2D(Collider2D collision)
        {
            
        }

        public Trick FindTrick(Trick[] tricks, string trickName)
        {
            foreach (Trick trick in tricks)
            {
                if (trick.name == trickName)
                {
                    return trick;
                }
            }
            return null;
        }

        public void PerformTrick(Trick trick)
        {
            m_LevelXP += trick.m_XP * m_TrickMultipliyer;
            m_TrickMultipliyer *= 2;
            m_Anim.runtimeAnimatorController = trick.m_TrickAnimator;
            Vector3 scale = trickFollow.transform.localScale;
            trickFollow.transform.localScale.Set(m_Direction * scale.x, scale.y, scale.z);
            Animator followAnimator = trickFollow.GetComponent<Animator>();
            followAnimator.runtimeAnimatorController = trick.m_TrickFollowColliderAnimator;
            followAnimator.enabled = true;
            m_CurrentTrick = trick;
            m_Anim.SetTrigger("StartTrick");
            m_InTrick = true;
        }

        public void FinishTrick()
        {
            m_InTrick = false;
            //Debug.Log("End animation");
            transform.Translate(m_Direction * m_CurrentTrick.m_AfterPositionVector);
            //SceneView.RepaintAll();
            m_Anim.CrossFade("Idle", 0.0f);
            Animator followAnimator = trickFollow.GetComponent<Animator>();
            followAnimator.Play("follow", -1, 0f);
            followAnimator.enabled = false;
            m_TrickCollider = null;
            transform.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        public void FailTrick()
        {
            m_InTrick = false;
            BoxCollider2D collider = trickFollow.GetComponent<BoxCollider2D>();
            transform.Translate(m_Direction * collider.offset);
            m_Anim.CrossFade("Idle", 0.0f);
            Animator followAnimator = trickFollow.GetComponent<Animator>();
            followAnimator.Play("follow", -1, 0f);
            followAnimator.enabled = false;
            m_TrickCollider = null;
            transform.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private void Flip()
        {
            // Switch the way the player is labelled as facing.
            m_FacingRight = !m_FacingRight;

            // Multiply the player's x local scale by -1.
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }
    }
}
