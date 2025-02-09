# Purpose
This little utility project was build out of frustration.

The need is to deploy a number of small .Net web applications to the AWS cloud while trying to minimize costs. Each application can be deployed as-is several times to serve different domains. Deployed applications are expected to handle a small amount of traffic (typically a few thousand visitors on a daily basis).

There seem to be several ways to skin this cat, with deploying .Net applications as serverless or on ECS being among the options. For some reason, the option to host several applications on EC2 instances seems to be habitually overlooked. Well, that's what this project tries to facilitate.

## Entities

The basic abstraction is the **_Package_**. A package is simply a folder that contains a .Net application that is build and ready to be deployed. There can be several packages that correspond to different applications or to pre-build for different architectures. 

Another abstraction is the **_Host_**, which simply represents an EC2 instance. An instance is where packages are deployed and run. There can be several hosts configured where packages can be deployed.

Finally, an **_Application_** represents a specific package that is deployed on one of the configured hosts.

## What's included

Deploy Tools consist of two web applications.
* A simple web UI that allows configuration and control. The UI is used to define packages, hosts and applications, deploy the applications and view deploy logs.
* A batch job server using Hangfire. The bactch server deploys applications and takes down existing deployments.

## What can be deployed

Simple .Net web applications or APIs can be deployed by Deploy Tools. If an external database is required, a package can be combined with an **_RDS Package_** in order to create an RDS instance that an application can use.

## Assumptions & limitations

* Hosts run EC2 Linux and are preconfigured for SSH access.
* A package is build to run without the .Net framework installed. Different architecture selections can be used (x64, arm) but linux should be assumed to be the running host.
* An ALB is used to serve traffic from the internet to the EC2 instances.
* An application runs on a single host. If it fails, it fails.
* DOTNET_ENVIRONMENT is set to Production when deploying an application. All running configuration should, therefore, reside in appsettings.Production.json.
* For RDS packages with specific storage, no storage auto-scaling is allowed.
* Initial configuration of RDS subnets and security groups is not provided. These should be configured manually.
* Configuration of RDS packages is not validated. In particular, combinations of DB engines, versions and instances are assumed to be valid.

## WIP/next features

* ALB retrieves / for each deployed application to determine application health. More specific health check endpoints should be configured per package.
* Application logs should be retrieved and send to an ELK instance. Only logs written to standard output will be captured.
* Redeployment of an updated package should be supported.
* Moving a deployed application to another host should be supported.
* More environments should be supported, not just Production.
* Deployment/use of certificates should be supported.