param([switch]$SubmitPrototype)
$ErrorActionPreference='Stop'
$plateRoot='C:\The Veilbound\ArmourAssetProduction'
$env:TRIPO_CLIENT_APP='codex';$env:TRIPO_DISTRIBUTION_CHANNEL='codex_plugin'
$front=Join-Path $plateRoot 'references\prepared\Prototype_Tin_Copper_front.png'
$back=Join-Path $plateRoot 'references\prepared\Prototype_Tin_Copper_back.png'
$result=Join-Path $plateRoot 'logs\prototype_result.json'
$lock=Join-Path $plateRoot 'logs\prototype_submission_intent.json'
if($SubmitPrototype -and ((Test-Path -LiteralPath $result) -or (Test-Path -LiteralPath $lock))){throw 'A prototype attempt already exists. Inspect saved task/balance; never submit automatically again.'}
$arguments=@('make',$front,$back,'--model','tripo-v3.1','-p','face_limit=16000','-p','generate_parts=true','-p','texture=false','-p','pbr=false','--seed','296059','-o',(Join-Path $plateRoot 'raw'),'--name','plate-tin-copper-prototype','--json')
if(-not $SubmitPrototype){& tripo @arguments --dry-run;exit $LASTEXITCODE}
$balanceText=& tripo balance --json;if($LASTEXITCODE -ne 0){throw 'Could not verify balance'}
$balance=$balanceText|ConvertFrom-Json
if($balance.frozen -ne 0 -or $balance.balance -lt 40){throw 'Unresolved holds or fewer than the estimated 40 credits. Stop.'}
@{status='submission_intent';estimated_credits=40;approved_paid_calls=1;bulk_authorised=$false}|ConvertTo-Json|Set-Content -LiteralPath $lock
& tripo @arguments --yes 1> $result 2> (Join-Path $plateRoot 'logs\prototype_progress.log')
exit $LASTEXITCODE
