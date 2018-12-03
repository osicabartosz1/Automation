Dim objShell
Set objShell = WScript.CreateObject ("WScript.shell")
for a=0 to 2
	objShell.run "C:\Users\Bartek\Desktop\4lekcja_Msgbox.vbs"
Next
Set objShell = Nothing