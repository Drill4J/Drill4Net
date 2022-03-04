
dotnet publish "src/Configurator/Drill4Net.Configurator.App/Drill4Net.Configurator.App.csproj" -c Release --runtime win-x64 --framework net6.0 -o ../Drill4Net-ci/distribution/apps/configurator
dotnet publish "src/Injector/Drill4Net.Injector.App/Drill4Net.Injector.App.csproj" -c Release --runtime win-x64 --framework net6.0 -o ../Drill4Net-ci/distribution/apps/injector
dotnet publish "src/Agent/Drill4Net.Agent.TestRunner/Drill4Net.Agent.TestRunner.csproj" -c Release --runtime win-x64 -o ../Drill4Net-ci/distribution/apps/test_runner
dotnet publish "src/Agent/Drill4Net.Agent.Transmitter/Drill4Net.Agent.Transmitter.csproj" -c Release --runtime win-x64 -o ../Drill4Net-ci/distribution/components/transmitter
dotnet publish "src/Agent/Drill4Net.Agent.Plugins.NUnit/Drill4Net.Agent.Plugins.NUnit.csproj" -c Release --runtime win-x64 -o ../Drill4Net-ci/distribution/components/transmitter_plugins/Drill4Net.Agent.Plugins.NUnit
dotnet publish "src/Agent/Drill4Net.Agent.Plugins.SpecFlow/Drill4Net.Agent.Plugins.SpecFlow.csproj" -c Release --runtime win-x64 -o ../Drill4Net-ci/distribution/components/transmitter_plugins/Drill4Net.Agent.Plugins.SpecFlow
dotnet publish "src/Agent/Drill4Net.Agent.Plugins.xUnit/Drill4Net.Agent.Plugins.xUnit.csproj" -c Release --runtime win-x64 -o ../Drill4Net-ci/distribution/components/transmitter_plugins/Drill4Net.Agent.Plugins.xUnit
dotnet publish "src/Injector/Plugins/Drill4Net.Injector.Plugins.SpecFlow/Drill4Net.Injector.Plugins.SpecFlow.csproj" -c Release --runtime win-x64 -o ../Drill4Net-ci/distribution/components/injector_plugins/Drill4Net.Injector.Plugins.SpecFlow
dotnet publish "src/Tests/Targets/TestFrameworks/Drill4Net.Target.Frameworks.Bdd.SpecFlow.xUnit/Drill4Net.Target.Frameworks.Bdd.SpecFlow.xUnit.csproj" -c Release --runtime win-x64 --framework net6.0 -o ../Drill4Net-ci/targets/specflow-xunit
New-Item -Path '../Drill4Net-ci/ci' -ItemType Directory
New-Item -Path '../Drill4Net-ci/ci/specflow-xunit' -ItemType Directory
New-Item -Path '../Drill4Net-ci/ci/specflow-xunit/build1' -ItemType Directory
New-Item -Path '../Drill4Net-ci/ci/specflow-xunit/build1/.gitkeep'
New-Item -Path '../Drill4Net-ci/distribution/templates' -ItemType Directory
Copy-Item -Path "./artefacts/templates/*" -Destination "../Drill4Net-ci/distribution/templates"
Copy-Item "./artefacts/templates/configurator_app.yml" -Destination "../Drill4Net-ci/distribution/apps/configurator/app.yml"
Copy-Item "./artefacts/templates/injector_app.yml" -Destination "../Drill4Net-ci/distribution/apps/injector/app.yml"
Copy-Item "./artefacts/docker-compose.yml" -Destination "../Drill4Net-ci/distribution/docker-compose.yml"
New-Item -Path '../Drill4Net-ci/distribution/apps/configurator/logs_drill' -ItemType Directory
New-Item -Path '../Drill4Net-ci/distribution/apps/configurator/logs_drill/.gitkeep'
New-Item -Path '../Drill4Net-ci/distribution/apps/injector/logs_drill/' -ItemType Directory
New-Item -Path '../Drill4Net-ci/distribution/apps/injector/logs_drill/.gitkeep'
New-Item -Path '../Drill4Net-ci/distribution/apps/test_runner/logs_drill' -ItemType Directory
New-Item -Path '../Drill4Net-ci/distribution/apps/test_runner/logs_drill/.gitkeep'
