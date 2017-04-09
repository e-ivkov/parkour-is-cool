using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor;
using System.Reflection;

namespace UnityStandardAssets._2D
{

    public class PlatformerCharacter2D : MonoBehaviour
    {

        const float epsilon = 0.00001f;
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
        public bool m_Grounded;            // Whether or not the player is grounded.
        private Transform m_CeilingCheck;   // A position marking where to check for ceilings
        const float k_CeilingRadius = .01f; // Radius of the overlap circle to determine if the player can stand up
        private Animator m_Anim;            // Reference to the player's animator component.
        private Rigidbody2D m_Rigidbody2D;
        private bool m_FacingRight = true;  // For determining which way the player is currently facing.

        //part about tricks
        private Trick m_CurrentTrick;
        public Trick CurrentTrick {
            get {
                return m_CurrentTrick;

            }
        }
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
        public bool m_InTrick = false;
        private GameObject trickFollow;

        public Trick m_FlipTrick;
        public Trick m_MonkeyVaultTrick;
        public Trick m_WallRun;

        //For state machine
        bool m_doTrick = false;
        public bool m_fail = false;

        //Player-controller input
        bool m_crouch;
        bool m_shift;
        float m_move;
        bool m_jump;
        bool m_w;
        bool m_s;
        bool m_u;
        bool m_h;
        bool m_j;
        bool m_k;
        bool m_airTrick;
        bool m_a;
        bool m_d;

        public CharacterFSM m_FSM;



        private void Awake()
        {
            // Setting up references.
            m_GroundCheck = transform.Find("GroundCheck");
            m_CeilingCheck = transform.Find("CeilingCheck");
            m_Anim = GetComponent<Animator>();
            m_Rigidbody2D = GetComponent<Rigidbody2D>();


            // FSM 


            m_FSM = new CharacterFSM();
            m_FSM.Advance(eCharacterState.INIT);
            m_FSM.AddTransition(eCharacterState.INIT, eCharacterState.IDLE, Idle);
            //Idle
            m_FSM.AddTransition(eCharacterState.IDLE, eCharacterState.IDLE, Idle);
            m_FSM.AddTransition(eCharacterState.IDLE, eCharacterState.SLOW_RUN, SlowRun);
            //Slow Run
            m_FSM.AddTransition(eCharacterState.SLOW_RUN, eCharacterState.RUN, FastRun);
            m_FSM.AddTransition(eCharacterState.SLOW_RUN, eCharacterState.SLOW_RUN, SlowRun);
            m_FSM.AddTransition(eCharacterState.SLOW_RUN, eCharacterState.IDLE, Idle);
            m_FSM.AddTransition(eCharacterState.SLOW_RUN, eCharacterState.TRICK, Trick);
            m_FSM.AddTransition(eCharacterState.SLOW_RUN, eCharacterState.JUMP, Jump);
            m_FSM.AddTransition(eCharacterState.SLOW_RUN, eCharacterState.FALL, Fall);
            //Run
            m_FSM.AddTransition(eCharacterState.RUN, eCharacterState.RUN, FastRun);
            m_FSM.AddTransition(eCharacterState.RUN, eCharacterState.SLOW_RUN, SlowRun);
            m_FSM.AddTransition(eCharacterState.RUN, eCharacterState.JUMP, Jump);
            m_FSM.AddTransition(eCharacterState.RUN, eCharacterState.FALL, Fall);
            m_FSM.AddTransition(eCharacterState.RUN, eCharacterState.FAIL, Fail);
            m_FSM.AddTransition(eCharacterState.RUN, eCharacterState.TRICK, Trick);

            //Jump
            m_FSM.AddTransition(eCharacterState.JUMP, eCharacterState.FALL, Fall);
            m_FSM.AddTransition(eCharacterState.JUMP, eCharacterState.AIR_TRICK, AirTrick);
            m_FSM.AddTransition(eCharacterState.JUMP, eCharacterState.FALL, Fall);
            m_FSM.AddTransition(eCharacterState.JUMP, eCharacterState.AIR_FAIL, AirFail);
            //Fall
            m_FSM.AddTransition(eCharacterState.FALL, eCharacterState.SLOW_RUN, SlowRun);
            m_FSM.AddTransition(eCharacterState.FALL, eCharacterState.FALL, Fall);
            m_FSM.AddTransition(eCharacterState.FALL, eCharacterState.RUN, FastRun);
            m_FSM.AddTransition(eCharacterState.FALL, eCharacterState.AIR_FAIL, AirFail);
            m_FSM.AddTransition(eCharacterState.FALL, eCharacterState.ROLL, Roll);
            //Trick


            m_FSM.AddTransition(eCharacterState.TRICK, eCharacterState.FALL, Fall);
            m_FSM.AddTransition(eCharacterState.TRICK, eCharacterState.FAIL, Fail);
           

            //Air trick
            m_FSM.AddTransition(eCharacterState.AIR_TRICK, eCharacterState.AIR_FAIL, AirFail);
            m_FSM.AddTransition(eCharacterState.AIR_TRICK, eCharacterState.FALL, Fall);

            //Fail
            m_FSM.AddTransition(eCharacterState.FAIL, eCharacterState.FALL, Fall);


            //Air Fail
            m_FSM.AddTransition(eCharacterState.AIR_FAIL, eCharacterState.STAND_UP, StandUp);


            //Stand Up

            m_FSM.AddTransition(eCharacterState.STAND_UP, eCharacterState.IDLE, Idle);

            //Roll


            //EXTRA POINTS TRICKS
            m_FSM.AddTransition(eCharacterState.TRICK, eCharacterState.TRICK, Trick);
            m_FSM.AddTransition(eCharacterState.TRICK, eCharacterState.AIR_TRICK, AirTrick);
            m_FSM.AddTransition(eCharacterState.AIR_TRICK, eCharacterState.TRICK, Trick);
            m_FSM.AddTransition(eCharacterState.AIR_TRICK, eCharacterState.AIR_TRICK, AirTrick);
            // Roll
            m_FSM.AddTransition(eCharacterState.ROLL, eCharacterState.SLOW_RUN, SlowRun);
            // TEST 
            m_FSM.AddTransition(eCharacterState.ROLL, eCharacterState.FAIL, Fail);
            trickFollow = GameObject.Find("TrickFollow");

        }

