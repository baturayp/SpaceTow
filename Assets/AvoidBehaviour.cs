using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidBehaviour : StateMachineBehaviour
{
    private static readonly int FarL = Animator.StringToHash("farL");
	private static readonly int FarR = Animator.StringToHash("farR");
	private static readonly int HitL = Animator.StringToHash("hitL");
	private static readonly int HitR = Animator.StringToHash("hitR");
	private static readonly int EmptyHit = Animator.StringToHash("emptyHit");
    private static readonly int ShortAvoidL = Animator.StringToHash("shortAvoidL");
	private static readonly int ShortAvoidR = Animator.StringToHash("shortAvoidR");
	private static readonly int LongAvoidL = Animator.StringToHash("longAvoidL");
	private static readonly int LongAvoidR = Animator.StringToHash("longAvoidR");
    
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Conductor.avoiding = true;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    //OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Conductor.avoidPos = 0;
        animator.ResetTrigger(FarL);
        animator.ResetTrigger(FarR);
        animator.ResetTrigger(HitL);
        animator.ResetTrigger(HitR);
        animator.ResetTrigger(EmptyHit);
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
