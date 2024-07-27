using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RGDCP1.Level
{
    public class CameraController : MonoBehaviour
    {
        private float z;
        private Vector2 velocity = new Vector2(0, 0);
        private Vector3 target;  
        
        [SerializeField]
        private float smoothTime = 0.4f;
        
        // HACK: This should be in a seperate player manager.
        List<GameObject> players;

        public void OnPlayerJoin(PlayerInput player)
        {
            players.Add(player.gameObject);
        }

        public void OnPlayerLeave(PlayerInput player)
        {
            players.Remove(player.gameObject);
        }


        // Start is called before the first frame update
        void Start()
        {
            target = transform.position;
            players = new List<GameObject>();
            z = transform.position.z;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            // Update target positon
            UpdateTarget();

            Vector2 pos = Vector2.SmoothDamp(transform.position, target, ref velocity, smoothTime);
            transform.position = new Vector3(pos.x, pos.y, z);
        }

        private void UpdateTarget()
        {
            if (players.Count == 0) return;

            target = new Vector3(0, 0, 0);

            foreach (GameObject p in players)
            {
                target += p.transform.position;
            }

            // TODO: Possible race condition when a player is added or removed before this happens?
            target /= players.Count;

        }
    }
}
