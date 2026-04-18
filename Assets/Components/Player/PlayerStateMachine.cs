using UnityEngine;

public class PlayerStateMachine : StateMachine
{
    private PlayerController playerController;
    private Animator animator;


    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();

        SetupStateMachine();

        Initialize<IdleState>();

        Activate();
    }

    private void SetupStateMachine()
    {
        AddPlayerStates();
        AddStateTransitions();
    }

    private void AddPlayerStates()
    {
        AddState(new IdleState(playerController, animator));
        //AddState(new WalkingState(playerController, animator));
        //AddState(new SprintingState(playerController, animator));
        //AddState(new JumpingState(playerController, animator));
        //AddState(new FallingState(playerController, animator));
    }

    private void AddStateTransitions()
    {
    }
}
