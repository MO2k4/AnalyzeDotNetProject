# Analyze Dotnet Project Dependencies

When developing dotnet and upgrading to the new csproj-format, i had to find a way to cleanup nuget transitive dependencies.

![](https://www.erikheemskerk.nl/content/images/2017/09/TransitiveReferencesDotNetCore.png | width=300)

I stumbled upon this thread ([Part 1](https://www.jerriepelser.com/blog/analyze-dotnet-project-dependencies-part-1/)/[Part 2](https://www.jerriepelser.com/blog/analyze-dotnet-project-dependencies-part-2/)) from Jerrie Pelser. He wrote a application to visualize the dependency tree of a dotnet-Project.

I added some logic to output the irrelevant dependencies, to safely remove them. If dependency D is imported already by dependency C, the direct reference can be removed. This gives us a cleaner NuGet-Dependency-Tree.

## Links:

[Upgrade to new Project-Format](https://natemcmaster.com/blog/2017/03/09/vs2015-to-vs2017-upgrade/)

[Upgrade Tooling to new project format](https://github.com/hvanbakel/CsprojToVs2017)

[Transitive NuGet dependencies: .NET Core’s got your back](https://www.erikheemskerk.nl/transitive-nuget-dependencies-net-core-got-your-back/)