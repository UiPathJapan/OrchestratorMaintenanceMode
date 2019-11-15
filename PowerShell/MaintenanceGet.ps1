Param(
  [parameter(mandatory=$true)][string]$server,
  [parameter(mandatory=$true)][string]$cred
  )

# 認証情報を取得する
$authRsp = Invoke-RestMethod -Method Post -Uri "https://$server/api/Account" -ContentType "application/json" -InFile $cred
$authToken = "Bearer " + $authRsp.result

$ctype = "application/json;charset=utf-8"
$hdata = @{Authorization=$authToken}

#write-host $authToken

# メンテナンスモードの情報を取得する
$getRsp = Invoke-RestMethod -Method GET -Uri "https://$server/api/Maintenance/Get" -ContentType $ctype -Headers $hdata

#write-host $getRsp

write-host "Current State:" $getRsp.state
write-host "Job Stops Attempted:" $getRsp.jobStopsAttempted
write-host "Job Kills Attempted:" $getRsp.jobKillsAttempted
write-host "Triggers Skipped:" $getRsp.triggersSkipped
write-host "System Triggers Skipped:" $getRsp.systemTriggersSkipped
write-host "Logs:"
$index = 0
ForEach ($x in $getRsp.maintenanceLogs)
{
  if ($x.timeStamp -ne $null)
  {
	$t = Get-Date $x.timeStamp
    write-host "  [$index]" $t $x.state
  }
  else
  {
    write-host "  [$index]" "----/--/--" "--:--:--" $x.state 
  }
  $index++
}
