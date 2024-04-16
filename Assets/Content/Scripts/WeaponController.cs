using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Transform shootSpawn;

    public bool shooting = false;

    public GameObject bulletPrefab;

    public AudioClip shootSound; // El audio que se reproducirá al disparar

    private AudioSource audioSource; // Componente AudioSource para reproducir el sonido

    // Start is called before the first frame update
    void Start()
    {
        // Obtener el componente AudioSource del GameObject
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(shootSpawn.position, shootSpawn.forward * 10f, Color.red);
        Debug.DrawLine(Camera.main.transform.position, Camera.main.transform.forward * 10f, Color.red);

        RaycastHit cameraHit;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out cameraHit))
        {
            Vector3 shootDirection = cameraHit.point - shootSpawn.position;
            shootSpawn.rotation = Quaternion.LookRotation(shootDirection);

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Shoot();
            }
        }
    }

    public void Shoot()
    {
        // Instanciar la bala
        Instantiate(bulletPrefab, shootSpawn.position, shootSpawn.rotation);

        // Reproducir el sonido de disparo
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
    }
}
