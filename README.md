<p align="center">
<br />
<a href="https://halliday.xyz"><img src="https://github.com/HallidayInc/UnitySDK/blob/master/hallidayLogo.svg" width="100" alt=""/></a>
</p>
<h1 align="center">Halliday Unity SDK</h1>

# Installation

Download the latest `.unitypackage` from the [releases](https://github.com/HallidayInc/UnitySDK/releases) page.

Right click the **Assets** in your Unity Project, select **Import Package**, and select the `.unitypackage` you downloaded. Make sure you select all the files and import them. You can now find the **HallidayClient** in your **Plugins** folder.

The package comes with a walkthrough script that you can modify and run to use to the SDK.

Note: The Nethereum DLL is included as part of the Unity Package, feel free to deselect it if you already have it installed as a dependency to avoid conflicts.

# Build

Add the HallidayClient script to a GameObject (if you attach the sample script, the HallidayClient script will be added automatically). Call the Initialize() function with your Halliday API key and use the Client's public API function to manage your user's blockchain actions.

# Usage
