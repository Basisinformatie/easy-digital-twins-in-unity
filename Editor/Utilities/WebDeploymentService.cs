using UnityEditor;
using UnityEngine;

namespace Rotterdam.DigitalTwins.Editor.Utilities
{
    public static class WebDeploymentService
    {
        public static void ConfigureForWebDeployment()
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.wasmArithmeticExceptions = WebGLWasmArithmeticExceptions.None;
            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
            PlayerSettings.WebGL.threadsSupport = true;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.memorySize = 512;
            
            // Modern WebAssembly features (SIMD, BigInt)
            PlayerSettings.WebGL.capabilities = WebGLCapability.Wasm2023;

            Debug.Log("[WebDeploymentService] Project configured for WebGL deployment.");
        }
    }
}
