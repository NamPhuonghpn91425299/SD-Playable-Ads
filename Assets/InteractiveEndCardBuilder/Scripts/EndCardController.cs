using System;
using System.Net;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[ExecuteInEditMode]
public class EndCardController : MonoBehaviour
{
    [Header("Used in Playground to view the end card at start")]
    [LunaPlaygroundField("TESTING", 1, "TESTING")]
    [SerializeField]
    public bool alwaysShowEndcardTESTING;

    [Header("End Card Settings")] [LunaPlaygroundField("Background Color", 0, "End Card Settings")] [SerializeField]
    private Color _backgroundColor;

    [LunaPlaygroundAsset("Background Image", 1, "End Card Settings")] [SerializeField]
    private Texture _backgroundTexture;

    [LunaPlaygroundAsset("Icon Image", 2, "End Card Settings")] [SerializeField]
    private Texture _iconTexture;

    [LunaPlaygroundField("Icon Color", 3, "End Card Settings")] [SerializeField]
    private Color _iconColor = Color.white;

    [LunaPlaygroundAsset("CTA Button Image", 4, "End Card Settings")] [SerializeField]
    private Texture _buttonTexture;

    [LunaPlaygroundField("CTA Button Color", 5, "End Card Settings")] [SerializeField]
    private Color _buttonColor = Color.white;

    [LunaPlaygroundField("Description Text", 6, "End Card Settings")] [SerializeField] [TextArea(3, 5)]
    private string _endCardDescriptionText = "An awesome description\nof my awesome game!";

    [LunaPlaygroundField("CTA Text", 7, "End Card Settings")] [SerializeField] [TextArea(1, 2)]
    private string _CTAButtonText = "Download\nNow!";

    [LunaPlaygroundField("Use Best Fit CTA", 8, "End Card Settings")] [SerializeField]
    private bool _useBestFitOnCTA;

    [LunaPlaygroundField("CTA Font Color", 9, "End Card Settings")] [SerializeField]
    private Color _CTAFontColor = Color.white;

    [LunaPlaygroundField("Description Font Color", 10, "End Card Settings")] [SerializeField]
    private Color _descriptionFontColor = Color.white;

    [LunaPlaygroundAsset("CTA Font", 11, "End Card Settings")] [SerializeField]
    private Font _CTAFont;

    [LunaPlaygroundAsset("Description Font", 12, "End Card Settings")] [SerializeField]
    private Font _descriptionFont;


    [Header("End Card Settings Portrait")]
    [LunaPlaygroundFieldStepAttribute(1f)]
    [LunaPlaygroundField("CTA Text Size", 1, "End Card Settings Portrait")]
    [SerializeField]
    [Range(10, 100)]
    private int _CTAFontSize = 10;

    [LunaPlaygroundFieldStepAttribute(1f)]
    [LunaPlaygroundField("Description Text Size", 1, "End Card Settings Portrait")]
    [SerializeField]
    [Range(10, 100)]
    private int _descriptionFontSize = 10;

    [LunaPlaygroundField("Center Description Horizontally", 0, "End Card Settings Portrait")] [SerializeField]
    private bool _centerDescriptionX = false;

    [LunaPlaygroundField("Center Description Vertically", 0, "End Card Settings Portrait")] [SerializeField]
    private bool _centerDescriptionY = false;

    [LunaPlaygroundField("Center Button Horizontally", 0, "End Card Settings Portrait")] [SerializeField]
    private bool _centerButtonX = false;

    [LunaPlaygroundField("Center Button Vertically", 0, "End Card Settings Portrait")] [SerializeField]
    private bool _centerButtonY = false;

    [LunaPlaygroundFieldStepAttribute(0.05f)]
    [LunaPlaygroundField("Description Position X", 1, "End Card Settings Portrait")]
    [SerializeField]
    [Range(0, 1)]
    private float _descriptionPositionX = 0.5f;

