# LeagueBackupper

## A tool that aims to backup different versions of the League of Legends client in the smallest possible size.

## The principle of this tool is to reduce disk usage by only saving the differences between different versions of game client files, and to fully restore the original files when needed

### In my testing, I tested the following 20 versions of game client (Chinese version)

- 11.23.409.567 
- 11.24.412.2185
- 12.1.414.4249 
- 12.11.446.9344
- 12.12.448.6653
- 12.13.453.3037
- 12.14.456.5556
- 12.15.458.1365
- 12.18.468.1686
- 12.19.471.6581
- 12.2.419.1399
- 12.2.419.4793
- 12.22.479.3960
- 12.23.483.5656
- 12.6.432.1258
- 12.7.433.4138
- 12.9.439.7639
- 12.9.440.3307
- 13.1.487.9641
- 13.3.491.6222

### The original size of all these versions of client is about  311 GB,and after backup processing, all files are only 28GB in size, a total reduction of 91% in size

## Usage

### Backup  a game client 

```bash
LeagueBackupper.Commandline.exe backup -g <game_root_folder_path> -b <repo_path>
```

*game_root_folder_path: The root folder of a game client which you want to backup.*

*repo_path: The folder of your repository used to storage the backed data.*



### Extract game client from your backup repository

```bash
LeagueBackupper.Commandline.exe extract  -b <repo_path> -v <patch_version> -o <output>
```

repo_path: The folder of your repository used to storage the backed data.

*patch_version: The game version which you want to extract from the repo.*

output: The destination folder which you want to storage all the extracted client files. (It's actually the root directory of the game)



### Validate game client 

```bash
LeagueBackupper.Commandline.exe extract  -b <repo_path> -v <patch_version> --validate-only
```

the same as extract game client  but it will not extract the file, but only verify whether the all client file are correct



## Libraries

- [LeagueTookit](https://github.com/LeagueToolkit/LeagueToolkit)   LeagueToolkit is a library for parsing and editing assets from League of Legends.In my project i use it to parse wad files of lol game client

## NOTE:

This software has been tested in many game versions, but there is still no guarantee that in future new versions, the backup will fail due to changes made by RIOT Game . Therefore, after backing up your game client files, please do not delete the original files first. Please execute the Validate command or test yourself before deleting the original files
