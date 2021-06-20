using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlanetPostProcessFeature : ScriptableRendererFeature {
    /* Parameters displayed in the inspector */
    [System.Serializable]
    public class PlanetPostProcessFeatureSettings {
        public bool isEnabled = true;
        public RenderPassEvent renderingPass = RenderPassEvent.AfterRendering;
        public Material featureMaterial;
    }



    /** MUST be named "settings" to be shown in the Render Features inspector */
    public PlanetPostProcessFeatureSettings settings = new PlanetPostProcessFeatureSettings();

    private RenderTargetHandle renderTextureHandle;
    private PlanetPostProcessRenderPass pass;



    public override void Create() {
        pass = new PlanetPostProcessRenderPass(
            "PlanetRenderPass", // Unique label
            this.settings.renderingPass,
            this.settings.featureMaterial
        );
    }


    /** Called every frame once per camera */
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        if (!this.settings.isEnabled)
            return;

        // Put informations into the pass
        var cameraColorTargetIdent = renderer.cameraColorTarget;
        this.pass.Setup(cameraColorTargetIdent);

        // Add pass to renderer
        renderer.EnqueuePass(this.pass);
    }
}
