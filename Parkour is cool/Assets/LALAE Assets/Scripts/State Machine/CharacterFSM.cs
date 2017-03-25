using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
namespace UnityStandardAssets._2D
{
    public enum eCharacterState {
        INIT,
        IDLE,
        SLOW_RUN,
        RUN,
        JUMP,
        FALL,
        TRICK,
        FAIL

    }
    public class CharacterFSM: FiniteStateMachine<eCharacterState>
    {



        private PlatformerCharacter2D m_Character;
        private bool m_Jump;
        

        private void Awake()
        {
            m_Character = GetComponent<PlatformerCharacter2D>();
        }

       

        public CharacterFSM()
        {

        }
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
           
        }


        void Run()
        {
            Debug.Log("RUN");
        }

        void SlowRun() {
            Debug.Log("SLOWAWWWRUN");
        }
    }
}
