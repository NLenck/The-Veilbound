# Rebuild the three existing models only. This script never calls Tripo or spends credits.
$ErrorActionPreference = 'Stop'
$productionRoot = 'C:\The Veilbound\EnemyAssetProduction'
$blenderPath = 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe'
$config = Get-Content -LiteralPath (Join-Path $productionRoot 'pipeline\batch_config.json') -Raw | ConvertFrom-Json
$allowed = @('Town/Husk', 'Forest/Wolf', 'Shrine/Gargoyle')
if ($config.jobs.Count -ne 3) { throw 'First batch must contain exactly three jobs.' }
foreach ($job in $config.jobs) {
    if ($allowed -notcontains "$($job.biome)/$($job.unit)") { throw 'Unapproved first-batch job.' }
    $source = Join-Path $productionRoot "models_raw\$($job.biome)\$($job.unit).glb"
    if (-not (Test-Path -LiteralPath $source)) { throw "Missing original generated GLB: $source" }
    $log = Join-Path $productionRoot "logs\$($job.unit)_blender.log"
    & $blenderPath --background --factory-startup --python-exit-code 1 --python (Join-Path $productionRoot 'pipeline\process_blender.py') -- $job.biome $job.unit *> $log
    if ($LASTEXITCODE -ne 0) { throw "Blender processing failed: $log" }
}
& $blenderPath --background --factory-startup --python-exit-code 1 --python (Join-Path $productionRoot 'pipeline\verify_exports.py') *> (Join-Path $productionRoot 'logs\export_verification.log')
if ($LASTEXITCODE -ne 0) { throw 'Export verification failed.' }
Write-Output 'Three local models rebuilt and FBX/GLB round trips verified. No API credits used.'
