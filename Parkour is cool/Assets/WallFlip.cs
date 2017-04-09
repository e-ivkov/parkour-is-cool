using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityStandardAssets._2D
{
    public class WallFlip : StateMachineBehaviour
    {

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            PlatformerCharacter2D character = animator.transform.GetComponent<PlatformerCharacter2D>();
            character.resetTriggers();
            if (animatorStateInfo.normalizedTime > 0.99)
            {
                
                animator.transform.GetComponent<PlatformerCharacter2D>().FinishTrick();
                character.m_Direction = -character.m_Direction;
            }
        }
    }
}
