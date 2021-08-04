$TestOutput = dotnet test --collect "XPlat Code Coverage"
$TestReports = $TestOutput | Select-String coverage.cobertura.xml | ForEach-Object { $_.Line.Trim() }
$TestReportsString = $TestReports -join ";"
reportgenerator "-reports:$TestReportsString" "-targetdir:./CoverageReport" "-reporttype:Html" riskHotspotsAnalysisThresholds:metricThresholdForCyclomaticComplexity=30