    [LunaPlaygroundFieldStepAttribute(0.05f)]
    [LunaPlaygroundField("Description Position Y", 1, "End Card Settings Portrait")]
    [SerializeField]
    [Range(0, 1)]
    private float _descriptionPositionY = 0.5f;

    [LunaPlaygroundFieldStepAttribute(0.05f)]
    [LunaPlaygroundField("Button Position X", 1, "End Card Settings Portrait")]
    [SerializeField]
    [Range(0, 1)]
    private float _buttonPositionX = 0.5f;

    [LunaPlaygroundFieldStepAttribute(0.05f)]
    [LunaPlaygroundField("Button Position Y", 1, "End Card Settings Portrait")]
    [SerializeField]
    [Range(0, 1)]
    private float _buttonPositionY = 0.5f;


    [Header("End Card Settings Landscape")]
    [LunaPlaygroundFieldStepAttribute(1f)]
    [LunaPlaygroundField("CTA Text Size", 1, "End Card Settings Landscape")]
    [SerializeField]
    [Range(10, 100)]
    private int _CTAFontSizeLandscape = 10;

    [LunaPlaygroundFieldStepAttribute(1f)]
    [LunaPlaygroundField("Description Text Size", 1, "End Card Settings Landscape")]
    [SerializeField]
    [Range(10, 100)]
    private int _descriptionFontSizeLandscape = 10;

    [LunaPlaygroundField("Center Description Horizontally", 0, "End Card Settings Landscape")] [SerializeField]
    private bool _centerDescriptionXLandscape = false;

    [LunaPlaygroundField("Center Description Vertically", 0, "End Card Settings Landscape")] [SerializeField]
    private bool _centerDescriptionYLandscape = false;

    [LunaPlaygroundField("Center Button Horizontally", 0, "End Card Settings Landscape")] [SerializeField]
    private bool _centerButtonXLandscape = false;

    [LunaPlaygroundField("Center Button Vertically", 0, "End Card Settings Landscape")] [SerializeField]
    private bool _centerButtonYLandscape = false;

    [LunaPlaygroundFieldStepAttribute(0.05f)]
    [LunaPlaygroundField("Description Position X", 1, "End Card Settings Landscape")]
    [SerializeField]
    [Range(0, 1)]
    private float _descriptionPositionXLandscape = 0.5f;

    [LunaPlaygroundFieldStepAttribute(0.05f)]
    [LunaPlaygroundField("Description Position Y", 1, "End Card Settings Landscape")]
    [SerializeField]
    [Range(0, 1)]
    private float _descriptionPositionYLandscape = 0.5f;

    [LunaPlaygroundFieldStepAttribute(0.05f)]
    [LunaPlaygroundField("Button Position X", 1, "End Card Settings Landscape")]
    [SerializeField]
    [Range(0, 1)]
    private float _buttonPositionXLandscape = 0.5f;

    [LunaPlaygroundFieldStepAttribute(0.05f)]
    [LunaPlaygroundField("Button Position Y", 1, "End Card Settings Landscape")]
    [SerializeField]
    [Range(0, 1)]
    private float _buttonPositionYLandscape = 0.5f;


    [Header("Icon Settings Portrait")]
    [LunaPlaygroundField("Center Icon Horizontally", 0, "Portrait Icon Settings")]
    [SerializeField]
    private bool _centerIconX = false;

    [LunaPlaygroundField("Center Icon Vertically", 0, "Portrait Icon Settings")] [SerializeField]
    private bool _centerIconY = false;

    [LunaPlaygroundFieldStepAttribute(0.05f)]
    [LunaPlaygroundField("X Position", 1, "Portrait Icon Settings")]
    [SerializeField]
    [Range(0, 1)]
    private float _iconPositionX = 0.5f;

