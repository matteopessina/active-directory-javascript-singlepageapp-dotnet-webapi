Getting Started
====================
This is a fork of https://github.com/Azure-Samples/active-directory-javascript-singlepageapp-dotnet-webapi.git

This sample demonstrates the use of ADAL for JavaScript for securing an single page app written independently of any frameworks, implemented with an ASP.NET Web API backend. Moreover, it illustrates the use of **AppRoles** for performing **Authorization on users** (https://docs.microsoft.com/en-us/azure/architecture/multitenant-identity/app-roles). This changed what this sample application is: no more a todo-list app but an application that allows users with appRole *TaskCreator* to create, edit and view tasks and users with appRole *TaskReader* to just see them. 

ADAL for Javascript is an open source library.  For distribution options, source code, and contributions, check out the ADAL JS repo at https://github.com/AzureAD/azure-activedirectory-library-for-js.

For more information about how the protocols work in this scenario and other scenarios, see [Authentication Scenarios for Azure AD](http://go.microsoft.com/fwlink/?LinkId=394414).

## How To Run This Sample

Getting started is simple!  To run this sample you will need:
- Visual Studio 2017
- An Internet connection
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, please see [How to get an Azure AD tenant](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/) 
- A user account in your Azure AD tenant. This sample will not work with a Microsoft account, so if you signed in to the Azure portal with a Microsoft account and have never created a user account in your directory before, you need to do that now.

### Step 1:  Clone or download this repository

From your shell or command line:
`git clone https://github.com/matteopessina/active-directory-javascript-singlepageapp-dotnet-webapi.git`

### Step 2:  Register the sample with your Azure Active Directory tenant

1. Sign in to the [Azure portal](https://portal.azure.com).
2. On the top bar, click on your account and under the **Directory** list, choose the Active Directory tenant where you wish to register your application (I created a new directory for this test).
3. Click on **More Services** in the left hand nav, and choose **Azure Active Directory**.
4. Click on **App registrations** and choose **Add**.
5. Enter a friendly name for the application, for example 'SinglePageApp-jQuery-DotNet' and select 'Web Application and/or Web API' as the Application Type. For the sign-on URL, enter the base URL for the sample, which is by default `https://localhost:44302/`. Click on **Create** to create the application.
6. While still in the Azure portal, choose your application, click on **Settings** and choose **Properties**.
7. Find the Application ID value and copy it to the clipboard.
8. For the App ID URI, enter `https://<your_tenant_name>/SinglePageApp-jQuery-DotNet`, replacing `<your_tenant_name>` with the name of your Azure AD tenant. 
9. Grant permissions across your tenant for your application. Go to Settings -> Required Permissions, and click on the **Grant Permissions** button in the top bar. Click **Yes** to confirm.

### Step 3:  Enable the OAuth2 implicit grant and AppRoles

By default, applications provisioned in Azure AD are not enabled to use the OAuth2 implicit grant. In order to run this sample, you need to explicitly opt in.

1. From the former steps, your browser should still be on the Azure portal.
2. From the application page, click on **Manifest** to open the inline manifest editor.
3. Search for the `oauth2AllowImplicitFlow` property. You will find that it is set to `false`; change it to `true`.
4. Create two GUID. One way to do that is from powershell: 
```
PS > [guid]::newGuid()
```
5. Add the following two AppRoles replacing the `GUID_1`, `GUID_2` placeholders and save the manifest.  
```json
"appRoles": [  
    {  
      "allowedMemberTypes": [
        "User"
      ],
      "displayName": "TaskCreator",
      "id": "GUID_1",
      "isEnabled": true,
      "description": "Can create, edit, delete task",
      "value": "TaskCreator"
    },
    {
      "allowedMemberTypes": [
        "User"
      ],
      "displayName": "TaskReader",
      "id": "GUID_2",
      "isEnabled": true,
      "description": "Can read task",
      "value": "TaskReader"
    }
  ]
```

### Step 4: Add users and assign appRoles

1. Go in **Azure Active Directory -> Users** and register two users. In case of guest ones you should validate the accounts checking in the inbox the validation email.
2. Go in **Azure Active Directory -> Enterprise applications** and click on the previously registered application.
3. Click on **Users and groups** and assign the *TaskReader* appGroup to both the users while assign the *TaskCreator* to just one. Note that a record is needed per assignment, thus you will have three records in the end.

### Step 5:  Configure the sample to use your Azure Active Directory tenant

1. Open the solution in Visual Studio 2017.
2. Create a file named ConfidentialAppSettings.config with the following code
```xml
<appSettings>
  <add key="ida:Tenant" value="[Enter your tenant here, e.g. contoso.onmicrosoft.com]" />
  <add key="ida:Audience" value="[Enter your client Id here, e.g.b g075edef-0efa-453b-997b-de1337c29185]" />
</appSettings>
``` 
3. Find the app key `ida:Tenant` and replace the value with your AAD tenant name.
4. Find the app key `ida:Audience` and replace the value with the Client ID from the Azure portal.
5. Open the file `App/Scripts/appVariables.js`.
6. Replace the value of `tenant` with your AAD tenant name.
7. Replace the value of `clientId` with the Client ID from the Azure portal.

### Step 5:  Run the sample

Clean the solution, rebuild the solution, and run it. 

You can trigger the sign in experience by either clicking on the sign in link on the top right corner, or by clicking directly on the Todo List tab.
Explore the sample by signing in, adding items to the To Do list, removing the user account, and starting again. 

With the user with *TaskCreator* appRole you will be able to create, edit and delete tasks, while with the other user you will be able to just read them.

## How user authorization is implemented

Since the appRoles assigned to the users are included as claims in their JWT access tokens, I implemented the `AppRolesAuthorizationAttribute`.  
```csharp
public class AppRolesAuthorizationAttribute : AuthorizationFilterAttribute
{
  public string AppRoles { get; set; }

  public override Task OnAuthorizationAsync(HttpActionContext actionContext, System.Threading.CancellationToken cancellationToken)
  {
    List<string> roles = AppRoles.Split(',').ToList();

    var principal = actionContext.RequestContext.Principal as ClaimsPrincipal;

    if (!principal.Identity.IsAuthenticated)
    {
      actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
      return Task.FromResult<object>(null);
    }

    if (!(principal.HasClaim(x => x.Type == "roles" && roles.Contains(x.Value))))
    {
      actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
      return Task.FromResult<object>(null);
    }

    //User is Authorized, complete execution
    return Task.FromResult<object>(null);
  }
}
```
I decorated the `TodoListController`, which is the endpoint for the CRUD operations on *Todo* items, with the previous authorization attribute, stating that only users with the claim whose type is *roles* and value is *TaskReader* can access to the not further decorated methods exposed by this class.
```csharp
[AppRolesAuthorization(AppRoles="TaskReader")]
public class TodoListController : ApiController
```
In fact, while I left the `Get` methods undecorated, I applied a further constraint on the methods corresponding to the Create-Delete-Update operations, that is the users need to be *TaskCreator* to call them.
```csharp
// POST: api/TodoList
[AppRolesAuthorization(AppRoles="TaskCreator")]
public void Post(Todo todo)
```

## Show different UI parts based on the user's appRole(s)

To prevent showing buttons or other UI parts that users without the *TaskCreator* appRoles cannot use, such as the Add, Edit, Delete buttons in the todo area, we take advantage of the access token format: JWT is just a json string encoded. 

## About the Code

The key files containing authentication logic are the following:

**App.js** - Provides the app configuration values used by ADAL for driving protocol interactions with AAD, indicates which routes should not be accessed without previous authentication, issues login and logout requests to Azure AD, handles both successful and failed authentication callbacks from Azure AD, and displays information about the user received in the id_token.

**index.html** - contains a reference to adal.js.

**todoListCtrl.js**- shows how to take advantage of the acquireToken() method in ADAL to get a token for accessing a resource.

**userDataCtrl.js** - shows how to extract user information from the cached id_token.

