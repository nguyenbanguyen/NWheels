Linux|Windows|Coverage
-----|-------|--------
TBD|[![Build status](https://ci.appveyor.com/api/projects/status/qcm23727dm8dplk5/branch/master?svg=true)](https://ci.appveyor.com/project/felix-b/nwheels-bk3vs/branch/master)|[![Coverage Status](https://coveralls.io/repos/github/felix-b/NWheels/badge.svg?branch=master)](https://coveralls.io/github/felix-b/NWheels?branch=master)

Welcome to NWheels
=======

Based on our experience, commonality in the needs of enterprise application projects is significantly higher than variability. 

We take this as an opportunity to build a community-based ecosystem, which implements A-to-Z architectural recipes, constructs technology stacks, creates concise programming models, and develops adjustable building blocks for common problem domains. 

We put those pieces together to turn enterprise application development into an easy win.

### How it works

_DISCLAIMER: we're in the middle of development. Some features listed below may not yet exist, or be unstable_. 

Developers:|NWheels:
---|---
Design an application as a set of microservices|Packages and deploys microservice containers to runtime environments. Handles scalability and fault tolerance independently of your cloud vendor.
Code and annotate business domains of the application in C#, abstracted from concrete technology stacks.|Implements a mix of [hexagonal architecture](http://alistair.cockburn.us/Hexagonal+architecture) with _prefer convention over implementation_. Eliminates mechanical and repetitive coding of adapter layers, replacing it with pluggable code generation. Examples: data access, serialization, network communication, RESTful APIs, GraphQL queries, etc.
Code and annotate conceptual UI models in C#, abstracted from concrete technology stacks. Use numerous UI themes. Directly tweak UI code and assets wherever unique touch is necessary.|Generates UI applications for target interaction platforms, including web, mobile, desktop, IVR, SmartTV, and IoT. Transparently handles UI model bindings to data and business capabilities, including entry validation and enforcement of authorization requirements. Makes internationalization transparent and trivial for both developers and translators.    
Declare cross-cutting requirements like authorization and event logging, through concise C# programming models|Transparently implements and enforces the requirements throughout all execution paths. For instance, event logging includes application-defined BI measurements, usage statistics, circuit breakers, alerts, and built-in cost-free performance profiling.  
Pick technology stack for each microservice|Generates integration layers of domain objects with selected technology stacks. Generates concrete implementations of declarative models. Certain technology stacks enable advanced distribution scenarios, such as parallel execution, elastic scalability, and actor/data grids. 
Compose the product out of pluggable features. Use features for both core product and customization layers. Through features, extend and override all aspects of system presentation, communication, and behavior.|Allows flexible vertical and horizontal composition of domain objects and user interfaces. Releases product features and customizations as pluggable NuGet packages into your project NuGet repo. Smoothly supports distributed development workflows and remote professional services outside of product vendor organization. 
When coding business domains and UI, reuse ready domain building blocks supplied by NWheels, and avoid reinventing the wheel.|Captures expertise in common problem domains (e.g. e-commerce, booking, marketing, CRM, and many others) into reusable _domain building block_ modules, based on well established and field-proven patterns and designs. Makes building blocks inheriteble, extensible, and easily adjustable to specific application requirements.  

# Demo

NWheels is already capable of bootstrapping a microservice with partially implemented web technology stack.

Imagine a very simple application:
- A single page web app, which lets user enter her name, and submit it with a button. 
- A microservice, which handles the submission. The microservice exposes RESTful API invoked by the web app button. 
- Business logic (_transaction script_), which receives user's name, and responds with a greeting text. The greeting text is then displayed in the web app.

NWheels-based implementation is below 50 lines of C# code, all layers included. 

_Note that web client implementation is a mockup prototype -- the real web client stack has yet to be developed._

## Running the demo 

### System requirements

- Running on your machine:
  - Linux, Windows, or macOS machine 
  - .NET Core SDK 1.1 or later ([download here](https://www.microsoft.com/net/download/core))

- Running in Docker (Linux container):
  ```bash
  $ docker run --name nwheels-demo -p 5000:5000 -it microsoft/dotnet:1.1-sdk /bin/bash
  ```

### Get sources and build

  ```bash
  $ git clone https://github.com/felix-b/NWheels.git nwheels
  $ cd nwheels/Source/
  $ dotnet restore
  $ dotnet build
  ```

### Run microservice

  ```bash
  $ dotnet NWheels.Samples.FirstHappyPath.HelloService/bin/Debug/netcoreapp1.1/hello.dll
  ```
  
### Open web application

- If running on your machine: 
  - Browse to [http://localhost:5000](http://localhost:5000)
- If running in docker container: 
  - Print container IP address:
    ```bash
    $ docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' nwheels-demo
    ```
  - Browse to http://_container_ip_address_:5000
 
## Source code explained

#### Program.cs - microservice entry point

It is super simple to bootstrap a microservice. Most of the time, you're all set with the defaults. For advanced scenarios, extensible API of `MicroserviceHostBuilder` lets you tailor technology stack to your requirements. 

```csharp
public static int Main(string[] args)
{
    var microservice = new MicroserviceHostBuilder("hello")
        .AutoDiscoverComponents()
        .UseDefaultWebStack(listenPortNumber: 5000)
        .Build();

    return microservice.Run(args);
}
```

#### HelloWorldTx.cs - business logic

Business logic for this demo is trivial. It is captured in a _transaction script component_ class. 

```csharp
[TransactionScriptComponent]
[SecurityCheck.AllowAnonymous]
public class HelloWorldTx
{
    [TransactionScriptMethod]
    public async Task<string> Hello(string name)
    {
        return $"Hello world, from {name}!";
    }
}
```

There's more under the hood, though. For instance, default web stack includes RESTful API endpoint, where transaction scripts are one type of supported resources. The endpoint transparently allows invocation of resources through HTTP and other protocols, subject to authorization requirements.

Here, `Hello` method can be invoked through HTTP request:

```HTTP
POST http://localhost:5000/tx/HelloWorld/Hello HTTP/1.1
User-Agent: Fiddler
Host: localhost:5000
Content-Length: 17

{"name": "NWheels"}
```
The endpoint will reply as follows:

```HTTP
HTTP/1.1 200 OK
Date: Wed, 05 Jul 2017 05:40:55 GMT
Content-Type: application/json
Server: Kestrel
Content-Length: 39

{"result":"Hello world, from NWheels!"}
```

### Authorization

It worths noting that `[SecurityCheck.AllowAnonymous]` attribute here is required to allow access without prior authentication and validation of claims. 

Authorization infrastructure of NWheels transparently enforces access control rules to resources, components, and data throughout all execution paths. The rules can either be declared with attributes (like in this example), or configured through access control API. Depending on application requirements, configuration through the API can either be hard-coded, or based on data in a persistent storage (e.g. DB).

#### HelloWorldApp.cs - web app

The next piece is user interface. NWheels dramatically boosts development and maintenance productivity by supporting declarative UI. The UI is declared through high-level conceptual models, abstracted from concrete technology stacks. 

The models focus on UI structure, navigation, and binding to business data and capabilities. Lower-level front-end/UX and client/server communication details are not concerned on this level. 

Auhtorization rules that control access to bound data and capabilities are automatically reflected in the user interface.

```csharp
[WebAppComponent]
public class HelloWorldApp : WebApp<Empty.SessionState>
{
    [DefaultPage]
    public class HomePage : WebPage<Empty.ViewModel>
    {
        [ViewModelContract]
        public class HelloWorldViewModel 
        {
            [FieldContract.Required]
            public string Name;
            [FieldContract.Semantics.Output, FieldContract.Presentation.Label("WeSay")]
            public string Message;
        }

        [ContentElement] 
        [TransactionWizard.Configure(SubmitCommandLabel = "Go")]
        public TransactionWizard<HelloWorldViewModel> Transaction { get; set; }

        protected override void ImplementController()
        {
            Transaction.OnSubmit.Invoke<HelloWorldTx>(
                tx => tx.Hello(Transaction.Model.Name)
            ).Then(
                result => Script.Assign(Transaction.Model.Message, result)
            );
        }
    }
}
```
Stunning high-usability user interfaces are created separately by UX experts in corresponding interaction platforms. The experts build UI technology stacks, and provide code generators that implement UI models on top of those stacks. User interfaces are allowed to have numerous themes and variations. 

Sometimes though, all this is not enough. Certain UI areas demand unique touch. In such cases, parts of generated platform-specific code and assets can be manually adjusted or replaced. 

Besides the web, we aim to support mobile native apps, desktop apps, SmartTV, IVR, and IoT platforms. 

# Getting Involved

Impressed? We'd like having you onboard!

Community is a vital part of the NWheels project. Here we are building a welcoming and friendly ecosystem for contributors.

Please make yourself familiar with our [Code of Conduct](CODE_OF_CONDUCT.md).

## Where to start

1. Run the demo
1. Carefully read our [Contribution Guidelines](CONTRIBUTING.md).
1. Join our team on Slack:
   - Send an email with subject `Join NWheels team` to [team@nwheels.io](mailto:nwheels.io). You will receive back an email from Slack with join link and instructions.
1. Read our [Roadmap](docs/Wiki/roadmap.md). Look through **Contribution Areas** section and choose areas you're interested in contributing to.
1. Start from resolving some issues, preferably those labeled  `first-timers`. 
1. Please feel free to communicate your thoughts and reach out for help.

# Current Status

Starting from February 2017, we are developing our second take at NWheels. 

### Current milestone: 01 - First Happy Path

- [Milestone](https://github.com/felix-b/NWheels/milestone/2)
- [Scrum board](https://github.com/felix-b/NWheels/projects/1)
- [Issues](https://github.com/felix-b/NWheels/issues?utf8=%E2%9C%93&q=is%3Aissue%20is%3Aopen%20milestone%3A%2201%20First%20happy%20path%22%20)

# History

The first take at NWheels was named _Milestone Afra_. It is now in use by two proprietary real-world applications. Further development was abandoned for high technical debt, few architectural mistakes, and in favor of targeting cross-platform .NET Core.

### Concept proven

Applications built on top of NWheels milestone Afra shown us that the core concept is correct and robust. With that, we learned a lot of lessons, and faced few mistakes in architecture and implementation.

### Timeline

Year|Summary
-|-
2013|Started development of [Hapil](https://github.com/felix-b/Hapil) library for code generation, which is an essential part of NWheels concept.
2014|Hapil library gained enough features. Started development of NWheels milestone Afra. Implemented server bootstrapping and metadata-based composition of domain objects. Added support for data persistence through Entity Framework.
2015|Development of NWheels milestone Afra continued. Added support for Mongo DB. Started development of model-based UI, and web UI stack based on a Bootstrap theme, AngularJS, and ASP.NET Web API.
2016|NWheels milestone Afra reached enough maturity to support full-stack development. Two proprietary real-world applications developed on top of NWheels milestone Afra: one released to production, one is in the beta stage. These applications proved that the concept of NWheels works, but taught us a few lessons.
2017|Further development of NWheels milestone Afra abandoned; started development of second take at NWheels, completely from scratch.

