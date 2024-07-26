# Changelog

Any notable changes to this project will be documented in this file.

## 1.1.0 - Alchemy Engine Core (2024-07-26)

This update focuses on the core of the Alchemy Engine. This allows for project level Configs and Services, which can be automatically initialized and destroyed with the project, and are controlled from the Project Settings, rather than objects in the scene.

- Created the Shadowpaw.Alchemy namespace
- Added Configs type for project-level configurations

## 1.0.0 - Initial Release (2024-07-25)

Initial release of Shadowpaw - Unity Toolkit

- `License`: Changed to the [UNLICENSE](https://unlicense.org) (Public Domain)
- `Core`: Added some magic code to enable the init and record keywords in Unity
- `Attributes`: Added Layer attribute for selecting a singular layer
- `Behaviours`: Added Bootstrapper for time sensitive initialization
- `Interfaces`: Added interfaces for IFactory, IFactory{T}, and ISubtypeFactory{TBase}
- `Interfaces`: Added interfaces for ILocator, ILocator{T}, and ISubtypeLocator{TBase}
- `Interfaces`: Added interfaces for IProvider, IProvider{T}, and ISubtypeProvider{TBase}
- `Types`: Added Proxy types for Unity's MonoBehaviour and ScriptableObject
- `Types`: Added Registry{T}, Registry{TKey, TValue}, and TypeRegistry for storing and retrieving objects
- `Types`: Added Singleton types for Behaviours and Objects
- `Types`: Added Timers for delayed and repeated actions
- `Types`: Added UnityID as a unique identifier for Unity objects
- `Utils`: Added extensions for LayerMask, List, Reflection, String, Task, and Type

Be good to each other 💕
