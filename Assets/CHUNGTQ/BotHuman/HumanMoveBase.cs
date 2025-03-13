using UnityEngine;

public class HumanMoveBase : MonoBehaviour
{
    [SerializeField] protected BotNetwork botNet;
    [SerializeField] protected BotConfigSO BotConfigSO;
    [SerializeField] protected Animator ator;
    public bool isHaveParent;
    public Transform myTrans;
    public bool isTakeDame;
    public AudioSource _source;
 
    private void Awake()
    {
        myTrans = transform;
    }
    protected virtual void OnEnable()
    {
        CheckParent();
        botNet.OnTakeDamage += OnTakeDame;
        botNet.OnBotDead += OnBotDead;
        
    }
    private void OnDisable()
    {
        
    }

    private void OnShowLunaEndGame(bool endGame)
    {
        if (endGame)
        {
            //Debug.Log(endGame);
            _source.Stop();
            _source.enabled = false;
        }
    }
    protected void Update()
    {
        CheckParent();
        OnShowLunaEndGame(UIEndGame.Instance.IsShowEndGame);
    }

    private void OnTakeDame(int dame)
    {
        isTakeDame = true;
        //ator.SetBool("isHit", true);
        Invoke(nameof(ResetTakeDame), 0.17f);
    }
    private void OnBotDead()
    {
    }
    private void ResetTakeDame()
    {
        isTakeDame = false;
        ator.SetBool("isHit", false);
    }
    protected virtual void CheckParent()
    {
        if (myTrans.parent != null)
        {
            isHaveParent = true;
        }
        else
        {
            isHaveParent = false;
        }
          
    }
    public void SetBotMove(Transform point)
    {
        if (!isTakeDame && !botNet.IsDead)
        {
            var targetRotation = Quaternion.LookRotation(point.position - myTrans.position);
            myTrans.rotation = Quaternion.Slerp(myTrans.rotation, targetRotation, 10 * Time.deltaTime);
            myTrans.position = Vector3.MoveTowards(myTrans.position, point.position, BotConfigSO.moveSpeed * Time.deltaTime);
        }
       
    }


}
