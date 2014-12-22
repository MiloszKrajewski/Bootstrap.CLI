Properties {
	$release = "1.0.0.0"
	$src = (get-item "./").fullname
	$sln = "$src\Bootstrap.CLI.sln"
	$snk = "$src\Bootstrap.CLI.snk"
	$nunit = "$src\packages\NUnit.Runners.2.6.4\tools\nunit-console.exe"
}

Include ".\common.ps1"

FormatTaskName (("-"*79) + "`n`n    {0}`n`n" + ("-"*79))

Task default -depends Rebuild

Task Rebuild -depends VsVars,Clean,KeyGen,Version {
	Build-Solution $sln "Any CPU"
}

Task Test -depends Rebuild {
	exec { cmd /c $nunit "$src\Bootstrap.CLI.Tests\bin\Release\Bootstrap.CLI.Tests.dll" }
}

Task KeyGen -depends VsVars -precondition { return !(test-path $snk) } {
	exec { cmd /c sn -k $snk }
}

Task Version {
	Update-AssemblyVersion $src $release 'Bootstrap.CLI.Tests'
}

Task Clean {
	Clean-BinObj $src
}

Task VsVars {
	Set-VsVars
}
