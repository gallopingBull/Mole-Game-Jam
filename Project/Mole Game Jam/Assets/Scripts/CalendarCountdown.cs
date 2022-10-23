using UnityEngine;
using TMPro;

public class CalendarCountdown : MonoBehaviour
{
    private float _gameLengthDuration = 0;
    private float _currentTime = 0;

    private bool _enableFade = false;

    private TextMeshProUGUI _textTarget;

    private Seasons _season = Seasons.winter;
    private SeasonTimes _seasonTimes;

    public TextMeshProUGUI[] SeasonNames;

    private void Awake()
    {
        _gameLengthDuration = GameManager.Instance.GameData.CalandarDuration;
        CalculateSeasonsDurations();
    }

    void FixedUpdate()
    {
        if (_currentTime < _gameLengthDuration && GameManager.Instance.GameStarted)
        {
            //Debug.Log($"current time: {_currentTime}");
            _currentTime += .1f;
            ProgressTime(_currentTime);
        }

        if (_enableFade)
            FadeOutText(Time.fixedDeltaTime);
    }

    private void ProgressTime(float time) 
    {
        int tmpTime = Mathf.RoundToInt(time);

        // game start

        if (tmpTime == _seasonTimes.Spring)
        {
            if (_season != Seasons.spring)
            {
                _season = Seasons.spring;
                _enableFade = true;
                SetText(_season);
            }
        }
        else if (tmpTime == _seasonTimes.Summer)
        {
            // is summer
            if (_season != Seasons.summer)
            {
                UIManager.Instance.HideHUD();
                _season = Seasons.summer;
                _enableFade = true;
                SetText(_season);
            }
        }
        else if (tmpTime == _seasonTimes.Fall)
        {
            // is fall
            if (_season != Seasons.fall)
            {
                UIManager.Instance.HideHUD();
                _season = Seasons.fall;
                _enableFade = true;
                SetText(_season);
            }
        }
        else if (tmpTime == _seasonTimes.Winter)
        {
            // is Winter
            if (_season != Seasons.winter)
            {
                UIManager.Instance.HideHUD();
                _season = Seasons.winter;
                _enableFade = true;
                GameEvents.OnTimerFinished?.Invoke();
            }
        }
    }
    
    private void CalculateSeasonsDurations()
    {
        int tmp = (int)(_gameLengthDuration / 3);
    
        _seasonTimes.Spring = 0;
        _seasonTimes.Summer = tmp;
        _seasonTimes.Fall = tmp * 2;
        _seasonTimes.Winter = (int)_gameLengthDuration;
    }
    
    private void SetText(Seasons season)
    {
        switch (season)
        {
            case Seasons.winter:
                _textTarget = SeasonNames[0];
                _textTarget.alpha = 100;
                break;
            case Seasons.spring:
                _textTarget = SeasonNames[1];
                _textTarget.alpha = 100;
                break;
            case Seasons.summer:
                _textTarget = SeasonNames[2];
                _textTarget.alpha = 100;
                break;
            case Seasons.fall:
                _textTarget = SeasonNames[3];
                _textTarget.alpha = 100;
                break;
            default:
                break;
        }
    }

    private void FadeOutText(float time)
    {
        var alpha = _textTarget.color.a;
        if (_textTarget.alpha <= 0)
        {
            UIManager.Instance.DisplayHUD();
            _enableFade = false;
        }
        if (_textTarget.alpha != 0)
        {
            alpha -= 45f * time;
            _textTarget.color = new Color(_textTarget.color.r, _textTarget.color.b, _textTarget.color.g, alpha);
        }
    }
}

public enum Seasons { winter, spring, fall, summer }
struct SeasonTimes
{
    public int Spring;
    public int Summer;
    public int Fall;
    public int Winter;
}
