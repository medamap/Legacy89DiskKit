#!/usr/bin/env bash
set -euo pipefail

if [[ $# -ne 1 ]]; then
  echo "Usage: scripts/release-native.sh <version>" >&2
  exit 1
fi

VERSION="$1"
if [[ ! "$VERSION" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
  echo "Version must be semantic version without leading v, for example: 2.0.0" >&2
  exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
PROJECT_PATH="$REPO_ROOT/CSharp/Legacy89DiskKit.NativeInterop/Legacy89DiskKit.NativeInterop.csproj"
TEST_APP_PATH="$REPO_ROOT/CSharp/NativeInteropTestApp/NativeInteropTestApp.csproj"
HEADER_PATH="$REPO_ROOT/include/legacy89diskkit_native.h"
RELEASE_NOTES_PATH="$REPO_ROOT/RELEASE_NOTES_v${VERSION}.md"
SAMPLE_IMAGE="${LEGACY89_SAMPLE_IMAGE:-}"
PUBLISH_ROOT="$REPO_ROOT/publish/v${VERSION}/native"
RELEASE_ROOT="$REPO_ROOT/release/v${VERSION}"

if [[ ! -f "$PROJECT_PATH" ]]; then
  echo "Native project not found: $PROJECT_PATH" >&2
  exit 1
fi

if [[ ! -f "$TEST_APP_PATH" ]]; then
  echo "Native smoke test app not found: $TEST_APP_PATH" >&2
  exit 1
fi

if [[ ! -f "$HEADER_PATH" ]]; then
  echo "Native public header not found: $HEADER_PATH" >&2
  exit 1
fi

if [[ ! -f "$RELEASE_NOTES_PATH" ]]; then
  echo "Release notes not found: $RELEASE_NOTES_PATH" >&2
  exit 1
fi

case "$(uname -s):$(uname -m)" in
  Darwin:arm64)
    HOST_RID="osx-arm64"
    HOST_LIB_NAME="Legacy89DiskKit.Native.dylib"
    INTERNAL_LIB_NAME="Legacy89DiskKit.NativeInterop.dylib"
    LIBRARY_PATH_VALUE="/opt/homebrew/opt/openssl@3/lib:/opt/homebrew/opt/brotli/lib"
    ARCHIVE_EXT="tar.gz"
    ;;
  Darwin:x86_64)
    HOST_RID="osx-x64"
    HOST_LIB_NAME="Legacy89DiskKit.Native.dylib"
    INTERNAL_LIB_NAME="Legacy89DiskKit.NativeInterop.dylib"
    LIBRARY_PATH_VALUE="/usr/local/opt/openssl@3/lib:/usr/local/opt/brotli/lib:/opt/homebrew/opt/openssl@3/lib:/opt/homebrew/opt/brotli/lib"
    ARCHIVE_EXT="tar.gz"
    ;;
  Linux:x86_64)
    HOST_RID="linux-x64"
    HOST_LIB_NAME="Legacy89DiskKit.Native.so"
    INTERNAL_LIB_NAME="Legacy89DiskKit.NativeInterop.so"
    LIBRARY_PATH_VALUE="${LIBRARY_PATH:-}"
    ARCHIVE_EXT="tar.gz"
    ;;
  MINGW*:*|MSYS*:*|CYGWIN*:*|Windows_NT:*)
    echo "Use scripts/release-native.ps1 on Windows." >&2
    exit 1
    ;;
  *)
    echo "Unsupported host platform for native release: $(uname -s) $(uname -m)" >&2
    exit 1
    ;;
esac

TARGET_ROOT="$PUBLISH_ROOT/$HOST_RID"
INCLUDE_ROOT="$TARGET_ROOT/include"
BIN_ROOT="$TARGET_ROOT/lib"
rm -rf "$TARGET_ROOT"
mkdir -p "$INCLUDE_ROOT" "$BIN_ROOT" "$RELEASE_ROOT"

echo "Publishing native library for $HOST_RID"
env LIBRARY_PATH="$LIBRARY_PATH_VALUE" dotnet publish "$PROJECT_PATH" \
  -c Release \
  -r "$HOST_RID" \
  -p:PublishAot=true \
  -p:NativeLib=Shared \
  -o "$TARGET_ROOT/build"

INTERNAL_LIB_PATH="$TARGET_ROOT/build/$INTERNAL_LIB_NAME"
PUBLIC_LIB_PATH="$BIN_ROOT/$HOST_LIB_NAME"

if [[ ! -f "$INTERNAL_LIB_PATH" ]]; then
  echo "Expected native library not found: $INTERNAL_LIB_PATH" >&2
  exit 1
fi

cp "$INTERNAL_LIB_PATH" "$PUBLIC_LIB_PATH"
cp "$HEADER_PATH" "$INCLUDE_ROOT/legacy89diskkit_native.h"

echo "Running native smoke check on $HOST_RID"
if [[ -n "$SAMPLE_IMAGE" ]]; then
  if [[ ! -f "$SAMPLE_IMAGE" ]]; then
    echo "Sample image not found: $SAMPLE_IMAGE" >&2
    exit 1
  fi

  dotnet run --project "$TEST_APP_PATH" -- "$PUBLIC_LIB_PATH" "$SAMPLE_IMAGE" >/dev/null
fi

ARCHIVE_BASE="Legacy89DiskKit.Native-v${VERSION}-${HOST_RID}"
if [[ "$ARCHIVE_EXT" == "zip" ]]; then
  ARCHIVE_PATH="$RELEASE_ROOT/${ARCHIVE_BASE}.zip"
  (
    cd "$PUBLISH_ROOT"
    zip -qry "$ARCHIVE_PATH" "$HOST_RID"
  )
else
  ARCHIVE_PATH="$RELEASE_ROOT/${ARCHIVE_BASE}.tar.gz"
  tar -C "$PUBLISH_ROOT" -czf "$ARCHIVE_PATH" "$HOST_RID"
fi

if [[ ! -f "$ARCHIVE_PATH" ]]; then
  echo "Expected native archive not found: $ARCHIVE_PATH" >&2
  exit 1
fi

echo "Native release artifacts created:"
printf '%s\n' "$ARCHIVE_PATH"
echo "Unverified native targets remain documented roadmap items for now."
