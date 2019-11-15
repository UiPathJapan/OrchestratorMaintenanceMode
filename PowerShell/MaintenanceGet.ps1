Param(
  [parameter(mandatory=$true)][string]$server,
  [parameter(mandatory=$true)][string]$cred
  )

# �F�؏����擾����
$authRsp = Invoke-RestMethod -Method Post -Uri "https://$server/api/Account" -ContentType "application/json" -InFile $cred
$authToken = "Bearer " + $authRsp.result

$ctype = "application/json;charset=utf-8"
$hdata = @{Authorization=$authToken}

#write-host $authToken

# �����e�i���X���[�h�̏����擾����
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
