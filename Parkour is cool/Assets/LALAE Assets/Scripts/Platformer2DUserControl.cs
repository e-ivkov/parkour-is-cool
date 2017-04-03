using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets._2D
{
    [RequireComponent(typeof (PlatformerCharacter2D))]
    public class Platformer2DUserControl : MonoBehaviour
    {
        
        private bool m_Jump;
        private PlatformerCharacter2D m_Character;


        private void Awake()
        {
            m_Character = GetComponent<PlatformerCharacter2D>();
            
        }


        private void Update()
        {
            if (!m_Jump)
            {
                // Read the jump input in Update so button presses aren't missed.
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }
        }


        private void FixedUpdate()
        {
          
            // Read the inputs.
            bool crouch = Input.GetKey(KeyCode.LeftControl);
            bool run = Input.GetKey(KeyCode.LeftShift);
            float hor = CrossPlatformInputManager.GetAxis("Horizontal");
            bool w = Input.GetKey(KeyCode.W);
            bool s = Input.GetKey(KeyCode.S);
            bool u = Input.GetKey(KeyCode.U);
            bool h = Input.GetKey(KeyCode.H);
            bool j = Input.GetKey(KeyCode.J);
            bool k = Input.GetKey(KeyCode.K);
            bool a = Input.GetKey(KeyCode.A);
            bool d = Input.GetKey(KeyCode.D);
            // Pass all parameters to the character control script.
            m_Character.UpdateStateMachine(hor,crouch, m_Jump, run, w,  s, a, d, u,  h,  j, k);
            m_Jump = false;
        }
    }
}
