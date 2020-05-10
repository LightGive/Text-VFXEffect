# Text-VFXEffect
<img src="https://66.media.tumblr.com/cfdccee125be88578d5697a3417078d5/8ba34de2fbf79b4a-35/s640x960/4589a6e79fe63bede7e770c9d5007b203cd2b72b.gifv">
Enter a short text in the input field and the content will be displayed in VFXEffect.

## Version
Unity 2019.3.12f

## Method

1. Get texture for each character from TextMeshPro font
2. Get the position where the dot is struck with GetPixel and make it a list of Vector3
3. Bake to Render Texture with Compute Shaders
4. Set the baked RenderTexture to AttributeMap of VFX SetPositionFromMap

Baking Vector3 to Texture uses Smrvfx from Keijiro.<br>
https://github.com/keijiro/Smrvfx

