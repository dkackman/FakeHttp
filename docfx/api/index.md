# FakeHttp API
FakeHttp has two namespaces:

1) [FakeHttp](xref:FakeHttp) contains [FakeHttpMessageHandler](xref:FakeHttp.FakeHttpMessageHandler) and related types.
2) [FakeHttp.Resources](xref:FakeHttp.Resources) contains specific implementations of [IReadOnlyResources](xref:FakeHttp.IReadOnlyResources) and [IResources](xref:FakeHttp.IResources) for accessing faked responses stored on the [file system](xref:FakeHttp.Resources.FileSystemResources), in [ZipArchives](xref:FakeHttp.Resources.ZipResources) or [embedded in an Assembly](xref:FakeHttp.Resources.AssemblyResources). 
