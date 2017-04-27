param([string]$RootDir)

if (-not $RootDir.EndsWith('\')) {
    $RootDir = $RootDir + '\'
}

write-host $RootDir

new-item -itemtype Directory -force -path "$($RootDir)packages"

new-item -itemtype Directory -force -path "$($RootDir)packages\Ammy.1.0.0"
new-item -itemtype Directory -force -path "$($RootDir)packages\Ammy.1.0.0\build"

new-item -itemtype Directory -force -path "$($RootDir)packages\Ammy.Host.1.0.0"
new-item -itemtype Directory -force -path "$($RootDir)packages\Ammy.Host.1.0.0\content"
new-item -itemtype Directory -force -path "$($RootDir)packages\Ammy.Host.1.0.0\lib"
new-item -itemtype Directory -force -path "$($RootDir)packages\Ammy.Host.1.0.0\lib\net40"

new-item -itemtype Directory -force -path "$($RootDir)packages\Ammy.Uwp.1.0.0\"
new-item -itemtype Directory -force -path "$($RootDir)packages\Ammy.Uwp.1.0.0\content"
new-item -itemtype Directory -force -path "$($RootDir)packages\Ammy.Uwp.1.0.0\lib"
new-item -itemtype Directory -force -path "$($RootDir)packages\Ammy.Uwp.1.0.0\lib\uap10.0"

new-item -itemtype Directory -force -path "$($RootDir)packages\Ammy.Wpf.1.0.0"
new-item -itemtype Directory -force -path "$($RootDir)packages\Ammy.Wpf.1.0.0\content"
new-item -itemtype Directory -force -path "$($RootDir)packages\Ammy.Wpf.1.0.0\lib"
new-item -itemtype Directory -force -path "$($RootDir)packages\Ammy.Wpf.1.0.0\lib\net40"

new-item -itemtype Directory -force -path "$($RootDir)packages\Ammy.XamarinForms.1.0.0"
new-item -itemtype Directory -force -path "$($RootDir)packages\Ammy.XamarinForms.1.0.0\lib"
new-item -itemtype Directory -force -path "$($RootDir)packages\Ammy.XamarinForms.1.0.0\content"
new-item -itemtype Directory -force -path "$($RootDir)packages\Ammy.XamarinForms.1.0.0\lib\portable-win+net45+wp80+win81+wpa81+MonoAndroid10+Xamarin.iOS10+xamarinmac20"

copy "$($RootDir)lib\adb\*.*" "$($RootDir)packages\Ammy.1.0.0\build"
copy "$($RootDir)src\IDE\Ammy.VisualStudio.Service\bin\Debug\*.*" "$($RootDir)packages\Ammy.1.0.0\build"
copy "$($RootDir)src\AmmyStandardLib\lib-wpf.ammy" "$($RootDir)packages\Ammy.Wpf.1.0.0\content\lib.ammy"
copy "$($RootDir)src\AmmyStandardLib\lib-xamarin.ammy" "$($RootDir)packages\Ammy.XamarinForms.1.0.0\content\lib.ammy"
copy "$($RootDir)src\AmmyStandardLib\lib-uwp.ammy" "$($RootDir)packages\Ammy.Uwp.1.0.0\content\lib.ammy"
copy "$($RootDir)src\internal.cs" "$($RootDir)packages\Ammy.1.0.0\build"

copy "$($RootDir)src\Core\Ammy.Host\bin\Debug\*.*" "$($RootDir)packages\Ammy.Host.1.0.0\lib\net40"
copy "$($RootDir)src\Platforms\Ammy.Sidekick.Common\bin\Debug\*.*" "$($RootDir)packages\Ammy.Wpf.1.0.0\lib\net40\"
copy "$($RootDir)src\Platforms\Ammy.Sidekick.Uwp\bin\Debug\*.*" "$($RootDir)packages\Ammy.Uwp.1.0.0\lib\uap10.0\"
copy "$($RootDir)src\Platforms\Ammy.Sidekick.XamarinForms\bin\Debug\*.*" "$($RootDir)packages\Ammy.XamarinForms.1.0.0\lib\portable-win+net45+wp80+win81+wpa81+MonoAndroid10+Xamarin.iOS10+xamarinmac20\"
