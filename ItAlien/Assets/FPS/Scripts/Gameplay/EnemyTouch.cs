using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    public class EnemyTouch : MonoBehaviour
    {

        [Header("Parameters")]
        [Tooltip("Amount of health to Inflict at The player")]
        public float DamageAmount;
        [Tooltip("Amount of health to Inflict itself")]
        public bool KillItself = true;
        [Tooltip("Amount of health to Inflict itself")]
        public float selfDamageAmount;
        [Tooltip("Amount of health to Inflict itself")]
        public float DamageCooldown = 1f;

        float cooldown = 0;
        Health m_Health;
        Collider m_Collider;

        protected void Start()
        {
            m_Health = gameObject.GetComponentInParent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, EnemyTouch>(m_Health, this, gameObject);

            m_Collider = GetComponent<Collider>();
            DebugUtility.HandleErrorIfNullGetComponent<Collider, Pickup>(m_Collider, this, gameObject);
            m_Collider.isTrigger = true;


            cooldown = 0.0f;
        }

        protected void Update()
        {
            if (cooldown > 0.0f)
            {
                cooldown -= Time.deltaTime;
            }
            return;
        }

        protected void MeeleDamage(PlayerCharacterController player)
        {
            Damageable playerHealth = player.GetComponent<Damageable>();

            if (playerHealth  /*&& playerHealth.CanPickup()*/)
            {

                playerHealth.InflictDamage(DamageAmount,false,gameObject.transform.parent.gameObject);

                if (KillItself )
                {
                    m_Health.Kill();
                }
                else
                {
                    m_Health.TakeDamage(selfDamageAmount, player.gameObject);
                }
                cooldown = DamageCooldown;
            }
        }

        void OnTriggerStay(Collider other)
        {
            PlayerCharacterController pickingPlayer = other.GetComponent<PlayerCharacterController>();

            if (pickingPlayer != null && cooldown <= 0.0f)
            {
                MeeleDamage(pickingPlayer);
            }
        }

    }
}
