using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : Entity
{
    private float _xInput;
    private float _yInput;
    private float curDigHoldTime = 0f;
    private float MAXDigHoldTime = 3f;
    private float _pickUpAnimTime = 2f;

    private bool _enableInput = true;
    private bool _enableMovement = false;
    private bool _isRecharging = false;
    private bool _holdingDigButton = false;
    private bool _holdingBurrowButton = false;

    public bool ActionsEnabled = true; 
    public Transform _targetTransform;
    public GameObject mainLight;
    private NavMeshAgent _navMeshAgent;

    private static PlayerController _instance;
    private InputHandler _inputHandler;
    private MovementComponent _movementComponent;
    private DigComponent _digComponent;
    private CarryComponent _carryComponent;
    private HideComponent _hideComponent;
    private EatComponent _eatComponent;
    private State _state = State.idle;
    private IEnumerator _lastCoroutine = null;
  
    private float _rechargeDelay = 2f;
    public static PlayerController Instance { get => _instance; set => _instance = value; }
    public bool EnableMovement { get => _enableMovement; set => _enableMovement = value; }
    public State State { get => _state; set => _state = value; }
    public bool EnableInput { get => _enableInput; set => _enableInput = value; }
    public NavMeshAgent NavMeshAgent { get => _navMeshAgent; set => _navMeshAgent = value; }

    private Animator _animator;
    private bool _encumbered;

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        _animator = GetComponentInChildren<Animator>();
        _inputHandler = GetComponent<InputHandler>();
        _movementComponent = GetComponent<MovementComponent>();
        _digComponent = GetComponent<DigComponent>();
        _carryComponent = GetComponent<CarryComponent>();
        _hideComponent = GetComponent<HideComponent>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }
    
    protected override void Start()
    {
        MAX_health = GameManager.Instance.GameData.PlayerHealth;
        MAX_stamina = GameManager.Instance.GameData.PlayerStamina;
        MAX_speed = GameManager.Instance.GameData.MovementSpeed;
        MAXDigHoldTime = GameManager.Instance.GameData.DigDuration;
        _rechargeDelay = GameManager.Instance.GameData.StaminaRechargeDelay;
        _carryComponent.EncumberedSpeeds[0] = GameManager.Instance.GameData.MovementSpeed;
        for (int i = 1; i < _carryComponent.EncumberedSpeeds.Length; i++)
        {
            _carryComponent.EncumberedSpeeds[i] =
                GameManager.Instance.GameData.EncumberedSpeeds[i - 1];
        }
        
        base.Start();
    }
    
    public void OnEnable()
    {
        GameEvents.OnDrop += Drop;
        GameEvents.OnCarry += PickUp;
        GameEvents.OnFoundWorm += DigForWorm;
        GameEvents.OnEat += Eat;
    }

    public void OnDisable ()
    {
        GameEvents.OnDrop -= Drop;
        GameEvents.OnCarry -= PickUp;
        GameEvents.OnFoundWorm -= DigForWorm;
        GameEvents.OnEat -= Eat;    
    }

    void Update()
    {
        if (!GameManager.Instance.IntroPlaying)
        {
           // Debug.Log($"state: {_state}");

            //Debug.Log($"holding dig button: {_holdingDigButton}");
            //Debug.Log($"holding burrow button: {_holdingBurrowButton}");
            ///Debug.Log($"isRecharging: {_isRecharging}");
            //Debug.Log($"stamina: {Stamina}");
            //Debug.Log($"curDigHoldTime: {curDigHoldTime}");
            //Debug.Log($"canDig: {_digComponent.CanDig}");
            if (_inputHandler && _enableInput)
            {
                if (Input.GetAxis("Hide") == 0 || Input.GetButtonUp("HidePC"))
                {
                    _holdingBurrowButton = false;
                    if(_state == State.hiding)
                        ExitState(State.hiding);
                }
         
                // movement
                if (_enableMovement)
                {
                    _xInput = Input.GetAxisRaw("Horizontal");
                    _yInput = Input.GetAxisRaw("Vertical");

                    if (_state != State.hiding || _state != State.digging)
                    {
                        if (_xInput == 0 && _yInput == 0)
                            EnterState(State.idle);
                        else
                        {
                            // ugly hack to fix no stamina/holing burrow -> walk
                            if(!_holdingBurrowButton)
                                EnterState(State.walking);
                        }
                    }
                }
                if (ActionsEnabled)
                {
                    // digging
                    if (( Input.GetAxis("Dig") == 1 || Input.GetKey("right shift")) && !_holdingBurrowButton)
                    {
                        _holdingDigButton = true;

                        if (_isRecharging)
                            CancelRechargeStamina();
                        if (_state != State.hiding)
                        {
                            if (Stamina > 0)
                                EnterState(State.digging);
                        }
                        if (Stamina <= 0)
                        {
                            Stamina = 0; // force to whole number
                            ExitState(State.digging);
                        }
                    }
                    else
                    {
                        // not holding dig trigger
                        if (Input.GetKeyUp(KeyCode.RightShift) || Input.GetAxis("Dig") == 0)
                        {
                            _holdingDigButton = false;

                            curDigHoldTime = 0f;
                            if (_state == State.digging)
                                ExitState(State.digging);
                        }
                    }


                    // hiding 
                    if ((Input.GetAxis("Hide") == 1 || Input.GetKey(KeyCode.Space)) && !_holdingDigButton)
                    {
                        _holdingBurrowButton = true;
                        if (_isRecharging)
                            CancelRechargeStamina();
                        if (_state != State.hiding)
                        {
                            if (Stamina > 0 + (GameManager.Instance.GameData.BurrowStaminaCost * 3))
                                EnterState(State.hiding);
                        }
                        Stamina -= GameManager.Instance.GameData.BurrowStaminaCost;
                        GameEvents.OnStaminaUpdateEvent?.Invoke(Stamina);
                        if (Stamina <= 0)
                        {
                            if (_state == State.hiding)
                            {
                                Stamina = 0; // force to whole number
                                ExitState(State.hiding);
                            }
                        }
                    }
                }

                if (Input.GetButtonDown("PickUp"))
                    GameEvents.OnCarry?.Invoke();

                if (Input.GetButtonDown("Eat"))
                    GameEvents.OnEat?.Invoke();

                if (Input.GetButtonDown("Drop"))
                    GameEvents.OnDrop?.Invoke();

                if (Input.GetButtonDown("Reload"))
                    GetComponent<ReloadScene>().ReloadCurrentScene();

                if (Input.GetButtonDown("ExitApp"))
                {
                    Application.Quit();
                }

            }
        }
        else
        {
            // skip intro cutscene
            if (Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Return))
                GameEvents.OnGameBegin?.Invoke();
            // handles ai movement for cutscene
            if (_navMeshAgent.enabled)
                _navMeshAgent.destination = _targetTransform.position;
        }
    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.IntroPlaying)
        {
            if (_enableMovement)
            {
                if (_state == State.hiding)
                    Speed = GameManager.Instance.GameData.PlayerBurrowSpeed;
                else 
                    Speed = _carryComponent.RunSpeedCarryingWorms;                
                _movementComponent.Move(_xInput, _yInput, Speed);
            }
        }
    }

    public void EnterState(State state)
    {
        switch (state)
        {
            case State.idle:
                _state = State.idle;
                if (!_holdingDigButton || !_holdingBurrowButton)
                {
                    if (!_isRecharging && Stamina != MAX_stamina)
                        StartRecharge();
                }
                if (!_carryComponent.IsCarrying)
                    _animator.SetTrigger("Idle");
                else
                    _animator.SetTrigger("Idle_Encumbered");
                break;

            case State.walking:
                _state = State.walking;
                if (!_isRecharging && Stamina < MAX_stamina)
                    StartRecharge();

                if (!_carryComponent.IsCarrying)
                    _animator.SetTrigger("Walk");
                else
                    _animator.SetTrigger("Walk_Encumbered");
                break;

            case State.digging:
 
                if (_state != State.digging)
                    _state = State.digging;

                if (_isRecharging)
                    CancelRechargeStamina();

                // start digging
                if (curDigHoldTime == 0)
                    _digComponent.Dig(this);

                // continue digging
                if (curDigHoldTime < MAXDigHoldTime)
                {
                    curDigHoldTime += .1f;
                    if (_digComponent.DigDown == true)
                        Stamina -= GameManager.Instance.GameData.DigStaminaCost;
                    else
                        Stamina -= (GameManager.Instance.GameData.DigStaminaCost * 1.25f);
                    GameEvents.OnStaminaUpdateEvent?.Invoke(Stamina);
                }

                // stop digging
                if (curDigHoldTime >= MAXDigHoldTime)
                {
                    _digComponent.HoleCompleted();
                    ExitState(State.digging);
                }

                break;

            case State.hiding:
                
                _state = State.hiding;

                if (_isRecharging)
                    CancelRechargeStamina();

                if (_animator.GetBool("Encumbered"))
                    _encumbered = true;
                if (!GetComponent<DustTrail>().EnableDustTrails)
                    GetComponent<DustTrail>().EnableDustTrails = true;
                _hideComponent.Hide();
                break;
            default:
                break;
        }
    }

    public void ExitState(State state)
    {
        switch (state)
        {
            case State.idle:
                break;
            case State.walking:
                break;
            case State.digging:
                if (_state != State.digging)
                    break;
                _digComponent.StopDig();
                EnableMovement = true;
              
                break;
            case State.hiding:  
                _hideComponent.UnHide();
                GameEvents.OnStaminaUpdateEvent?.Invoke(Stamina);
                if (GetComponent<DustTrail>().EnableDustTrails)
                    GetComponent<DustTrail>().EnableDustTrails = false;
                if (_encumbered)
                    _animator.SetBool("Encumbered", true);
                EnterState(State.idle);
                break;

            default:
                break;
        }
    }

    public void Eat()
    {
    
    }
    
    public void PickUp()
    {
        if (_carryComponent.CanPickUp)
        {
            _animator.SetTrigger("Grab");
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            _pickUpAnimTime = 2f;
            StartCoroutine("DisableMovement");
        }
    }
    
    private void DigForWorm()
    {
        _animator.SetTrigger("Grab");
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        _pickUpAnimTime = 3f;
        StartCoroutine("DisableMovement");
    }

    public void Drop()
    {
        Debug.Log("drop");
    }

    private IEnumerator DisableMovement()
    {
        _enableMovement = false;
        yield return new WaitForSeconds(_pickUpAnimTime);
        _enableMovement = true;
        _animator.SetBool("FoundWorm", false);
        _animator.SetTrigger("Idle_Encumbered");
        StopCoroutine("DisableMovement");
    }
    
    public override void Death()
    {
        base.Death();
        GameManager.Instance.GameOver(FailStates.playerDied);
    }
    
    public override void RegainHealth(float healthValue)
    {
        base.RegainHealth(healthValue);
        GameEvents.OnHelathUpdateEvent.Invoke(Health);
    }

    public override void DamageTaken(float damageValue)
    {
        base.DamageTaken(damageValue);
        Debug.Log($"health: {Health}");
        Debug.Log($"damageValue: {damageValue}");
        GameEvents.OnDamageEvent?.Invoke(Health);
    }

    private IEnumerator RechargeStamina()
    {
        yield return new WaitForSeconds(_rechargeDelay);
        while (Stamina < MAX_stamina)
        {
            RegainStamina(Stamina += .1f);
            yield return new WaitForSeconds(.01f);
        }
        Stamina = 100; // force to whole number
        _isRecharging = false;
    }
    
    private void StartRecharge()
    {
        _isRecharging = true;
        _lastCoroutine = RechargeStamina();
        StartCoroutine(_lastCoroutine);
    }

    private void CancelRechargeStamina()
    {
        _isRecharging = false;
        StopCoroutine(_lastCoroutine);
    }

    public void EnableMainLight()
    {
        mainLight.SetActive(true);
    }
}

public enum State { idle, walking, digging, hiding }
