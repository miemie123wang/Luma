Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$adviceAuditProject = Join-Path $repoRoot 'tools/Luma.AdviceAudit/Luma.AdviceAudit.csproj'
$localizationCheckProject = Join-Path $repoRoot 'tools/Luma.LocalizationCheck/Luma.LocalizationCheck.csproj'
$appProject = Join-Path $repoRoot 'Luma/Luma.csproj'

function Invoke-Step {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Name,

        [Parameter(Mandatory = $true)]
        [string[]] $Command
    )

    Write-Host ""
    Write-Host "==> $Name"
    & $Command[0] @($Command[1..($Command.Length - 1)])

    if ($LASTEXITCODE -ne 0) {
        throw "$Name failed with exit code $LASTEXITCODE."
    }
}

Push-Location $repoRoot
try {
    $invariantSets = @(
        'matrix-smoke',
        'high-risk',
        'regression',
        'travel-t6-sept-iles',
        'travel-aps-c-landscape'
    )

    foreach ($set in $invariantSets) {
        Invoke-Step "Advice invariants: $set" @(
            'dotnet',
            'run',
            '--project',
            $adviceAuditProject,
            '--',
            '--set',
            $set,
            '--check-invariants'
        )
    }

    Invoke-Step 'Localization validation' @(
        'dotnet',
        'run',
        '--project',
        $localizationCheckProject
    )

    Invoke-Step 'App build' @(
        'dotnet',
        'build',
        $appProject
    )

    Write-Host ""
    Write-Host 'Advice quality checks passed.'
}
finally {
    Pop-Location
}