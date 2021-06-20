using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlanetPostProcessRenderPass : ScriptableRenderPass {
    /** Label this pass for Unity */
    private string profilerTag;
    
    private Material featureMaterial;
    private RenderTargetIdentifier cameraColorTargetIdent;
    private RenderTargetHandle tmpTexture;



    public PlanetPostProcessRenderPass(string profilerTag, RenderPassEvent renderingPass, Material featureMaterial) {
        this.profilerTag = profilerTag;
        this.renderPassEvent = renderingPass;
        this.featureMaterial = featureMaterial;
    }


    /** Custom parameters */
    public void Setup(RenderTargetIdentifier cameraColorTargetIdent) {
        this.cameraColorTargetIdent = cameraColorTargetIdent;
    }


    /** Called each frame before Execute - Set up things pass need */
    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
        // Create temporary render texture that matches camera
        cmd.GetTemporaryRT(this.tmpTexture.id, cameraTextureDescriptor);
    }


    /** For every eligible camera every frame. */
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
        // Command buffer
        CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
        cmd.Clear();

        // Apply Material to tmp texture
        cmd.Blit(cameraColorTargetIdent, this.tmpTexture.Identifier(), this.featureMaterial, 0);
        // Blit back again
        cmd.Blit(this.tmpTexture.Identifier(), cameraColorTargetIdent);

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }


    /** Called after Execute - Clean anything allocated in Configure */
    public override void FrameCleanup(CommandBuffer cmd) {
        cmd.ReleaseTemporaryRT(this.tmpTexture.id);
    }
}
