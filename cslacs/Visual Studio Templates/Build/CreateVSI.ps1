#Visual Studio Templates folder location - BATCH file will be calling this script from that location
$root = (Get-Location).Path
#Load Zip Library
[System.Reflection.Assembly]::LoadFrom($root + "\Build\Ionic.Utils.Zip.dll")
#Delete Generated Item Templates
del ($root + "\Item Templates\cs\Generated Templates\*.zip") 
del ($root + "\Item Templates\vb\Generated Templates\*.zip") 

$cs_tmplt_folders = Get-ChildItem -path ($root + "\Item Templates\cs\Templates Source") -exclude ".svn"
$vb_tmplt_folders = Get-ChildItem -path ($root + "\Item Templates\vb\Templates Source") -exclude ".svn"

#Zip all Item Templates and place them in "Generated Templates"
foreach ($tmplt_folder in $cs_tmplt_folders)
{
  "folder: " + ([System.IO.DirectoryInfo]$tmplt_folder).FullName
  $template_zip_name = $root + "\Item Templates\cs\Generated Templates\" + $tmplt_folder.Name + ".zip"
  #$template_zip_name
  $template_zip = New-Object Ionic.Utils.Zip.ZipFile($template_zip_name)
  foreach ($tmplt_item in Get-ChildItem $tmplt_folder -exclude ".svn")
  {
    $template_zip.AddFile($tmplt_item.FullName, "")
  }
  $template_zip.Save()
}
foreach ($tmplt_folder in $vb_tmplt_folders)
{
  "folder: " + ([System.IO.DirectoryInfo]$tmplt_folder).FullName
  $template_zip_name = $root + "\Item Templates\vb\Generated Templates\" + $tmplt_folder.Name + ".zip"
  #$template_zip_name
  $template_zip = New-Object Ionic.Utils.Zip.ZipFile($template_zip_name)
  foreach ($tmplt_item in Get-ChildItem $tmplt_folder -exclude ".svn")
  {
    $template_zip.AddFile($tmplt_item.FullName, "")
  }
  $template_zip.Save()
}

#Create VSI
$cs_vsi_zip_name = $root + "\VSI\cs\cslacs.vsi"
$vb_vsi_zip_name = $root + "\VSI\vb\cslavb.vsi"

del $cs_vsi_zip_name
del $vb_vsi_zip_name

#Create CS VSI
$vsi_zip = New-Object Ionic.Utils.Zip.ZipFile($cs_vsi_zip_name)
foreach($templ_zip in  Get-ChildItem -path ($root + "\Item Templates\cs\Generated Templates")  -exclude ".svn")
{
  $vsi_zip.AddFile($templ_zip.FullName, "")
}
$vsi_zip.AddFile($root + "\VSI\cs\Source\cslacs.vscontent","")
#Add Snippets
foreach($snip_item in Get-ChildItem -path ($root + "\Snippets\cs") -exclude ".svn")
{
  $vsi_zip.AddFile($snip_item.FullName, "")
}

$vsi_zip.Save()

#Create VB VSI
$vsi_zip = New-Object Ionic.Utils.Zip.ZipFile($vb_vsi_zip_name)
foreach($templ_zip in  Get-ChildItem -path ($root + "\Item Templates\vb\Generated Templates")  -exclude ".svn")
{
  $vsi_zip.AddFile($templ_zip.FullName, "")
}
$vsi_zip.AddFile($root + "\VSI\vb\Source\cslavb.vscontent","")
#Add Snippets
foreach($snip_item in Get-ChildItem -path ($root + "\Snippets\vb") -exclude ".svn")
{
  $vsi_zip.AddFile($snip_item.FullName, "")
}
$vsi_zip.Save()