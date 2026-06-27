using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using static UnityEditor.PlayerSettings;

public class Unit : BaseEntity
{
    [SerializeField]
    UnitDataScriptable UnitData = null;

    Transform BulletSlot;
    float LastActionDate = 0f;

    [HideInInspector]
    public BaseEntity EntityTarget = null;
    [HideInInspector]
    public TargetBuilding CaptureTarget = null;
    [HideInInspector]
    public Vector3 MoveToTarget = Vector3.zero;

    private Vector3 prevEndPathPointComputed = Vector3.zero;
    private bool destUpdate = false;

    FSM_Director fsm;

    public NavMeshAgent NavMeshAgent;

    [HideInInspector]
    public Squad squadRef = null;

    public SquadOrder SquadOrder
    {
        get { return squadOrder; }
        set
        {
            CancelSquadOrder();

            squadOrder = value;
            if (squadOrder != null)
                squadOrder.Enter(this);
        }
    }

    private SquadOrder squadOrder = null;

    public UnitDataScriptable GetUnitData { get { return UnitData; } }
    public int Cost { get { return UnitData.Cost; } }
    public int GetTypeId { get { return UnitData.TypeId; } }
    override public void Init(ETeam _team)
    {
        if (IsInitialized)
            return;

        base.Init(_team);

        HP = UnitData.MaxHP;
        OnDeadEvent += Unit_OnDead;
    }
    void Unit_OnDead()
    {
        if (IsCapturing())
            StopCapture();

        if (GetUnitData.DeathFXPrefab)
        {
            GameObject fx = Instantiate(GetUnitData.DeathFXPrefab, transform);
            fx.transform.parent = null;
        }

        Destroy(gameObject);
    }
    #region MonoBehaviour methods
    override protected void Awake()
    {
        base.Awake();

        NavMeshAgent = GetComponent<NavMeshAgent>();
        BulletSlot = transform.Find("BulletSlot");

        // fill NavMeshAgent parameters
        NavMeshAgent.speed = GetUnitData.Speed;
        NavMeshAgent.angularSpeed = GetUnitData.AngularSpeed;
        NavMeshAgent.acceleration = GetUnitData.Acceleration;


        fsm = GetComponentInChildren<FSM_Director>();
        fsm.fsmEntity = this;
    }
    override protected void Start()
    {
        // Needed for non factory spawned units (debug)
        if (!IsInitialized)
            Init(Team);

        base.Start();
    }
    override protected void Update()
    {
        // Attack / repair task debug test $$$ to be removed for AI implementation
        if (EntityTarget != null)
        {
            if (EntityTarget.GetTeam() != GetTeam())
                ComputeAttack();
            else
                ComputeRepairing();
        }
	}
    #endregion

    #region IRepairable
    override public bool NeedsRepairing()
    {
        return HP < GetUnitData.MaxHP;
    }
    override public void Repair(int amount)
    {
        HP = Mathf.Min(HP + amount, GetUnitData.MaxHP);
        base.Repair(amount);
    }
    override public void FullRepair()
    {
        Repair(GetUnitData.MaxHP);
    }
    #endregion

    #region Tasks methods : Moving, Capturing, Targeting, Attacking, Repairing ...

    // $$$ To be updated for AI implementation $$$

    public bool HasReachDest(float radius = -1f, bool realDest = false)
    {
        float dist = NavMeshAgent.remainingDistance;
        if (dist != Mathf.Infinity 
            && NavMeshAgent.pathStatus == NavMeshPathStatus.PathComplete 
            && dist < ((radius < NavMeshAgent.stoppingDistance) ? NavMeshAgent.stoppingDistance : radius))
        {
            return true;
        }
        return false;
    }

    public Vector3 GetDestination()
    { return NavMeshAgent.pathEndPosition; }

    public Vector3 GetRealDestination()
    { return NavMeshAgent.destination; }

    public void StopMoving()
    {
        if (NavMeshAgent)
        {
            NavMeshAgent.SetDestination(transform.position);
            NavMeshAgent.isStopped = true;
        }
    }

    public void MoveTo(Vector3 pos)
    {
        if (NavMeshAgent)
        {
            NavMeshAgent.SetDestination(pos);
            NavMeshAgent.isStopped = false;
        }
    }

    private void CancelSquadOrder()
    {
        if (squadOrder != null)
            squadOrder.Exit(this);
        squadOrder = null;
    }

    public int GetNavMeshArea()
    {
        if (NavMeshAgent == null)
            return -1;

        return this.NavMeshAgent.areaMask;
    }

    // Moving Task
    public void SetTargetPos(Vector3 pos, float StoppingDistance = -1f, bool stopOnArrived = false)
    {
        //already go here
        if (MoveToTarget == pos && squadOrder as MoveOrder != null)
            return;

        MoveToTarget = pos;

        StoppingDistance = (StoppingDistance < NavMeshAgent.stoppingDistance) ? NavMeshAgent.stoppingDistance : StoppingDistance;

        MoveOrder moveOrder = new MoveOrder();
        moveOrder.StoppingDistance = StoppingDistance;
        moveOrder.stopOnArrived = stopOnArrived;
        SquadOrder = moveOrder;
    }

