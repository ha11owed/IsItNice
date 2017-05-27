# IsItNice
The result of a 24 hours Windows 8 Hackaton from 2011.

## About

This repository contains a WinRT Application developed after a Microsoft Hackaton on the subject in 2011.
This was our first contact with WinRT, and we worked in a timebox manner (24 hours, although the solution contains improvements/fixes to the original code).

The application enables its users to:
* Take pictures of products
* Send it to friends from the contacts that also have the application
* Allows one to comment the picture.

Yes, the application was developed in a pre-WhatsApp world :bowtie: \

The application has the following components:
*   A WinRT client application with support for push/pull azure notifications
*   A Windows Azure backend for
  * the authentification
  * connecting users based on their contact lists
  * storing the images of the users
  * pushing notifications to the client

## Repository structure:

```
.
+-- Code\
|   +-- ClientLogic\
|   +-- IsItNiceGUI\
|   +-- Libs\
|   +-- IsItNice.sln
+-- Presentation\
|   +-- resources\
|   +-- Win8 App Presentation.pdf
```

As seen in the presentation, the team consisted of 3 members.
