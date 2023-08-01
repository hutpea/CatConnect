using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatConnect
{
    public class RotateObject : MonoBehaviour
    {
        public float speed;
        void Start()
        {
        
        }

        void Update()
        {
            transform.Rotate(Vector3.forward, speed);
        }
    }
}
