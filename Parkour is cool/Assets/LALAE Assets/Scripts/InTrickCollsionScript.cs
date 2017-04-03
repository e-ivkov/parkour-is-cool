using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace UnityStandardAssets._2D
{
    public class InTrickCollsionScript : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            PlatformerCharacter2D userControl = GameObject.FindGameObjectWithTag("Player").GetComponent<PlatformerCharacter2D>();
            if (userControl.TrickCollider != null && userControl.TrickCollider != collision)
            {
                userControl.m_fail = true;
            }
        }
    }
}
