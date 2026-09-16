#!/usr/bin/env bash
#
# Packages the built mod into BetterSuppressors_<version>.zip next to this script.
#
# Usage:
#   export SPT_RUNTIME=/path/to/SPT/SPT_Runtime
#   ./package.sh
#
set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="$HERE/src/BetterSuppressors"
CONFIGURATION="${CONFIGURATION:-Release}"

# Where the mod lands when the zip is extracted into the SPT folder.
DEST="${DEST:-user/mods/BetterSuppressors}"

VERSION="$(sed -n 's:.*<Version>\([0-9][^<]*\)</Version>.*:\1:p' "$PROJECT/BetterSuppressors.csproj" | head -1)"

if [[ -z "$VERSION" ]]; then
    echo "package.sh: could not read <Version> out of $PROJECT/BetterSuppressors.csproj" >&2
    exit 1
fi

echo "==> building $CONFIGURATION"
dotnet build "$PROJECT" -c "$CONFIGURATION" -v q --nologo

# Ask MSBuild for the path rather than assuming the TFM folder.
DLL="$(dotnet msbuild "$PROJECT" -getProperty:TargetPath -p:Configuration="$CONFIGURATION" -v:q --nologo | tr -d '\r')"

for f in "$DLL" "$PROJECT/config.json"; do
    [[ -f "$f" ]] || { echo "package.sh: missing $f" >&2; exit 1; }
done

STAGE="$(mktemp -d)"
trap 'rm -rf "$STAGE"' EXIT

mkdir -p "$STAGE/$DEST"
cp "$DLL" "$PROJECT/config.json" "$STAGE/$DEST/"

ZIP="$HERE/BetterSuppressors_${VERSION}.zip"
rm -f "$ZIP"
( cd "$STAGE" && zip -r -q -X "$ZIP" . )

echo "==> $ZIP"
unzip -l "$ZIP" | sed '1,3d;$d'
