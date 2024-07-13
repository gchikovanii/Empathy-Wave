var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.EmphatyWave_ApiService>("apiservice");

builder.AddProject<Projects.EmphatyWave_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
