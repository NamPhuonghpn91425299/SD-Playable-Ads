using UnityEngine;

namespace PathCreation.Examples
{
    // Example of creating a path at runtime from a set of points.

    public class GeneratePathExample : MonoBehaviour
    {

        public bool closedLoop = true;
        public Transform[] waypoints;
        [SerializeField] PathCreator pathCreator;
        [SerializeField] PathFollower pathFollower;
        PathCreator newPathCreator;

        public void StartMoveLoop()
        {
            BezierPath bezierPath = new BezierPath(waypoints, closedLoop, PathSpace.xyz);
             newPathCreator = Instantiate(pathCreator);


            newPathCreator.bezierPath = bezierPath;
            pathFollower.enabled = true;
            pathFollower.pathCreator = newPathCreator;
            pathFollower.RegisterEvent();

        }

        public void OnDeath()
        {
            Destroy(newPathCreator);
        }
    }
}