    [LunaPlaygroundFieldStepAttribute(0.05f)]
    [LunaPlaygroundField("Y Position", 2, "Portrait Icon Settings")]
    [SerializeField]
    [Range(0, 1)]
    private float _iconPositionY = 0.5f;

    [LunaPlaygroundFieldStepAttribute(0.05f)]
    [LunaPlaygroundField("Icon Size", 3, "Portrait Icon Settings")]
    [SerializeField]
    [Range(0.1f, 1)]
    private float _iconSize = 0.5f;

    [LunaPlaygroundField("Round edges", 3, "Portrait Icon Settings")] [SerializeField]
    private bool _roundEdgesOnIcon = false;


    [Header("Icon Settings Landscape")]
    [LunaPlaygroundField("Center Icon Horizontally", 0, "Landscape Icon Settings")]
    [SerializeField]
    private bool _centerIconXLandscape = false;

    [LunaPlaygroundField("Center Icon Vertically", 0, "Landscape Icon Settings")] [SerializeField]
    private bool _centerIconYLandscape = false;

    [LunaPlaygroundFieldStepAttribute(0.05f)]
    [LunaPlaygroundField("X Position", 1, "Landscape Icon Settings")]
    [SerializeField]
    [Range(0, 1)]
    private float _iconPositionXLandscape = 0.5f;

    [LunaPlaygroundFieldStepAttribute(0.05f)]
    [LunaPlaygroundField("Y Position", 2, "Landscape Icon Settings")]
    [SerializeField]
    [Range(0, 1)]
    private float _iconPositionYLandscape = 0.5f;

    [LunaPlaygroundFieldStepAttribute(0.05f)]
    [LunaPlaygroundField("Icon Size", 3, "Landscape Icon Settings")]
    [SerializeField]
    [Range(0.1f, 1)]
    private float _iconSizeLandscape = 0.5f;

    [LunaPlaygroundField("Round edges", 3, "Landscape Icon Settings")] [SerializeField]
    private bool _roundEdgesOnIconLandscape = false;


    [Header("Clickable options")] [LunaPlaygroundField("Clickable Option", 3, "Clickable Options")] [SerializeField]
    private EndCardClickableOptionsType _endCardClickableOptions = EndCardClickableOptionsType.CTAButtonOnly;


    [Header("End Card Animation Settings")]
    [LunaPlaygroundField("Button animation", 3, "Animation settings")]
    [SerializeField]
    private ButtonAnimationType _buttonAnimationType;

    [LunaPlaygroundField("Portrait animation", 3, "Animation settings")] [SerializeField]
    private EndCardPortraitAnimationType _endCardPortraitAnimationType;

    [LunaPlaygroundField("Landscape animation", 3, "Animation settings")] [SerializeField]
    private EndCardLandscapeAnimationType _endCardLandscapeAnimationType;


    [Header("Drag and drops")] [SerializeField]
    private RawImage _backgroundImage;

    [SerializeField] private Button _CTAButton;
    [SerializeField] private Button _ScreenCTAButton;
    [SerializeField] private Text _CTAButtonTextText;
    [SerializeField] private Text _EndCardDescriptionTextText;
    [SerializeField] private Mask _maskIcon;
    [SerializeField] private RectTransform _iconRect;
    [SerializeField] private RectTransform _iconRectMask;
    [SerializeField] private RectTransform _descriptionRect;
    [SerializeField] private RectTransform _buttonRect;
    [SerializeField] private Animator _CTAButtonAnimator;
    [SerializeField] private Animator _endCardAnimator;
    [SerializeField] private RawImage _iconImage;
    [SerializeField] private RawImage _CTAButtonImage;

    private int _CTAButtonAnimationScale;
    private int _CTAButtonAnimationWobble;
    private int _CTAButtonAnimationOpacity;

    private int _endCardAnimationScale;
    private int _endCardAnimationLeftToRight;
    private int _endCardAnimationRightToLeft;
    private int _endCardAnimationBottomToTop;
    private int _endCardAnimationTopToBottom;

