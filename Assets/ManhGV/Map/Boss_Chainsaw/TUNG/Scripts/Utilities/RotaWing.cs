using UnityEngine;

public class RotaWing : MonoBehaviour
{
    [SerializeField] WingValue[] wingValues;
    [SerializeField] float rotaRate = 0.05f;
    bool _slowDown;

    public bool SlowDown
    {
        get => _slowDown;
        set => _slowDown = value;
    }

    private void OnEnable()
    {
        Updater.Ins.Tick += Tick;
        _slowDown = false;
    }

    private void Tick()
    {
        if (_slowDown)
        {
            if (rotaRate < 0.5f) rotaRate += Time.deltaTime * 0.1f;
        }
        foreach (var wingInfo in wingValues)
            wingInfo.wingTrans.Rotate(Time.deltaTime / rotaRate * wingInfo.rotaAxi, Space.Self);
    }

    public void OnDisable()
    {
        Updater.Ins.Tick -= Tick;
    }

    public void SetRotaRate(float val)
    {
        rotaRate = val;
    }
}

[System.Serializable]
struct WingValue
{
    public Transform wingTrans;
    public Vector3 rotaAxi;
}