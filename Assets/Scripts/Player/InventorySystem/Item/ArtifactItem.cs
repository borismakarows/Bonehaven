using UnityEngine;

[CreateAssetMenu(fileName = "Artifact", menuName = "Item/Artifact")]
public class ArtifactItem : Item
{
    public override ItemTypes Type => ItemTypes.Artifact;
    public string description;
}
