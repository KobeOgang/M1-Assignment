using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CannonController : MonoBehaviour
{
    [Header("References")]
    public GameObject projectilePrefab;
    public Transform launchPoint;
    public TextMeshProUGUI distanceText;
    public Camera mainCamera;

    [Header("Launch Settings")]
    public float acceleration = 10f;
    public float initialVelocity = 5f;

    private Vector3 cameraOriginalPosition;
    private Quaternion cameraOriginalRotation;
    private bool isProjectileInAir = false;
    private bool canShootAgain = true;

    private void Start()
    {
        cameraOriginalPosition = mainCamera.transform.position;
        cameraOriginalRotation = mainCamera.transform.rotation;
    }

    private void Update()
    {
        if (isProjectileInAir || !canShootAgain)
            return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            LaunchProjectile();
        }
    }

    private void LaunchProjectile()
    {
        isProjectileInAir = true;
        canShootAgain = false;

        GameObject projectile = Instantiate(projectilePrefab, launchPoint.position, launchPoint.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        Vector3 launchDirection = launchPoint.forward;
        float time = 2f; 
        Vector3 initialVelocityVector = launchDirection * (0.5f * acceleration * time + initialVelocity * time);
        rb.velocity = initialVelocityVector;

        
        StartCoroutine(TrackDistance(projectile));

        StartCoroutine(CameraFollow(projectile));
    }

    private System.Collections.IEnumerator TrackDistance(GameObject projectile)
    {
        Vector3 startPosition = projectile.transform.position;
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        bool hasHitGround = false;

        while (!hasHitGround && projectile != null)
        {
            yield return null;

            if (Physics.Raycast(projectile.transform.position, Vector3.down, out RaycastHit hit, 0.5f))
            {
                if (hit.collider.CompareTag("Ground"))
                {
                    hasHitGround = true;
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }

        if (hasHitGround && projectile != null)
        {
            float distance = Vector3.Distance(startPosition, projectile.transform.position);
            distanceText.text = "Distance: " + distance.ToString("F2") + "m";

            Destroy(projectile, 2f);
        }

        StartCoroutine(ResetShootAvailability());
    }

    private System.Collections.IEnumerator CameraFollow(GameObject projectile)
    {
        while (projectile != null)
        {
            Vector3 targetPosition = projectile.transform.position;
            Vector3 smoothPosition = Vector3.Lerp(mainCamera.transform.position, targetPosition + new Vector3(0, 3f, -5f), Time.deltaTime * 5f);
            mainCamera.transform.position = smoothPosition;

            mainCamera.transform.LookAt(projectile.transform.position);

            yield return null;
        }
        mainCamera.transform.position = cameraOriginalPosition;
        mainCamera.transform.rotation = cameraOriginalRotation;
    }

    private System.Collections.IEnumerator ResetShootAvailability()
    {
        yield return new WaitForSeconds(2f);
        isProjectileInAir = false;
        canShootAgain = true;
    }
}
