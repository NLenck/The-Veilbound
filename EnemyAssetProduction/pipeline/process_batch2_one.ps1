param([Parameter(Mandatory=$true)][string]$Unit)
$ErrorActionPreference='Stop'
$productionRoot='C:\The Veilbound\EnemyAssetProduction'
$config=Get-Content -LiteralPath (Join-Path $productionRoot 'pipeline\batch_config_town_shrine.json') -Raw | ConvertFrom-Json
$jobs=@($config.jobs | Where-Object unit -EQ $Unit)
if ($jobs.Count -ne 1) { throw 'Unit is outside the authorized local-processing batch.' }
$job=$jobs[0]
& 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe' --background --factory-startup --python-exit-code 1 --python (Join-Path $productionRoot 'pipeline\process_blender.py') -- $job.biome $Unit batch_config_town_shrine.json *> (Join-Path $productionRoot "logs\$($Unit)_blender.log")
if ($LASTEXITCODE -ne 0) { throw "Local Blender processing failed: $Unit" }
& 'C:\Users\nicho\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' (Join-Path $productionRoot 'pipeline\record_batch2.py')
if ($LASTEXITCODE -ne 0) { throw 'Manifest update failed.' }
