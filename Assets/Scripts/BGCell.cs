using UnityEngine;

public class BGCell : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _bgSprite;
    [SerializeField] private Sprite _emptySprite_light;
    [SerializeField] private Sprite _emptySprite_dark;
    [SerializeField] private Sprite _blockedSprite;
    [SerializeField] private Color _startColor;
    [SerializeField] private Color _correctColor;
    [SerializeField] private Color _incorrectColor;
    [SerializeField] private Sprite _correctSprite;
    [SerializeField] private Sprite hardLevelSprite;
    [SerializeField] private Sprite _inCorrectSprite;
    [SerializeField] private Sprite _fullSprite;

    private Sprite _originalSprite;
    
    [SerializeField] public  bool IsBlocked;
    [SerializeField] public  bool IsFilled;
    
    public void Init(int blockValue,bool isDark,bool isblocked)
    {
        IsBlocked = blockValue == -1;
        if(IsBlocked)
        {
            IsFilled = true;
        }
        _bgSprite.sprite = IsBlocked ? _blockedSprite : isDark ? _emptySprite_dark : _emptySprite_light;
        if (isblocked)
        {
            _bgSprite.sprite = null;
        }
        _originalSprite = _bgSprite.sprite;
    }

    public void ResetHighLight()
    {
        _bgSprite.sprite = _originalSprite;
    }

    public void UpdateHighlight(bool isCorrect)
    {
        _bgSprite.sprite = isCorrect ? _correctSprite : _inCorrectSprite;
        
    }

    public void UpdateBlue()
    {
        _bgSprite.sprite= _fullSprite;
    }
   

}
