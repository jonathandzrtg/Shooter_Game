using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEditor;
using Mirror;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    public bool Player = true;
    public bool Active = true;

    //Personaje
    Transform playerTr;
    Rigidbody playerRb;
    Animator playerAnim;
    RagdollController playerRagdoll;

    public float maxHealth = 100f;
    public float currentHealth;

    public float playerSpeed = 0f;

    public bool hasPistol = false;

    private Vector2 newDirection;

    //Camara
    public Transform cameraAxis;
    public Transform cameraTrack;
    public Transform cameraWeaponTrack;
    private Transform theCamera;

    private float rotY = 0f;
    private float rotX = 0f;

    public float camRotSpeed = 200f;
    public float minAngle = -45f;
    public float maxAngle = 45f;
    public float cameraSpeed = 200f;

    // Items
    public GameObject nearItem;
    public GameObject itemPrefab;
    public Transform itemSlot;
    public GameObject crosshair;

    // Sonidos de pasos
    public AudioClip[] stepSounds;
    public float stepInterval = 0.5f; // Intervalo entre pasos
    private float stepTimer = 0f;
    private AudioSource audioSource; // Componente AudioSource para reproducir los sonidos de los pasos

    // Start is called before the first frame update
    void Start()
    {
        if (isLocalPlayer) {
            playerTr = this.transform;
            playerRb = GetComponent<Rigidbody>();
            playerAnim = GetComponentInChildren<Animator>();
            playerRagdoll = GetComponentInChildren<RagdollController>();

            theCamera = Camera.main.transform;

            // Configurar el componente AudioSource
            audioSource = gameObject.AddComponent<AudioSource>();

            Cursor.lockState = CursorLockMode.Locked;

            currentHealth = maxHealth;
            Active = true;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Player)
        {
            MoveLogic();
            CameraLogic();
        }

        if(!Active) 
        {
            return;
        }
        
        ItemLogic();
        AnimLogic();

        if(Input.GetKeyDown(KeyCode.Y)) 
        {
            TakeDamage(10f);
        }
    }

    public void MoveLogic()
    {
        if (!playerRb.isKinematic)
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");
            float theTime = Time.deltaTime;

            newDirection = new Vector2(moveX, moveZ);

            Vector3 side = playerSpeed * moveX * theTime * playerTr.right;
            Vector3 forward = playerSpeed * moveZ * theTime * playerTr.forward;

            Vector3 endDirection = side + forward;

            playerRb.velocity = endDirection;

            // Control de sonido de pasos
            if (newDirection.magnitude > 0 && IsGrounded() && Time.time > stepTimer)
            {
                // Reproducir sonido de paso aleatorio
                AudioClip stepSound = stepSounds[Random.Range(0, stepSounds.Length)];
                audioSource.PlayOneShot(stepSound);

                // Reiniciar el temporizador de pasos
                stepTimer = Time.time + stepInterval;
            }
        }
        else
        {
            // Aquí puedes agregar lógica para mover el personaje cuando está muerto
            // Por ejemplo, puedes hacer que el personaje caiga hacia abajo o se mueva en una dirección específica.
            // playerRb.velocity = new Vector3(0, -1, 0); // Ejemplo: hacer que el personaje caiga hacia abajo
        }
    }

    //public void MoveLogic()
    //{
    //    float moveX = Input.GetAxis("Horizontal");
    //    float moveZ = Input.GetAxis("Vertical");
    //    float theTime = Time.deltaTime;

    //    newDirection = new Vector2(moveX, moveZ);

    //    Vector3 side = playerSpeed * moveX * theTime * playerTr.right;
    //    Vector3 forward = playerSpeed * moveZ * theTime * playerTr.forward;

    //    Vector3 endDirection = side + forward;

    //    playerRb.velocity = endDirection;

    //    // Control de sonido de pasos
    //    if (newDirection.magnitude > 0 && IsGrounded() && Time.time > stepTimer)
    //    {
    //        // Reproducir sonido de paso aleatorio
    //        AudioClip stepSound = stepSounds[Random.Range(0, stepSounds.Length)];
    //        audioSource.PlayOneShot(stepSound);

    //        // Reiniciar el temporizador de pasos
    //        stepTimer = Time.time + stepInterval;
    //    }
    //}

    public void CameraLogic()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        float theTime = Time.deltaTime;

        rotY += mouseY * theTime * camRotSpeed;
        rotX = mouseX * theTime * camRotSpeed;

        playerTr.Rotate(0, rotX, 0);

        rotY = Mathf.Clamp(rotY, minAngle, maxAngle);

        Quaternion localRotation = Quaternion.Euler(-rotY, 0, 0);
        cameraAxis.localRotation = localRotation;

        if (hasPistol)
        {
            cameraTrack.gameObject.SetActive(false);
            cameraWeaponTrack.gameObject.SetActive(true);

            crosshair.gameObject.SetActive(true);

            theCamera.position = Vector3.Lerp(theCamera.position, cameraWeaponTrack.position, cameraSpeed * theTime);
            theCamera.rotation = Quaternion.Lerp(theCamera.rotation, cameraWeaponTrack.rotation, cameraSpeed * theTime);
        }
        else 
        {
            cameraTrack.gameObject.SetActive(true);
            cameraWeaponTrack.gameObject.SetActive(false);

            theCamera.position = Vector3.Lerp(theCamera.position, cameraTrack.position, cameraSpeed * theTime);
            theCamera.rotation = Quaternion.Lerp(theCamera.rotation, cameraTrack.rotation, cameraSpeed * theTime);
        }

    }

    public void AnimLogic()
    {
        playerAnim.SetFloat("X", newDirection.x);
        playerAnim.SetFloat("Y", newDirection.y);

        playerAnim.SetBool("holdPistol", hasPistol);

        if (hasPistol)
        {
            playerAnim.SetLayerWeight(1, 1);
        }
    }

    // Verificar si el jugador está en el suelo
    private bool IsGrounded()
    {
        // Implementa la lógica para verificar si el jugador está en el suelo
        // Por ejemplo, puedes utilizar un Raycast hacia abajo desde el jugador
        // y verificar si golpea una superficie sólida.
        // Aquí hay un ejemplo simple:

        RaycastHit hit;
        float distanceToGround = 0.1f; // Ajusta esta distancia según el tamaño de tu personaje

        if (Physics.Raycast(transform.position, -Vector3.up, out hit, distanceToGround))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ItemLogic()
    {
        if (nearItem != null && Input.GetKeyDown(KeyCode.E)) 
        { 
            GameObject instantiatedItem = Instantiate(itemPrefab, itemSlot.position, itemSlot.rotation);

            Destroy(nearItem.gameObject);

            instantiatedItem.transform.parent = itemSlot;

            hasPistol = true;

            nearItem = null;
        }
    }

    public void TakeDamage(float damage) 
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Debug.Log("Moriste");
            playerRagdoll.Active(true);
            Active = false;
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Item")) 
        {
            Debug.Log("Objeto cerca");
            nearItem = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            Debug.Log("No hay objeto cerca");
            nearItem = null;
        }
    }
}
