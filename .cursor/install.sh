#!/usr/bin/env bash
# Candy Belt Sort — Cloud Agent bootstrap for the Unity 6 project.
# Idempotent: installs the Unity Editor once, then imports the project so all
# scripts compile and the Library is populated. The editor runs headless with
# Unity's free entitlements; UNITY_LICENSE / UNITY_SERIAL are only needed for
# Pro-only features and are activated best-effort when present.
set -euo pipefail

UNITY_VERSION="6000.2.8f1"
UNITY_CHANGESET="c9992ac36c34"
UNITY_ROOT="${UNITY_ROOT:-$HOME/unity/$UNITY_VERSION}"
UNITY_BIN="$UNITY_ROOT/Editor/Unity"
DL_URL="https://download.unity3d.com/download_unity/${UNITY_CHANGESET}/LinuxEditorInstaller/Unity.tar.xz"
PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BUILD_TARGET="StandaloneLinux64"

log() { printf '\n\033[1;36m[cbs-install]\033[0m %s\n' "$*"; }

# Run the Unity Editor headless. Logs go to a real temp file (Unity cannot
# open() a /dev/stdout that the runner has wired to a socket), then are echoed.
# Returns Unity's exit code.
run_unity() {
  local logf rc
  logf="$(mktemp)"
  set +e
  "$UNITY_BIN" -batchmode -nographics -logFile "$logf" "$@"
  rc=$?
  set -e
  cat "$logf"
  rm -f "$logf"
  return "$rc"
}

# 1) System libraries the Unity Editor links against on Ubuntu 24.04.
if ! dpkg -s libgtk-3-0t64 >/dev/null 2>&1 && ! dpkg -s libgtk-3-0 >/dev/null 2>&1; then
  log "Installing Unity Linux runtime libraries"
  sudo apt-get update -qq
  sudo DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
    libgtk-3-0 libnss3 libasound2t64 libgbm1 libnotify4 libxtst6 libxss1 \
    libglu1-mesa libcap2 libxrandr2 libxcursor1 libxi6 libxinerama1 \
    libgl1 libgl1-mesa-dri xvfb
fi

# 2) Unity Editor — download + extract once. A snapshot preserves it, so this
#    is skipped on subsequent boots.
if [ ! -x "$UNITY_BIN" ]; then
  log "Downloading Unity Editor $UNITY_VERSION (~4.4 GB)"
  mkdir -p "$UNITY_ROOT"
  tmp="$(mktemp -d)"
  curl -fL --retry 4 --retry-delay 5 -o "$tmp/Unity.tar.xz" "$DL_URL"
  log "Extracting Unity Editor"
  tar -xf "$tmp/Unity.tar.xz" -C "$UNITY_ROOT"
  rm -rf "$tmp"
fi
log "Unity Editor present at $UNITY_BIN"

# 3) License activation (optional — only if credentials are provided).
#    Personal: set UNITY_LICENSE to the contents of a .ulf file.
#    Plus/Pro: set UNITY_SERIAL + UNITY_EMAIL + UNITY_PASSWORD.
if [ -n "${UNITY_LICENSE:-}" ]; then
  log "Activating Unity license from UNITY_LICENSE (.ulf)"
  ulf="$(mktemp --suffix=.ulf)"
  printf '%s' "$UNITY_LICENSE" > "$ulf"
  # -manualLicenseFile returns a non-zero exit code even on success.
  run_unity -manualLicenseFile "$ulf" -quit || true
  rm -f "$ulf"
elif [ -n "${UNITY_SERIAL:-}" ] && [ -n "${UNITY_EMAIL:-}" ] && [ -n "${UNITY_PASSWORD:-}" ]; then
  log "Activating Unity license with serial + account"
  run_unity -serial "$UNITY_SERIAL" -username "$UNITY_EMAIL" -password "$UNITY_PASSWORD" -quit || true
else
  log "No Unity license credentials found; using free entitlements (fine for editor batchmode)."
fi

# 4) Import the project so scripts compile and the Library is populated. This
#    also runs the game's own level validation as a smoke test. The editor runs
#    headless with free entitlements, so no license activation is required.
log "Importing project + compiling scripts + validating levels (batchmode)"
run_unity -projectPath "$PROJECT_DIR" -buildTarget "$BUILD_TARGET" \
  -executeMethod CandyBeltSort.CandyBeltMenus.ValidateLevels -quit
log "Project import + level validation complete."
