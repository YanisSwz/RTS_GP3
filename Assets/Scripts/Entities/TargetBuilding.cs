using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TargetBuilding : MonoBehaviour
{
    [SerializeField]
    float CaptureGaugeStart = 100f;
    [SerializeField]
    float CaptureGaugeSpeed = 1f;
    [SerializeField]
    int BuildPoints = 5;
    [SerializeField]
    Material BlueTeamMaterial = null;
    [SerializeField]
    Material RedTeamMaterial = null;

    Material NeutralMaterial = null;
    MeshRenderer BuildingMeshRenderer = null;
    Image GaugeImage;
    Image MinimapImage;

    int[] TeamScore;
    float CaptureGaugeValue;
    ETeam OwningTeam = ETeam.Neutral;
    ETeam CapturingTeam = ETeam.Neutral;
    public ETeam GetTeam() { return OwningTeam; }

    [Header("Menace Point")]
    public float menacePointRadiusDetection = 30;
    public LayerMask menaceDetectionLayer;
    [HideInInspector]
    public List<Unit> allyNearLab = new List<Unit>();
    List<Unit> unitsInSight = new List<Unit>();
    //use for menace point generation
    AIController AIController = null;


    private EntityVisibility _Visibility;
    public EntityVisibility Visibility
    {
        get
        {
            if (_Visibility == null)
            {
                _Visibility = GetComponent<EntityVisibility>();
            }
            return _Visibility;
        }
    }


    #region MonoBehaviour methods
    void Start()
    {
        BuildingMeshRenderer = GetComponentInChildren<MeshRenderer>();
        NeutralMaterial = BuildingMeshRenderer.material;

        GaugeImage = GetComponentInChildren<Image>();
        if (GaugeImage)
            GaugeImage.fillAmount = 0f;
        CaptureGaugeValue = CaptureGaugeStart;
        TeamScore = new int[2];
        TeamScore[0] = 0;
        TeamScore[1] = 0;

        Transform minimapTransform = transform.Find("MinimapCanvas");
        if (minimapTransform != null)
            MinimapImage = minimapTransform.GetComponentInChildren<Image>();
    }
    void Update()
    {
        //generate menace point
        if (AIController != null)
        {
            unitsInSight.Clear();
            allyNearLab.Clear();

            List<RaycastHit> hitUnits = new List<RaycastHit>(Physics.SphereCastAll(transform.position, menacePointRadiusDetection, Vector3.up, menaceDetectionLayer));
            foreach (RaycastHit hit in hitUnits)
            {
                Unit unitInSight = null;
                if (hit.rigidbody && hit.rigidbody.gameObject.TryGetComponent<Unit>(out unitInSight))
                {
                    //get enemies in sight
                    if (unitInSight.GetTeam() != OwningTeam)
                        unitsInSight.Add(unitInSight);
                    else
                        allyNearLab.Add(unitInSight);
                }
            }
            if (unitsInSight.Count > 0)
            {
                MenaceMemory menace = new MenaceMemory();
                menace.time = Time.time;
                menace.nbEnemiesSpotted = unitsInSight.Count;

                menace.enemyPower = 0;
                foreach (Unit unit in unitsInSight)
                    menace.enemyPower += unit.Cost;

                foreach (Unit unit in unitsInSight)
                    menace.enemyAveragePos += unit.transform.position;
                menace.enemyAveragePos /= (float)(menace.nbEnemiesSpotted);

                AIController.AddMenaceMemory(menace);
            }
        }

        if (CapturingTeam == OwningTeam || CapturingTeam == ETeam.Neutral)
            return;


        CaptureGaugeValue -= TeamScore[(int)CapturingTeam] * CaptureGaugeSpeed * Time.deltaTime;

        GaugeImage.fillAmount = 1f - CaptureGaugeValue / CaptureGaugeStart;

        if (CaptureGaugeValue <= 0f)
        {
            CaptureGaugeValue = 0f;
            OnCaptured(CapturingTeam);
        }
    }
    #endregion

    #region Capture methods
    public void StartCapture(Unit unit)
    {
        if (unit == null)
            return;

        TeamScore[(int)unit.GetTeam()] += unit.Cost;

        if (CapturingTeam == ETeam.Neutral)
        {
            if (TeamScore[(int)GameServices.GetOpponent(unit.GetTeam())] == 0)
            {
                CapturingTeam = unit.GetTeam();
                GaugeImage.color = GameServices.GetTeamColor(CapturingTeam);
            }
        }
        else
        {
            if (TeamScore[(int)GameServices.GetOpponent(unit.GetTeam())] > 0)
                ResetCapture();
        }
    }
    public void StopCapture(Unit unit)
    {
        if (unit == null)
            return;

        TeamScore[(int)unit.GetTeam()] -= unit.Cost;
        if (TeamScore[(int)unit.GetTeam()] == 0)
        {
            ETeam opponentTeam = GameServices.GetOpponent(unit.GetTeam());
            if (TeamScore[(int)opponentTeam] == 0)
            {
                ResetCapture();
            }
            else
            {
                CapturingTeam = opponentTeam;
                GaugeImage.color = GameServices.GetTeamColor(CapturingTeam);
            }
        }
    }
    void ResetCapture()
    {
        CaptureGaugeValue = CaptureGaugeStart;
        CapturingTeam = ETeam.Neutral;
        GaugeImage.fillAmount = 0f;
    }
    void OnCaptured(ETeam newTeam)
    {
        Debug.Log("target captured by " + newTeam.ToString());
        
        UnitController teamController = GameServices.GetControllerByTeam(newTeam);
        AIController = teamController as AIController;

        if (OwningTeam != newTeam)
        {
            if (teamController != null)
                teamController.CaptureTarget(BuildPoints, this);

            if (OwningTeam != ETeam.Neutral)
            {
                // remove points to previously owning team
                teamController = GameServices.GetControllerByTeam(OwningTeam);
                if (teamController != null)
                    teamController.LoseTarget(BuildPoints, this);
            }
        }



        ResetCapture();
        OwningTeam = newTeam;
        if (Visibility) { Visibility.Team = OwningTeam; }
        if (MinimapImage) { MinimapImage.color = GameServices.GetTeamColor(OwningTeam); }
        BuildingMeshRenderer.material = newTeam == ETeam.Blue ? BlueTeamMaterial : RedTeamMaterial;
    }
    #endregion
}
