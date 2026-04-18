using UnityEngine;

public class BasePlayerState : BaseState
{
    protected PlayerController player;
    protected Animator animator;

    protected BasePlayerState(PlayerController player, Animator animator)
    {
        this.player = player;
        this.animator = animator;
    }
}
