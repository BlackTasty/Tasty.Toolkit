set /P key="Please input the nuget API key: "
set /P versionVm="Set Tasty.ViewModel version (leave empty to skip): "
set /P versionJson="Set Tasty.ViewModel.JsonNet version (leave empty to skip): "
set /P versionLog="Set Tasty.Logging version (leave empty to skip): "

cd Tasty.ViewModel
IF NOT "%versionVm%"=="" (
	nuget push TastyApps.Core.ViewModel.%versionVm%.nupkg %key% -Source https://api.nuget.org/v3/index.json
)

cd ../Tasty.ViewModel.JsonNet
IF NOT "%versionJson%"=="" (
	nuget push TastyApps.Core.ViewModel.JsonNet.%versionJson%.nupkg %key% -Source https://api.nuget.org/v3/index.json
)

cd ../Tasty.Logging
IF NOT "%versionLog%"=="" (
	nuget push TastyApps.Core.Logging.%versionLog%.nupkg %key% -Source https://api.nuget.org/v3/index.json
)