using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

namespace Rotterdam.DigitalTwins.Editor.Utilities
{
    public static class WebDeploymentService
    {
        public static void ConfigureForWebDeployment()
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            
            // PlayerSettings.WebGL.wasmArithmeticExceptions = WebGLWasmArithmeticExceptions.None;
            SetEnumProperty(typeof(PlayerSettings.WebGL), "wasmArithmeticExceptions", "None");

            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
            PlayerSettings.WebGL.threadsSupport = true;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.memorySize = 1026;
            
            // PlayerSettings.WebGL.capabilities = WebGLCapability.Wasm2023;
            SetEnumProperty(typeof(PlayerSettings.WebGL), "capabilities", "Wasm2023");

            Debug.Log("[WebDeploymentService] Project configured for WebGL deployment.");
        }

        private static void SetEnumProperty(Type targetType, string propertyName, string enumValueName)
        {
            PropertyInfo prop = targetType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
            if (prop == null) return;

            Type enumType = prop.PropertyType;
            if (!enumType.IsEnum) return;

            try
            {
                object value = Enum.Parse(enumType, enumValueName);
                prop.SetValue(null, value);
            }
            catch
            {
                // If the specific enum value doesn't exist, we skip it (older Unity version)
            }
        }
    }
}
