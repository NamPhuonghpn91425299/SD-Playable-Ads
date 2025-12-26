using System.Collections;
using UnityEngine;
    
    public class CrossHairRotate : MonoBehaviour
    {
        [Header("Charge Fill Value")]
        [SerializeField] private float _chargeFillAmount = 0f;
    
        [SerializeField] Vector2 _minMaxRotateVal;
        [SerializeField] Vector2 _minMaxScaleVal;
        [SerializeField] RectTransform _rectScale;
        [SerializeField] RectTransform _rectRotate;
        Vector2 _chargeAmountMinMax = new Vector2(0, 1);
        
        [Header("Speed")]
        [SerializeField] float speedScaleUp = 1f;
        [SerializeField] float speedScaleDown = 2f;
    
        private void OnEnable()
        {
            _rectRotate = GetComponent<RectTransform>();
        }
    
        private void Update()
        {
            float rotateValue = Remap(_chargeFillAmount, _chargeAmountMinMax.x, _chargeAmountMinMax.y, _minMaxRotateVal.x, _minMaxRotateVal.y);
            float scaleValue = Remap(_chargeFillAmount, _chargeAmountMinMax.x, _chargeAmountMinMax.y, _minMaxScaleVal.x, _minMaxScaleVal.y);
            _rectRotate.localRotation = Quaternion.Euler(0, 0, rotateValue);
            _rectScale.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
            
            if(Input.GetKey(KeyCode.L))
                ScaleUp();
            if(Input.GetKey(KeyCode.P))
                ScaleDown();
        }
        
        public void ScaleUp()
        {
            if(_chargeFillAmount < 1)
                _chargeFillAmount = Mathf.MoveTowards(_chargeFillAmount, 1, speedScaleUp * Time.deltaTime);
        }
    
        public void ScaleDown()
        {
            StartCoroutine(IEScaleDown());
        }

        private IEnumerator IEScaleDown()
        {
            _chargeFillAmount = 1;
            while (_chargeFillAmount > 0.01f)
            {
                _chargeFillAmount = Mathf.MoveTowards(_chargeFillAmount, 0, speedScaleDown * Time.deltaTime);
                yield return null;
            }
            _chargeFillAmount = 0; // Đảm bảo giá trị không âm
        }
        public static float Remap(float main, float minIn, float maxIn, float minOut, float maxOut)
        {
            return minOut + (main - minIn) * (maxOut - minOut) / (maxIn - minIn);
        }

    }