Param(
  [parameter(mandatory=$true)][string]$server,
  [parameter(mandatory=$true)][string]$cred,
  [parameter(mandatory=$true)][ValidateSet("Draining","Suspended")]$phase,
  [switch]$force,
  [switch]$killJobs
  )


# 認証情報を取得する
$authRsp = Invoke-RestMethod -Method Post -Uri "https://$server/api/Account" -ContentType "application/json" -InFile $cred
$authToken = "Bearer " + $authRsp.result

$ctype = "application/json;charset=utf-8"
$hdata = @{Authorization=$authToken}

#write-host $authToken

$pp = "?phase=$phase"
if ($force)
{
  $pp += "&force=true"
}
if ($killJobs)
{
  $pp += "&killJobs=true"
}

#write-host $pp

# メンテナンスモードを開始する
$postRsp = Invoke-RestMethod -Method POST -Uri "https://$server/api/Maintenance/Start$pp" -ContentType $ctype -Headers $hdata

write-host $postRsp
