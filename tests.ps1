$PSScriptFilePath = (Get-Item $MyInvocation.MyCommand.Path).FullName

" PSScriptFilePath = $PSScriptFilePath"


& dotnet test 
if (-not $?)
{
	throw "The DOTNET test process returned an error code."
}