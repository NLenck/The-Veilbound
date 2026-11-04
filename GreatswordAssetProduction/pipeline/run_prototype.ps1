$ErrorActionPreference='Stop'
$production='C:\The Veilbound\GreatswordAssetProduction'
$inputImage='C:\The Veilbound\Veilborne_Greatsword_Tripo_Pack\references\all_10_swords_tripo_input.png'
$marker=Join-Path $production 'logs\PAID_PROTOTYPE_STARTED.json'
if(Test-Path -LiteralPath $marker){throw 'The single paid attempt has already started. Inspect its saved result/task; do not resubmit.'}
$plan=(Get-Content -LiteralPath (Join-Path $production 'logs\prototype_plan.txt') -Raw)|ConvertFrom-Json
if(-not $plan.valid -or $plan.steps.Count -ne 1 -or $plan.model -ne 'v3.1-20260211' -or $plan.steps[0].payload.texture -or $plan.steps[0].payload.pbr -or -not $plan.steps[0].payload.generate_parts){throw 'Unexpected plan: refusing paid task'}
$env:TRIPO_CLIENT_APP='codex';$env:TRIPO_DISTRIBUTION_CHANNEL='codex_plugin'
$balanceText=tripo balance --json
if($LASTEXITCODE -ne 0){throw 'Cannot verify balance'}
$balanceText | Set-Content -LiteralPath (Join-Path $production 'qa\balance_before.json')
$balance=$balanceText|ConvertFrom-Json
if($balance.balance -lt 40 -or $balance.frozen -ne 0){throw 'Balance or frozen credits require inspection before submission'}
@{input=$inputImage;input_sha256=(Get-FileHash -LiteralPath $inputImage -Algorithm SHA256).Hash;max_paid_tasks=1;expected_credits=40;model='v3.1-20260211';started_utc=[DateTime]::UtcNow.ToString('o')}|ConvertTo-Json|Set-Content -LiteralPath $marker
$env:TRIPO_CLIENT_APP='codex';$env:TRIPO_DISTRIBUTION_CHANNEL='codex_plugin'
tripo make $inputImage -p face_limit=30000 -p generate_parts=true -p texture=false -p pbr=false --name veilbound-ten-greatswords-prototype -o (Join-Path $production 'raw') --json --yes --no-open 1> (Join-Path $production 'logs\prototype_result.json') 2> (Join-Path $production 'logs\prototype_progress.log')
$generationExit=$LASTEXITCODE
$generationExit|Set-Content -LiteralPath (Join-Path $production 'logs\prototype_exit_code.txt')
Write-Output ('TRIPO_PROTOTYPE_FINISHED_EXIT '+$generationExit)
exit $generationExit
