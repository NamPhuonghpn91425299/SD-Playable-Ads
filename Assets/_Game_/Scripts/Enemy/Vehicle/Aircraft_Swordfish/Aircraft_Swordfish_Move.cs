
using UnityEngine;
using PathCreation.Examples;

public class Aircraft_Swordfish_Move : StateBase
{
    [Header("Path Settings")]
    [SerializeField] private PointGroup assignedPath; // Để debug trong Inspector
    [SerializeField] private BotIdentity botIdentity; // Tham chiếu đến BotIdentity để lấy thông tin về đường đi
    [SerializeField] private GeneratePathExample generatePathExample;





    private void Start()
    {
        InitializePath();
    



    }

    private void InitializePath()
    {
        if (assignedPath == null && botIdentity != null)
            assignedPath = botIdentity.AssignedPath;
        generatePathExample.waypoints = botIdentity.Waypoints.ToArray();
        StartMoveLoop();

    }




    public override void EnterState()
    {

    }

    public override void UpdateState()
    {

    }

    public override void ExitState()
    {


    }

    public void StartMoveLoop()
    {
        generatePathExample.StartMoveLoop();
    }
}
