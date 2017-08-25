# Qualisys Viewfinder 2.0
A cross-platform mobile framework for real-time motion capture applications.

## Source code commit conventions 
Tested platforms for each commit must always be specified in the form of:

__Tested on:__

E.g. __Tested on:__ Android, iOS

Regarding code, the following symbols always precede the description:

* __*__ for changes
* __+__ for new additions
* __-__ for code deletion

## For future developer people!

There is still some work to be done and features to be added to the new Viewfinder.

__Important__
Over the development of the application, a few lines of code were added and edited to the RT-SDK in the solution. Do not overwrite these, otherwise you'll have a bad time.
These changes are mainly serialization attributes and public getters and setters for data binding. Just merge the new changes from the RT-SDK carefully and you should be fine.

__More Features!__

- Implement auto-exposure on CameraPage in the settings drawer. Gotta update RT-SDK to access this functionality.
- Store previous session's IP and master password.
- Allow user to change every camera stream mode and settings from grid view (som QTM).

__Known Issues__

- The UrhoSurface (3D application in the cameraPage) will sometimes stop listening to input touches after going back and forth a couple of times.
  This was probably caused by wrongful event subscribing and unsubscribing. A fix was pushed on Aug 24th to help with this, but it should still be looked for.
	
- When opening and initializing the 3D application (CameraApplication), there can sometimes be some problems where the markers want to be used but the 
  polygons that comprise a marker circle are yet not created. This exception is caught, but could slow down startup.
  
- Probably the most prominent crash is when going back from the CameraPage. This should be due to improper object disposal and the way the 3d urhoSurface is being terminated.

- There have been some instances of a random crash where some Audio component of Urho's implementation of SDL for Android crashes when it tries to write to
  some buffer. This is stupid because no audio is being used and this component is never initialized. Look into how to strip this from the app during compilation time.
  Or maybe it's a known bug?

__Tips__ 

- Watch out for synchronizaton issues. A separate task is being run for every type of stream during a real-time connection.
- There are also several active QTMNetworkConnections running in parallel
- Update your Nuget packages frequently! 
