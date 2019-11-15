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

# �����e�i���X���[�h���I������
$postRsp = Invoke-RestMethod -Method POST -Uri "https://$server/api/Maintenance/End" -ContentType $ctype -Headers $hdata

write-host $postRsp
