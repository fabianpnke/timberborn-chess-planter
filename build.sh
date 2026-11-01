#!/usr/bin/env bash
#
# Builds the mod assembly and installs the whole mod folder into Timberborn.
#
# Environment overrides:
#   TIMBERBORN_MANAGED  folder holding the game's Timberborn.*.dll files
#   TIMBERBORN_MODS     the game's Mods folder
#
set -euo pipefail

repo="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
mods_dir="${TIMBERBORN_MODS:-$HOME/Documents/Timberborn/Mods}"
target="$mods_dir/ChessPlanter"

dotnet build "$repo/src/ChessPlanter/ChessPlanter.csproj" --nologo "$@"

if [[ ! -d "$mods_dir" ]]; then
  echo "Timberborn's Mods folder was not found at '$mods_dir'." >&2
  echo "Set TIMBERBORN_MODS to point at it." >&2
  exit 1
fi

mkdir -p "$target"
# The game's Workshop uploader keeps the item id in workshop_data.json inside the installed
# folder. Excluded, so --delete leaves it alone and updates go to the same Workshop item.
rsync -a --delete --exclude '.DS_Store' --exclude 'workshop_data.json' "$repo/mod/" "$target/"

echo "Installed to $target"
