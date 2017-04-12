# ChatBotService-HomeAutomation
ChatBot for HomeAutomation Integration with the SmartThings App
Author: Jeremy Brooks

Overview
This was a project a worked on to integrate Azure Bot Framework with a SmarthThings app to be able to control my devices across multiple platforms using different forms of chat such as Webchat, Skype, and FB messenger. To get started I assume you have a SmartThings hub and access to create your own applications using the SmartThings SDK. The commands are based on some basic syntax, when using chat you must you the basic follwoing basic commands:

turn on *room name*                <------------turns on the room light
turn off *room name*               <------------turns off the room light
turn on *room name* 50             <------------turns on the room light and adjusts the brightness based on a scale of 0-100
*any other command not starting with "turn on" or "turn off" will return the status of your resources"

The commandlets specifically look for the syntax "turn on/off" otherwise it will return the current status of all your device whether they are on or off and what the dimming level is on. I have not tested this with other resources besides lights, but I believe you should be able to control any device that has on/off and value configurations. 

All the private information will be stored in the Application Settings of the Azure Web App such as the token, Microsoft AppID, AppPassword, and SmartThings Subscription. 

In order to get the token I used the Postman to generate it, see this link for creating the token using Postman: https://blog.brooksjc.com/2017/01/30/smartthings-api-creating-a-custom-app-testing-out-the-api-with-postman-part-3/

SmarthThings Applicaiton with API commandlets.
https://github.com/jcbrooks92/SmartThingsCode-HomeAuto 

Azure Botframework
https://docs.botframework.com/en-us/#navtitle
