using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class SkyManager : MonoBehaviour
    {
        [Header("Parameters")]
        [Tooltip("Material Assign to Skybox on Awake")]
        public Material skyboxMaterial;
        [Tooltip("Towards Tint")]
        public Color worstSkyTint;
        [Tooltip("Exposure degrade")]
        public float exposureDegrade = 0.008f;
        [Tooltip("Time for Sky Transiction ")]
        public float timeTransiction=1.5f;

        Material currentSkyboxMaterial;
        // Start is called before the first frame update
        void Awake()
        {
            currentSkyboxMaterial = new Material(skyboxMaterial.shader);
            currentSkyboxMaterial.CopyPropertiesFromMaterial(skyboxMaterial);
            RenderSettings.skybox = currentSkyboxMaterial;
            EventManager.AddListener<ObjectiveCompletedEvent>(OnObjectiveUpdateEvent);

        }

        void OnObjectiveUpdateEvent(ObjectiveCompletedEvent evt) => UpdateSky();

        public void UpdateSky()
        {
            StartCoroutine(SkyTransiction());
        }

        IEnumerator SkyTransiction()
        {
            float currentTime = 0f;
            float startingExposure = RenderSettings.skybox.GetFloat("_Exposure");
            Color startingColor = RenderSettings.skybox.GetColor("_Tint");
            Color desiredColor = Color.Lerp(RenderSettings.skybox.GetColor("_Tint"), worstSkyTint, 0.2f);
            while (timeTransiction > currentTime)
            {
                yield return new WaitForFixedUpdate();
                currentTime += Time.fixedDeltaTime;
                RenderSettings.skybox.SetFloat("_Exposure", Mathf.Lerp(startingExposure, startingExposure- exposureDegrade,currentTime/timeTransiction));
                RenderSettings.skybox.SetColor("_Tint", Color.Lerp(startingColor, desiredColor, currentTime / timeTransiction));
            }
        }
    }
}

