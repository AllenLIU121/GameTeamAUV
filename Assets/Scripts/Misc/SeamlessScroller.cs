using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages a set of transforms to create a seamless, infinite scrolling effect.
/// Can be started, stopped, and highlights a specific image on stop.
/// </summary>
public class SeamlessScroller : MonoBehaviour
{
    [Header("Scrolling Configuration")]
    [Tooltip("The speed at which the images scroll horizontally.")]
    [SerializeField] private float scrollSpeed = 5f;
    
    [Tooltip("The width of a single scrolling image. All images should have the same width.")]
    [SerializeField] private float imageWidth = 10f;

    [Tooltip("The X position where the centered image should be highlighted when stopped.")]
    [SerializeField] private float highlightPositionX = 0f;

    [Header("Object References")]
    [Tooltip("The list of image transforms that will be scrolled.")]
    [SerializeField] private Transform[] scrollingImages;
    
    [Tooltip("The sprite to show on the highlighted image when scrolling stops.")]
    [SerializeField] private Sprite specialStopSprite;
    
    [Tooltip("The default sprite for the images.")]
    [SerializeField] private Sprite defaultSprite;

    // --- Private Fields ---
    private bool _isScrolling = true;
    private List<SpriteRenderer> _imageSpriteRenderers = new List<SpriteRenderer>();
    private float _totalWidth;
    private float _leftResetThreshold; // The X position when an image should be moved to the right end.

    void Awake()
    {
        // Best Practice: Cache component references to avoid repeated calls to GetComponent.
        // It's safer to get components from children if the structure is consistent.
        foreach (var image in scrollingImages)
        {
            // Assuming the sprite is on a child object named "Sprite" or similar.
            // Adjust GetChild(1) if your hierarchy is different. A more robust way is to find by name or tag.
            SpriteRenderer renderer = image.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                _imageSpriteRenderers.Add(renderer);
            }
            else
            {
                Debug.LogWarning($"SpriteRenderer not found on a child of {image.name}. Please check the hierarchy.", this);
            }
        }

        // Best Practice: Calculate derived values once.
        if (scrollingImages.Length > 0)
        {
            _totalWidth = imageWidth * scrollingImages.Length;
            // The reset threshold is based on the leftmost point an image can reach.
            // Assuming the images are laid out starting from X=0.
            _leftResetThreshold = -(imageWidth / 2);
        }
    }

    void Start()
    {
        StartScrolling();
    }

    void Update()
    {
        if (!_isScrolling)
        {
            return; // If not scrolling, do nothing in Update.
        }

        HandleScrolling();
    }

    /// <summary>
    /// Handles the continuous movement and repositioning of images.
    /// </summary>
    private void HandleScrolling()
    {
        // Best Practice: Use Time.deltaTime for frame-rate independent movement.
        float movement = scrollSpeed * Time.deltaTime;

        foreach (Transform image in scrollingImages)
        {
            image.Translate(Vector3.left * movement, Space.World);

            // Best Practice: Use a clear threshold for repositioning logic.
            // When an image moves completely off-screen to the left...
            if (image.localPosition.x < _leftResetThreshold)
            {
                // ...move it to the far right end of the line.
                image.localPosition += new Vector3(_totalWidth, 0, 0);
            }
        }
    }
    
    /// <summary>
    /// Starts the scrolling animation.
    /// </summary>
    public void StartScrolling()
    {
        if (_isScrolling) return; // Already scrolling

        _isScrolling = true;
        
        // Reset all sprites to the default when scrolling starts.
        foreach (var renderer in _imageSpriteRenderers)
        {
            renderer.sprite = defaultSprite;
        }
    }

    /// <summary>
    /// Stops the scrolling animation and highlights the image closest to the center.
    /// </summary>
    public void StopScrolling()
    {
        if (!_isScrolling) return; // Already stopped

        _isScrolling = false;

        // Find the image closest to the highlight position.
        Transform imageToHighlight = GetImageClosestToPosition(highlightPositionX);

        if (imageToHighlight != null)
        {
            // Find the corresponding SpriteRenderer and apply the special sprite.
            int index = System.Array.IndexOf(scrollingImages, imageToHighlight);
            if (index != -1 && index < _imageSpriteRenderers.Count)
            {
                _imageSpriteRenderers[index].sprite = specialStopSprite;
            }
        }
    }

    /// <summary>
    /// A helper method to find which transform is closest to a given world X position.
    /// </summary>
    /// <param name="xPosition">The target X position in world space.</param>
    /// <returns>The transform of the closest image.</returns>
    private Transform GetImageClosestToPosition(float xPosition)
    {
        return scrollingImages
            .OrderBy(image => Mathf.Abs(image.position.x - xPosition))
            .FirstOrDefault();
    }
}