    // Targetting Task - attack
    public void SetAttackTarget(BaseEntity target)
    {
        if (CanAttack(target) == false)
            return;

        if (CaptureTarget != null)
            StopCapture();

        if (target.GetTeam() != GetTeam())
            StartAttacking(target);
    }

    // Targetting Task - capture
    public bool SetCaptureTarget(TargetBuilding target)
    {
        //check distance
        if (target.GetTeam() == GetTeam() || CanCapture(target) == false)
        {
            //go to idle because order failed
            CancelSquadOrder();
            return false;
        }

        //already capturing the target
        if (IsCapturing(target) && (squadOrder as CaptureOrder) != null)
            return true;

        CaptureTarget = target;

        CaptureOrder captureOrder = new CaptureOrder();
        SquadOrder = captureOrder;

        return true;
    }

    // Targetting Task - repairing
    public void SetRepairTarget(BaseEntity entity)
    {
        if (CanRepair(entity) == false)
            return;

        if (CaptureTarget != null)
            StopCapture();

        if (entity.GetTeam() == GetTeam())
            StartRepairing(entity);
    }
    public bool CanAttack(BaseEntity target)
    {
        if (target == null)
            return false;

        // distance check
        if ((target.transform.position - transform.position).sqrMagnitude > GetUnitData.AttackDistanceMax * GetUnitData.AttackDistanceMax)
            return false;

        return true;
    }

    // Attack Task
    public void StartAttacking(BaseEntity target)
    {
        EntityTarget = target;
    }
    public void ComputeAttack()
    {
        if (CanAttack(EntityTarget) == false)
            return;

        if (NavMeshAgent)
            NavMeshAgent.isStopped = true;

        transform.LookAt(EntityTarget.transform);
        // only keep Y axis
        Vector3 eulerRotation = transform.eulerAngles;
        eulerRotation.x = 0f;
        eulerRotation.z = 0f;
        transform.eulerAngles = eulerRotation;

        if ((Time.time - LastActionDate) > UnitData.AttackFrequency)
        {
            LastActionDate = Time.time;
            // visual only ?
            if (UnitData.BulletPrefab)
            {
                GameObject newBullet = Instantiate(UnitData.BulletPrefab, BulletSlot);
                newBullet.transform.parent = null;
                newBullet.GetComponent<Bullet>().ShootToward(EntityTarget.transform.position - transform.position, this);
            }
            // apply damages
            int damages = Mathf.FloorToInt(UnitData.DPS * UnitData.AttackFrequency);
            EntityTarget.AddDamage(damages);
        }
    }
    public bool CanCapture(TargetBuilding target)
    {
        if (target == null)
            return false;

        // distance check
        if ((target.transform.position - transform.position).magnitude > GetUnitData.CaptureDistanceMax)
            return false;

        return true;
    }

    // Capture Task
    public void StartCapture(TargetBuilding target)
    {
        if (CanCapture(target) == false)
            return;

        if (NavMeshAgent)
            NavMeshAgent.isStopped = true;

        CaptureTarget = target;
        CaptureTarget.StartCapture(this);
    }
    public void StopCapture()
    {
        if (CaptureTarget == null)
            return;

        CaptureTarget.StopCapture(this);
        CaptureTarget = null;
    }

    public bool IsCapturing()
    {
        return CaptureTarget != null;
    }

    public bool IsCapturing(TargetBuilding target)
    {
        return CaptureTarget == target;
    }

    // Repairing Task
    public bool CanRepair(BaseEntity target)
    {
        if (GetUnitData.CanRepair == false || target == null)
            return false;

        // distance check
        if ((target.transform.position - transform.position).sqrMagnitude > GetUnitData.RepairDistanceMax * GetUnitData.RepairDistanceMax)
            return false;

        return true;
    }
    public void StartRepairing(BaseEntity entity)
    {
        if (GetUnitData.CanRepair)
        {
            EntityTarget = entity;
        }
    }

    // $$$ TODO : add repairing visual feedback
    public void ComputeRepairing()
    {
        if (CanRepair(EntityTarget) == false)
            return;

        if (NavMeshAgent)
            NavMeshAgent.isStopped = true;

        transform.LookAt(EntityTarget.transform);
        // only keep Y axis
        Vector3 eulerRotation = transform.eulerAngles;
        eulerRotation.x = 0f;
        eulerRotation.z = 0f;
        transform.eulerAngles = eulerRotation;

        if ((Time.time - LastActionDate) > UnitData.RepairFrequency)
        {
            LastActionDate = Time.time;

            // apply reparing
            int amount = Mathf.FloorToInt(UnitData.RPS * UnitData.RepairFrequency);
            EntityTarget.Repair(amount);
        }
    }
    #endregion
}
