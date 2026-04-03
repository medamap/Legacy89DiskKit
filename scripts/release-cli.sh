#!/usr/bin/env bash
set -euo pipefail

if [[ $# -ne 1 ]]; then
  echo "Usage: scripts/release-cli.sh <version>" >&2
  exit 1
fi

VERSION="$1"
if [[ ! "$VERSION" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
  echo "Version must be semantic version without leading v, for example: 2.0.0" >&2
  exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
PROJECT_PATH="$REPO_ROOT/CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj"
TEST_PROJECT_PATH="$REPO_ROOT/CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj"
RELEASE_NOTES_PATH="$REPO_ROOT/RELEASE_NOTES_v${VERSION}.md"
PUBLISH_ROOT="$REPO_ROOT/publish/v${VERSION}"
RELEASE_ROOT="$REPO_ROOT/release/v${VERSION}"
SAMPLE_IMAGE="${LEGACY89_SAMPLE_IMAGE:-}"
RIDS=("win-x64" "linux-x64" "osx-x64" "osx-arm64")

if [[ ! -f "$PROJECT_PATH" ]]; then
  echo "CLI project not found: $PROJECT_PATH" >&2
  exit 1
fi

if [[ ! -f "$TEST_PROJECT_PATH" ]]; then
  echo "Test project not found: $TEST_PROJECT_PATH" >&2
  exit 1
fi

if [[ ! -f "$RELEASE_NOTES_PATH" ]]; then
  echo "Release notes not found: $RELEASE_NOTES_PATH" >&2
  exit 1
fi

case "$(uname -s):$(uname -m)" in
  Darwin:arm64)
    HOST_RID="osx-arm64"
    HOST_EXECUTABLE="Legacy89DiskKit.Cli"
    ;;
  Darwin:x86_64)
    HOST_RID="osx-x64"
    HOST_EXECUTABLE="Legacy89DiskKit.Cli"
    ;;
  Linux:x86_64)
    HOST_RID="linux-x64"
    HOST_EXECUTABLE="Legacy89DiskKit.Cli"
    ;;
  *)
    echo "Unsupported host platform for smoke checks: $(uname -s) $(uname -m)" >&2
    exit 1
    ;;
esac

rm -rf "$PUBLISH_ROOT" "$RELEASE_ROOT"
mkdir -p "$PUBLISH_ROOT" "$RELEASE_ROOT"

echo "Running tests"
dotnet test "$TEST_PROJECT_PATH" /p:UseAppHost=false

for RID in "${RIDS[@]}"; do
  OUTPUT_DIR="$PUBLISH_ROOT/$RID"
  mkdir -p "$OUTPUT_DIR"

  echo "Publishing $RID"
  dotnet publish "$PROJECT_PATH" \
    -c Release \
    -r "$RID" \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishAot=false \
    -o "$OUTPUT_DIR"

  if [[ "$RID" == win-x64 ]]; then
    EXECUTABLE_PATH="$OUTPUT_DIR/Legacy89DiskKit.Cli.exe"
  else
    EXECUTABLE_PATH="$OUTPUT_DIR/Legacy89DiskKit.Cli"
  fi

  if [[ ! -f "$EXECUTABLE_PATH" ]]; then
    echo "Expected executable not found for $RID: $EXECUTABLE_PATH" >&2
    exit 1
  fi
done

HOST_ARTIFACT="$PUBLISH_ROOT/$HOST_RID/$HOST_EXECUTABLE"

echo "Running smoke checks on $HOST_RID"
"$HOST_ARTIFACT" --help >/dev/null
"$HOST_ARTIFACT" disk --help >/dev/null
"$HOST_ARTIFACT" list --help >/dev/null
if [[ -n "$SAMPLE_IMAGE" ]]; then
  if [[ ! -f "$SAMPLE_IMAGE" ]]; then
    echo "Sample image not found: $SAMPLE_IMAGE" >&2
    exit 1
  fi

  "$HOST_ARTIFACT" list "$SAMPLE_IMAGE" -e sjis >/dev/null
fi

for RID in "${RIDS[@]}"; do
  ARCHIVE_BASENAME="Legacy89DiskKit.Cli-v${VERSION}-${RID}"
  if [[ "$RID" == "win-x64" ]]; then
    ARCHIVE_PATH="$RELEASE_ROOT/${ARCHIVE_BASENAME}.zip"
    (
      cd "$PUBLISH_ROOT"
      zip -qry "$ARCHIVE_PATH" "$RID"
    )
  else
    ARCHIVE_PATH="$RELEASE_ROOT/${ARCHIVE_BASENAME}.tar.gz"
    tar -C "$PUBLISH_ROOT" -czf "$ARCHIVE_PATH" "$RID"
  fi

  if [[ ! -f "$ARCHIVE_PATH" ]]; then
    echo "Expected archive not found for $RID: $ARCHIVE_PATH" >&2
    exit 1
  fi
done

echo "Release artifacts created:"
find "$RELEASE_ROOT" -maxdepth 1 -type f | sort