        private void Start()
        {

        }



        public void UpdateStateMachine(float hor, bool crouch, bool jump, bool run, bool w, bool s, bool a,
            bool d, bool u, bool h, bool j, bool k)
        {
            m_crouch = crouch;
            m_shift = run;
            m_move = hor;
            m_jump = jump;
            m_w = w;
            m_s = s;
            m_u = u;
            m_h = h;
            m_j = j;
            m_k = k;
            m_a = a;
            m_d = d;

            m_airTrick = (w || s || u || h || j || k || a || d);


            if (m_Grounded)
            {

                if (!m_AutoRun) m_FSM.Advance(eCharacterState.IDLE);
                if (m_AutoRun && !m_shift) m_FSM.Advance(eCharacterState.SLOW_RUN);
                if (m_shift) m_FSM.Advance(eCharacterState.RUN);
                if (m_jump) m_FSM.Advance(eCharacterState.JUMP);
                if (m_fail) m_FSM.Advance(eCharacterState.FAIL);
            }
            else
            {

                if (m_airTrick) m_FSM.Advance(eCharacterState.AIR_TRICK);
                if (m_Rigidbody2D.velocity.y < 0) m_FSM.Advance(eCharacterState.FALL);
                
            }

        }

        //public static void Misc_ClearLogConsole()
        //{
        //    Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
        //    Type logEntries = assembly.GetType("UnityEditorInternal.LogEntries");
        //    MethodInfo clearConsoleMethod = logEntries.GetMethod("Clear");
        //    clearConsoleMethod.Invoke(new object(), null);
        //}
        private void FixedUpdate()
        {
            // Misc_ClearLogConsole();
            if (m_FSM.GetPrevState().ToString() == m_FSM.GetPrevState().ToString())
                Debug.Log(m_FSM.GetState().ToString());
            m_Grounded = false;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject && colliders[i].gameObject != GameObject.Find("TrickFollow"))
                    m_Grounded = true;
            }


        }



        void Idle()
        {
            m_AutoRun = false;
            resetTriggers();
            m_Anim.SetTrigger("Idle");
            if (Math.Abs(m_move) > 0) m_AutoRun = true;
        }



        void FastRun()
        {



            //only control the player if grounded or airControl is turned on

            if (Math.Abs(m_move) > 0)
            {
                m_Direction = Math.Sign(m_move);
            }
            else
            {
                m_move = m_Direction;
            }


            // Reduce the speed if crouching by the crouchSpeed multiplier
            // move = (crouch ? move * m_CrouchSpeed : move);

            // The Speed animator parameter is set to the absolute value of the horizontal input.
            resetTriggers();
            m_Anim.SetTrigger("FastRun");

            // Move the character
            float speed = m_MaxSpeed;
            speed *= m_RunMultiplier;
            m_Rigidbody2D.velocity = new Vector2(m_move * speed, m_Rigidbody2D.velocity.y);

            // If the input is moving the player right and the player is facing left...
            if (m_move > 0 && !m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (m_move < 0 && m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }


        }


        void SlowRun()
        {



            //only control the player if grounded or airControl is turned on



            if (Math.Abs(m_move) > 0)
            {
                m_Direction = Math.Sign(m_move);
            }
            else
            {
                m_move = m_Direction;
            }


            // Reduce the speed if crouching by the crouchSpeed multiplier
            // move = (crouch ? move * m_CrouchSpeed : move);

            // The Speed animator parameter is set to the absolute value of the horizontal input.


            // Move the character
            float speed = m_MaxSpeed;
            resetTriggers();
            m_Anim.SetTrigger("SlowRun");


            m_Rigidbody2D.velocity = new Vector2(m_move * speed, m_Rigidbody2D.velocity.y);

            // If the input is moving the player right and the player is facing left...
            if (m_move > 0 && !m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (m_move < 0 && m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }


        }
        String[] triggers = { "FastRun", "SlowRun", "Fall", "Idle", "Jump", "StartTrick", "Monkey", "AirFail","Roll" };


        public void resetTriggers()
        {
            foreach (String trigger in triggers)
            {
                m_Anim.ResetTrigger(trigger);
            }
        }

        void Jump()
        {
            // If the player should jump...


            // Add a vertical force to the player.
            resetTriggers();
            m_Anim.SetTrigger("Jump");
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));



        }

        void Fall()
        {
            //TO DO add FAIL STATE       

            if (m_s) m_FSM.Advance(eCharacterState.ROLL);

            resetTriggers();
            m_Anim.SetTrigger("Fall");
            

        }

        void AirTrick()
        {

            float x = CrossPlatformInputManager.GetAxis("Horizontal");
            float y = CrossPlatformInputManager.GetAxis("Vertical");
            if (Math.Abs(x) > 0 && Math.Sign(x) == m_Direction && !m_InTrick)
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





        void Trick()
        {

            float x = CrossPlatformInputManager.GetAxis("Horizontal");
            float y = CrossPlatformInputManager.GetAxis("Vertical");
            string coliderTag = m_TrickCollider.transform.tag;
            //Debug.Log(coliderTag);
            switch (coliderTag)
            {
                case "StandardBlock":
                    if (Math.Abs(x) > 0 && Math.Sign(x) == m_Direction)
                    {
                        //Trick[] tricks = Resources.FindObjectsOfTypeAll<Trick>();

                        transform.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
                        PerformTrick(m_MonkeyVaultTrick);
                    }
                    else
                    {
                        //TO DO FAIL ANIMATION
                        m_fail = true;
                    }
                    break;
                case "Wall":
                    if (y > 0)
                    {
                        PerformTrick(m_WallRun);
                    }
                    else
                        m_fail = true;
                    break;

                default:
                    m_fail = true;
                    break;
            }
        }

        void Fail()
        {

            m_fail = false;
            resetTriggers();
            FailTrick();
            m_Anim.CrossFade("FailTrick", 0);
        }

        void AirFail()
        {

            resetTriggers();

            m_Anim.SetTrigger("AirFail");
            m_TrickMultipliyer = 1;

        }

        void StandUp()
        {
            m_Rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        void Roll() {
            resetTriggers();
            m_Anim.CrossFade("Landing",0);
            if (m_Grounded) {
                m_FSM.Advance(eCharacterState.SLOW_RUN);
            }
        }

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (!m_InTrick)
            {
                m_TrickCollider = collision;
                m_FSM.Advance(eCharacterState.TRICK);
                
            }



        }

        public void OnCollisionEnter2D(Collision2D collision)
        {

            m_FSM.Advance(eCharacterState.AIR_FAIL);

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


            Debug.Log("StartTRick");
            //m_Anim.runtimeAnimatorController = trick.m_TrickAnimator;
            Vector3 scale = trickFollow.transform.localScale;
            trickFollow.transform.localScale.Set(m_Direction * scale.x, scale.y, scale.z);
                Animator followAnimator = trickFollow.GetComponent<Animator>();
            followAnimator.runtimeAnimatorController = trick.m_TrickFollowColliderAnimator;
            followAnimator.enabled = true;
            m_CurrentTrick = trick;
            //resetTriggers();
            resetTriggers();
            m_Anim.SetTrigger("StartTrick");
            m_Anim.SetInteger("TrickIndex", trick.m_index);
            m_InTrick = true;


        }

        public void FinishTrick()
        {
            Debug.Log("FINISH!!");
            m_InTrick = false;
            m_LevelXP += m_CurrentTrick.m_XP * m_TrickMultipliyer;
            m_TrickMultipliyer *= 2;
            //Debug.Log("End animation");
            transform.Translate(m_Direction * m_CurrentTrick.m_AfterPositionVector);
            resetTriggers();

            //SceneView.RepaintAll();

            Animator followAnimator = trickFollow.GetComponent<Animator>();
            followAnimator.Play("follow", -1, 0f);
            followAnimator.enabled = false;
            m_TrickCollider = null;
            transform.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;

            m_FSM.Advance(eCharacterState.FALL);

        }

        public void FailTrick()
        {

            //m_fail = false;
            m_InTrick = false;
            BoxCollider2D collider = trickFollow.GetComponent<BoxCollider2D>();
            transform.Translate(m_Direction * collider.offset);
            Animator followAnimator = trickFollow.GetComponent<Animator>();
            followAnimator.Play("follow", -1, 0f);
            followAnimator.enabled = false;
            m_TrickMultipliyer = 1;
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
