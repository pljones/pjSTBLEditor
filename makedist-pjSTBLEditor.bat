@echo off
set TargetName=pjSTBLEditor
set ConfigurationName=Release
set base=%TargetName%
rem -%ConfigurationName%
set src=%TargetName%-Source


set out=S:\Sims3\Tools\pjSTBLEditor\
set helpFolder=%out%HelpFiles

set mydate=%date: =0%
set dd=%mydate:~0,2%
set mm=%mydate:~3,2%
set yy=%mydate:~8,2%
set mytime=%time: =0%
set h=%mytime:~0,2%
set m=%mytime:~3,2%
set s=%mytime:~6,2%
set suffix=%yy%-%mm%%dd%-%h%%m%

if EXIST "%PROGRAMFILES%\nsis\makensis.exe" goto gotNotX86
if EXIST "%PROGRAMFILES(x86)%\nsis\makensis.exe" goto gotX86
echo "Could not find makensis."
goto noNSIS

:gotNotX86:
set MAKENSIS=%PROGRAMFILES%\nsis\makensis.exe
goto gotNSIS
:gotX86:
set MAKENSIS=%PROGRAMFILES(x86)%\nsis\makensis.exe
:gotNSIS:
set nsisv=/V3

if x%ConfigurationName%==xRelease goto REL
set pdb=
goto noREL
:REL:
set pdb=-xr!*.pdb
:noREL:


rem there shouldn't be any to delete...
del /q /f %out%%TargetName%*%suffix%.*

pushd ..
7za a -r -t7z -mx9 -ms -xr!.?* -xr!*.suo -xr!zzOld -xr!bin -xr!obj -xr!Makefile -xr!*.Config "%out%%src%_%suffix%.7z" "pjSTBLEditor"
popd


pushd XAML\bin\%ConfigurationName%
echo %suffix% >%TargetName%-Version.txt
attrib +r %TargetName%-Version.txt
del /f /q HelpFiles
rem xcopy "%helpFolder%\*" HelpFiles /s /i /y
7za a -r -t7z -mx9 -ms -xr!.?* -xr!*vshost* -xr!*.Config %pdb% "%out%%base%_%suffix%.7z" *
del /f %TargetName%-Version.txt
del /f /q HelpFiles
popd


7za x -o"%base%-%suffix%" "%out%%base%_%suffix%.7z"
pushd "%base%-%suffix%"
(
echo !cd %base%-%suffix%
for %%f in (*) do echo File /a %%f
rem pushd HelpFiles
rem echo SetOutPath $INSTDIR\HelpFiles
rem for %%f in (*) do echo File /a HelpFiles\%%f
rem echo SetOutPath $INSTDIR
rem popd
rem pushd Resources
rem echo SetOutPath $INSTDIR\Resources
rem for %%f in (*) do echo File /a Resources\%%f
rem echo SetOutPath $INSTDIR
rem popd
dir /-c "..\%base%-%suffix%" | find " bytes" | for /f "tokens=3" %%f in ('find /v " free"') do @echo StrCpy $0 %%f
) > ..\INSTFILES.txt

(
for %%f in (*) do echo Delete $INSTDIR\%%f
rem pushd HelpFiles
rem for %%f in (*) do echo Delete $INSTDIR\HelpFiles\%%f
rem echo RmDir HelpFiles
rem popd
rem pushd Resources
rem for %%f in (*) do echo Delete $INSTDIR\Resources\%%f
rem echo RmDir Resources
rem popd
) > UNINST.LOG
attrib +r +h UNINST.LOG
popd

"%MAKENSIS%" "/DINSTFILES=INSTFILES.txt" "/DUNINSTFILES=UNINST.LOG" "/DVSN=%suffix%" %nsisv% mknsis.nsi "/XOutFile %out%%base%_%suffix%.exe"

:done:
rmdir /s/q %base%-%suffix%
del INSTFILES.txt
:noNSIS:
pause
