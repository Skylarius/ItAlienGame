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
        [Tooltip("Sea Game Obcject ")]
        public GameObject seaGameobject;
        [Tooltip("Sea Game Obcject ")]
        public Material seaMaterial;
        [Tooltip("Towards Tint")]
        public Color worstSeaTint;

        Material currentSkyboxMaterial;
        Material currentSeaMaterial;

        // Start is called before the first frame update
        void Awake()
        {
            currentSkyboxMaterial = new Material(skyboxMaterial.shader);
            currentSkyboxMaterial.CopyPropertiesFromMaterial(skyboxMaterial);
            RenderSettings.skybox = currentSkyboxMaterial;
            EventManager.AddListener<ObjectiveCompletedEvent>(OnObjectiveUpdateEvent);
            currentSeaMaterial = new Material(seaMaterial.shader);
            currentSeaMaterial.CopyPropertiesFromMaterial(seaMaterial);
            seaGameobject.GetComponent<Renderer>().material = currentSeaMaterial;
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
            Color startingSkyColor = RenderSettings.skybox.GetColor("_Tint");
            Color desiredSkyColor = Color.Lerp(RenderSettings.skybox.GetColor("_Tint"), worstSkyTint, 0.4f);
            Color startingSeaColor = currentSeaMaterial.GetColor("_Color");
            Color desiredSeaColor = Color.Lerp(currentSeaMaterial.GetColor("_Color"), worstSeaTint, 0.5f);

            while (timeTransiction > currentTime)
            {
                yield return new WaitForFixedUpdate();
                currentTime += Time.fixedDeltaTime;
                RenderSettings.skybox.SetFloat("_Exposure", Mathf.Lerp(startingExposure, startingExposure- exposureDegrade,currentTime/timeTransiction));
                RenderSettings.skybox.SetColor("_Tint", Color.Lerp(startingSkyColor, desiredSkyColor, currentTime / timeTransiction));
                seaMaterial.SetColor("_Color", Color.Lerp(startingSeaColor, desiredSeaColor, currentTime / timeTransiction));

            }
        }
    }
}

