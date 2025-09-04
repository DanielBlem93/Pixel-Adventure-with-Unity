using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Destination
{
    public Transform point;
    [Tooltip("Sekunden, die die Plattform am Ziel wartet.")]
    public float waitTime = 2f;
    [Tooltip("Event, das ausgelöst wird, wenn die Plattform dieses Ziel erreicht.")]
    public UnityEvent onArrive;

}

public class PlatformMover : MonoBehaviour
{
   
    [SerializeField] private List<Destination> destinations = new List<Destination>();

    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool loop = true; // true = loop; false = ping-pong
    [SerializeField] private float arriveThreshold = 0.05f;

    [Header("Global Events")]
    [SerializeField] private UnityEvent<int> onArriveAtIndex; // liefert index des erreichten Ziels

    private int currentIndex = 0;
    private int direction = 1;
    private Coroutine moveCoroutine;

    private void Start()
    {
        // Starte auf dem ersten Ziel
        if (destinations != null && destinations.Count > 0 && destinations[0].point != null)
        {
            transform.position = destinations[0].point.position;
        }
        moveCoroutine = StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        while (true)
        {
            if (destinations == null || destinations.Count == 0)
            {
                yield return null;
                continue;
            }

            // valid target?
            Destination dest = destinations[currentIndex];
            if (dest == null || dest.point == null)
            {
                // springe zum nächsten (oder warte kurz)
                AdvanceIndex();
                yield return null;
                continue;
            }

            yield return MoveToDest(dest);
            ArriveOnDest(dest);
            yield return WaitAtDest(dest);
            AdvanceIndex(); //next index
            yield return null;
        }
    }
    IEnumerator MoveToDest(Destination dest)
    {
        // Bewegung zum Ziel
        while (Vector3.Distance(transform.position, dest.point.position) > arriveThreshold)
        {
            transform.position = Vector3.MoveTowards(transform.position, dest.point.position, speed * Time.deltaTime);
            yield return null;
        }
    }

    void ArriveOnDest(Destination dest)
    {
        dest.onArrive?.Invoke();
        onArriveAtIndex?.Invoke(currentIndex);
    }

    IEnumerator WaitAtDest(Destination dest)
    {
        float wait = Mathf.Max(0f, dest.waitTime);
        float t = 0f;
        while (t < wait)
        {
            t += Time.deltaTime;
            yield return null;
        }
    }

    private void AdvanceIndex()
    {
        if (destinations == null || destinations.Count <= 1) return;

        if (loop)
        {
            currentIndex = (currentIndex + 1) % destinations.Count;
        }
        else // ping-pong
        {
            if (currentIndex == destinations.Count - 1) direction = -1;
            else if (currentIndex == 0) direction = 1;
            currentIndex += direction;
            // safety clamp
            currentIndex = Mathf.Clamp(currentIndex, 0, destinations.Count - 1);
        }
    }

    // Editor / andere Skripte können diese Methoden verwenden
    public void AddDestination(Transform t, float waitTime = 0f)
    {
        if (t == null) return;
        Destination d = new Destination { point = t, waitTime = waitTime };
        destinations.Add(d);
    }

    public void InsertDestination(int index, Transform t, float waitTime = 0f)
    {
        if (t == null) return;
        index = Mathf.Clamp(index, 0, destinations.Count);
        Destination d = new Destination { point = t, waitTime = waitTime };
        destinations.Insert(index, d);
    }

    public void RemoveDestinationAt(int index)
    {
        if (index < 0 || index >= destinations.Count) return;
        destinations.RemoveAt(index);
    }

    public IReadOnlyList<Destination> GetDestinations() => destinations;

    // Visualisierung im Scene View
    private void OnDrawGizmos()
    {
        if (destinations == null || destinations.Count == 0) return;

        for (int i = 0; i < destinations.Count; i++)
        {
            var d = destinations[i];
            if (d == null || d.point == null) continue;
            Gizmos.DrawWireSphere(d.point.position, 0.2f);

            if (i + 1 < destinations.Count && destinations[i + 1] != null && destinations[i + 1].point != null)
                Gizmos.DrawLine(d.point.position, destinations[i + 1].point.position);

            if (i == 0 && Application.isPlaying == false)
                Gizmos.DrawLine(transform.position, d.point.position);
        }
    }
}