    private bool fireStoreClickOnOpen = false;


    public UnityEvent EndCardOpenedEvent = new UnityEvent();

    //-------------
    private float _iconSizeScaler = 500;
    //-------------

    public static EndCardController Instance;

    private ScreenOrientationType _getCurrentScreenOrientationType 
    {
        get
        {
            return Screen.width < Screen.height ? ScreenOrientationType.Portrait : ScreenOrientationType.Landscape;
        }
    }

    private ScreenOrientationType _currentScreenType = ScreenOrientationType.Undefined;

    void Awake()
    {
        CreateInstance();
        _ScreenCTAButton.gameObject.SetActive(false);
        _CTAButtonAnimationScale = Animator.StringToHash("Scale");
        _CTAButtonAnimationWobble = Animator.StringToHash("Wobble");
        _CTAButtonAnimationOpacity = Animator.StringToHash("Opacity");

        _endCardAnimationScale = Animator.StringToHash("Scale");
        _endCardAnimationLeftToRight = Animator.StringToHash("LeftToRight");
        _endCardAnimationRightToLeft = Animator.StringToHash("RightToLeft");
        _endCardAnimationBottomToTop = Animator.StringToHash("BottomToTop");
        _endCardAnimationTopToBottom = Animator.StringToHash("TopToBottom");

        //TESTING:
        if (alwaysShowEndcardTESTING)
        {
            _endCardAnimator.SetBool("TEST", true);
        }
    }

    void SetClickableOptions()
    {
        _ScreenCTAButton.gameObject.SetActive(false); //Always turn off
        _CTAButton.onClick.AddListener(ClickCTA); //Always do this
        switch (_endCardClickableOptions)
        {
            case EndCardClickableOptionsType.CTAButtonOnly:
                //We always do this anyway
                break;
            case EndCardClickableOptionsType.Fullscreen:
                _ScreenCTAButton.gameObject.SetActive(true);
                _ScreenCTAButton.onClick.AddListener(ClickCTA);
                break;
            case EndCardClickableOptionsType.FireAppStoreClickOnShow:
                EndCardOpenedEvent.AddListener(ClickCTA);
                break;
        }
    }

