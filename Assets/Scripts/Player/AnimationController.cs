using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    [Header("Components")]
    private Animator _animator;


    [Header("Animations IDs")]
    //GROUND LOCO AND JUMP
    private int _animIDGrounded;
    private int _animIDMotionSpeed;
    private int _animIDRun;
    private int _animIDJump;
    private int _animIDFreeFall;
       
       // COMBAT
    private int _animIDWithdraw;
    private int _animIDSheat;
    private int _animIDSlash;
    private int _animIDShoot;
    private int _animIDGuard;


#region Unity Funcs
    void Awake()
    {
        _animator = GetComponent<Animator>();   
        AssignAnimationIDs();
    }
 
    void OnEnable()
    {
        SubscribeEvents();
    }

    void OnDisable()
    {
        UnsubscribeEvents();
    }
#endregion


#region Event Subscriptions
    private void SubscribeEvents()
    {
        Combat.OnArmed +=  PlayDrawAnim;
        Combat.OnSlashing += PlaySlashAnim;
        Combat.OnShooting += PlayShootAnim; 
        Combat.OnGuarding += PlayGuardAnim;
        PlayerController.OnGrounded += UpdateGrounded;
        PlayerController.OnRun += UpdateRun;
        PlayerController.OnJump += Jump;
        PlayerController.OnFreeFall += FreeFall;
    } 

    private void UnsubscribeEvents()
    {
        Combat.OnArmed -=  PlayDrawAnim;
        Combat.OnSlashing -= PlaySlashAnim;
        Combat.OnShooting -= PlayShootAnim; 
        Combat.OnGuarding -= PlayGuardAnim;
        PlayerController.OnGrounded -= UpdateGrounded;
        PlayerController.OnRun -= UpdateRun;
        PlayerController.OnJump -= Jump;
        PlayerController.OnFreeFall -= FreeFall;
    }
#endregion


#region Assign hash Maps
   
    private void AssignAnimationIDs()
    {
        // Locomotion
        _animIDRun = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        
        
        //Combat 
        _animIDWithdraw = Animator.StringToHash("IsWithdrawing");
        _animIDSheat = Animator.StringToHash("IsSheating");
        _animIDSlash = Animator.StringToHash("IsSlashing");
        _animIDShoot = Animator.StringToHash("IsShooting");
        _animIDGuard = Animator.StringToHash("IsGuarding");
    }
#endregion

#region Anim Controls
    private void PlaySlashAnim()
    {
        _animator.ResetTrigger(_animIDSlash);
        _animator.SetTrigger(_animIDSlash);
    }

    private void PlayDrawAnim(bool isArming)
    {
        if (isArming)
        {
            _animator.ResetTrigger(_animIDWithdraw);
            _animator.SetTrigger(_animIDWithdraw);
            
        }
        else
        {
            _animator.ResetTrigger(_animIDSheat);
            _animator.SetTrigger(_animIDSheat);
        }
    }

    private void PlayShootAnim(float _shootRotationSpeed, float _rotationDegreeY)
    {   
        _animator.ResetTrigger(_animIDShoot);
        _animator.SetTrigger(_animIDShoot);
    }

    private void PlayGuardAnim(bool isGuarding)
    {
        if (_animator.GetBool(_animIDGuard) == isGuarding) return;

        _animator.SetBool(_animIDGuard,isGuarding); 
    }

    private void UpdateGrounded(bool isGrounded)
    {
        _animator.SetBool(_animIDGrounded,isGrounded);
    }

    private void UpdateRun(float _animationBlend, float _inputMagnitude)
    {
        _animator.SetFloat(_animIDRun, _animationBlend);
        _animator.SetFloat(_animIDMotionSpeed, _inputMagnitude);
    }

    public void ResetJump()
    {
        _animator.SetBool(_animIDFreeFall,false);
        _animator.SetBool(_animIDJump,false);
    }

    private void Jump()
    {
        _animator.SetBool(_animIDJump,true);
    }

    private void FreeFall()
    {
        _animator.SetBool(_animIDFreeFall,true);
    }
#endregion
}
