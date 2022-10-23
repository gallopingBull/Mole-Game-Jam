using System.Collections;
using UnityEngine;

public class DigComponent : MonoBehaviour
{
    private bool _canDig = false;
    private bool _digDown = false;  
    [SerializeField]
    private GameObject _holePrefab;
    [SerializeField]
    private GameObject _wormDugHolePrefab;
    private static GameObject _moundTarget;

    private Animator _animator;
    private DigDetection _detect;
    public enum DigStates
    {
        init_dig, digging, digging_complete
    }

    private DigStates _digState = DigStates.digging_complete;

    public static GameObject MoundTarget { get => _moundTarget; set => _moundTarget = value; }
    public bool DigDown { get => _digDown; set => _digDown = value; }

    private void Start() => Init();

    private void Init()
    {
        _detect = GetComponentInChildren<DigDetection>();
        _animator = GetComponentInChildren<Animator>();
    }

    public void Dig(Entity digger)
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;

        if (_digState == DigStates.digging_complete)
        {
            // check if section in front of player can be dug
            DetectionResults results = _detect.Results;
            //Debug.Log($"results: {_detect.Results}");
            _canDig = IsValid(results);

            // init dig action
            if (_canDig)
                EnterState(DigStates.init_dig);
        }
    }

    private void EnterState(DigStates state)
    {
        switch (state)
        {
            case DigStates.init_dig:
                GetComponent<Rigidbody>().velocity = Vector3.zero;
                PlayerController.Instance.EnableMovement = false;
                _digState = DigComponent.DigStates.init_dig;

                _animator.SetTrigger("DigInit");
                if (_digDown)
                    _animator.SetTrigger("DigDown");
                else
                    _animator.SetTrigger("DigForward");
                StartCoroutine(InitDig());
                break;

            case DigStates.digging:
                _digState = DigComponent.DigStates.digging;
                break;

            case DigStates.digging_complete:
                _animator.SetTrigger("DigCompleted");
                _canDig = false;
                _digState = DigStates.digging_complete;
      
                break;
            default:
                break;
        }
    }

    private void ExitState(DigStates state)
    {
        switch (state)
        {
            case DigStates.init_dig:
                break;
            case DigStates.digging:
                break;
            case DigStates.digging_complete:
                break;
            default:
                break;
        }
    }


    private IEnumerator InitDig()
    {
        yield return new WaitForSeconds(.1f);
        EnterState(DigStates.digging);
        StopCoroutine("InitDig");
    }

    public void StopDig()
    {
        if (_digState == DigStates.digging || _digState == DigStates.init_dig)
            EnterState(DigComponent.DigStates.digging_complete);
    }

    public void HoleCompleted()
    {
        if (_digState != DigStates.digging)
            return;

        if (_detect.Results == DetectionResults.dig_floor)
            SpawnDigPrefab();
        else
            DestroyDigableWall();
        EnterState(DigComponent.DigStates.digging_complete);
    }

    private bool IsValid(DetectionResults results)
    {
        bool valid = false;
        switch (results)
        {
            case DetectionResults.dig_wall:
                _digDown = false;
                valid = true;
                break;
            case DetectionResults.dig_floor:
                _digDown = true;
                if (_detect.FoundWorm)
                    _animator.SetBool("FoundWorm", true);
                valid = true;
                break;
            case DetectionResults.not_enough_space:
                Debug.Log("warning: not enough space, too close to wall ");
                break;
            case DetectionResults.wall:
                Debug.Log("warning: can't brake through a regular wall");
                break;
            case DetectionResults.hole:
                Debug.Log("warning: can't dig next on hole");
                break;
            default:
                break;
        }

        return valid;
    }

    public void SpawnDigPrefab()
    {
        if (_detect.FoundWorm)
        {
            var ass = Instantiate(_wormDugHolePrefab, _detect._targetPos2);
            ass.transform.parent = null;
            ass.transform.position = _detect._hitPoint2 + new Vector3(0, .5f, 0);

            GameEvents.OnFoundWorm?.Invoke();
        }
        else
        {
            if (_moundTarget)
                Destroy(_moundTarget);
            var tmp = Instantiate(_holePrefab, _detect._targetPos2);
            tmp.transform.parent = null;
            tmp.transform.position = _detect._hitPoint2 - new Vector3(0, .1f, 0);
            // give random rotation for visual variation
        }

    }

    public void DestroyDigableWall()
    {
        //var tmp = Instantiate(_holePrefab, _detect._targetPos2);
        //tmp.transform.parent = null;
        //tmp.transform.position = _detect._hitPoint2 - new Vector3(0, .1f, 0);
        var diggableWall = _detect.TargetWall.GetComponent<DiggableWall>();
        if (diggableWall != null)
        {
            diggableWall.DestroyLinkedWalls();
        }
        Destroy(_detect.TargetWall);
        
    }
}



