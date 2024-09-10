using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Control bar value display
/// </summary>
public class Bar : MonoBehaviour
{   
    [SerializeField] private Image _barSprite;
    [SerializeField] private TMP_Text _valueDisplayLabel;
    [SerializeField] private float _reduceSpeed = 2f;
    [SerializeField] private bool _faceToCamera = true;
    [SerializeField] private bool _gradualColorChange = true;
    [SerializeField] private Color _healthyColor = Color.green;

    private Color _injuredColor = Color.yellow; 
    private Color _criticalColor = Color.red;
    private float _target = 1f;
    private Camera _cam;
    
    public void UpdateBar(float currentHealth, float maxHealth) 
    {
        _target = currentHealth / maxHealth;

        if (_valueDisplayLabel != null)
        {
            _valueDisplayLabel.text = ((int)currentHealth).ToString();
        }
    }

    void Start() 
    {
        _cam = Camera.main;

        if (!_gradualColorChange)
        {
            _barSprite.color = _healthyColor;
        }
    }

    void Update() 
    {
        if (_faceToCamera) 
        {
            transform.rotation = UnityEngine.Quaternion.LookRotation(_cam.transform.forward);
        }

        if (_gradualColorChange)
        {
            if (_target > 0.7) 
            {
                _barSprite.color = Color.Lerp(_barSprite.color, _healthyColor, _reduceSpeed * Time.deltaTime);
            }
            else if (_target > 0.3)
            {
                _barSprite.color = Color.Lerp(_barSprite.color, _injuredColor, _reduceSpeed * Time.deltaTime);
            }
            else
            {
                _barSprite.color = Color.Lerp(_barSprite.color, _criticalColor, _reduceSpeed * Time.deltaTime);
            }
        }
        
        _barSprite.fillAmount = Mathf.MoveTowards(_barSprite.fillAmount, _target, _reduceSpeed * Time.deltaTime);
    }
}