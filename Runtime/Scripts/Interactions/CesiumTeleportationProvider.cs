using System;
#if USING_CESIUM && USING_XR_INTERACTION_TOOLKIT
using CesiumForUnity;
#endif
using UnityEngine;
#if USING_XR_INTERACTION_TOOLKIT
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
#endif

namespace Rotterdam.DigitalTwins.Runtime
{
#if USING_CESIUM && USING_XR_INTERACTION_TOOLKIT
    [AddComponentMenu("Cesium/Cesium Teleportation")]
    [RequireComponent(typeof(Cesium3DTileset))]
#endif
    public class CesiumTeleportationProvider : MonoBehaviour
    {
#if USING_CESIUM && USING_XR_INTERACTION_TOOLKIT
        [SerializeField]
        [Tooltip("Interaction layer is usually set to 31.")]
        private InteractionLayerMask _teleportLayer = 1 << 31;

        private Cesium3DTileset _tileset;

        private void Awake()
        {
            _tileset = GetComponent<Cesium3DTileset>();
            if (_tileset != null && !_tileset.createPhysicsMeshes)
            {
                Debug.LogWarning($"CesiumTeleportation: 'Create Physics Meshes' is off on {gameObject.name}. Teleportation needs colliders.", gameObject);
            }
        }

        private void OnEnable()
        {
            if (_tileset != null)
            {
                _tileset.OnTileGameObjectCreated += OnTileGameObjectCreated;
            }
        }

        private void OnDisable()
        {
            if (_tileset != null)
            {
                _tileset.OnTileGameObjectCreated -= OnTileGameObjectCreated;
            }
        }

        private void OnTileGameObjectCreated(GameObject tile)
        {
            if (tile == null || !enabled)
                return;

            var teleportationArea = tile.GetComponent<TeleportationArea>();
            if (teleportationArea == null)
            {
                teleportationArea = tile.AddComponent<TeleportationArea>();
            }
            
            teleportationArea.interactionLayers = _teleportLayer;
        }
#else
        private void Awake()
        {
            Debug.LogWarning("CesiumTeleportationProvider is disabled because USING_CESIUM or USING_XR_INTERACTION_TOOLKIT is not defined.");
        }
#endif
    }
}
