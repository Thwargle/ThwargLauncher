[System.Reflection.Assembly]::LoadWithPartialName("System.Windows.Forms")

function CheckDll
{
  param( [string]$dllpath)
  if (!(Test-Path $dllpath))
  {
    $result = [System.Windows.Forms.MessageBox]::Show("Missing $dllpath. Copy into place?", "Decal.Adapter", 4)
    if ($result -eq "YES")
    {
      $folderPath =  Split-Path $dllpath -parent
      $filename = Split-Path $dllpath -leaf
      New-Item -ItemType Directory -Force -Path $folderPath
      Copy-Item $filename $folderPath
    }
  }
}


$dllpath = "C:\Program Files (x86)\Decal 3.0\Decal.Adapter.dll"
CheckDll $dllpath
$dllpath = "C:\Games\VirindiPlugins\VirindiChatSystem5\VCS5.dll"
CheckDll $dllpath
$dllpath = "C:\Games\VirindiPlugins\VirindiViewService\VirindiViewService.dll"
CheckDll $dllpath
