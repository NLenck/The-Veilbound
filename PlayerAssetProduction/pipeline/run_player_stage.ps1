param([Parameter(Mandatory=$true)][ValidateSet('Male','Female')][string]$Variant,
      [Parameter(Mandatory=$true)][ValidateSet('Generation','Rig','Animation')][string]$Stage)
$ErrorActionPreference='Stop'
$productionRoot='C:\The Veilbound\PlayerAssetProduction'
$config=Get-Content -LiteralPath (Join-Path $productionRoot 'pipeline\player_batch.json') -Raw | ConvertFrom-Json
if ($Stage -eq 'Animation' -and $config.animations_authored_locally) { throw 'Paid retargeting is disabled after the refunded service failure; use local Blender locomotion.' }
$logs=Join-Path $productionRoot 'logs'
$resultPath=Join-Path $logs "$($Variant)_$($Stage)_result.json"
$marker=Join-Path $logs "$($Variant)_$($Stage)_submission_started.json"
if ((Test-Path -LiteralPath $marker) -or (Test-Path -LiteralPath $resultPath)) { throw 'This stage has already been attempted; no automatic retries.' }
$cost=$config.stages.$Stage
$billed=0
foreach ($v in @('Male','Female')) { foreach ($s in @('Generation','Rig','Animation')) {
    $prior=Join-Path $logs "$($v)_$($s)_result.json"
    if (Test-Path -LiteralPath $prior) {
        $r=Get-Content -LiteralPath $prior -Raw | ConvertFrom-Json
        if ($r.status -ne 'success') { throw 'Failed or ambiguous previous task; inspect before any further paid action.' }
        $billed+=$r.credits_consumed
    }
}}
$env:TRIPO_CLIENT_APP='codex';$env:TRIPO_DISTRIBUTION_CHANNEL='codex_plugin'
$balanceRaw=tripo balance
if ($LASTEXITCODE -ne 0) { throw 'API balance read failed.' }
$balance=$balanceRaw | ConvertFrom-Json
if ($balance.frozen -ne 0 -or $balance.balance -lt $cost -or $billed+$cost -gt $config.hard_credit_cap -or 450-$balance.balance -ne $billed) { throw 'Player credit cap or balance reconciliation failed.' }
$commandArgs=@()
if ($Stage -eq 'Generation') {
    $commandArgs=@('make')
    foreach ($view in @('front','left','back','right')) { $commandArgs += Join-Path $productionRoot "inputs\$Variant\$($Variant)_$view.png" }
    $commandArgs+=@('--for','toy','--model','tripo-v3.1','-p','face_limit=20000','-p','smart_low_poly=true','-p','texture_quality=detailed','-p','texture=true','-p','pbr=true','--name',"veilbound-player-$($Variant.ToLowerInvariant())",'-o',(Join-Path $productionRoot "models_raw\$Variant"))
} elseif ($Stage -eq 'Rig') {
    $generation=Get-Content -LiteralPath (Join-Path $logs "$($Variant)_Generation_result.json") -Raw | ConvertFrom-Json
    $check=Get-Content -LiteralPath (Join-Path $logs "$($Variant)_rig_check_detail.json") -Raw | ConvertFrom-Json
    if ($check.riggable -ne $true -or $check.rig_type -ne 'biped') { throw 'The free rig check did not confirm a riggable biped.' }
    $commandArgs=@('make',$generation.task_id,'--then','rig:rig_type=biped,spec=mixamo,out_format=glb','--name',"veilbound-player-$($Variant.ToLowerInvariant())-rig",'-o',(Join-Path $productionRoot "rigs_raw\$Variant"))
} else {
    $rig=Get-Content -LiteralPath (Join-Path $logs "$($Variant)_Rig_result.json") -Raw | ConvertFrom-Json
    # Each clip uses make so its exact paid command has an offline dry run.
    $plans=@()
    foreach ($clip in @('idle','walk','run')) {
        $clipArgs=@('make',$rig.task_id,'--then',"retarget:preset:$($clip),out_format=glb,animate_in_place=true",'--name',"veilbound-player-$($Variant.ToLowerInvariant())-$clip",'-o',(Join-Path $productionRoot "animations_raw\$Variant"))
        $env:TRIPO_CLIENT_APP='codex';$env:TRIPO_DISTRIBUTION_CHANNEL='codex_plugin'
        $clipPlan=tripo @clipArgs --dry-run --json
        if ($LASTEXITCODE -ne 0) { throw "Animation dry run failed: $clip" }
        $parsedPlan=$clipPlan | ConvertFrom-Json
        if (-not $parsedPlan.valid -or $parsedPlan.steps.Count -ne 1) { throw 'Invalid animation plan.' }
        $clipPlan | Set-Content -LiteralPath (Join-Path $logs "$($Variant)_$($clip)_dry_run.json")
        $plans+=@{clip=$clip;args=$clipArgs;plan=$parsedPlan}
    }
    $plans | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath (Join-Path $logs "$($Variant)_Animation_dry_run.json")
    @{variant=$Variant;stage=$Stage;starting_balance=$balance.balance;prior_spend=$billed;estimate=30;utc=[DateTime]::UtcNow.ToString('o')} | ConvertTo-Json | Set-Content -LiteralPath $marker
    $clipResults=@()
    foreach ($entry in $plans) {
        $clip=$entry.clip;$clipArgs=$entry.args
        $clipPath=Join-Path $logs "$($Variant)_$($clip)_result.json"
        $env:TRIPO_CLIENT_APP='codex';$env:TRIPO_DISTRIBUTION_CHANNEL='codex_plugin'
        tripo @clipArgs --json --yes 1> $clipPath 2> (Join-Path $logs "$($Variant)_$($clip)_progress.log")
        $clipExit=$LASTEXITCODE
        if ($clipExit -ne 0) { Write-Output "Animation $clip stopped with exit code $clipExit; do not resubmit.";exit $clipExit }
        $clipResult=Get-Content -LiteralPath $clipPath -Raw | ConvertFrom-Json
        if ($clipResult.status -ne 'success' -or $clipResult.credits_consumed -gt 10) { throw 'Animation status or billed-cost mismatch.' }
        Copy-Item -LiteralPath $clipResult.model_file -Destination (Join-Path $productionRoot "animations_raw\$Variant\$clip.glb")
        $clipResults+=@{clip=$clip;result=$clipResult}
        $clipResults | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath (Join-Path $logs "$($Variant)_Animation_partial_results.json")
        Write-Output "Completed $Variant / $clip : $($clipResult.credits_consumed) credits."
    }
    $summary=@{status='success';type='movement_pack';credits_consumed=($clipResults | ForEach-Object {$_.result.credits_consumed} | Measure-Object -Sum).Sum;clips=$clipResults}
    $summary | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $resultPath
    Write-Output ($summary | ConvertTo-Json -Depth 20 -Compress)
    exit 0
}
$env:TRIPO_CLIENT_APP='codex';$env:TRIPO_DISTRIBUTION_CHANNEL='codex_plugin'
$plan=tripo @commandArgs --dry-run --json
if ($LASTEXITCODE -ne 0) { throw 'Exact stage dry run failed; no submission made.' }
$plan | Set-Content -LiteralPath (Join-Path $logs "$($Variant)_$($Stage)_dry_run.json")
$parsed=$plan | ConvertFrom-Json
if (-not $parsed.valid) { throw 'Exact stage plan is invalid.' }
@{variant=$Variant;stage=$Stage;starting_balance=$balance.balance;prior_spend=$billed;estimate=$cost;utc=[DateTime]::UtcNow.ToString('o')} | ConvertTo-Json | Set-Content -LiteralPath $marker
$env:TRIPO_CLIENT_APP='codex';$env:TRIPO_DISTRIBUTION_CHANNEL='codex_plugin'
tripo @commandArgs --json --yes 1> $resultPath 2> (Join-Path $logs "$($Variant)_$($Stage)_progress.log")
$taskExit=$LASTEXITCODE
if ($taskExit -ne 0) { Write-Output "Stopped with Tripo exit code $taskExit; no retry or resubmission.";exit $taskExit }
$result=Get-Content -LiteralPath $resultPath -Raw | ConvertFrom-Json
if ($result.status -ne 'success' -or $result.credits_consumed -gt $cost) { throw 'Stage status or billed cost mismatch.' }
if ($Stage -eq 'Generation') { Copy-Item -LiteralPath $result.model_file -Destination (Join-Path $productionRoot "models_raw\$Variant\$Variant.glb") }
if ($Stage -eq 'Rig') { Copy-Item -LiteralPath $result.model_file -Destination (Join-Path $productionRoot "rigs_raw\$Variant\$($Variant)_rigged.glb") }
Write-Output ($result | ConvertTo-Json -Depth 12 -Compress)
