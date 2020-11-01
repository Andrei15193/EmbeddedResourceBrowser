![Build and Deploy](https://github.com/Andrei15193/EmbeddedResourceBrowser/workflows/Build%20and%20Deploy/badge.svg?branch=master)

### EmbeddedResourceBrowser
A small utility library for browsing the embedded resources of an assembly.

For project documentation go to https://Andrei15193.github.io/EmbeddedResourceBrowser

To see the latest development (`dev` branch) go to https://andrei15193.github.io/EmbeddedResourceBrowser/dev

----

The library is rather small and provides a directory-like structure for browsing embedded resources. You can load resources in multiple ways depending on how you wish to use them in your application. Before detailing each approach it is important to note that embedded resources in .NET assemblies use the `.` character as a delimiter for directories. This can be a bit confusing because we can add the `.` in the name of a directory or a file and there will be absolutely no difference of having a directory structure or
using `.` in the name. For instance the following two resources are identical and the compiler will issue an error indicating this.

```
Assembly/
    Directory/
        embedded file.txt
    Directory.embedded file.txt

Both resources are embedded (by default) with the following logical name:
Assembly.Directory.embedded file.txt
```

If you wish, you can provide a custom logical name for your embedded resource, keep in mind that you need to specify this manually. For instance, you can use `/` as a separator by specifying the logical name of an embedded resource. For more information refer to: [Common MSBuild project items - EmbeddedResource](https://docs.microsoft.com/visualstudio/msbuild/common-msbuild-project-items?view=vs-2019#embeddedresource).

```xml
<EmbeddedResource Include="Directory.embedded file.txt" LogicalName="Directory/embedded file.txt"/>
```

Implicitly, the library uses the `.` character as a delimiter for directories in the remained of the name. The remained of the name is determined by removing the assembly name from the beginning of the embedded resource name (which can contain `.` characters because the assembly name is available through the assembly object from which resources are being loaded) and removing the file name which contains exactly one `.` character which delimits the extension. For instance, the following resource `My.Assembly.Name.One.Two.Three.File.Extension` has the `One.Two.Three` name remained because the assembly name is `My.Assembly.Name` and the file name is `File.Extension`. The default delimiter for splitting the directory path from the remained is `.` leading to the following structure: `My.Assembly.Name/One/Two/Three/File.Extension`.

### Working With One Assembly

Generally you may be working with just one assembly from where you load embedded resources. In this case initialize a new [EmbeddedDirectory](https://andrei15193.github.io/EmbeddedResourceBrowser/EmbeddedResourceBrowser.EmbeddedDirectory.html) by passing the assembly object containing the embedded resources to the constructor. This will load all resources in a directory tree where the root is the directory having the same name as the assembly name and all resources can be browsed, and loaded, from there. The following method will concatenate the contents of each embedded file at the top level of the assembly.

```c#
string ConcatResources()
{
    var embeddedDirectory = new EmbeddedDirectory(typeof(ACustomType).Assembly);

    var resultBuilder = new StringBuilder();
    foreach (var file in embeddedDirectory.Files)
        using (var streamReader = new StreamReader(file.OpenRead()))
            resultBuilder.AppendLine(streamReader.ReadToEnd());
    return resultBuilder.ToString();
}
```

### Working With Multiple Assemblies

Sometimes the project can be rather complex and embedded resources are placed in multiple assemblies. In this case an [EmbeddedDirectory](https://andrei15193.github.io/EmbeddedResourceBrowser/EmbeddedResourceBrowser.EmbeddedDirectory.html) can be created by providing all assemblies as constructor arguments which in turn will create a root directory containing a subdirectory for each provided assembly that have the same structure at in the _working with one assembly_ scenario. The following method will concatenate the contents of each embedded file at the top level of each assembly, but also append the assembly name from where files are being read.

```c#
string ConcatResources()
{
    var embeddedDirectory = new EmbeddedDirectory(typeof(ACustomTypeFromAssembly1).Assembly, typeof(ACustomTypeFromAssembly2).Assembly);

    var resultBuilder = new StringBuilder();
    foreach (var assemblyDirectory in embeddedDirectory.Subdirectories)
    {
        resultBuilder.AppendLine(assemblyDirectory.Name);
        resultBuilder.AppendLine(new string('-', 80));
        foreach (var file in embeddedDirectory.Files)
            using (var streamReader = new StreamReader(file.OpenRead()))
                resultBuilder.AppendLine(streamReader.ReadToEnd());
    }
    return resultBuilder.ToString();
}
```

This works well with pattern matching. To find different file or directory structures you can use the [GetAllFiles](https://andrei15193.github.io/EmbeddedResourceBrowser/EmbeddedResourceBrowser.EmbeddedDirectory.GetAllFiles.html) and [GetAllSubdirectories](https://andrei15193.github.io/EmbeddedResourceBrowser/EmbeddedResourceBrowser.EmbeddedDirectory.GetAllSubdirectories.html) methods to get all files/directories and use [LINQ](https://docs.microsoft.com/dotnet/csharp/programming-guide/concepts/linq) to match different structures.

### Working With Multiple Assemblies (Merge Method)

A different scenario when working with multiple assemblies is when we want to have resources that are placed in multiple assemblies, but instead of leading each of them we want to have a precedence between them, so even though we have multiple assemblies with embedded resources, we want to provide an implicit set that can be overriden in other assemblies. For instance, I would like to provide a default set of embedded resources that can be overridden by users of my library.

Given the following structure of embedded directories, I can override any of these resources (or add) from a different assembly using the [Merge](https://andrei15193.github.io/EmbeddedResourceBrowser/EmbeddedResourceBrowser.EmbeddedDirectory.Merge.html) method.

```
MyAssembly/
    Directory1/
        File 1.txt
        File 2.txt
    Directory2/
        File 1.txt
```

This would be the default set of resources, now, when loading mapping resources from multiple assemblies using the [Merge](https://andrei15193.github.io/EmbeddedResourceBrowser/EmbeddedResourceBrowser.EmbeddedDirectory.Merge.html) method some of these resources can be overridden and a few more resources can be added to the set. Having a second assembly with the following structure will create an [EmbeddedDirectory](https://andrei15193.github.io/EmbeddedResourceBrowser/EmbeddedResourceBrowser.EmbeddedDirectory.html) with resources from both assemblies.

```
OtherAssembly/
    Directory1/
        File 2.txt
        File 3.txt
    Directory3/
        File 1.txt
```

The resulting structure will be the following, note that the root directory, or any directory, does not have the assembly name anymore since directories can contain resources from mixed assemblies.

```
/
    Directory1/
        File 1.txt (from MyAssembly)
        File 2.txt (from OtherAssembly, matching resource names)
        File 3.txt (from OtherAssembly)
    Directory2/
        File 1.txt (from MyAssembly)
    Directory3/
        File 1.txt (from OtherAssembly)
```

To create an [EmbeddedDirectory](https://andrei15193.github.io/EmbeddedResourceBrowser/EmbeddedResourceBrowser.EmbeddedDirectory.html) using the [Merge](https://andrei15193.github.io/EmbeddedResourceBrowser/EmbeddedResourceBrowser.EmbeddedDirectory.Merge.html) method simply call the static method instead of calling the constructor.

```c#
string ConcatResources()
{
    var embeddedDirectory = EmbeddedDirectory.Merge(typeof(ACustomTypeFromAssembly1).Assembly, typeof(ACustomTypeFromAssembly2).Assembly);

    var resultBuilder = new StringBuilder();
    foreach (var file in embeddedDirectory.Files)
        using (var streamReader = new StreamReader(file.OpenRead()))
            resultBuilder.AppendLine(streamReader.ReadToEnd());
    return resultBuilder.ToString();
}
```