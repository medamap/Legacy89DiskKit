#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage:
  scripts/install-cli.sh --source <publish-dir-or-executable> [--prefix <prefix>]
  scripts/install-cli.sh --uninstall [--prefix <prefix>]

This installs the published executable as:
  <prefix>/lib/legacy89diskkit/Legacy89DiskKit.Cli
and creates:
  <prefix>/bin/l89 -> ../lib/legacy89diskkit/Legacy89DiskKit.Cli

Defaults:
  prefix=/usr/local when writable, otherwise ~/.local
EOF
}

SOURCE=""
UNINSTALL=0
PREFIX=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --source)
      SOURCE="${2:-}"
      shift 2
      ;;
    --prefix)
      PREFIX="${2:-}"
      shift 2
      ;;
    --uninstall)
      UNINSTALL=1
      shift
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown argument: $1" >&2
      usage >&2
      exit 1
      ;;
  esac
done

if [[ -z "$PREFIX" ]]; then
  if [[ -w /usr/local/bin || ( ! -e /usr/local/bin && -w /usr/local ) ]]; then
    PREFIX="/usr/local"
  else
    PREFIX="$HOME/.local"
  fi
fi

BIN_DIR="$PREFIX/bin"
LIB_DIR="$PREFIX/lib/legacy89diskkit"
TARGET_EXE="$LIB_DIR/Legacy89DiskKit.Cli"
TARGET_LINK="$BIN_DIR/l89"

if [[ "$UNINSTALL" -eq 1 ]]; then
  rm -f "$TARGET_LINK"
  rm -f "$TARGET_EXE"
  rmdir "$LIB_DIR" 2>/dev/null || true
  echo "Removed l89 from $PREFIX"
  exit 0
fi

if [[ -z "$SOURCE" ]]; then
  echo "--source is required unless --uninstall is specified." >&2
  usage >&2
  exit 1
fi

mkdir -p "$BIN_DIR" "$LIB_DIR"

if [[ -d "$SOURCE" ]]; then
  SOURCE_EXE="$SOURCE/Legacy89DiskKit.Cli"
  if [[ ! -f "$SOURCE_EXE" ]]; then
    echo "Executable not found: $SOURCE_EXE" >&2
    exit 1
  fi

  rm -rf "$LIB_DIR"
  mkdir -p "$LIB_DIR"
  cp -R "$SOURCE"/. "$LIB_DIR"/
  chmod 755 "$TARGET_EXE"
else
  SOURCE_EXE="$SOURCE"
  if [[ ! -f "$SOURCE_EXE" ]]; then
    echo "Executable not found: $SOURCE_EXE" >&2
    exit 1
  fi

  install -m 755 "$SOURCE_EXE" "$TARGET_EXE"
fi

ln -sfn "../lib/legacy89diskkit/Legacy89DiskKit.Cli" "$TARGET_LINK"

echo "Installed:"
echo "  $TARGET_EXE"
echo "  $TARGET_LINK"

case ":$PATH:" in
  *":$BIN_DIR:"*) ;;
  *)
    echo "Note: $BIN_DIR is not on PATH."
    ;;
esac
