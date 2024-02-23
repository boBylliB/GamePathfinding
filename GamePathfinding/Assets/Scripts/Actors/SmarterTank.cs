using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmarterTank : Kinematic
{
    Arrive myMoveType;
    SmarterFiring myBallisticType;

    public float firingSpeed = 1f;
    public float reloadTime = 3f;
    public Kinematic projectilePrefab;
    public GameObject tankMoveTarget;
    public bool selectSoonest = true;

    public Material successMat;
    public Material failMat;

    public bool debug = false;
    public LineRenderer lr;
    public int linesPerSegment = 5;

    public List<GameObject> noBounceObstacles;
    public List<GameObject> bounceObstacles;

    private float fireTimer = 0;

    // Start is called before the first frame update
    void Start()
    {
        myMoveType = new Arrive();
        myMoveType.character = this;
        myMoveType.target = tankMoveTarget;

        myBallisticType = new SmarterFiring();
        myBallisticType.character = this;
        myBallisticType.target = myTarget;
        myBallisticType.firingSpeed = firingSpeed;
        myBallisticType.selectSoonest = selectSoonest;
        myBallisticType.debug = debug;
        myBallisticType.lr = lr;
        myBallisticType.linesPerSegment = linesPerSegment;
        myBallisticType.noBounceObstacles = noBounceObstacles;
        myBallisticType.bounceObstacles = bounceObstacles;
    }

    // Update is called once per frame
    protected override void Update()
    {
        fireTimer -= Time.deltaTime;
        steeringUpdate = new SteeringOutput();
        steeringUpdate.linear = myMoveType.getSteering().linear;
        if (fireTimer <= 0 && Input.GetKeyDown("space"))
        {
            Debug.Log("Attempting to fire!");
            Trajectory firingTrajectory = myBallisticType.getTrajectory();
            if (firingTrajectory != null)
            {
                GetComponent<Renderer>().material = successMat;
                // Setting rotation manually whenever firing to snap to the direction of the firing vector
                transform.eulerAngles = new Vector3(Mathf.Asin(-firingTrajectory.launchVec.normalized.y) * Mathf.Rad2Deg, Mathf.Atan2(firingTrajectory.launchVec.x, firingTrajectory.launchVec.z) * Mathf.Rad2Deg, 0);
                // Create and launch a new projectile
                Kinematic projectile = Instantiate(projectilePrefab, transform.position, Quaternion.Euler(transform.eulerAngles));
                projectile.linearVelocity = firingTrajectory.launchVec;
                fireTimer = reloadTime;
            }
            else
            {
                GetComponent<Renderer>().material = failMat;
                Debug.Log("Null trajectory");
            }
        }
        base.Update();
    }
}
