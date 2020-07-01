This file demonstarates how to register an app on OneDrive to generate the required tokens for C2 communication.

1. Go to url -  
	https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationsListBlade 

2. Create a new Application.  
	<img src="./UsageImages/OneDrive/Register.png"/>
	Give it any name and a valid url for redirect_url values.  

3. Grant the necessary API permissions  
	<img src="./UsageImages/OneDrive/Permissions.png"/>  
	
4. Goto the url -  
	https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id=<application_id>&scope=<requested_scope>&redirect_uri=<redirect_uri>&response_type=token  
	Replace Client_id with Application ID of the registered application.  
	Make sure redirect_uri string has the same value from registration.  

5. The new URL should contain the access_token  
	<img src="./UsageImages/OneDrive/token.png"/>
