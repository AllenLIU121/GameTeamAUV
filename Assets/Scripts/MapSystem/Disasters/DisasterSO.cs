using UnityEngine;

public enum DisasterTriggerZone { MapWide, SeaSide, MountainSide }
public enum SafeZoneCondition { InNode, AwayFromNode, InShelter }

[CreateAssetMenu(fileName = "New Disaster", menuName = "Map/Disaster")]
public class DisasterSO : ScriptableObject
{
    [Header("灾害信息")]
    public string disasterName;
    public DisasterTriggerZone triggerZone;

    [Header("安全区条件")]
    public SafeZoneCondition safeCondition;

    [Tooltip("当安全条件是'InNode'时, 需指定该类型")]
    public NodeType requiredNodeType;
    [Tooltip("当安全条件是'AwayFromNode'时, 需指定该类型")]
    public NodeType awayFromNodeType;
}
