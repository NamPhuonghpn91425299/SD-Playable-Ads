using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class botZomNorsuitDeadExplosion : BaseState<botZomState>
{
    [SerializeField] private BotNetwork botNetwork;
    [SerializeField] protected Animator anim;
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip[] listSounDead;
    public Vector3 BotDeadPos;
    public Vector3 posGas;
    private Transform TF;

    public BoxCollider boxCollider;
    public override void EnterState()
    {
        _source.volume = 0.1f;
        TF = transform;
        if(boxCollider!=null)
            boxCollider.enabled = false;
            
        botNetwork.OnBotDead.Invoke();
        BotDeadPos = this.transform.position;
        BotDeathHandler.Instance.OnBotDeath(BotDeadPos);
        AudioClip clipPlay = listSounDead[0];
        _source.PlayOneShot(clipPlay);
        botNetwork.Path.IsUse = false;
        BotDeath.Instance.GetBotDeath();
        
        StartCoroutine(HideBotOnDie());

        posGas = botNetwork.posExplosion;

        PrintExplosionDirection(TF, posGas);
    }
    IEnumerator HideBotOnDie()
    {
        yield return new WaitForSeconds(5f);
        gameObject.SetActive(false);
        
    }
    
    void PrintExplosionDirection(Transform target, Vector3 explosionCenter)
    {
        Vector3 direction = (target.position - explosionCenter).normalized;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            if (direction.x > 0)
            {
                anim.SetTrigger("E_Phai");
                Debug.Log("Phải");
            }
            else
            {
                anim.SetTrigger("E_Trai");
                Debug.Log("Trái");
            }
        }
        else
        {
            if (direction.z > 0)
            {
                anim.SetTrigger("E_Truoc");
                Debug.Log("Trước");
            }
            else
            {
                anim.SetTrigger("E_Sau");
                Debug.Log("Sau");
            }
        }
    }
    
    public override void UpdateState()
    {
        
    }

    public override void ExitState()
    {
        _source.Stop();
    }

    public override botZomState GetNextState()
    {
        return StateKey;
    }
}