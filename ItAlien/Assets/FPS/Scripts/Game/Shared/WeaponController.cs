using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    public enum WeaponShootType
    {
        Manual,
        Automatic,
        Charge,
    }

    [System.Serializable]
    public struct CrosshairData
    {
        public Sprite CrosshairSprite;
        public int CrosshairSize;
        public Color CrosshairColor;
    }

    [RequireComponent(typeof(AudioSource))]
    public class WeaponController : MonoBehaviour
    {
        [Header("Information")]
        public string WeaponName;
        public Sprite WeaponIcon;
        public CrosshairData CrosshairDataDefault;
        public CrosshairData CrosshairDataTargetInSight;

        [Header("Internal References")]
        public GameObject WeaponRoot;
        public Transform WeaponMuzzle;

        [Header("Shoot Parameters")]
        public WeaponShootType ShootType;
        public ProjectileBase ProjectilePrefab;
        public float DelayBetweenShots = 0.5f;
        public float BulletSpreadAngle = 0f;
        public int BulletsPerShot = 1;
        [Range(0f, 2f)]
        public float RecoilForce = 1;
        [Range(0f, 1f)]
        public float AimZoomRatio = 1f;
        public Vector3 AimOffset;

        [Header("Melee Parameters")]
        public float MeleeDamage = 25f;
        public float MeleeRadius = 1.5f;
        public LayerMask MeleeHitLayer;
        public AudioClip MeleeSwingSound;
        public AudioClip MeleeHitSound;

        [Header("Throwable Weapon Parameters")]
        public float ThrowForce = 30f;
        public float ThrowDamage = 500f;
        public float ThrowTorque = 10f;
        public AudioClip ThrowSound;

        [Header("Ammo Parameters")]
        public bool AutomaticReload = true;
        public bool HasPhysicalBullets = false;
        public int ClipSize = 30;
        public GameObject ShellCasing;
        public Transform EjectionPort;
        [Range(0.0f, 5.0f)]
        public float ShellCasingEjectionForce = 2.0f;
        [Range(1, 30)]
        public int ShellPoolSize = 1;
        public float AmmoReloadRate = 1f;
        public float AmmoReloadDelay = 2f;
        public int MaxAmmo = 8;

        [Header("Charging parameters (charging weapons only)")]
        public bool AutomaticReleaseOnCharged;
        public float MaxChargeDuration = 2f;
        public float AmmoUsedOnStartCharge = 1f;
        public float AmmoUsageRateWhileCharging = 1f;

        [Header("Audio & Visual")]
        public Animator WeaponAnimator;
        public GameObject MuzzleFlashPrefab;
        public bool UnparentMuzzleFlash;
        public AudioClip ShootSfx;
        public AudioClip ChangeWeaponSfx;
        public bool UseContinuousShootSound = false;
        public AudioClip ContinuousShootStartSfx;
        public AudioClip ContinuousShootLoopSfx;
        public AudioClip ContinuousShootEndSfx;
        AudioSource m_ContinuousShootAudioSource = null;
        bool m_WantsToShoot = false;

        public UnityAction OnShoot;
        public event Action OnShootProcessed;

        int m_CarriedPhysicalBullets;
        float m_CurrentAmmo;
        float m_LastTimeShot = Mathf.NegativeInfinity;
        public float LastChargeTriggerTimestamp { get; private set; }
        Vector3 m_LastMuzzlePosition;

        public GameObject Owner { get; set; }
        public GameObject SourcePrefab { get; set; }
        public bool IsCharging { get; private set; }
        public float CurrentAmmoRatio { get; private set; }
        public bool IsWeaponActive { get; private set; }
        public bool IsCooling { get; private set; }
        public float CurrentCharge { get; private set; }
        public Vector3 MuzzleWorldVelocity { get; private set; }

        public float GetAmmoNeededToShoot() =>
            (ShootType != WeaponShootType.Charge ? 1f : Mathf.Max(1f, AmmoUsedOnStartCharge)) /
            (MaxAmmo * BulletsPerShot);

        public int GetCarriedPhysicalBullets() => m_CarriedPhysicalBullets;
        public int GetCurrentAmmo() => Mathf.FloorToInt(m_CurrentAmmo);

        AudioSource m_ShootAudioSource;

        public bool IsReloading { get; private set; }

        const string k_AnimAttackParameter = "Attack";
        const string k_AnimMeleeAttackParameter = "MeleeAttack";

        private Queue<Rigidbody> m_PhysicalAmmoPool;
        private bool isMeleeAttacking = false;

        void Awake()
        {
            m_CurrentAmmo = MaxAmmo;
            m_CarriedPhysicalBullets = HasPhysicalBullets ? ClipSize : 0;
            m_LastMuzzlePosition = WeaponMuzzle.position;

            m_ShootAudioSource = GetComponent<AudioSource>();
            DebugUtility.HandleErrorIfNullGetComponent<AudioSource, WeaponController>(m_ShootAudioSource, this,
                gameObject);

            if (UseContinuousShootSound)
            {
                m_ContinuousShootAudioSource = gameObject.AddComponent<AudioSource>();
                m_ContinuousShootAudioSource.playOnAwake = false;
                m_ContinuousShootAudioSource.clip = ContinuousShootLoopSfx;
                m_ContinuousShootAudioSource.outputAudioMixerGroup =
                    AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.WeaponShoot);
                m_ContinuousShootAudioSource.loop = true;
            }

            if (HasPhysicalBullets)
            {
                m_PhysicalAmmoPool = new Queue<Rigidbody>(ShellPoolSize);

                for (int i = 0; i < ShellPoolSize; i++)
                {
                    GameObject shell = Instantiate(ShellCasing, transform);
                    shell.SetActive(false);
                    m_PhysicalAmmoPool.Enqueue(shell.GetComponent<Rigidbody>());
                }
            }
        }

        public void AddCarriablePhysicalBullets(int count) => m_CarriedPhysicalBullets = Mathf.Max(m_CarriedPhysicalBullets + count, MaxAmmo);

        void ShootShell()
        {
            Rigidbody nextShell = m_PhysicalAmmoPool.Dequeue();

            nextShell.transform.position = EjectionPort.transform.position;
            nextShell.transform.rotation = EjectionPort.transform.rotation;
            nextShell.gameObject.SetActive(true);
            nextShell.transform.SetParent(null);
            nextShell.collisionDetectionMode = CollisionDetectionMode.Continuous;
            nextShell.AddForce(nextShell.transform.up * ShellCasingEjectionForce, ForceMode.Impulse);

            m_PhysicalAmmoPool.Enqueue(nextShell);
        }

        void PlaySFX(AudioClip sfx) => AudioUtility.CreateSFX(sfx, transform.position, AudioUtility.AudioGroups.WeaponShoot, 0.0f);

        void Reload()
        {
            if (m_CarriedPhysicalBullets > 0)
            {
                m_CurrentAmmo = Mathf.Min(m_CarriedPhysicalBullets, ClipSize);
            }

            IsReloading = false;
        }

        public void StartReloadAnimation()
        {
            if (m_CurrentAmmo < m_CarriedPhysicalBullets)
            {
                GetComponent<Animator>().SetTrigger("Reload");
                IsReloading = true;
            }
        }

        void Update()
        {
            UpdateAmmo();
            UpdateCharge();
            UpdateContinuousShootSound();

            if (Time.deltaTime > 0)
            {
                MuzzleWorldVelocity = (WeaponMuzzle.position - m_LastMuzzlePosition) / Time.deltaTime;
                m_LastMuzzlePosition = WeaponMuzzle.position;
            }

            if (isMeleeAttacking)
            {
                CheckMeleeHits();
            }
        }

        void CheckMeleeHits()
        {
            Collider[] hits = Physics.OverlapSphere(WeaponMuzzle.position, MeleeRadius, MeleeHitLayer);
            foreach (Collider hit in hits)
            {
                if (hit.gameObject != Owner)
                {
                    Health health = hit.GetComponentInParent<Health>();
                    if (health != null)
                    {
                        health.TakeDamage(MeleeDamage, Owner);

                        if (MeleeHitSound)
                        {
                            m_ShootAudioSource.PlayOneShot(MeleeHitSound);
                        }
                    }
                }
            }
        }

        void UpdateAmmo()
        {
            if (gameObject.CompareTag("melee"))
            {
                m_CurrentAmmo = MaxAmmo;
                CurrentAmmoRatio = 1f;
                return;
            }

            if (AutomaticReload && m_LastTimeShot + AmmoReloadDelay < Time.time && m_CurrentAmmo < MaxAmmo && !IsCharging)
            {
                m_CurrentAmmo += AmmoReloadRate * Time.deltaTime;
                m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo, 0, MaxAmmo);
                IsCooling = true;
            }
            else
            {
                IsCooling = false;
            }

            if (MaxAmmo == Mathf.Infinity)
            {
                CurrentAmmoRatio = 1f;
            }
            else
            {
                CurrentAmmoRatio = m_CurrentAmmo / MaxAmmo;
            }
        }

        void UpdateCharge()
        {
            if (IsCharging)
            {
                if (CurrentCharge < 1f)
                {
                    float chargeLeft = 1f - CurrentCharge;
                    float chargeAdded = 0f;
                    if (MaxChargeDuration <= 0f)
                    {
                        chargeAdded = chargeLeft;
                    }
                    else
                    {
                        chargeAdded = (1f / MaxChargeDuration) * Time.deltaTime;
                    }

                    chargeAdded = Mathf.Clamp(chargeAdded, 0f, chargeLeft);
                    float ammoThisChargeWouldRequire = chargeAdded * AmmoUsageRateWhileCharging;
                    if (ammoThisChargeWouldRequire <= m_CurrentAmmo)
                    {
                        UseAmmo(ammoThisChargeWouldRequire);
                        CurrentCharge = Mathf.Clamp01(CurrentCharge + chargeAdded);
                    }
                }
            }
        }

        void UpdateContinuousShootSound()
        {
            if (UseContinuousShootSound)
            {
                if (m_WantsToShoot && m_CurrentAmmo >= 1f)
                {
                    if (!m_ContinuousShootAudioSource.isPlaying)
                    {
                        m_ShootAudioSource.PlayOneShot(ShootSfx);
                        m_ShootAudioSource.PlayOneShot(ContinuousShootStartSfx);
                        m_ContinuousShootAudioSource.Play();
                    }
                }
                else if (m_ContinuousShootAudioSource.isPlaying)
                {
                    m_ShootAudioSource.PlayOneShot(ContinuousShootEndSfx);
                    m_ContinuousShootAudioSource.Stop();
                }
            }
        }

        public void ShowWeapon(bool show)
        {
            WeaponRoot.SetActive(show);

            if (show && ChangeWeaponSfx)
            {
                m_ShootAudioSource.PlayOneShot(ChangeWeaponSfx);
            }

            IsWeaponActive = show;
        }

        public void UseAmmo(float amount)
        {
            m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo - amount, 0f, MaxAmmo);
            m_CarriedPhysicalBullets -= Mathf.RoundToInt(amount);
            m_CarriedPhysicalBullets = Mathf.Clamp(m_CarriedPhysicalBullets, 0, MaxAmmo);
            m_LastTimeShot = Time.time;
        }

        public bool HandleShootInputs(bool inputDown, bool inputHeld, bool inputUp)
        {
            m_WantsToShoot = inputDown || inputHeld;

            if (gameObject.CompareTag("melee") && inputDown)
            {
                return TryMeleeAttack();
            }
            else if (gameObject.CompareTag("long") && inputDown)
            {
                return TryThrowWeapon();
            }
            else if (!gameObject.CompareTag("long"))
            {
                switch (ShootType)
                {
                    case WeaponShootType.Manual:
                        if (inputDown)
                        {
                            return TryShoot();
                        }
                        return false;

                    case WeaponShootType.Automatic:
                        if (inputHeld)
                        {
                            return TryShoot();
                        }
                        return false;

                    case WeaponShootType.Charge:
                        if (inputHeld)
                        {
                            TryBeginCharge();
                        }
                        if (inputUp || (AutomaticReleaseOnCharged && CurrentCharge >= 1f))
                        {
                            return TryReleaseCharge();
                        }
                        return false;

                    default:
                        return false;
                }
            }

            return false;
        }

        bool TryMeleeAttack()
        {
            if (m_LastTimeShot + DelayBetweenShots < Time.time)
            {
                if (WeaponAnimator)
                {
                    WeaponAnimator.SetTrigger(k_AnimMeleeAttackParameter);
                }

                if (MeleeSwingSound)
                {
                    m_ShootAudioSource.PlayOneShot(MeleeSwingSound);
                }

                isMeleeAttacking = true;
                m_LastTimeShot = Time.time;

                Invoke("EndMeleeAttack", 0.5f);

                OnShoot?.Invoke();
                OnShootProcessed?.Invoke();

                return true;
            }

            return false;
        }

        bool TryThrowWeapon()
        {
            if (m_LastTimeShot + DelayBetweenShots < Time.time)
            {
                if (ThrowSound)
                {
                    m_ShootAudioSource.PlayOneShot(ThrowSound);
                }

                ThrowCurrentWeapon();

                OnShoot?.Invoke();
                OnShootProcessed?.Invoke();

                return true;
            }

            return false;
        }

        void ThrowCurrentWeapon()
        {
            GameObject thrownWeapon = Instantiate(gameObject, WeaponMuzzle.position, WeaponMuzzle.rotation);

            // Disabilita il WeaponController invece di distruggerlo
            WeaponController weaponController = thrownWeapon.GetComponent<WeaponController>();
            if (weaponController)
            {
                weaponController.enabled = false;
            }

            if (!thrownWeapon.GetComponent<Collider>())
            {
                thrownWeapon.AddComponent<BoxCollider>();
            }

            Rigidbody rb = thrownWeapon.GetComponent<Rigidbody>();
            if (!rb)
            {
                rb = thrownWeapon.AddComponent<Rigidbody>();
            }

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            Camera playerCamera = Camera.main;
            if (playerCamera)
            {
                rb.AddForce(playerCamera.transform.forward * ThrowForce, ForceMode.Impulse);
                rb.AddTorque(UnityEngine.Random.insideUnitSphere * ThrowTorque, ForceMode.Impulse);
            }

            ThrownWeaponBehaviour throwBehaviour = thrownWeapon.AddComponent<ThrownWeaponBehaviour>();
            throwBehaviour.Damage = ThrowDamage;
            throwBehaviour.Owner = Owner;
            throwBehaviour.HitLayers = MeleeHitLayer; // Usa lo stesso layer delle armi melee

            var components = Owner.GetComponents<MonoBehaviour>();
            foreach (var component in components)
            {
                var method = component.GetType().GetMethod("RemoveWeapon");
                if (method != null && method.GetParameters().Length == 1)
                {
                    method.Invoke(component, new object[] { this });
                    break;
                }
            }

            ShowWeapon(false);

            Destroy(thrownWeapon, 10f);
        }

        void EndMeleeAttack()
        {
            isMeleeAttacking = false;
        }

        bool TryShoot()
        {
            if (m_CurrentAmmo >= 1f && m_LastTimeShot + DelayBetweenShots < Time.time)
            {
                HandleShoot();
                m_CurrentAmmo -= 1f;
                return true;
            }
            return false;
        }

        bool TryBeginCharge()
        {
            if (!IsCharging
                && m_CurrentAmmo >= AmmoUsedOnStartCharge
                && Mathf.FloorToInt((m_CurrentAmmo - AmmoUsedOnStartCharge) * BulletsPerShot) > 0
                && m_LastTimeShot + DelayBetweenShots < Time.time)
            {
                UseAmmo(AmmoUsedOnStartCharge);
                LastChargeTriggerTimestamp = Time.time;
                IsCharging = true;
                return true;
            }
            return false;
        }

        bool TryReleaseCharge()
        {
            if (IsCharging)
            {
                HandleShoot();
                CurrentCharge = 0f;
                IsCharging = false;
                return true;
            }
            return false;
        }

        void HandleShoot()
        {
            int bulletsPerShotFinal = ShootType == WeaponShootType.Charge
                ? Mathf.CeilToInt(CurrentCharge * BulletsPerShot)
                : BulletsPerShot;

            for (int i = 0; i < bulletsPerShotFinal; i++)
            {
                Vector3 shotDirection = GetShotDirectionWithinSpread(WeaponMuzzle);
                ProjectileBase newProjectile = Instantiate(ProjectilePrefab, WeaponMuzzle.position,
                    Quaternion.LookRotation(shotDirection));
                newProjectile.Shoot(this);
            }

            if (MuzzleFlashPrefab != null)
            {
                GameObject muzzleFlashInstance = Instantiate(MuzzleFlashPrefab, WeaponMuzzle.position,
                    WeaponMuzzle.rotation, WeaponMuzzle.transform);
                if (UnparentMuzzleFlash)
                {
                    muzzleFlashInstance.transform.SetParent(null);
                }
                Destroy(muzzleFlashInstance, 2f);
            }

            if (HasPhysicalBullets)
            {
                ShootShell();
                m_CarriedPhysicalBullets--;
            }

            m_LastTimeShot = Time.time;

            if (ShootSfx && !UseContinuousShootSound)
            {
                m_ShootAudioSource.PlayOneShot(ShootSfx);
            }

            if (WeaponAnimator)
            {
                WeaponAnimator.SetTrigger(k_AnimAttackParameter);
            }

            OnShoot?.Invoke();
            OnShootProcessed?.Invoke();
        }

        public Vector3 GetShotDirectionWithinSpread(Transform shootTransform)
        {
            float spreadAngleRatio = BulletSpreadAngle / 180f;
            Vector3 spreadWorldDirection = Vector3.Slerp(shootTransform.forward, UnityEngine.Random.insideUnitSphere,
                spreadAngleRatio);

            return spreadWorldDirection;
        }
    }
}