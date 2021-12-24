Fix docker build not showing any output from commands:
```
export DOCKER_BUILDKIT=0
```

Run docker-compose
```
docker-compose -f "docker-compose-agent.yml" up -d --build
```


For Development:

Use Visual Studio Community 2022 because C:\Program Files\dotnet\sdk\6.0.101\Sdks\Microsoft.NET.Sdk\targets\Microsoft.NET.Sdk.DefaultItems.targets(134,5): warning NETSDK1182: Targeting .NET 6.0 in Visual Studio 2019 is not supported.

Run installer Visual Studio Community 2022 - https://docs.microsoft.com/ru-ru/visualstudio/releases/2022/release-notes

Choose workload 1) ASP .Net and web development and 2) .Net desktop development

Choose individual component

![image](https://user-images.githubusercontent.com/10828883/146935845-0c7d20fd-1169-4ce2-a8b7-065480da50ea.png)

![image](https://user-images.githubusercontent.com/10828883/146935918-04b6d149-c729-43c0-8d28-25b3c24414a5.png)

![image](https://user-images.githubusercontent.com/10828883/146935978-9b333d1d-3248-4395-aba5-605b50b4fc7c.png)

![image](https://user-images.githubusercontent.com/10828883/146935999-72f72bcd-e63c-4462-8df4-7b4f841d4fa2.png)

![image](https://user-images.githubusercontent.com/10828883/146936041-06d7d56d-5183-46e5-a102-75a7f1dd7c7c.png)


or

![image](https://user-images.githubusercontent.com/10828883/146936108-19ec64a2-8b7c-46c3-aada-49284fd8c87f.png)

Configuring NuGet Package Sources:

In Visual Studio, go to Tools > Options and then select Package Sources under the NuGet Package Manager.

Choose the Add icon (+), edit the Name - nuget.org, and then https://api.nuget.org/v3/index.json in the Source Click Update after updating the feed link.

