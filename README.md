MeshEffectForTextMeshPro
===

### NOTE: This project is WIP.
### NOTE: This project will be integrated to [UIEffect](https://github.com/mob-sakai/UIEffect).

MeshEffectForTextMeshPro provide visual effect components for TextMeshPro in Unity.

![](https://user-images.githubusercontent.com/12690315/49157470-2dc50a80-f363-11e8-82b5-007a6ae04f2a.png)

[![](https://img.shields.io/github/release/mob-sakai/MeshEffectForTextMeshPro.svg?label=latest%20version)](https://github.com/mob-sakai/MeshEffectForTextMeshPro/releases)
[![](https://img.shields.io/github/release-date/mob-sakai/MeshEffectForTextMeshPro.svg)](https://github.com/mob-sakai/MeshEffectForTextMeshPro/releases)
![](https://img.shields.io/badge/unity-5.5%2B-green.svg)
[![](https://img.shields.io/github/license/mob-sakai/MeshEffectForTextMeshPro.svg)](https://github.com/mob-sakai/MeshEffectForTextMeshPro/blob/master/LICENSE.txt)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-orange.svg)](http://makeapullrequest.com)

<< [Description](#Description) | [WebGL Demo](#demo) | [Download](https://github.com/mob-sakai/MeshEffectForTextMeshPro/releases) | [Usage](#usage) | [Example of using](#example-of-using) | [Development Note](#development-note) | [Change log](https://github.com/mob-sakai/MeshEffectForTextMeshPro/blob/master/CHANGELOG.md) >>

### What's new? [See changelog ![](https://img.shields.io/github/release-date/mob-sakai/MeshEffectForTextMeshPro.svg?label=last%20updated)](https://github.com/mob-sakai/MeshEffectForTextMeshPro/blob/develop/CHANGELOG.md)
### Do you want to receive notifications for new releases? [Watch this repo ![](https://img.shields.io/github/watchers/mob-sakai/MeshEffectForTextMeshPro.svg?style=social&label=Watch)](https://github.com/mob-sakai/MeshEffectForTextMeshPro/subscription)



<br><br><br><br>
## Description

Do you like TextMeshPro? I’m lovin’ it:)  

As you know, TextMeshPro, the ultimate text solution, is one of the greatest assets in Unity.  
It is more beautiful than the standard TextMesh and Text, it is highly functional and free.

One of the big mystery of TextMeshPro is that "typical mesh effects (vertex effects) for uGUI can not be used for TextMeshPro".  
Since TextMeshPro does not call `IMeshModifier` interface, TextMeshPro ignores the typical mesh effects.  
The mesh effects of TextMeshPro (eg VertexJitter, VertexColorCycler etc) are very unique in their implementation and can only be used for TextMeshPro...

I think that an easy way to implement a common mesh effect is necessary.

This project provides a base class for mesh effect.  
It works well not only for standard Graphic components (Image, RawImage, Text, etc.) but also for TextMeshPro and TextMeshProUGUI.  
Just change your mesh effect a few places, it will support TextMeshPro!

Let's decorate your TextMeshPro with effects!


#### Features

* Works well not only for standard Graphic components (Image, RawImage, Text, etc.) but also for TextMeshPro and TextMeshProUGUI.
* Support multiple fonts.
* There is no useless allocation.
* Good performance.
* You can implement MeshEffect with VertexHelper or Mesh.
* Easily make your mesh effect support TextMeshPro. [See detail](#make-your-mesh-effect-support-textmeshpro)



<br><br><br><br>
## Demo

[WebGL Demo](http://mob-sakai.github.io/MeshEffectForTextMeshPro)



<br><br><br><br>
## Usage

1. Download MeshEffectForTextMeshPro.unitypackage from [Releases](https://github.com/mob-sakai/MeshEffectForTextMeshPro/releases).
2. Import the package into your Unity project. Select `Import Package > Custom Package` from the `Assets` menu.  
![](https://user-images.githubusercontent.com/12690315/46567584-3525f400-c970-11e8-9839-5c9e810b0b80.png)
3. Add any effect component to TextMeshPro from `Add Component` in inspector or `Component > MeshEffectForTextMeshPro > ...` menu.  
4. Adjust the parameters of the effect as you like, in inspector.  
5. Enjoy!


##### Requirement

* Unity 5.5+ *(including Unity 2019.x)*
* TextMeshPro v1.0.0+



<br><br><br><br>
## Development Note

#### Make your mesh effect supports TextMeshPro

1. Change the base class from `BaseMeshEffect` to `Coffee.UIExtensions.BaseMeshEffect`.
```cs
// Before
public class YourMeshEffect : BaseMeshEffect
or
public class YourMeshEffect : MonoBehavior, IMeshModifier

// After
public class YourMeshEffect : Coffee.UIExtensions.BaseMeshEffect
```
2. If you are using specific methods, override it properly.
* OnEnable, OnDisable, LateUpdate, OnDidApplyAnimationProperties, OnValidate, ModifyMesh.
* Call `base.xxx` except ModifyMesh.
```cs
// Before
void OnEnable ()
{
    ...
}
void ModifyMesh (VertexHelper vh)
{
    ...
}

// After
protected override void OnEnable ()
{
    ...
    base.OnEnable();
}
public override void ModifyMesh (VertexHelper vh)
{
    ...
    //base.ModifyMesh(vh);  <- ModifyMesh's base method is unnecessary.
}
```
3. Change `graphic.SetVerticesDirty` to `SetVerticesDirty`
```cs
// Before
public bool horizontal { get { return this.m_Horizontal; } set { this.m_Horizontal = value; graphic.SetVerticesDirty(); } }

// After
public bool horizontal { get { return this.m_Horizontal; } set { this.m_Horizontal = value; SetVerticesDirty(); } }
```
4. If there are compile errors, remove them.



<br><br><br><br>
## License

* MIT
* © UTJ/UCL



## Author

[mob-sakai](https://github.com/mob-sakai)



## See Also

* GitHub page : https://github.com/mob-sakai/MeshEffectForTextMeshPro
* Releases : https://github.com/mob-sakai/MeshEffectForTextMeshPro/releases
* Issue tracker : https://github.com/mob-sakai/MeshEffectForTextMeshPro/issues
* Current project : https://github.com/mob-sakai/MeshEffectForTextMeshPro/projects/1
* Change log : https://github.com/mob-sakai/MeshEffectForTextMeshPro/blob/master/CHANGELOG.md
