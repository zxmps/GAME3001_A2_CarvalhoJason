using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public Tilemap tilemap;
    public AudioClip CheerSound; 
    private AudioSource audioSource;
    private Queue<Vector3> pathQueue = new Queue<Vector3>();
    private bool isMoving = false;
    private Vector3 endTilePosition;

    void Start()
    {
        if (tilemap == null)
        {
            tilemap = FindObjectOfType<Tilemap>();
            Debug.Log("Tilemap was automatically assigned: " + tilemap);
        }

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null) 
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.LogWarning("No AudioSource found on Player! Automatically added one.");
        }
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M) && pathQueue.Count > 0 && !isMoving)
        {
            StartCoroutine(FollowPath());
        }
    }

    public void SetPath(List<Node> path)
    {
        pathQueue.Clear();
        foreach (Node node in path)
        {
            Vector3 worldPos = tilemap.GetCellCenterWorld(node.position);
            pathQueue.Enqueue(worldPos);
        }

        if (path.Count > 0)
        {
            endTilePosition = tilemap.GetCellCenterWorld(path[path.Count - 1].position); 
        }
    }

    IEnumerator FollowPath()
    {
        isMoving = true;

        while (pathQueue.Count > 0)
        {
            Vector3 nextPosition = pathQueue.Dequeue();
            LookWhereYouAreGoing(nextPosition - transform.position);

            while (Vector3.Distance(transform.position, nextPosition) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, nextPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
        }

        isMoving = false;

        if (CheerSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(CheerSound);
            Debug.Log(" Player finished moving! Playing sound.");
        }
        else
        {
            Debug.LogError("udioSource or CheerSound is missing!");
        }
    }


    void LookWhereYouAreGoing(Vector3 direction)
{
    direction = new Vector3(
        Mathf.Round(direction.x), Mathf.Round(direction.y), 0);

    if (direction.x > 0) 
    {
        transform.rotation = Quaternion.Euler(0, 0, 0); 
    }
    else if (direction.x < 0) 
    {
        transform.rotation = Quaternion.Euler(0, 180, 0); 
    }
    else if (direction.y > 0) 
    {
        transform.rotation = Quaternion.Euler(0, 0, 90); 
    }
    else if (direction.y < 0) 
    {
        transform.rotation = Quaternion.Euler(0, 0, -90);
    }
}



    public void StartMoving()
    {
        if (pathQueue.Count > 0 && !isMoving)
        {
            StartCoroutine(FollowPath());
        }
    }
}
