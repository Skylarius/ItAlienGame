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
        public float exposureDegrade;

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
            Debug.Log("SkyManager::UpdateSky");
            RenderSettings.skybox.SetFloat("_Exposure", RenderSettings.skybox.GetFloat("_Exposure") - exposureDegrade);
            RenderSettings.skybox.SetColor("_Tint", Color.Lerp(RenderSettings.skybox.GetColor("_Tint"), worstSkyTint, 0.1f));

        }
    }
}

