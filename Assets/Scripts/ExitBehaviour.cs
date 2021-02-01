using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitBehaviour : StateMachineBehaviour
{
    private static readonly int FarL = Animator.StringToHash("farL");
	private static readonly int FarR = Animator.StringToHash("farR");
	private static readonly int HitL = Animator.StringToHash("hitL");
	private static readonly int HitR = Animator.StringToHash("hitR");
	private static readonly int EmptyHit = Animator.StringToHash("emptyHit");

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Conductor.punching = false;
        animator.ResetTrigger(FarL);
        animator.ResetTrigger(FarR);
        animator.ResetTrigger(HitL);
        animator.ResetTrigger(HitR);
        animator.ResetTrigger(EmptyHit);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       Conductor.punching = false;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
