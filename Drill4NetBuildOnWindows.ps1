
dotnet publish "src/Configurator/Drill4Net.Configurator.App/Drill4Net.Configurator.App.csproj" -c Release --runtime win-x64 --framework net6.0 -o ../distribution/apps/configurator
dotnet publish "src/Injector/Drill4Net.Injector.App/Drill4Net.Injector.App.csproj" -c Release --runtime win-x64 --framework net6.0 -o ../distribution/apps/injector
dotnet publish "src/Agent/Drill4Net.Agent.TestRunner/Drill4Net.Agent.TestRunner.csproj" -c Release --runtime win-x64 -o ../distribution/apps/test_runner
dotnet publish "src/Agent/Drill4Net.Agent.Transmitter/Drill4Net.Agent.Transmitter.csproj" -c Release --runtime win-x64 -o ../distribution/components/transmitter
dotnet publish "src/Agent/Drill4Net.Agent.Plugins.NUnit/Drill4Net.Agent.Plugins.NUnit.csproj" -c Release --runtime win-x64 -o ../distribution/components/transmitter_plugins/Drill4Net.Agent.Plugins.NUnit
dotnet publish "src/Agent/Drill4Net.Agent.Plugins.SpecFlow/Drill4Net.Agent.Plugins.SpecFlow.csproj" -c Release --runtime win-x64 -o ../distribution/components/transmitter_plugins/Drill4Net.Agent.Plugins.SpecFlow
dotnet publish "src/Agent/Drill4Net.Agent.Plugins.xUnit/Drill4Net.Agent.Plugins.xUnit.csproj" -c Release --runtime win-x64 -o ../distribution/components/transmitter_plugins/Drill4Net.Agent.Plugins.xUnit
dotnet publish "src/Injector/Plugins/Drill4Net.Injector.Plugins.SpecFlow/Drill4Net.Injector.Plugins.SpecFlow.csproj" -c Release --runtime win-x64 -o ../distribution/components/injector_plugins/Drill4Net.Injector.Plugins.SpecFlow
dotnet publish "src/Tests/Targets/TestFrameworks/Drill4Net.Target.Frameworks.Bdd.SpecFlow.xUnit/Drill4Net.Target.Frameworks.Bdd.SpecFlow.xUnit.csproj" -c Release --runtime win-x64 --framework net6.0 -o ../distribution/targets/specflow-xunit
New-Item -Path '../distribution/ci' -ItemType Directory
New-Item -Path '../distribution/ci/specflow-xunit' -ItemType Directory
New-Item -Path '../distribution/ci/specflow-xunit/.gitkeep'
New-Item -Path '../distribution/templates' -ItemType Directory
Copy-Item -Path "./artefacts/templates/*" -Destination "../distribution/templates"
Copy-Item "./artefacts/templates/configurator_app.yml" -Destination "../distribution/apps/configurator/app.yml"
Copy-Item "./artefacts/templates/injector_app.yml" -Destination "../distribution/apps/injector/app.yml"

Remove-item "../distribution/apps/injector/_redirect.yml"
New-Item -Path "../distribution/apps/injector/_redirect.yml"
Add-Content "../distribution/apps/injector/_redirect.yml" "Path: C:\Users\WDAGUtilityAccount\Desktop\distribution\apps\injector\inj_xUnit_SpecFlow.yml"
New-Item -Path '../distribution/apps/configurator/logs_drill' -ItemType Directory
New-Item -Path '../distribution/apps/configurator/logs_drill/.gitkeep'
New-Item -Path '../distribution/apps/injector/logs_drill/' -ItemType Directory
New-Item -Path '../distribution/apps/injector/logs_drill/.gitkeep'
New-Item -Path '../distribution/apps/test_runner/logs_drill' -ItemType Directory
New-Item -Path '../distribution/apps/test_runner/logs_drill/.gitkeep'
