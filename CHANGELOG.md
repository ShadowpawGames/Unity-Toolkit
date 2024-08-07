# Changelog

Any notable changes to this project will be documented in this file.

## 1.1.2 - Version Change (2024-08-07)

Updated to Unity 2023+ to include the new "Awaitable" system, removing the reliance on UniTask.  

## 1.1.1 - Toolkit Additions (2024-08-01)

Focused on adding simple utilities to the toolkit.

- `Types`: Added IPrimitiveValue to allow for primitive values to be serialized in Unity.
  - Allows the user to specify any type of primitive value in the inspector.
  - Automatically serializes the value to the correct type.
  - Useful for lists and dictionaries of primitive values.

## 1.1.0 - Alchemy Engine Core (2024-07-26)

This update focuses on the core of the Alchemy Engine. This allows for project level Configs and Services, which can be automatically initialized and destroyed with the project, and are controlled from the Project Settings, rather than objects in the scene.

- Created the Shadowpaw.Alchemy namespace
- Added Alchemy Engine and associated types
- Added Config types for project-level configs 
- Added Service types for project-level services
- Added Injector utility for dependency injection
- Added Injector Contexts for scoped dependency injection

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
