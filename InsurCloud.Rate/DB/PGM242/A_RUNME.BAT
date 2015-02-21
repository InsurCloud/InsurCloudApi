SQLCMD -E -d master -i AA_CreateDB.first
SQLCMD -E -d master -i AA_RunScripts.first
SQLCMD -E -d master -i AA_TurnOffCmdShell.first

PAUSE