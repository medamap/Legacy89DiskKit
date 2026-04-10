#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PREFIX_ARG=""
NEXT_IS_PREFIX=0

for arg in "$@"; do
  if [[ "$NEXT_IS_PREFIX" -eq 1 ]]; then
    PREFIX_ARG="$arg"
    NEXT_IS_PREFIX=0
    continue
  fi

  if [[ "$arg" == "--prefix" ]]; then
    NEXT_IS_PREFIX=1
  fi
done

"$SCRIPT_DIR/install-cli.sh" "$@"

if [[ -z "$PREFIX_ARG" ]]; then
  if [[ -w /usr/local/bin || ( ! -e /usr/local/bin && -w /usr/local ) ]]; then
    PREFIX_ARG="/usr/local"
  else
    PREFIX_ARG="$HOME/.local"
  fi
fi

BIN_DIR="$PREFIX_ARG/bin"
if [[ ":$PATH:" != *":$BIN_DIR:"* ]]; then
  echo
  echo "To use l89 in the current shell, run:"
  echo "  export PATH=\"$BIN_DIR:\$PATH\""
fi
