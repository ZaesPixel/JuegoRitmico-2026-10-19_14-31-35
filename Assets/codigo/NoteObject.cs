using UnityEngine;

public class NoteObject : MonoBehaviour
{
    public float beatOfThisNote;
    private Conductor conductor;
    public Vector3 startPos;
    public Vector3 endPos;

    public void Initialize(float beat, Conductor cond, Vector3 start, Vector3 end)
    {
        beatOfThisNote = beat;
        conductor = cond;
        startPos = start;
        endPos = end;
        transform.position = startPos;
    }

    void Update()
    {
        float beatsFromHit = beatOfThisNote - conductor.songPositionInBeats;
        float interpolation = 1f - (beatsFromHit / conductor.beatsAdvance);

        transform.position = Vector3.Lerp(startPos, endPos, interpolation);

        if (interpolation > 1.2f)
        {
            Destroy(gameObject);
        }
    }
}
