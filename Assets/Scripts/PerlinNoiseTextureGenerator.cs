using UnityEngine;

public class PerlinNoiseTextureGenerator : MonoBehaviour
{
    [Header("Noise Settings")]
    [SerializeField] private int textureWidth = 512;
    [SerializeField] private int textureHeight = 512;
    [SerializeField] private float noiseScale = 10f;
    [SerializeField] private float offsetX = 0f;
    [SerializeField] private float offsetY = 0f;
    
    [Header("Terrain Colors")]
    [SerializeField] private Color lightDirtColor = new Color(0.8f, 0.7f, 0.6f); // Used when noise values are low
    [SerializeField] private Color grassColor = new Color(0.3f, 0.6f, 0.2f); // Used when noise values are around 0.5
    [SerializeField] private Color darkDirtColor = new Color(0.3f, 0.2f, 0.15f); // Used when noise values are high
    
    [Header("Color Blending")]
    [SerializeField] private float grassThresholdMin = 0.4f;
    [SerializeField] private float grassThresholdMax = 0.6f;
    [SerializeField] private float blendSmoothness = 0.1f;
    
    [Header("Auto Apply")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool applyToMaterial = true;
    
    private Texture2D generatedTexture;
    private Material planeMaterial;
    
    void Start()
    {
        if (generateOnStart)
        {
            GenerateTexture();
        }
    }
    
    public void GenerateTexture()
    {
        // Start with a blank texture that we'll fill pixel by pixel
        generatedTexture = new Texture2D(textureWidth, textureHeight);
        generatedTexture.filterMode = FilterMode.Bilinear;
        generatedTexture.wrapMode = TextureWrapMode.Clamp;
        
        // We'll build up an array of colors, one for each pixel in the texture
        Color[] pixels = new Color[textureWidth * textureHeight];
        
        // Walk through every pixel position in the texture
        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                // Convert pixel coordinates to noise space coordinates
                // Dividing by texture size normalizes to 0-1, then we scale by noiseScale
                // Adding offsets lets us shift the pattern around
                float noiseValue = Mathf.PerlinNoise(
                    (x / (float)textureWidth) * noiseScale + offsetX,
                    (y / (float)textureHeight) * noiseScale + offsetY
                );
                
                // Turn the noise value into a terrain color based on how high or low it is
                Color pixelColor = GetTerrainColor(noiseValue);
                pixels[y * textureWidth + x] = pixelColor;
            }
        }
        
        // Push all the colors we calculated into the texture at once
        generatedTexture.SetPixels(pixels);
        generatedTexture.Apply();
        
        // If enabled, automatically stick this texture onto the plane's material
        if (applyToMaterial)
        {
            ApplyTextureToMaterial();
        }
    }
    
    private Color GetTerrainColor(float noiseValue)
    {
        // We're mapping noise values to colors like this:
        // Really low values → light dirt
        // Values around 0.5 → grass
        // Really high values → dark dirt
        
        // Check if we're in the pure light dirt zone (well below grass threshold)
        if (noiseValue < grassThresholdMin - blendSmoothness)
        {
            return lightDirtColor;
        }
        // Check if we're in the pure dark dirt zone (well above grass threshold)
        else if (noiseValue > grassThresholdMax + blendSmoothness)
        {
            return darkDirtColor;
        }
        // We're in a transition zone between light dirt and grass
        // Calculate how far through the transition we are (0 = all light dirt, 1 = all grass)
        else if (noiseValue >= grassThresholdMin - blendSmoothness && noiseValue <= grassThresholdMin)
        {
            float t = (noiseValue - (grassThresholdMin - blendSmoothness)) / blendSmoothness;
            return Color.Lerp(lightDirtColor, grassColor, t);
        }
        // We're in the pure grass zone
        else if (noiseValue >= grassThresholdMin && noiseValue <= grassThresholdMax)
        {
            return grassColor;
        }
        // We're in a transition zone between grass and dark dirt
        // Calculate how far through the transition we are (0 = all grass, 1 = all dark dirt)
        else // noiseValue > grassThresholdMax && noiseValue <= grassThresholdMax + blendSmoothness
        {
            float t = (noiseValue - grassThresholdMax) / blendSmoothness;
            return Color.Lerp(grassColor, darkDirtColor, t);
        }
    }
    
    private void ApplyTextureToMaterial()
    {
        // Find the renderer that's drawing this plane
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            // If there's no material at all, make a basic one so we have something to work with
            if (meshRenderer.sharedMaterial == null)
            {
                Material defaultMat = new Material(Shader.Find("Standard"));
                meshRenderer.sharedMaterial = defaultMat;
            }
            
            // Accessing .material automatically creates a material instance in edit mode
            // This prevents us from accidentally modifying the original material asset
            // In play mode it just returns the material normally
            planeMaterial = meshRenderer.material;
            if (planeMaterial != null)
            {
                // Assign our generated texture as the main texture on the material
                planeMaterial.mainTexture = generatedTexture;
            }
        }
        else
        {
            Debug.LogWarning("No MeshRenderer found on GameObject. Texture generated but not applied.");
        }
    }
    
    // Lets other scripts grab the texture we generated
    public Texture2D GetGeneratedTexture()
    {
        return generatedTexture;
    }
    
    // Regenerates the texture using current settings (useful for runtime changes)
    public void RegenerateTexture()
    {
        GenerateTexture();
    }
    
    void OnValidate()
    {
        // Unity calls this whenever we change values in the inspector
        // We use it to prevent invalid settings that would break the texture generation
        
        // Textures need at least 1 pixel in each dimension
        textureWidth = Mathf.Max(1, textureWidth);
        textureHeight = Mathf.Max(1, textureHeight);
        // Noise scale can't be zero or negative (would cause division issues)
        noiseScale = Mathf.Max(0.1f, noiseScale);
        // Thresholds must be between 0 and 1 (noise values are always 0-1)
        grassThresholdMin = Mathf.Clamp01(grassThresholdMin);
        grassThresholdMax = Mathf.Clamp01(grassThresholdMax);
        
        // Make sure the min threshold isn't higher than max (would break the logic)
        if (grassThresholdMin > grassThresholdMax)
        {
            grassThresholdMin = grassThresholdMax;
        }
    }
}

