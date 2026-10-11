# LeagueBackupper

A tool for backing up multiple versions of the League of Legends client with minimal disk footprint.

## How it works

LeagueBackupper stores only the **differences** between client versions, rather than duplicating every identical file for each version. When you need a specific version, it fully reconstructs the original client from the stored data.

## Usage

### Backup a game client

```bash
LeagueBackupper.Commandline.exe backup -g <game_root_folder_path> -r <repo_path>
```

- `<game_root_folder_path>`: The root folder of the game client you want to back up.
- `<repo_path>`: The folder where your backup repository stores the backed-up data.

### Extract a game client from your repository

```bash
LeagueBackupper.Commandline.exe extract -r <repo_path> -v <patch_version> -o <output>
```

- `<repo_path>`: The folder where your backup repository is stored.
- `<patch_version>`: The game version you want to extract.
- `<output>`: The destination folder where the extracted client files will be placed (this becomes the root directory of the game).

### Validate a backed-up client

```bash
LeagueBackupper.Commandline.exe extract -r <repo_path> -v <patch_version> --validate-only
```

Same as `extract`, but does not write any files — it only verifies that all backed-up client files are intact and correct.



## Libraries

- [LeagueToolkit](https://github.com/LeagueToolkit/LeagueToolkit) — a library for parsing and editing League of Legends assets. Used here to parse WAD files in the game client.

## Known Issues

- **Slow backup / extraction speed.** Due to the current implementation, backup and extraction operations are relatively slow. When working with a large repository, please be patient and allow the process to complete.

## ⚠️ Note

Although this tool has been tested across many game versions, Riot Games may introduce changes in future updates that could cause the backup process to fail.

**After backing up your game client, do not delete the original files first.** Run the `validate` command (or manually test the extracted client) before removing the original.
