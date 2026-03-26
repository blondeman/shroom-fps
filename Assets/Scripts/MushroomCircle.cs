using UnityEngine;

public class MushroomCircle : MonoBehaviour
{
    public int mushroomCount;
    public float radius;
    public GameObject[] mushroomPrefabs;
    public GameObject portal;
    public GameObject portalFrame;

    public int currentMushrooms;
    public AudioClip sfx;

    void Start()
    {
        portal.SetActive(false);
        portalFrame.SetActive(true);

        /*while (currentMushrooms < mushroomCount)
        {
            AddMushroom();
        }*/
    }

    public void AddMushroom()
    {
        float angle = Mathf.PI * 2 * currentMushrooms / mushroomCount;
        Vector3 mushroomPosition = new Vector3(
            Mathf.Cos(angle) * radius,
            0,
            Mathf.Sin(angle) * radius
        );

        GameObject mushroomPrefab = mushroomPrefabs[currentMushrooms];
        Instantiate(mushroomPrefab, transform.position + mushroomPosition, Quaternion.identity, transform);

        currentMushrooms++;

        if (currentMushrooms >= mushroomCount)
        {
            CreatePortal();
        }
    }

    void CreatePortal()
    {
        portal.SetActive(true);
        portalFrame.SetActive(false);
        AudioSource.PlayClipAtPoint(sfx, transform.position);
    }
}
