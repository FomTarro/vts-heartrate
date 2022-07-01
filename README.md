# VTS-Heartrate
A VTube Studio plugin that allows for connectivity between heart rate monitors (HRM) and VTube Studio!
 
[Download the latest version here!](https://skeletom-ch.itch.io/vts-heartrate) Or, in the [release section](https://github.com/FomTarro/vts-heartrate/releases) of this repo.

![Working Example](img/akari_gif.gif)
 
# Features

💓 Support for <b>many heart rate monitors</b> with [pulsoid.net](https://www.pulsoid.net), [hyperate.io](https://www.hyperate.io/) and [ANT+](https://www.thisisant.com/consumer/ant-101/what-is-ant)!  

💓 Configurable <b>model tinting</b> that scales with pulse!

💓 Automatic <b>expression and hotkey triggering</b> at desired heartrate thresholds!

💓 Custom <b>tracking parameters</b> for pulse and breath!

💓 NEW! <b>Plugin API</b> so that you can build your own apps that consume or write heartrate data!

# About
This plugin is developed by Tom "Skeletom" Farro. If you need to contact him, the best way to do so is via [Twitter](https://www.twitter.com/FomTarro).

If you're more of an email-oriented person, you can contact his support email: [tom@skeletom.net](mailto:tom@skeletom.net).

# Usage
Getting up and running is relatively straightforward. The plugin will automatically connect to VTube Studio on launch. From there, do the following steps:

* Input an estimated <b>minimum</b> and <b>maximum</b> heartrate.
* Select a desired heartrate <b>input method</b>. You can connect over bluetooth using your phone and [pulsoid.net](https://www.pulsoid.net) or [hyperate.io](https://www.hyperate.io/), or connect directly to your PC with an [ANT+](https://www.thisisant.com/consumer/ant-101/what-is-ant) USB dongle.
* Add [<b>Art Mesh Tint modules</b>](#art-mesh-tinting) and configure them to parts of your model!
* Add [<b>Expression and Hotkey Trigger modules</b>](#automatic-expression-triggering) and configure them to activate model functions automatically!
* Hook up [<b>Custom Tracking Parameters</b>](#custom-tracking-parameters), to your model for things like breathing speed!
 

 
## Heartrate Input Methods
In the interest of being widely accessible, this plugin features a wide set of possible input methods, which you may freely switch between at any time.
 
### Test Slider
The slider is primarily useful for quick testing of different heartrate values. The slider range is from 0 to 255.
 
### Read from File
This input method allows you to read heartrate data from an external file.
The file must simply contain the <b>numeric heartrate value</b> in <b>plain text</b>. File path must be absolute. Useful if you have another program that can output heartrate data.
 
### API WebSocket
This input method allows you to read heartrate data from the <b>Input API</b>. For more information about the API, consult the [API Documentation](#API).

### Pulsoid
[<b>Pulsoid</b>](https://www.pulsoid.net) is a free third-party app for Android/iOS which allows for easy, reliable connectivity to a wide set of heartrate monitors via the Bluetooth of your mobile device. Once you have a Pulsoid account, you can use this input method to collect heartrate data from the service.
 
By clicking the <b>'Login' button</b> in the plugin, you will be asked to grant this plugin permission to connect to your account. You will then be given an <b>'Authentication Token'</b> which you must paste into the plugin.
 
### ANT+
[<b>ANT+</b>](https://www.thisisant.com/consumer/ant-101/what-is-ant) is a low-power protocol supported by many sport and fitness sensors.

This input method allows for direct connection to your ANT+ device, provided that you have a <b>USB receiver</b> plugged in.

By clicking the <b>'Refresh' button</b>, this plugin will begin a continuous scan for devices that output heartrate data.
Then, simply select one from the dropdown and click the <b>'Connect' button</b>.

Please note that this plugin is not an officially licensed or certified affiliate of the ANT+ Brand.

### HypeRate
[<b>HypeRate</b>](https://www.hyperate.io/) is a free third-party app for Android/iOS which allows for easy, reliable connectivity to a wide set of heartrate monitors via the Bluetooth of your mobile device. Once you have a HypeRate account, you can use this input method to collect heartrate data from the service.

## Outputs
 
### Art Mesh Tinting
This output will gradually tint the matched <b>Art Meshes</b> the desired color, based on your current heartrate. For example, you can use this to make your model's face flushed for workouts, or run cold during horror games.
 
<b>Art Meshes</b> are matched as long as their names or tags contain <b>any</b> of the provided list of text to match. For example, providing the text '<b>head,mouth,neck</b>' will match Art Meshes with names like '<b>forehead</b>', '<b>outermouth2</b>', and '<b>leftneckside1</b>'.
Text to match must be <b>comma separated</b> and should <b>not contain spaces</b>.
 
If you are unsure of what your Art Meshes are named, a great web-tool was developed by <b>Hawkbar</b> called [<b>VTubeStudioTagger</b>](https://hawk.bar/VTubeStudioTagger/), which offers an intuitive way to discover the names of your model's Art Meshes.
 
![Working Example](img/color_setup.png)

### Automatic Expression Triggering
This output will cause an <b>Expression</b> to <b>activate or deactivate</b> when the current heartrate is <b>above or below a given threshold</b>, based on the selected behavior settings.

For example, the configuration in the provided image will cause the `angry` Expression to automatically activate when the heartrate exceeds 125 BPM, and will deactivate when the heartrate falls back beneath 125 BPM.

![Working Example](img/expression_trigger.png)

### Automatic Hotkey Triggering
This output will cause a <b>Hotkey</b> to be triggered when the current heartrate is <b>above or below a given threshold</b>, based on the selected behavior settings.
 
For example, the configuration in the provided image will cause the `panic` Item Scene to automatically toggle when the heartrate exceeds 125 BPM, and will toggle again when the heartrate falls back beneath 125 BPM.

![Working Example](img/hotkey_trigger.png)

### Custom Tracking Parameters
This plugin outputs <b>four custom tracking parameters</b> for use. They are as follows:
 
* `VTS_Heartrate_Linear`: A value that scales from 0.0 to 1.0 as your heartrate moves across the expected range.
* `VTS_Heartrate_Pulse`: A value that oscillates between 0.0 and 1.0 with a frequency exactly matching your heartrate.
* `VTS_Heartrate_Breath`: A value that oscillates between 0.0 and 1.0 with a frequency slower than `VTS_Heartrate_Pulse`, suitable for controlling your model's `ParamBreath` output parameter.
* `VTS_Heartrate_BPM`: A value that represents the actual current BPM from 0 to 255, rather than a normalized value from 0.0 to 1.0.

For more information on how to integrate these tracking parameters into your model, please refer to the [Official VTube Studio documentation](https://github.com/DenchiSoft/VTubeStudio/wiki/Plugins#what-are-custom-parameters).
 
![Custom Parameter setup](img/parameter_setup.png)

### Copy Profile Settings
As of version 1.0.0, settings are saved on a per-model basis. This feature allows you to <b>copy all of your output settings</b> (art mesh tints, expression triggers) from one model to your currently loaded model.

As a result, any settings configured for your currently loaded model will be permanently erased.

# API
As of version 1.2.0, vts-heartrate features its own <b>Plugin API</b>, so that you can build your own apps that consume or write heartrate data! That's right, this VTube Studio plugin now supports plugins of its own.
 
There are three underlying API endpoints, all accessible via WebSocket.
 
### Data API
 
The <b>Data API</b> is a read-only endpoint accessible at `ws://localhost:<your chosen port>/data`. Upon connecting to this endpoint, your WebSocket will receive a message containing current heartrate and output parameter data once per frame (60 times per second).
 
The message structure is as follows:
```
{
    apiVersion: "1.0",
    messageType: "DataResponse",
    timestamp: 1656382245785
    data: {
        heartrate: 103,
        parameters: {
            vts_heartrate_bpm: 103,
            vts_heartrate_pulse: 0.125
            vts_heartrate_breath: 0.354
            vts_heartrate_linear: 0.650
        },
        tints: [
            {
                baseColor: { r: 255, g: 128, b: 128, a: 255 },
                currentColor: { r: 255, g: 200, b: 200, a: 255 },
                matchers: ['head', 'face']
            },
            {
                baseColor: { r: 255, g: 128, b: 128, a: 255 },
                currentColor: { r: 255, g: 200, b: 200, a: 255 },
                matchers: ['mouth', 'neck']
            }
        ]
    }
}
```
For more information about the output parameters, consult the [Custom Tracking Parameter Documentation](#Custom-Tracking-Parameters) and [Art Mesh Tinting Documentation](#Art-Mesh-Tinting).
 
### Event API
 
The <b>Event API</b> is a read-only endpoint accessible at `ws://localhost:<your chosen port>/events`. Upon connecting to this endpoint, your WebSocket will receive a message containing event information every time an [Expression Trigger](#Automatic-Expression-Triggering) or [Hotkey Trigger](#Automatic-Hotkey-Triggering) is triggered.
 
For <b>Expressions</b>, the message structure is as follows:
```
{
    apiVersion: "1.0",
    messageType: "ExpressionEventResponse",
    timestamp: 1656382245785
    data: {
        threshold: 120,
        heartrate: 121,
        expression: "angry.exp3.json",
        behavior: 2,
        activated: true
    }
}
```
 
The complete list of possible `behavior` values is as follows:
```
UNKNOWN = -1,    
ACTIVATE_ABOVE_DEACTIVATE_BELOW = 0,
DEACTIVATE_ABOVE_ACTIVATE_BELOW = 1,
ACTIVATE_ABOVE = 2,
DEACTIVATE_ABOVE = 3,
ACTIVATE_BELOW = 4,
DEACTIVATE_BELOW = 5,
```
 
For <b>Hotkeys</b>, the message structure is as follows:
```
{
    apiVersion: "1.0",
    messageType: "HotkeyEventResponse",
    timestamp: 1656382245785
    data: {
        threshold: 120,
        heartrate: 121,
        hotkey: "Blush",
        behavior: 1
    }
}
```
 
The complete list of possible `behavior` values is as follows:
```
UNKNOWN = -1,    
ACTIVATE_ABOVE_ACTIVATE_BELOW = 0,
ACTIVATE_ABOVE = 1,
ACTIVATE_BELOW = 2,
```
 
### Input API
 
The <b>Input API</b> is a read and write endpoint accessible at `ws://localhost:<your chosen port>/input`. Upon connecting to this endpoint, your WebSocket will be able to write heartrate data for use with the [API WebSocket](#API-WebSocket) input method.
 
#### Authenticating Your Plugin
 
First, you will need to authenticate your client before you are granted write permission. In order to authenticate, you must first <b>request a token</b> by sending a message with the following structure:
```
{
    messageType: "AuthenticationRequest",
    data: {
        pluginName: "My Heartrate Plugin",
        pluginAuthor: "Skeletom",
    }
}
```
 
The user will then be prompted to either Approve or Deny access to this plugin. If the request is approved, the API server will then respond with a message containing a token.
 
You only need to do this step if your plugin has not already been granted a token. If it has a token already, you can skip to the next step.
 
The message structure is as follows:
```
{
    apiVersion: "1.0",
    messageType: "AuthenticationResponse",
    timestamp: 1656382245785
    data: {
        pluginName: "My Heartrate Plugin",
        pluginAuthor: "Skeletom",
        token: "e404d80b-c160-4af4-a0b9-9c0159f3010e",
        authenticated: false
    }
}
```
 
It is strongly recommended that you save this token so that your plugin may use it again in the future. Finally, you must now submit an authentication request using the token, by sending a message with the following structure:
```
{
    messageType: "AuthenticationRequest",
    data: {
        pluginName: "My Heartrate Plugin",
        pluginAuthor: "Skeletom",
        token: "e404d80b-c160-4af4-a0b9-9c0159f3010e"
    }
}
```
 
Assuming you have provided a token that the user has granted access for, the API Server will finally respond with a message indicating that your plugin is fully authenticated, and that you may begin writing heartwrate data.
 
The message structure is as follows:
```
{
    apiVersion: "1.0",
    messageType: "AuthenticationResponse",
    timestamp: 1656382245785
    data: {
        pluginName: "My Heartrate Plugin",
        pluginAuthor: "Skeletom",
        token: "e404d80b-c160-4af4-a0b9-9c0159f3010e",
        authenticated: true
    }
}
```
 
#### Writing Heartrate Data
 
Once your plugin is fully authenticated, you can write heartrate data sending a message with the following structure:
```
{
    messageType: "InputRequest",
    data: {
        heartrate: 78
    }
}
```
 
### Errors
 
In the event that something goes wrong, such as the API Server receiving a message it cannot parse, or a message from an unauthenticated client, your WebSocket will receive an error message.
 
The message structure is as follows:
```
{
    apiVersion: "1.0",
    messageType: "ErrorResponse",
    timestamp: 1656382245785
    data: {
        errorCode: 403,
        message: "This client is not authenticated!"
    }
}
```
 
The complete list of possible `errorCode` values is as follows (they are HTTP status codes):
```
OK = 200,
BAD_REQUEST = 400,
FORBIDDEN = 403,
SERVER_ERROR = 500,
```



# Roadmap
 
Planned features include the following:
* Localization into additional languages
* More robust system logging

