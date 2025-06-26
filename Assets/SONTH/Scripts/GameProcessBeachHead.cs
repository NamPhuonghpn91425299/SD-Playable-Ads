using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameProcessBeachHead : MonoBehaviour
{
    [SerializeField] private Color _colorChange;
    [SerializeField] private Color _defaultColor;
    [SerializeField] private float _durationEffectOnBotIcon = 1f;
    [Header("Helicoper normal")]
    [SerializeField] private Image _iconHelicopterNormal;
    [SerializeField] private Text _botHelicopterNormalCount;
    [SerializeField] private List<BotNetwork> _lstBotHelicopterNormal;
    [Header("Helicoper heavy")]
    [SerializeField] private Image _iconHelicopterHeavy;
    [SerializeField] private Text _botHelicopterHeavyCount;
    [SerializeField] private List<BotNetwork> _lstBotHelicopterHeavy;
    [Header("Air Strike")]
    [SerializeField] private Image _iconAirStrike;
    [SerializeField] private Text _botAirStrikeCount;
    [SerializeField] private List<BotNetwork> _lstBotAirStrike;
    [Header("BattleShip")]
    [SerializeField] private Image _iconBattleShip;
    [SerializeField] private Text _botBattleShipCount;
    [SerializeField] private List<BotNetwork> _lstBotBattleShip;
    private bool _firstTouch = true;
    public static GameProcessBeachHead Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        foreach (var elem in _lstBotHelicopterNormal)
        {
            elem.OnBotNetWorkDead += OnBotHelicopterNormalDead;
        }
        foreach (var elem in _lstBotHelicopterHeavy)
        {
            elem.OnBotNetWorkDead += OnBotHelicopterHeavyDead;
        }
        foreach (var elem in _lstBotAirStrike)
        {
            elem.OnBotNetWorkDead += OnBotAirStrikeDead;
        }
        foreach (var elem in _lstBotBattleShip)
        {
            elem.OnBotNetWorkDead += OnBotBattleShipDead;
        }
    }

    private void OnDisable()
    {
        foreach (var elem in _lstBotHelicopterNormal)
        {
            elem.OnBotNetWorkDead -= OnBotHelicopterNormalDead;
        }
        foreach (var elem in _lstBotHelicopterHeavy)
        {
            elem.OnBotNetWorkDead -= OnBotHelicopterHeavyDead;
        }
        foreach (var elem in _lstBotAirStrike)
        {
            elem.OnBotNetWorkDead -= OnBotAirStrikeDead;
        }
        foreach (var elem in _lstBotBattleShip)
        {
            elem.OnBotNetWorkDead -= OnBotBattleShipDead;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SetUpStart();
        SetActiveBot(false);
        RocketController.Instance.listBot = GetListBot();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0) && _firstTouch)
        {
            _firstTouch = false;
            SetActiveBot(true);
        }
    }
    private void SetUpStart()
    {
        _botHelicopterNormalCount.text = _lstBotHelicopterNormal.Count.ToString();
        _botHelicopterHeavyCount.text = _lstBotHelicopterHeavy.Count.ToString();
        _botAirStrikeCount.text = _lstBotAirStrike.Count.ToString();
        _botBattleShipCount.text = _lstBotBattleShip.Count.ToString();
    }
    private void SetActiveBot(bool isActive)
    {
        foreach (var elem in _lstBotHelicopterNormal)
        {
            elem.gameObject.SetActive(isActive);
        }
        foreach (var elem in _lstBotHelicopterHeavy)
        {
            elem.gameObject.SetActive(isActive);
        }
        foreach (var elem in _lstBotAirStrike)
        {
            elem.gameObject.SetActive(isActive);
        }
        foreach (var elem in _lstBotBattleShip)
        {
            elem.gameObject.SetActive(isActive);
        }
    }
    private void OnBotHelicopterNormalDead(BotNetwork botNetwork)
    {
        _lstBotHelicopterNormal.Remove(botNetwork);
        StartCoroutine(EffectOnBotDeadIcon(_iconHelicopterNormal, _botHelicopterNormalCount, _lstBotHelicopterNormal.Count));
        _botHelicopterNormalCount.text = _lstBotHelicopterNormal.Count.ToString();
        if (RocketController.Instance.listBot.Contains(botNetwork))
        {
            RocketController.Instance.listBot.Remove(botNetwork);
        }
    }
    private void OnBotHelicopterHeavyDead(BotNetwork botNetwork)
    {
        _lstBotHelicopterHeavy.Remove(botNetwork);
        StartCoroutine(EffectOnBotDeadIcon(_iconHelicopterHeavy, _botHelicopterHeavyCount, _lstBotHelicopterHeavy.Count));
        _botHelicopterHeavyCount.text = _lstBotHelicopterHeavy.Count.ToString();
        if (RocketController.Instance.listBot.Contains(botNetwork))
        {
            RocketController.Instance.listBot.Remove(botNetwork);
        }
    }
    private void OnBotAirStrikeDead(BotNetwork botNetwork)
    {
        _lstBotAirStrike.Remove(botNetwork);
        StartCoroutine(EffectOnBotDeadIcon(_iconAirStrike, _botAirStrikeCount, _lstBotAirStrike.Count));
        _botAirStrikeCount.text = _lstBotAirStrike.Count.ToString();
        if (RocketController.Instance.listBot.Contains(botNetwork))
        {
            RocketController.Instance.listBot.Remove(botNetwork);
        }
    }
    private void OnBotBattleShipDead(BotNetwork botNetwork)
    {
        _lstBotBattleShip.Remove(botNetwork);
        StartCoroutine(EffectOnBotDeadIcon(_iconBattleShip, _botBattleShipCount, _lstBotBattleShip.Count));
        _botBattleShipCount.text = _lstBotBattleShip.Count.ToString();
        if (RocketController.Instance.listBot.Contains(botNetwork))
        {
            RocketController.Instance.listBot.Remove(botNetwork);
        }
    }
    private IEnumerator EffectOnBotDeadIcon(Image img, Text text, int botCout)
    {
        img.color = _colorChange;
        float timeElapsed = 0;
        if (botCout > 0)
        {
            while (timeElapsed < _durationEffectOnBotIcon)
            {
                timeElapsed += Time.deltaTime;
                img.color = Color.Lerp(_colorChange, _defaultColor, timeElapsed / _durationEffectOnBotIcon);
                text.color = Color.Lerp(_colorChange, _defaultColor, timeElapsed / _durationEffectOnBotIcon);
                yield return null;
            }
        }
    }
    public List<BotNetwork> GetListBot()
    {
        List<BotNetwork> lst = new List<BotNetwork> ();
        foreach (var elem in _lstBotHelicopterNormal)
        {
            lst.Add(elem);
        }
        foreach (var elem in _lstBotHelicopterHeavy)
        {
            lst.Add(elem);
        }
        foreach (var elem in _lstBotAirStrike)
        {
            lst.Add(elem);
        }
        foreach (var elem in _lstBotBattleShip)
        {
            lst.Add(elem);
        }
        return lst;
    }
}
