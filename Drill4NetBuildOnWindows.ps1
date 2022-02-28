dotnet publish "src/Configurator/Drill4Net.Configurator.App/Drill4Net.Configurator.App.csproj" -c Release --runtime win-x64 --framework net6.0 -o ../distribution/apps/configurator
dotnet publish "src/Injector/Drill4Net.Injector.App/Drill4Net.Injector.App.csproj" -c Release --runtime win-x64 --framework net6.0 -o ../distribution/apps/injector
dotnet publish "src/Agent/Drill4Net.Agent.TestRunner/Drill4Net.Agent.TestRunner.csproj" -c Release --runtime win-x64 -o ../distribution/apps/test_runner
dotnet publish "src/Agent/Drill4Net.Agent.Transmitter/Drill4Net.Agent.Transmitter.csproj" -c Release --runtime win-x64 -o ../distribution/components/transmitter
dotnet publish "src/Agent/Drill4Net.Agent.Plugins.NUnit/Drill4Net.Agent.Plugins.NUnit.csproj" -c Release --runtime win-x64 -o ../distribution/components/transmitter_plugins/Drill4Net.Agent.Plugins.NUnit
dotnet publish "src/Agent/Drill4Net.Agent.Plugins.SpecFlow/Drill4Net.Agent.Plugins.SpecFlow.csproj" -c Release --runtime win-x64 -o ../distribution/components/transmitter_plugins/Drill4Net.Agent.Plugins.SpecFlow
dotnet publish "src/Agent/Drill4Net.Agent.Plugins.xUnit/Drill4Net.Agent.Plugins.xUnit.csproj" -c Release --runtime win-x64 -o ../distribution/components/transmitter_plugins/Drill4Net.Agent.Plugins.xUnit
dotnet publish "src/Injector/Plugins/Drill4Net.Injector.Plugins.SpecFlow/Drill4Net.Injector.Plugins.SpecFlow.csproj" -c Release --runtime win-x64 -o ../distribution/components/injector_plugins/Drill4Net.Injector.Plugins.SpecFlow
dotnet publish "src/Tests/Targets/TestFrameworks/Drill4Net.Target.Frameworks.Bdd.SpecFlow.xUnit/Drill4Net.Target.Frameworks.Bdd.SpecFlow.xUnit.csproj" -c Release --runtime win-x64 --framework net6.0 -o ../distribution/Drill4Net.Target.Frameworks.Bdd.SpecFlow.xUnit
