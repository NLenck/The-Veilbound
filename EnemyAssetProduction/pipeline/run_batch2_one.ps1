param([Parameter(Mandatory=$true)][string]$Unit)
$ErrorActionPreference='Stop'
$productionRoot='C:\The Veilbound\EnemyAssetProduction'
$config=Get-Content -LiteralPath (Join-Path $productionRoot 'pipeline\batch_config_town_shrine.json') -Raw | ConvertFrom-Json
$job=@($config.jobs | Where-Object unit -EQ $Unit)
if ($job.Count -ne 1 -or $config.jobs.Count -ne 18 -or $config.hard_credit_cap -ne 900) { throw 'Unexpected batch scope.' }
$job=$job[0]
if ($job.biome -notin @('Town','Shrine') -or $Unit -in @('Husk','Gargoyle')) { throw 'Unapproved generation.' }
$logs=Join-Path $productionRoot 'logs'
$resultPath=Join-Path $logs "$($Unit)_result.json"
$started=Join-Path $logs "$($Unit)_submission_started.json"
if ((Test-Path -LiteralPath $resultPath) -or (Test-Path -LiteralPath $started)) { throw 'Submission already attempted. Never automatically retry or resubmit.' }
$env:TRIPO_CLIENT_APP='codex'
$env:TRIPO_DISTRIBUTION_CHANNEL='codex_plugin'
$balanceText=tripo balance
if ($LASTEXITCODE -ne 0) { throw 'Balance check failed.' }
$balance=$balanceText | ConvertFrom-Json
$billed=0
foreach ($configuredJob in $config.jobs) {
    $prior=Join-Path $logs "$($configuredJob.unit)_result.json"
    if (Test-Path -LiteralPath $prior) {
        $r=Get-Content -LiteralPath $prior -Raw | ConvertFrom-Json
        if ($r.status -ne 'success') { throw 'Previous failed or ambiguous task requires inspection.' }
        $billed += $r.credits_consumed
    }
}
if ($balance.frozen -ne 0 -or $balance.balance -lt 50 -or $billed+50 -gt 900 -or $config.initial_balance-$balance.balance -ne $billed) { throw 'Balance or spend cap reconciliation failed.' }
$planPath=Join-Path $logs "$($Unit)_dry_run.json"
$plan=Get-Content -LiteralPath $planPath -Raw | ConvertFrom-Json
if (-not $plan.valid -or $plan.model -ne 'P1-20260311' -or $plan.steps.Count -ne 1) { throw 'Missing or unexpected dry-run plan.' }
@{unit=$Unit;biome=$job.biome;starting_balance=$balance.balance;prior_batch_spend=$billed;estimate=50;utc=[DateTime]::UtcNow.ToString('o')} | ConvertTo-Json | Set-Content -LiteralPath $started
tripo make $job.input --for toy -p "face_limit=$($job.target_triangles)" -p texture=true -p pbr=false --name "veilbound-$($Unit.ToLowerInvariant())" -o (Join-Path $productionRoot "models_raw\$($job.biome)") --json --yes 1> $resultPath 2> (Join-Path $logs "$($Unit)_progress.log")
$generationExit=$LASTEXITCODE
if ($generationExit -ne 0) { Write-Output "Tripo stopped with exit code $generationExit. No automatic retries."; exit $generationExit }
$result=Get-Content -LiteralPath $resultPath -Raw | ConvertFrom-Json
if ($result.status -ne 'success' -or $result.credits_consumed -gt 50 -or $billed+$result.credits_consumed -gt 900) { throw 'Generation or billed-cost mismatch; stop batch.' }
Copy-Item -LiteralPath $result.model_file -Destination (Join-Path $productionRoot "models_raw\$($job.biome)\$Unit.glb")
Write-Output ($result | Select-Object task_id,status,credits_consumed,preview,model_file | ConvertTo-Json -Compress)
