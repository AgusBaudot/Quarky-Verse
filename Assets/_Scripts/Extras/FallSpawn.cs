using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallSpawn : MonoBehaviour
{
    [Header("Configuración de Teleport")]
    [SerializeField, Tooltip("Arrastra aquí el Empty que marca el punto de Spawn")]
    private Transform _spawnPoint;

    [SerializeField, Tooltip("Tag que tiene el objeto del Jugador")]
    private string _playerTag = "Player";

    
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag(_playerTag))
        {
            TeleportPlayer(other.gameObject);
        }
    }

    private void TeleportPlayer(GameObject player)
    {
        if (_spawnPoint == null)
        {
            return;
        }

        
        CharacterController cc = player.GetComponent<CharacterController>();
        
        if (cc != null) cc.enabled = false;

        player.transform.position = _spawnPoint.position;
        player.transform.rotation = _spawnPoint.rotation;

        if (cc != null) cc.enabled = true;

        
    }
}
