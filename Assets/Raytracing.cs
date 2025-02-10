using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

//[ExecuteInEditMode]
public class Raytracing : MonoBehaviour
{
    public RayTracingShader rayTracingShader = null;

    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        CommandBuffer cmdBuffer = new CommandBuffer();

        cmdBuffer.BuildRayTracingAccelerationStructure(rtAS);

        cmdBuffer.SetRayTracingShaderPass(rayTracingShader, "Raytracing");

        cmdBuffer.SetRayTracingAccelerationStructure(rayTracingShader, Shader.PropertyToID("g_AccelerationStructure"), rtAS);

        cmdBuffer.SetRayTracingMatrixParam(rayTracingShader, Shader.PropertyToID("g_InvViewMatrix"), Camera.main.cameraToWorldMatrix);
        cmdBuffer.SetRayTracingMatrixParam(rayTracingShader, Shader.PropertyToID("g_InvProjMatrix"), Camera.main.projectionMatrix.inverse);

        cmdBuffer.SetRayTracingTextureParam(rayTracingShader, Shader.PropertyToID("g_Output"), rtOutput);

        cmdBuffer.SetRenderTarget(rtOutput);
        cmdBuffer.ClearRenderTarget(true, true, Color.black);

        cmdBuffer.DispatchRays(rayTracingShader, "RayGenShader", cameraWidth, cameraHeight, 1);

        Graphics.ExecuteCommandBuffer(cmdBuffer);

        cmdBuffer.Release();

        Graphics.Blit(rtOutput, dest);
    }

    void Update()
    {
        if (cameraWidth != Camera.main.pixelWidth || cameraHeight != Camera.main.pixelHeight)
        {
            if (rtOutput != null)
            {
                rtOutput.Release();
            }

            rtOutput = new RenderTexture(Camera.main.pixelWidth, Camera.main.pixelHeight, 0, RenderTextureFormat.ARGBFloat);
            rtOutput.enableRandomWrite = true;
            rtOutput.Create();

            cameraWidth = (uint)Camera.main.pixelWidth;
            cameraHeight = (uint)Camera.main.pixelHeight;
        }
    }

    private void Start()
    {
        if (rtAS == null)
        {
            var settings = new RayTracingAccelerationStructure.Settings();
            settings.rayTracingModeMask = RayTracingAccelerationStructure.RayTracingModeMask.Everything;
            settings.managementMode = RayTracingAccelerationStructure.ManagementMode.Manual;
            settings.layerMask = 255;

            rtAS = new RayTracingAccelerationStructure(settings);
        }

        var renderers = (Renderer[])FindObjectsByType(typeof(Renderer), FindObjectsSortMode.None);

        foreach (var renderer in renderers)
        {
            var obj = renderer.gameObject;
            var config = new RayTracingMeshInstanceConfig();

            config.mesh = obj.GetComponent<MeshFilter>().sharedMesh;
            config.material = renderer.material;
            config.enableTriangleCulling = true;
            config.subMeshFlags = RayTracingSubMeshFlags.Enabled;
            config.accelerationStructureBuildFlags = RayTracingAccelerationStructureBuildFlags.PreferFastTrace;

            rtAS.AddInstance(config, obj.transform.localToWorldMatrix);
        }
    }

    void OnDestroy()
    {
        if (rtAS != null)
        {
            rtAS.Release();
            rtAS = null;
        }

        if (rtOutput != null)
        {
            rtOutput.Release();
            rtOutput = null;
        }
    }

    private uint cameraWidth = 0;
    private uint cameraHeight = 0;

    private RenderTexture rtOutput = null;

    private RayTracingAccelerationStructure rtAS = null;
}
