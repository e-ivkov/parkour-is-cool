using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityStandardAssets._2D
{
    public class WallClimb : StateMachineBehaviour
    {

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}


        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {


            PlatformerCharacter2D character = animator.transform.GetComponent<PlatformerCharacter2D>();
            BoxCollider2D collider = (BoxCollider2D)character.TrickCollider;
            character.resetTriggers();
            Debug.Log(collider.bounds.size.y);
            if (collider.bounds.size.y < character.CurrentTrick.m_AfterPositionVector.y)
            {
                
                animator.CrossFade("Wall Climb", 0);
            }
            else
            {   
                Vector2 force = new Vector2(-character.m_Direction,0);
                character.transform.Translate(force);
                character.FinishTrick();
            }
        }



    }

    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}
}
