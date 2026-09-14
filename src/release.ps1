# 构建后自动把英文产物改名为中文发布名（读取最新的 WinIconTools-v*.exe）
$ErrorActionPreference = 'Stop'
try {
    $src = Get-ChildItem -Path (Join-Path $PSScriptRoot '..') -Filter 'WinIconTools-v*.exe' |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($src) {
        $dst = Join-Path (Split-Path $src.FullName) ($src.Name -replace '^WinIconTools', '桌面图标美化工具')
        Move-Item -Force $src.FullName $dst
        Write-Host ("[OK] Renamed to: " + (Split-Path $dst -Leaf))
    }
} catch {
    Write-Host ("[warn] rename failed: " + $_.Exception.Message + " (可手动重命名)")
}
