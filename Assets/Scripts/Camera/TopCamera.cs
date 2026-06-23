using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class TopCamera : MonoBehaviour
{
    [SerializeField]
    int MoveSpeed = 5;
    [SerializeField]
    int KeyboardSpeedModifier = 20;
    [SerializeField]
    int ZoomSpeed = 100;
    [SerializeField]
    int MinHeight = 5;
    [SerializeField]
    int MaxHeight = 100;
    [SerializeField]
    AnimationCurve MoveSpeedFromZoomCurve = new AnimationCurve();
    [SerializeField]
    float TerrainBorder = 100f;
    [SerializeField, Tooltip("Set to false for debug camera movement")]
    bool EnableMoveLimits = true;

    private float ZoomSpeedModificator = 1f;
    Vector3 Move = Vector3.zero;
    Vector3 TerrainSize = Vector3.zero;

    #region Camera movement methods
    public void Zoom(float value)
    {
        if (value < 0f)
        {
            Move.y += ZoomSpeed * Time.deltaTime;
        }
        else if (value > 0f)
        {
            Move.y -= ZoomSpeed * Time.deltaTime;
        }
    }
    float ComputeZoomSpeedModifier()
    {
        float zoomRatio = Mathf.Clamp(1f - (MaxHeight - transform.position.y) / (MaxHeight - MinHeight), 0f, 1f);
        float zoomSpeedModifier = MoveSpeedFromZoomCurve.Evaluate(zoomRatio);
        //Debug.Log("zoomSpeedModifier " + zoomSpeedModifier);

        ZoomSpeedModificator = zoomSpeedModifier * Time.deltaTime;
        return zoomSpeedModifier;
    }
    public void MouseMove(Vector2 move)
    {
        if (Mathf.Approximately(move.sqrMagnitude, 0f))
            return;

       // MoveFunc(move);
    }

    public void MoveFunc(Vector2 dir)
    {
        dir *= MoveSpeed;
        Move.x = dir.x;
        Move.z = dir.y;
    }


    // Direct focus on one entity (no smooth)
    public void FocusEntity(BaseEntity entity)
    {
        if (entity == null)
            return;

        Vector3 newPos = entity.transform.position;
        newPos.y = transform.position.y;

        transform.position = newPos;
    }

    #endregion

    #region MonoBehaviour methods
    void Start()
    {
        TerrainSize = GameServices.GetTerrainSize();
        ComputeZoomSpeedModifier();
    }

    void Update()
    {
        if (Move != Vector3.zero)
        {
            transform.position += new Vector3(Move.x * ZoomSpeedModificator, Move.y, Move.z * ZoomSpeedModificator);
            if (EnableMoveLimits)
            {
                // Clamp camera position (max height, terrain bounds)
                Vector3 newPos = transform.position;
                newPos.x = Mathf.Clamp(transform.position.x, TerrainBorder, TerrainSize.x - TerrainBorder);
                newPos.y = Mathf.Clamp(transform.position.y, MinHeight, MaxHeight);
                newPos.z = Mathf.Clamp(transform.position.z, TerrainBorder, TerrainSize.z - TerrainBorder);
                transform.position = newPos;
            }
        }

        Move.y = 0f;
    }
    #endregion
}
