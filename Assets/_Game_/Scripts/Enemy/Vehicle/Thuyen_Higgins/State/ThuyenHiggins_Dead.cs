using System;
using static GameConstants;
using UnityEngine;

public class ThuyenHiggins_Dead : StateBase
{
    [SerializeField] private bool isCano;
    [SerializeField] private ParticleSystem explodeParticles;
    [SerializeField] private GameObject _body;
    [SerializeField] private GameObject animExplosion; // Transform để hiển thị hiệu ứng nổ
    public bool DeadFake = false;

    private void OnEnable()
    {
        DeadFake = false;
    }

    public override void EnterState()
    {
        if (DeadFake)
        {
            
        }
        else
        {
            _body.SetActive(false);
            animExplosion.SetActive(true);
            explodeParticles.Play();
        }
        if(!isCano)
            botContext.botNetwork.OnDespawn(4.5f);
        else
        {
            botContext.ChangeAnimAndType(HashDead);
            botContext.botNetwork.OnDespawn(3f);
        }
    }

    public override void UpdateState()
    {
        
    }

    public override void ExitState()
    {
        
    }

    private void OnDisable()
    {
        OnInitState();
    }

    public void OnInitState()
    {
        _body.SetActive(true);
        animExplosion.SetActive(false);
    }
}