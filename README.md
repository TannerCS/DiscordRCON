# DiscordRCON [[Invite Link]](https://discord.com/oauth2/authorize?client_id=744249367598333992&scope=bot&permissions=125952)
An RCON Discord Bot For Rust

---
## Command Usage

```json
!help

Command Usage Guide
Example command: "!server add <IP:PORT|Server ID> <RCON Password (Optional)>"
- "<>" means a field is required (unless it has an "(optional)" flag)
- "|" means "or". It can take an IP and port "or" a server ID
------------------------------------------------------
"!server" - Shows the help dialogue for server related information
"!rcon" - Shows the help dialogue for RCON related information
"!prefix <new prefix>" - Change command prefix
```

```json
!server

"!server status <IP:PORT>" - returns information about a specific server.
"!server watch|add <IP:PORT> <rconPwd (optional)> <RCON port (optional)>" - Add server to watchlist
"!server unwatch|remove <IP:PORT|Server ID>" - Remove server from watchlist
"!server watchlist|wl" - View server watchlist
"!server updatepwd|rconpwd|pwd <IP:PORT|Server ID> <RCON Password>" - Update RCON password
"!server updateport|rconport|port <IP:PORT|Server ID> <RCON Port>" - Update RCON port
```

```json
!rcon

"!rcon send <IP:PORT|Server ID|all> <command>" - Sends a custom user-specified command
```

```json
!prefix

Your current prefix is: !
To change the prefix, type !prefix <new prefix>
```
---
## Quick-Start Guide

Note: This is **ONLY** if you want to host your own bot.

1. Clone bot `git clone https://github.com/TannerCS/DiscordRCON.git`
2. CD into DiscordRCON `cd DiscordRCON`
3. Build project `dotnet build`
4. CD into compiled program `cd bin\Debug\netcoreapp3.1`
5. Create token.txt file and insert your [bot token](https://discord.com/developers/applications)
6. Run project `dotnet DiscordRCON.dll` **or** run `DiscordRCON.exe` if you're on Windows.
7. ???
8. Profit
