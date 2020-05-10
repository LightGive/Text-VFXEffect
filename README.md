# Text-VFXEffect
![gif](https://i.imgur.com/CxktVqH.gif)
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

