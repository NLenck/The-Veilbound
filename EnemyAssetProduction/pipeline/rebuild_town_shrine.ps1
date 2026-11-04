# Local rebuild of the eighteen continuation models only; never submits API jobs.
$ErrorActionPreference='Stop'
$productionRoot='C:\The Veilbound\EnemyAssetProduction'
$config=Get-Content -LiteralPath (Join-Path $productionRoot 'pipeline\batch_config_town_shrine.json') -Raw | ConvertFrom-Json
if ($config.jobs.Count -ne 18) { throw 'Unexpected continuation scope.' }
foreach ($job in $config.jobs) {
    & (Join-Path $productionRoot 'pipeline\process_batch2_one.ps1') -Unit $job.unit
    if ($LASTEXITCODE -ne 0) { throw "Local rebuild failed: $($job.unit)" }
}
& 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe' --background --factory-startup --python-exit-code 1 --python (Join-Path $productionRoot 'pipeline\verify_exports.py') -- batch_config_town_shrine.json *> (Join-Path $productionRoot 'logs\town_shrine_export_verification.log')
if ($LASTEXITCODE -ne 0) { throw 'Continuation export verification failed.' }
Write-Output 'All eighteen continuation models rebuilt locally and verified; no API credits used.'
