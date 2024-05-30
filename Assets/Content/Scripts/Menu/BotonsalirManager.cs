using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BotonSalirManager : MonoBehaviour

{
    public void BotonSalir()
    {
        SceneManager.LoadScene("Main menu");
    }
    
}