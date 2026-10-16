param(
    [string]$ProjectKey = "bjjsm",
    [string]$ProjectName = "BjjSM",
    [string]$SonarHostUrl = "http://localhost:9000",
    [string]$SonarToken = $env:SONAR_TOKEN
)

$ErrorActionPreference = "Stop"

if (-not $SonarToken) {
    $mcpConfigFile = "$env:USERPROFILE\.gemini\config\mcp_config.json"
    if (Test-Path $mcpConfigFile) {
        $mcpJson = Get-Content $mcpConfigFile -Raw | ConvertFrom-Json
        $tokenArg = $mcpJson.mcpServers.sonarqube.args | Where-Object { $_ -like "SONARQUBE_TOKEN=*" }
        if ($tokenArg) { $SonarToken = $tokenArg -replace "^SONARQUBE_TOKEN=", "" }
    }
}

if (-not $SonarToken) {
    Write-Error "SonarQube token is required. Set `$env:SONAR_TOKEN or pass -SonarToken <token>."
}

dotnet-sonarscanner begin `
    /k:"$ProjectKey" `
    /n:"$ProjectName" `
    /d:sonar.host.url="$SonarHostUrl" `
    /d:sonar.token="$SonarToken" `
    /d:sonar.cs.analyzeRazorCode=true `
    /d:sonar.cs.opencover.reportsPaths="**/TestResults/**/coverage.opencover.xml" `
    /d:sonar.exclusions="**/bin/**,**/obj/**,**/wwwroot/lib/**,**/Migrations/**,**/*.g.cs,**/*.Designer.cs" `
    /d:sonar.coverage.exclusions="**/tests/**,**/*Tests/**,**/*Tests.cs,**/Migrations/**,**/Program.cs,**/DependencyInjection.cs,**/*.razor" `
    /d:sonar.cpd.exclusions="**/Migrations/**,**/Program.cs,**/CurriculumThemePresets.cs,**/SpacedRepetitionPresets.cs"

try {
    dotnet build BjjSM.slnx --no-incremental /p:SkipAutoSonar=true
    dotnet test BjjSM.slnx --no-build --collect:"XPlat Code Coverage;Format=opencover" /p:SkipAutoSonar=true
    dotnet-sonarscanner end /d:sonar.token="$SonarToken"
} finally {
    if (Test-Path ".sonarqube") {
        Remove-Item -Recurse -Force ".sonarqube" -ErrorAction SilentlyContinue
    }
}
