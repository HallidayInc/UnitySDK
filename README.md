<p align="center">
<br />
<a href="https://halliday.xyz"><img src="https://github.com/HallidayInc/UnitySDK/blob/master/hallidayLogo.svg" width="150" alt=""/></a>
<br />
</p>
<h1 align="center">Halliday Unity SDK</h1>
<br />

# Installation

Download the latest `.unitypackage` from the [releases](https://github.com/HallidayInc/UnitySDK/releases) page.

Right click the **Assets** in your Unity Project and select the `.unitypackage` you downloaded. Select the files and import them into your project.

The package comes with a few sample scripts that you can modify and run to use to the SDK.

Note: The Newtonsoft DLL is included as part of the Unity Package, feel free to deselect it if you already have it installed as a dependency to avoid conflicts.

# Build

Add the HallidayClient script to a GameObject (if you attach the sample script, the HallidayClient script will be added automatically). Call the Initialize() function with your Halliday API key and use the Client's public API function to manage your user's blockchain actions.

# Usage

The sample scripts explain how to use the SDK.
