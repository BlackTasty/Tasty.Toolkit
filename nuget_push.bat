@echo off

set spacer="========="

echo %spacer%"Configure keys"%spacer%
echo.
set /P nuggetKey="NuGet API key (leave empty to skip): "
echo.
set /P gitKey="GitHub PAT (leave empty to skip): "
echo.
echo.
echo %spacer%"Configure versions"%spacer%
echo.
set /P versionVm="Tasty.ViewModel version (leave empty to skip): "
echo.
set /P versionJson="Tasty.ViewModel.JsonNet version (leave empty to skip): "
echo.
set /P versionLog="Tasty.Logging version (leave empty to skip): "
echo.
set /P versionSql="Tasty.SQLiteManager version (leave empty to skip): "
echo.

cd Tasty.ViewModel
IF NOT "%versionVm%"=="" (
	echo %spacer%"Uploading Tasty.ViewModel "%versionVm%" "%spacer%
	IF NOT "%nuggetKey%"=="" (
		echo "Pushing to NuGet repository..."
		nuget push TastyApps.Core.ViewModel.%versionVm%.nupkg %nuggetKey% -Source https://api.nuget.org/v3/index.json
	)
	
	IF NOT "%gitKey%"=="" (
		echo "Pushing to GitHub repository..."
		dotnet nuget push "TastyApps.Core.ViewModel.%versionVm%.nupkg"  --api-key %gitKey% --source "github"
	)
)

cd ../Tasty.ViewModel.JsonNet
IF NOT "%versionJson%"=="" (
	echo %spacer%"Uploading Tasty.ViewModel.JsonNet "%versionJson%" "%spacer%
	IF NOT "%nuggetKey%"=="" (
		echo "Pushing to NuGet repository..."
		nuget push TastyApps.Core.ViewModel.JsonNet.%versionJson%.nupkg %nuggetKey% -Source https://api.nuget.org/v3/index.json
	)
	
	IF NOT "%gitKey%"=="" (
		echo "Pushing to GitHub repository..."
		dotnet nuget push "TastyApps.Core.ViewModel.JsonNet.%versionJson%.nupkg"  --api-key %gitKey% --source "github"
	)
)

cd ../Tasty.Logging
IF NOT "%versionLog%"=="" (
	echo %spacer%"Uploading Tasty.Logging "%versionLog%" "%spacer%
	IF NOT "%nuggetKey%"=="" (
		echo "Pushing to NuGet repository..."
		nuget push TastyApps.Core.Logging.%versionLog%.nupkg %nuggetKey% -Source https://api.nuget.org/v3/index.json
	)
	
	IF NOT "%gitKey%"=="" (
		echo "Pushing to GitHub repository..."
		dotnet nuget push "TastyApps.Core.Logging.%versionLog%.nupkg"  --api-key %gitKey% --source "github"
	)
)

cd ../Tasty.SQLiteManager
IF NOT "%versionSql%"=="" (
	echo %spacer%"Uploading Tasty.SQLiteManager "%versionSql%" "%spacer%
	IF NOT "%nuggetKey%"=="" (
		echo "Pushing to NuGet repository..."
		nuget push TastyApps.Core.SQLiteManager.%versionSql%.nupkg %nuggetKey% -Source https://api.nuget.org/v3/index.json
	)
	
	IF NOT "%gitKey%"=="" (
		echo "Pushing to GitHub repository..."
		dotnet nuget push "TastyApps.Core.SQLiteManager.%versionSql%.nupkg"  --api-key %gitKey% --source "github"
	)
)

cmd /k