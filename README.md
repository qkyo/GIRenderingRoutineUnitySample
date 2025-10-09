# GIRenderingRoutineUnitySample

![image](https://github.com/qkyo/GIRenderingRoutineUnitySample/blob/main/Readme/banner.gif)

### Rendering Pipeline
![image](https://github.com/qkyo/GIRenderingRoutineUnitySample/blob/main/Readme/manual_flowchart_ad.png)
    
### Prerequisites
* Unity Version: 2021.3.18f1

### File Structure
```
Assets
├── RT Render Pipeline
│   ├── Editor                 # Assets and scripts that are edited and 
│   │                            compiled in the Unity Editor.
│   ├── External               # Native plugins.
│   ├── Runtime                # Assets and scripts that are directly loaded
│   │                            and executed during the game runtime.
│   ├── Shader                 # The ray tracing shader.
│   └── ShaderLibrary          # Shader function libraries, shader define, 
│                                and include files.
├── Scenes
│   └── Fox   
│       ├── FoxScene.unity     
│       ├── Animation          # Models animation.
│       ├── glb                # Models we use in sample scene.
│       ├── GUI                # GUI components and scripts.
│       └── Materials          # Models’ material.
└── StreamingAssets            # Local cache file.
```
