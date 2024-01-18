param (
    [Parameter(Mandatory=$true)]
    [ValidatePattern("^\d+\.\d+\.(?:\d+\.\d+$|\d+$)")]
    [string]
    $ReleaseVersionNumber,
    [Parameter(Mandatory=$true)]
    [string]
    [AllowEmptyString()]
    $PreReleaseName
)

$PSScriptFilePath = (Get-Item $MyInvocation.MyCommand.Path).FullName

" PSScriptFilePath = $PSScriptFilePath"

$SolutionRoot = Split-Path -Path $PSScriptFilePath -Parent

# Make sure we don't have a release folder for this version already
$BuildFolder = Join-Path -Path $SolutionRoot -ChildPath "build";
$ReleaseFolder = Join-Path -Path $BuildFolder -ChildPath "Releases\v$ReleaseVersionNumber$PreReleaseName";
if ((Get-Item $ReleaseFolder -ErrorAction SilentlyContinue) -ne $null)
{
    Write-Warning "$ReleaseFolder already exists on your local machine. It will now be deleted."
    Remove-Item $ReleaseFolder -Recurse
}

# Set the version number and copyright date in project file
$DateYear = (Get-Date).year

$ProjectPath = Join-Path -Path $SolutionRoot -ChildPath "src\TemporaryDb\TemporaryDb.csproj"

[xml]$project = Get-Content -Path $ProjectPath

$project.Project.PropertyGroup.Version = "$ReleaseVersionNumber$PreReleaseName" 
$project.Project.PropertyGroup.Copyright = "Copyright © Josh Clark $DateYear"

$project.Save($ProjectPath)


& dotnet pack --configuration Release --output "$ReleaseFolder"
if (-not $?)
{
    throw "The DOTNET pack process returned an error code."
}