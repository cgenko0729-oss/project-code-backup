using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class 
    UIFXController : MonoBehaviour
{
     
    [SerializeField] private Image frameImage;

    // duplicated at runtime so other buttons using the same material are untouched
    private Material mat;

    public float outlineWidth = 0.0077f;
    public float outlineGlowAmount = 1.49f;
    public Color outlineColor = Color.white;

    public float InnerOutlineThickness = 0.005f;
    public Color InnerOutlineColor = Color.white;
    public float InnerOutlineGlow = 4.0f;

    public float hueShiftAmount = 180f;
    public float hueSaturation = 1f;
    public float hueBrightness = 1f;

    // New Defaults for Overlay
    public Color overlayColor = Color.white;
    public float overlayGlow = 1f;
    public float overlayBlend = 1f;

    void Awake()
    {
        if (frameImage == null)
        {
            //frameImage = GetComponent<Image>();

            frameImage = GetComponentInChildren<Image>();
        }

        if (frameImage != null)
        {
            mat = new Material(frameImage.material);
            frameImage.material = mat;
        }
        else
        {
            Debug.Log("frameImageが設定されていないか子オブジェクトにもImageコンポーネントがありません");
        }
    }

    // --------------------------------------------------------------------------------
    // CORE FIX: ApplyMaterialChanges
    // This forces the Unity UI Mask/RectMask2D system to refresh its internal cache.
    // Without this, the Scroll View will keep rendering the OLD material state.
    // --------------------------------------------------------------------------------
    private void ApplyMaterialChanges()
    {
        if (frameImage != null)
        {
            frameImage.SetMaterialDirty();
            frameImage.enabled = false;
            frameImage.enabled = true;
        }
    }

     public void ToggleOverlayUiFX(bool enabled, bool isRuntimeValue = false, Color color = default, float glow = 1f, float blend = 1f)
    {
        // 1. Determine values (Runtime vs Defaults)
        Color finalColor = isRuntimeValue ? color : overlayColor;
        float finalGlow = isRuntimeValue ? glow : overlayGlow;
        float finalBlend = isRuntimeValue ? blend : overlayBlend;

        if (color == default && isRuntimeValue) finalColor = Color.white; // Safety check

        // 2. Set the Float (Syncs the Inspector Checkbox)
        mat.SetFloat("_EnableOverlay", enabled ? 1.0f : 0.0f);

        // 3. Set the Keyword (Actually compiles the shader logic)
        // CRITICAL FIX: Changed "OVERLAY_ON" to "ENABLE_OVERLAY" to match Shader
        if (enabled)
        {
            mat.EnableKeyword("ENABLE_OVERLAY");
            
            // Set properties
            mat.SetColor("_OverlayColor", finalColor);
            mat.SetFloat("_OverlayGlow", finalGlow);
            mat.SetFloat("_OverlayBlend", Mathf.Clamp01(finalBlend));
        }
        else
        {
            mat.DisableKeyword("ENABLE_OVERLAY");
        }

        ApplyMaterialChanges();
    }

    public void SetHueShift(bool enabled, bool isRuntimeSetValue = false, float hueShift = 180f, float saturation = 1f, float brightness = 1f)
    {

        float finalHueShift = isRuntimeSetValue ? hueShift : hueShiftAmount;
        float finalSaturation = isRuntimeSetValue ? saturation : hueSaturation;
        float finalBrightness = isRuntimeSetValue ? brightness : hueBrightness;

        if (enabled) mat.EnableKeyword("HSV_ON");
        else mat.DisableKeyword("HSV_ON");

        mat.SetFloat("_HsvShift", finalHueShift);
        mat.SetFloat("_HsvSaturation", finalSaturation);
        mat.SetFloat("_HsvBright", finalBrightness);
    }

    /// <summary>
    /// Controls the Greyscale color effect.
    /// </summary>
    /// <param name="enabled">Whether to enable the Greyscale effect.</param>
    /// <param name="blend">How much to blend to greyscale (0=original, 1=full greyscale).</param>
    /// <param name="luminosity">Adjusts the luminosity of the effect.</param>
    /// <param name="tintColor">A color to tint the greyscale image.</param>
    /// <param name="affectsOutline">If true, the effect is applied after the outline.</param>
    public void SetGreyscale(bool enabled, float blend = 1f, float luminosity = 0f, Color? tintColor = null, bool affectsOutline = false)
    {
        if (enabled) mat.EnableKeyword("GREYSCALE_ON");
        else mat.DisableKeyword("GREYSCALE_ON");

        if (affectsOutline) mat.EnableKeyword("GREYSCALEOUTLINE_ON");
        else mat.DisableKeyword("GREYSCALEOUTLINE_ON");

        mat.SetFloat("_GreyscaleBlend", Mathf.Clamp01(blend));
        mat.SetFloat("_GreyscaleLuminosity", luminosity);
        mat.SetColor("_GreyscaleTintColor", tintColor ?? Color.white);
    }

    /* ─────────────────────────────────────────────
     *  OUTLINE  (base+distortion+scroll)
     * ───────────────────────────────────────────── */

    public void SetInnerOutline(bool enabled, bool isRuntimeSetValue = false, float thickness = 1, float glow = 4, Color baseColor = default)
    {

        float finalThickness = isRuntimeSetValue ? thickness : InnerOutlineThickness;
        float finalGlow = isRuntimeSetValue ? glow : InnerOutlineGlow;
        Color finalColor = isRuntimeSetValue ? baseColor : InnerOutlineColor;

        if (baseColor == default) baseColor = Color.white;

        if (enabled) mat.EnableKeyword("INNEROUTLINE_ON");
        else mat.DisableKeyword("INNEROUTLINE_ON");

        mat.SetColor("_InnerOutlineColor", finalColor);
        mat.SetFloat("_InnerOutlineThickness", thickness);
        mat.SetFloat("_InnerOutlineGlow", glow);
    }

    public void ToggleOutline(bool enabled, bool isRunTimeSetValue = false, float width = 0.0077f, float glowAmount = 1.49f, Color baseColor = default)
    {
        float finalWidth = isRunTimeSetValue ? width : outlineWidth;
        float finalGlowAmount = isRunTimeSetValue ? glowAmount : outlineGlowAmount;
        Color finalColor = isRunTimeSetValue ? baseColor : outlineColor;

        if (baseColor == default) baseColor = Color.white;

        if (enabled)
        {
            mat.EnableKeyword("OUTBASE_ON");
            mat.EnableKeyword("OUTBASE8DIR_ON");
            mat.SetFloat("_OutlineWidth", finalWidth);
            mat.SetFloat("_OutlineGlow", finalGlowAmount);
            mat.SetColor("_OutlineColor", finalColor);
        }
        else
        {
            mat.DisableKeyword("OUTBASE_ON");
            mat.DisableKeyword("OUTBASE8DIR_ON");
        }
    }

    public void SetOutlineColor(Color  baseColor)
    {
        mat.SetColor ("_OutlineColor",  baseColor);
    }

    public void SetOutline
    (
        bool   enabled,
        Color  baseColor,
        float  alpha              = 1f,
        float  glow               = 1.5f,
        bool   distortion         = false,
        float  distortionAmount   = 0.5f,
        Vector2 distortionSpeedXY = default   // units = UV / sec
    )
    {
        if (enabled)  mat.EnableKeyword  ("OUTBASE_ON");
        else          mat.DisableKeyword ("OUTBASE_ON");

        // core look
        mat.SetColor ("_OutlineColor",  baseColor);
        mat.SetFloat ("_OutlineAlpha",  Mathf.Clamp01(alpha));
        mat.SetFloat ("_OutlineGlow",   glow);

        // distortion branch
        if (distortion)
        {
            mat.EnableKeyword ("OUTDIST_ON");
            mat.SetFloat ("_OutlineDistortAmount", distortionAmount);
            mat.SetFloat ("_OutlineDistortTexXSpeed", distortionSpeedXY.x);
            mat.SetFloat ("_OutlineDistortTexYSpeed", distortionSpeedXY.y);
        }
        else mat.DisableKeyword("OUTDIST_ON");
    }

    public void SetOutlineTextureEnable(bool enabled)
    {
        // NOTE: The texture branch lives inside OUTBASE_ON, so ensure base outline is on.
        if (enabled) {
            mat.EnableKeyword("OUTBASE_ON");
            mat.EnableKeyword("OUTTEX_ON");
        } else {
            mat.DisableKeyword("OUTTEX_ON");
            // do NOT disable OUTBASE_ON here; other outline settings may still be wanted
        }
    }

    public void SetOutlineTextureScroll(Vector2 speedXY)
    {
        mat.SetFloat("_OutlineTexXSpeed", speedXY.x);   // tiles per second along X
        mat.SetFloat("_OutlineTexYSpeed", speedXY.y);   // tiles per second along Y
    }

    public void SetOutlineTexture(Texture tex, Vector2? tiling = null, Vector2? offset = null)
    {
        if (tex != null) mat.SetTexture("_OutlineTex", tex);
        if (tiling.HasValue) mat.SetTextureScale("_OutlineTex", tiling.Value);
        if (offset.HasValue) mat.SetTextureOffset("_OutlineTex", offset.Value);
    }


    /* ─────────────────────────────────────────────
     *  OVERLAY  (additive OR multiply)
     * ───────────────────────────────────────────── */

    public void SetOverlayEnable(bool   enabled, Color overlayColor)
    {
        if (enabled)  mat.EnableKeyword  ("OVERLAY_ON");
        else          mat.DisableKeyword ("OVERLAY_ON");
        mat.SetColor("_OverlayColor", overlayColor);
    }



   

    public void SetOverlay
    (
        bool   enabled,
        Color  overlayColor,
        float  glow             = 1f,
        float  blend            = 1f,          // 0-1  (inspector field “Overlay Blend”)
        Vector2 scrollSpeedXY   = default,     // UV / sec
        bool   multiplyInstead  = false        // inspector toggle “Is overlay multiplicative?”
    )
    {
        if (enabled)  mat.EnableKeyword  ("OVERLAY_ON");
        else          mat.DisableKeyword ("OVERLAY_ON");

        if (multiplyInstead) mat.EnableKeyword ("OVERLAYMULT_ON");
        else                 mat.DisableKeyword("OVERLAYMULT_ON");

        mat.SetColor("_OverlayColor", overlayColor);
        mat.SetFloat("_OverlayGlow",  glow);
        mat.SetFloat("_OverlayBlend", Mathf.Clamp01(blend));

        mat.SetFloat("_OverlayTextureScrollXSpeed", scrollSpeedXY.x);
        mat.SetFloat("_OverlayTextureScrollYSpeed", scrollSpeedXY.y);
    }

    /* ─────────────────────────────────────────────
     *  WAVE UV
     * ───────────────────────────────────────────── */
    public void SetWaveUV
    (
        bool   enabled,
        float  amount    = 7f,      // inspector “Wave Amount”
        float  speed     = 10f,     // “Wave Speed”
        float  strength  = 7.5f,    // “Wave Strength”
        Vector2 waveXY   = default  // “Wave X / Y Axis” (0-1)
    )
    {
        if (enabled)  mat.EnableKeyword  ("WAVEUV_ON");
        else          mat.DisableKeyword ("WAVEUV_ON");

        mat.SetFloat("_WaveAmount",   amount);
        mat.SetFloat("_WaveSpeed",    speed);
        mat.SetFloat("_WaveStrength", strength);
        mat.SetFloat("_WaveX",        Mathf.Clamp01(waveXY.x));
        mat.SetFloat("_WaveY",        Mathf.Clamp01(waveXY.y));
    }

    /* ─────────────────────────────────────────────
     *  ROUND WAVE UV  (ripples)
     * ───────────────────────────────────────────── */

    public void SetRoundWaveEnable(bool  enabled)
    {
        if (enabled)  mat.EnableKeyword  ("ROUNDWAVEUV_ON");
        else          mat.DisableKeyword ("ROUNDWAVEUV_ON");
    }

    public void SetRoundWave
    (
        bool  enabled,
        float strength = 0.7f,
        float speed    = 2f
    )
    {
        if (enabled)  mat.EnableKeyword  ("ROUNDWAVEUV_ON");
        else          mat.DisableKeyword ("ROUNDWAVEUV_ON");

        mat.SetFloat("_RoundWaveStrength", strength);
        mat.SetFloat("_RoundWaveSpeed",    speed);
    }

    /* ─────────────────────────────────────────────
     *  The original helpers are kept unchanged
     * ───────────────────────────────────────────── */
    public void SetGlow(bool enabled, Color color, float intensity = 10f, float globalMult = 1f)
    {
        if (enabled)  mat.EnableKeyword  ("GLOW_ON");
        else          mat.DisableKeyword ("GLOW_ON");

        mat.SetColor("_GlowColor",  color);
        mat.SetFloat("_Glow",       intensity);
        mat.SetFloat("_GlowGlobal", globalMult);
    }

    public void EnableGlow(bool enabled)
{
    if (mat == null) return;
    if (enabled) mat.EnableKeyword("GLOW_ON");
    else mat.DisableKeyword("GLOW_ON");
}

public void SetGlowColor(Color c)
{
    if (mat == null) return;
    mat.SetColor("_GlowColor", c);
}

public void SetGlowIntensity(float intensity)
{
    if (mat == null) return;
    mat.SetFloat("_Glow", intensity);

    // If you are using Mask/ScrollRect and you notice it doesn't update,
    // uncomment the next line (but avoid toggling enabled every frame).
    // frameImage?.SetMaterialDirty();
}

public void SetGlowGlobal(float globalMult)
{
    if (mat == null) return;
    mat.SetFloat("_GlowGlobal", globalMult);
}



    public void SetOutlineSimple(bool enabled, Color color, float width = 0.004f, float glow = 1.5f)
    {
        if (enabled)  mat.EnableKeyword  ("OUTBASE_ON");
        else          mat.DisableKeyword ("OUTBASE_ON");

        mat.SetColor("_OutlineColor", color);
        mat.SetFloat("_OutlineWidth", width);
        mat.SetFloat("_OutlineGlow",  glow);
    }


    /// <summary>Starts a one–off shine sweep: enable, start at 1, tween to 0, then (optionally) switch off.</summary>
public Tween TriggerShine(
    float duration       = 0.4f,
    Color? color         = null,   // keep current if null
    float width          = 0.05f,
    float glow           = 0.23f,
    float rotateRadians  = 0f,
    bool  disableOnEnd   = false,
    Ease  ease           = Ease.OutQuad)
{
    // 1) make sure branch is compiled & active
    mat.EnableKeyword("SHINE_ON");

    // 2) configure look
    if (color.HasValue)  mat.SetColor("_ShineColor", color.Value);
    mat.SetFloat("_ShineWidth",  width);
    mat.SetFloat("_ShineGlow",   glow);
    mat.SetFloat("_ShineRotate", rotateRadians);

    // 3) force starting point
    mat.SetFloat("_ShineLocation", 1f);

        // 4) tween location ↓ to 0
        return DOTween.To(() => 1f,
                          x => mat.SetFloat("_ShineLocation", x),
                          0f,
                          duration)
                      .SetEase(ease)
                      .SetUpdate(true)
                  .SetId(mat)                    // so we can kill it later if needed
                  .OnComplete(() =>
                  {
                      if (disableOnEnd) mat.DisableKeyword("SHINE_ON");
                  });
}



}

