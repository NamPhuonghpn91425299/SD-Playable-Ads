using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayitaNhaydu : MonoBehaviour
{
    private Transform TF;
    [SerializeField] private EnemyBase thisBotNetwork;
    [SerializeField] private LayerMask ground;
    [SerializeField] private GameObject body;
    [SerializeField] private Transform _du;
    [SerializeField] private GameObject _spriteRenderer;
    [Header("Spawn Troop")]
    [SerializeField] private Transform spawnPos;
    [SerializeField] private BotDefinition botDefinition;
    [SerializeField] PointGroup pointGroup;
    private EnemyBase enemyBase;
    [Header("Tốc độ rơi khi chưa bung dù,hoặc dù hỏng")]
    [SerializeField] private float dropSpeed = 7;
    [SerializeField] private float dropBot;
    [SerializeField] private float dropDistanceDeath = 7;
    [Header("Tốc độ rơi sau khi bung dù")]
    [SerializeField] private float openParachuteDropSpeed = 2;

    [Header("Độ đung đưa của dù theo trục X")]
    [SerializeField] private AnimationCurve parachuteRotaX;

    [Header("Độ đung đưa của dù theo trục Z")]
    [SerializeField] private AnimationCurve parachuteRotaZ;

    private Coroutine C_MoveFirstDistance;
    private Vector3 landPos;
    private bool isOpenParachute;
    private float _countSwingTime;
    public float DistanceStopSwing = 1;
    [Header("Raycast Settings")]
    [SerializeField] private float raycastForwardOffset = 3f;
    [SerializeField] private float groundOffset = 0f; // Độ cao từ mặt đất (âm = chìm xuống, dương = nổi lên)
    [SerializeField] private bool isFallingAfterDeath = false;

    // MỚI: trạng thái tìm ground trong quá trình rơi
    private bool hasFoundGround = false;           // đã tìm thấy ground thông qua raycast
    private Vector3 parachuteOpenPos = Vector3.zero; // vị trí mở dù (dùng để vẽ đường dù)
    private Vector3 fallStartPos = Vector3.zero;   // vị trí bắt đầu rơi, dùng để vẽ đường xanh

    public void SetupPointSpawnInfantry(PointGroup pointGroup) => this.pointGroup = pointGroup;
    
    private void Awake()
    {
        TF = transform;
    }

    private void OnEnable()
    {
        ResetState();

        // Lưu vị trí bắt đầu rơi để vẽ đường debug
        fallStartPos = TF.position;

        // KHÔNG còn tìm ground 1 lần nữa ở đây nữa — ta sẽ raycast liên tục trong quá trình rơi
        _spriteRenderer.SetActive(true);
        C_MoveFirstDistance = StartCoroutine(MoveFirstDistance());
    }

    private void Update()
    {
        if (isOpenParachute)
        {
            _countSwingTime += Time.deltaTime;
            if (TF.position.y - landPos.y > DistanceStopSwing)
            {
                transform.localRotation = Quaternion.Euler(
                    parachuteRotaX.Evaluate(_countSwingTime),
                    transform.localEulerAngles.y,
                    parachuteRotaZ.Evaluate(_countSwingTime)
                );
            }
        }

        if (isFallingAfterDeath)
        {
            // Rơi xuống theo dropBot
            TF.Translate(dropBot * Time.deltaTime * Vector3.down);
            
            // Khi chạm đất
            if (TF.position.y - landPos.y <= DistanceStopSwing)
            {
                isFallingAfterDeath = false;
                _spriteRenderer.SetActive(false);
                //Debug.Log("Bot đã chạm đất sau khi chết.");
            }
        }

#if UNITY_EDITOR
        // Vẽ debug line: đường nhảy xuống màu xanh từ điểm bắt đầu rơi đến vị trí đang rơi (hoặc đến firstDes nếu có)
        Debug.DrawLine(fallStartPos, TF.position, Color.cyan);

        // Nếu đã mở dù, vẽ đường dù màu vàng từ vị trí mở dù tới điểm chạm đất
        if (isOpenParachute && hasFoundGround)
        {
            Debug.DrawLine(parachuteOpenPos, landPos, Color.yellow);
        }
#endif
    }
    
    void ResetState()
    {
        enemyBase = null;
        _countSwingTime = 0;
        isOpenParachute = false;
        isFallingAfterDeath = false;
        hasFoundGround = false;
        parachuteOpenPos = Vector3.zero;
        landPos = Vector3.zero;
    }


    public Vector2 FirstDistanceFallMinMax = new Vector2(10, 10);
    public Vector2 HitchForceMinMax = new Vector2(1.25f, 1.7f);
    
    IEnumerator MoveFirstDistance()
    {
        yield return new WaitForEndOfFrame(); // Đảm bảo đã thiết lập xong
        var startY = TF.position.y;
        float firstDistance = Random.Range(FirstDistanceFallMinMax.x, FirstDistanceFallMinMax.y);
    
        Vector3 firstDes = new Vector3(TF.position.x, startY - firstDistance, TF.position.z);

        //Debug.Log($"Bắt đầu rơi từ: {startY}, cần rơi {firstDistance}, mục tiêu: {firstDes.y}");

        // Trong quá trình rơi ban đầu, ta liên tục raycast xuống để tìm ground. Khi tìm được, set landPos và ngừng raycasting.
        while (Mathf.Abs(TF.position.y - firstDes.y) > 0.1f) // So sánh chính xác hơn
        {
            // Move xuống
            TF.position = Vector3.MoveTowards(TF.position, firstDes, dropSpeed * Time.deltaTime);

            // Nếu chưa tìm ground thì raycast mỗi frame
            if (!hasFoundGround)
            {
                RaycastHit dropPosHit;
                if (Physics.Raycast(TF.position + Vector3.forward * raycastForwardOffset, Vector3.down, out dropPosHit, 300f, ground))
                {
                    hasFoundGround = true;
                    landPos = dropPosHit.point + Vector3.up * groundOffset; // set landPos ngay khi tìm thấy
#if UNITY_EDITOR
                    // vẽ vòng tròn màu vàng tại điểm chạm ground
                    // (thực tế vẽ vòng tròn sẽ thực hiện trong OnDrawGizmos để dễ thấy trong Scene view)
#endif
                }
            }

            yield return null;
        }

        // Mở dù (hiện visual dù)
        _du.gameObject.SetActive(true);

        // Lưu vị trí mở dù để vẽ đường dù màu vàng
        parachuteOpenPos = TF.position;

        StartCoroutine(ScaleCoroutine(_du, Vector3.one * 0.01f, new Vector3(0.01f, 0.01f, 1f), Vector3.one, .1f, .3f));
        float hitchForce = Random.Range(HitchForceMinMax.x, HitchForceMinMax.y);
        Vector3 forceDes = TF.position + (Vector3.up * hitchForce);
        while (TF.position.y < forceDes.y)
        {
            TF.Translate(dropSpeed * Time.deltaTime * Vector3.up);
            yield return null;
        }
        isOpenParachute = true;

        // Nếu trước đó chưa tìm thấy ground (ví dụ vì ray không trúng), thử raycast một lần từ vị trí mở dù
        if (!hasFoundGround)
        {
            RaycastHit dropPosHit;
            if (Physics.Raycast(TF.position + Vector3.forward * raycastForwardOffset, Vector3.down, out dropPosHit, 1000f, ground))
            {
                hasFoundGround = true;
                landPos = dropPosHit.point + Vector3.up * groundOffset;
            }
            else
            {
                // fallback: nếu vẫn không tìm thấy, đặt landPos là firstDes (an toàn hơn để tránh null)
                landPos = firstDes;
            }
        }

        // Rơi chậm theo dù tới khi gần mặt đất
        while (TF.position.y - landPos.y > DistanceStopSwing)
        {
            TF.Translate(openParachuteDropSpeed * Time.deltaTime * Vector3.down);
            yield return null;
        }

        StartCoroutine(ScaleCoroutine(_du, Vector3.one, new Vector3(0.01f, 0.01f, 1f), Vector3.one * 0.01f, .2f, .1f,true));
        
        enemyBase = BotSpawnManager.Instance.ExecuteSpawnOrder(botDefinition,spawnPos,pointGroup,false);
        enemyBase.OnInit();
        enemyBase.TF.parent = null;
        if (pointGroup.points.Count <= 0) 
            enemyBase.stateController.OnInit(GameConstants.EnemyState.Attack);
        else 
            enemyBase.stateController.OnInit(GameConstants.EnemyState.Move);
        _spriteRenderer.SetActive(false);
    }

    private IEnumerator ScaleCoroutine(Transform target, Vector3 _form, Vector3 _to, Vector3 _to2,float _time1,float _time2,bool candespawn = false)
    {
        float elapsed = 0f;
        Vector3 from = _form;
        Vector3 to = _to;
        target.localScale = from;
        while (elapsed < _time1)
        {
            float t = elapsed / _time1;
            target.localScale = Vector3.Lerp(from, to, t);

            elapsed += Time.deltaTime;
            yield return null;
        }
        
        from = to;
        to = _to2;
        target.localScale = from;
        while (elapsed < _time2)
        {
            float t = elapsed / _time2;
            target.localScale = Vector3.Lerp(from, to, t);

            elapsed += Time.deltaTime;
            yield return null;
        }
        
        target.localScale = to;

        if (candespawn)
        {
            yield return HelperCoroutine.GetWait(0.5f);
            thisBotNetwork.OnDespawn(0f);
        }
    }

    
    public void OnDead()
    {
        if (C_MoveFirstDistance != null) StopCoroutine(C_MoveFirstDistance);
        isFallingAfterDeath = true;
        StartCoroutine(ScaleCoroutine(_du, TF.localScale, new Vector3(0.01f, 0.01f, 1f), Vector3.one * 0.01f, .2f, .1f,true));
        if (enemyBase != null)
        {
            enemyBase.TF.parent = TF;
            enemyBase.OnTakeDamage(new DamageInfo{damage = 10000,damageType = DamageType.Normal});
        }
        else
        {
            enemyBase = BotSpawnManager.Instance.ExecuteSpawnOrder(botDefinition,spawnPos,pointGroup,false);
            enemyBase.OnInit();
            enemyBase.TF.parent = TF;
            enemyBase.stateController.OnInit(GameConstants.EnemyState.Move);
            enemyBase.OnTakeDamage(new DamageInfo{damage = 10000,damageType = DamageType.Normal});
        }
        _spriteRenderer.SetActive(false);
    }

#if UNITY_EDITOR
    // Vẽ vòng tròn tại điểm chạm ground trong Scene view và một số gizmos hỗ trợ
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Vẽ vòng tròn màu vàng tại landPos
        if (hasFoundGround)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(landPos, 0.5f);
        }

        // Vẽ đường nhảy xuống màu xanh: từ điểm bắt đầu rơi đến first điểm hiện tại (handled in Update via Debug.DrawLine)
        // Vẽ đường dù màu vàng (handled in Update via Debug.DrawLine)

        // Dùng Handles để vẽ một vòng tròn lớn hơn rõ ràng trong Scene
        if (hasFoundGround)
        {
            Handles.color = Color.yellow;
            Handles.DrawWireDisc(landPos, Vector3.up, 0.75f);
        }
    }
#endif
}