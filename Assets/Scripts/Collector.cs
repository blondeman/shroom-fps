using UnityEngine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Collector : MonoBehaviour
{
    public MushroomCircle mc;
    int points = 0;

    public float interactDistance;

    public TextMeshPro interactUi;
    public TextMeshProUGUI textUi;
    public AudioClip[] PickupAudioClips;

    void Start()
    {
        SetScore();
        interactUi.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, mc.transform.position) <= interactDistance)
        {
            interactUi.gameObject.SetActive(true);
            if(mc.currentMushrooms >= 7) {
                interactUi.text = "Press E to enter";

                if (Input.GetKeyDown(KeyCode.E))
                {
                    NextLevel();
                }
            }else if(points <= mc.currentMushrooms) {
                interactUi.text = "Bring me more mushrooms";
            }else {
                interactUi.text = "Press E to place mushroom";

                if (Input.GetKeyDown(KeyCode.E))
                {
                    mc.AddMushroom();
                }
            }
        }else {
            interactUi.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(other.tag == "Pickup")
        {
            points ++;
            GameObject.Destroy(other.gameObject);
            SetScore();

            //AudioSource.PlayClipAtPoint(PickupAudioClips[Random.Range(0, PickupAudioClips.Length-1)], transform.position, 1);
        }
    }

    private void SetScore()
    {
        textUi.text = "Shrooms: " + points.ToString() + "/7";
    }

    private void NextLevel()
    {
        int sceneToLoad = SceneManager.GetActiveScene().buildIndex+1;

        if (sceneToLoad >= SceneManager.sceneCountInBuildSettings)
        {
            sceneToLoad = 0;
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}
