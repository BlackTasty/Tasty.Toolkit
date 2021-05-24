set /P key="Please input the nuget API key: "
set /P versionVm="Set Tasty.ViewModel version: "
set /P versionJson="Set Tasty.ViewModel.JsonNet version: "
cd Tasty.ViewModel
nuget push TastyApps.Core.ViewModel.%versionVm%.nupkg %key% -Source https://api.nuget.org/v3/index.json
cd ../Tasty.ViewModel.JsonNet
nuget push TastyApps.Core.ViewModel.JsonNet.%versionJson%.nupkg %key% -Source https://api.nuget.org/v3/index.json