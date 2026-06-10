# Detener servicio OVR
$service = Get-Service -Name "OVRService" -ErrorAction SilentlyContinue

if ($service -and $service.Status -ne "Stopped") {
    Stop-Service -Name "OVRService" -Force
    Write-Host "OVRService detenido."
} else {
    Write-Host "OVRService ya estaba detenido."
}

# Matar proceso OVRServer_x64 si sigue vivo
Get-Process -Name "OVRServer_x64" -ErrorAction SilentlyContinue | Stop-Process -Force

Write-Host "Procesos OVRServer_x64 cerrados (si existían)."