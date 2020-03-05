using UnityEngine;

[DisallowMultipleComponent]
public class PersistableObject : MonoBehaviour
{
    /*
     * This class is to be used on objects to be saved in binary and should be extended to save things such as:
     * Custom components
     * component values
     * etc
     */

    public virtual void Save(GameDataWriter writer)
    {
        writer.Write(transform.localPosition);
        writer.Write(transform.localRotation);
        writer.Write(transform.localScale);
    }

    public virtual void Load(GameDataReader reader)
    {
        transform.localPosition = reader.ReadVector3();
        transform.localRotation = reader.ReadQuaternion();
        transform.localScale = reader.ReadVector3();
    }
}