    void CreateInstance()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning($"There are 2 instances of EndCardController.cs on the {name} object. Destroying...");
            Destroy(Instance);
        }
    }

    public void OpenEndCard()
    {
        SetClickableOptions();
        EndCardOpenedEvent.Invoke();

        SetButtonAnimation();
        SetEndCardAnimationType();
        Luna.Unity.Analytics.LogEvent("Triggered open endcard", 1);
        EndGame();
    }

    void SetCTAButtonPosition()
    {
        var buttonPositionX = IsPortrait() ? _buttonPositionX : _buttonPositionXLandscape;
        var buttonPositionY = IsPortrait() ? _buttonPositionXLandscape : _buttonPositionYLandscape;

        if (IsPortrait())
        {
            buttonPositionX = IsPortrait() && !_centerButtonX ? _buttonPositionX : 0.5f;
            buttonPositionY = IsPortrait() && !_centerButtonY ? _buttonPositionY : 0.5f;
        }
        else
        {
            buttonPositionX = !IsPortrait() && !_centerButtonXLandscape ? _buttonPositionXLandscape : 0.5f;
            buttonPositionY = !IsPortrait() && !_centerButtonYLandscape ? _buttonPositionYLandscape : 0.5f;
        }

        _buttonRect.anchorMin = new Vector2(buttonPositionX, buttonPositionY);
        _buttonRect.anchorMax = new Vector2(buttonPositionX, buttonPositionY);
        _buttonRect.pivot = new Vector2(buttonPositionX, buttonPositionY);
    }

    public void ClickCTA()
    {
        Luna.Unity.Playable.InstallFullGame();
    }

    private void EndGame()
    {
        Luna.Unity.LifeCycle.GameEnded();
    }

    void Start()
    {
        SetBackgroundImage();
        SetBackgroundColor();
        SetText();
        SetMask();
        SetFontSize();
        SetIconSize();
        SetPositionIcon();
        SetFontColor();
        SetPositionDescriptionText();
        SetFont();
        SetCTAButtonPosition();
        SetBestFitCTA();
        SetIconImage();
        SetIconColor();
        SetCTAButtonImage();
        SetCTAButtonColor();
    }

    private void SetIconImage()
    {
        _iconImage.texture = _iconTexture;
    }


    void Update()
    {
#if UNITY_EDITOR
        SetBackgroundImage();
        SetBackgroundColor();
        SetText();
        SetMask();
        SetFontSize();
        SetIconSize();
        SetPositionIcon();
        SetFontColor();
        SetPositionDescriptionText();
        SetFont();
        SetCTAButtonPosition();
        SetBestFitCTA();
        SetIconImage();
        SetIconColor();
        SetCTAButtonImage();
        SetCTAButtonColor();
#endif
#if !UNITY_EDITOR
    if (_currentScreenType != _getCurrentScreenOrientationType)
        {
            _currentScreenType = _getCurrentScreenOrientationType;
            SetCTAButtonPosition();
            SetFontSize();
            SetPositionIcon();
            SetPositionDescriptionText();
        }
#endif
    }
    

    void SetIconColor()
    {
        _iconImage.color = _iconColor;
    }

    private void SetBestFitCTA()
    {
        _CTAButtonTextText.resizeTextForBestFit = _useBestFitOnCTA;
    }

    void SetFont()
    {
        if (_CTAFont != null)
        {
            _CTAButtonTextText.font = _CTAFont;
        }

        if (_descriptionFont != null)
        {
            _EndCardDescriptionTextText.font = _descriptionFont;
        }
    }

    void SetFontColor()
    {
        _CTAButtonTextText.color = _CTAFontColor;
        _EndCardDescriptionTextText.color = _descriptionFontColor;
    }

    void SetCTAButtonImage()
    {
        _CTAButtonImage.texture = _buttonTexture;
    }

    void SetCTAButtonColor()
    {
        _CTAButtonImage.color = _buttonColor;
    }

    void SetButtonAnimation()
    {
        switch (_buttonAnimationType)
        {
            case ButtonAnimationType.None:
                break;
            case ButtonAnimationType.Opacity:
                _CTAButtonAnimator.SetBool(_CTAButtonAnimationOpacity, true);
                break;
            case ButtonAnimationType.Wobble:
                _CTAButtonAnimator.SetBool(_CTAButtonAnimationWobble, true);
                break;
            case ButtonAnimationType.Scale:
                _CTAButtonAnimator.SetBool(_CTAButtonAnimationScale, true);
                break;
        }
    }

    void SetEndCardAnimationType()
    {
        if (!alwaysShowEndcardTESTING)
        {
            if (IsPortrait())
            {
                switch (_endCardPortraitAnimationType)
                {
                    case EndCardPortraitAnimationType.None:
                        break;
                    case EndCardPortraitAnimationType.Scale:
                        _endCardAnimator.SetBool(_endCardAnimationScale, true);
                        break;
                    case EndCardPortraitAnimationType.SlideBottomToTop:
                        _endCardAnimator.SetBool(_endCardAnimationBottomToTop, true);
                        break;
                    case EndCardPortraitAnimationType.SlideTopToBottom:
                        _endCardAnimator.SetBool(_endCardAnimationTopToBottom, true);
                        break;
                }
            }
            else
            {
                switch (_endCardLandscapeAnimationType)
                {
                    case EndCardLandscapeAnimationType.None:
                        break;
                    case EndCardLandscapeAnimationType.Scale:
                        _endCardAnimator.SetBool(_endCardAnimationScale, true);
                        break;
                    case EndCardLandscapeAnimationType.SlideLeftToRight:
                        _endCardAnimator.SetBool(_endCardAnimationLeftToRight, true);
                        break;
                    case EndCardLandscapeAnimationType.SlideRightToLeft:
                        _endCardAnimator.SetBool(_endCardAnimationRightToLeft, true);
                        break;
                }
            }
        }
    }

    void SetPositionIcon()
    {
        var iconPosX = IsPortrait() ? _iconPositionX : _iconPositionXLandscape;
        var iconPosY = IsPortrait() ? _iconPositionY : _iconPositionYLandscape;

        if (IsPortrait())
        {
            iconPosX = IsPortrait() && !_centerIconX ? _iconPositionX : 0.5f;
            iconPosY = IsPortrait() && !_centerIconY ? _iconPositionY : 0.5f;
        }
        else
        {
            iconPosX = !IsPortrait() && !_centerIconXLandscape ? _iconPositionXLandscape : 0.5f;
            iconPosY = !IsPortrait() && !_centerIconYLandscape ? _iconPositionYLandscape : 0.5f;
        }

        _iconRectMask.anchorMin = new Vector2(iconPosX, iconPosY);
        _iconRectMask.anchorMax = new Vector2(iconPosX, iconPosY);
        _iconRectMask.pivot = new Vector2(iconPosX, iconPosY);
    }

    void SetPositionDescriptionText()
    {
        var descriptionX = IsPortrait() ? _descriptionPositionX : _descriptionPositionXLandscape;
        var descriptionY = IsPortrait() ? _descriptionPositionY : _descriptionPositionYLandscape;

        if (IsPortrait())
        {
            descriptionX = IsPortrait() && !_centerDescriptionX ? _descriptionPositionX : 0.5f;
            descriptionY = IsPortrait() && !_centerDescriptionY ? _descriptionPositionY : 0.5f;
        }
        else
        {
            descriptionX = !IsPortrait() && !_centerDescriptionXLandscape ? _descriptionPositionXLandscape : 0.5f;
            descriptionY = !IsPortrait() && !_centerDescriptionYLandscape ? _descriptionPositionYLandscape : 0.5f;

            descriptionX = Mathf.Abs(descriptionX + -1); //Fix to go left to right
        }

        _descriptionRect.anchorMin = new Vector2(descriptionX, descriptionY);
        _descriptionRect.anchorMax = new Vector2(descriptionX, descriptionY);
        _descriptionRect.pivot = new Vector2(descriptionX, descriptionY);
    }

    void SetIconSize()
    {
        var ratio = 1f;

        var size = IsPortrait() ? _iconSize : _iconSizeLandscape;

        _iconRect.sizeDelta = new Vector2(size * _iconSizeScaler * ratio, size * _iconSizeScaler);
        _iconRectMask.sizeDelta = new Vector2(size * _iconSizeScaler * ratio, size * _iconSizeScaler);
    }

    void SetFontSize()
    {
        _CTAButtonTextText.fontSize = IsPortrait() ? _CTAFontSize : _CTAFontSizeLandscape;
        _EndCardDescriptionTextText.fontSize = IsPortrait() ? _descriptionFontSize : _descriptionFontSizeLandscape;
    }

    void SetMask()
    {
        bool enableEdge = IsPortrait() ? _roundEdgesOnIcon : _roundEdgesOnIconLandscape;
        _maskIcon.enabled = enableEdge;
    }

    void SetText()
    {
        _CTAButtonTextText.text = _CTAButtonText;
        _EndCardDescriptionTextText.text = _endCardDescriptionText;
    }

    void SetBackgroundImage()
    {
        _backgroundImage.texture = _backgroundTexture;
    }

    void SetBackgroundColor()
    {
        _backgroundImage.color = _backgroundColor;
    }

    bool IsPortrait()
    {
        if (Screen.width < Screen.height)
            return true;
        else
            return false;
    }
}