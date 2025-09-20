#!/usr/bin/env bash
set -euo pipefail

# Unity CI build script for Linux host that builds Windows and Linux players
# and copies README files into each build output folder.
#
# Required env vars described in tools/.env.example
# Usage:
#   cp tools/.env.example tools/.env
#   # edit tools/.env to set UNITY_PATH, etc.
#   ./tools/build.sh

ROOT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")"/.. && pwd)

ENV_PATH="${ROOT_DIR}/tools/.env"
if [[ ! -f "${ENV_PATH}" ]]; then
	echo "ERROR: Missing ${ENV_PATH}. Copy tools/.env.example to tools/.env and configure UNITY_PATH." >&2
	exit 1
fi
# shellcheck disable=SC1090
. "${ENV_PATH}"

PROJECT_PATH=${PROJECT_PATH:-"$ROOT_DIR"}
OUTPUT_DIR=${OUTPUT_DIR:-"$ROOT_DIR/Builds"}
BUILD_VERSION=${BUILD_VERSION:-$(grep '^  bundleVersion:' "$PROJECT_PATH/ProjectSettings/ProjectSettings.asset" | awk '{print $2}')}
LOG_DIR="$OUTPUT_DIR/_logs"
WIN_DIR="$OUTPUT_DIR/windows/$BUILD_VERSION"
LINUX_DIR="$OUTPUT_DIR/linux/$BUILD_VERSION"

if [[ -z "${UNITY_PATH}" ]]; then
	echo "ERROR: UNITY_PATH is required" >&2
	exit 1
fi

if [[ ! -x "${UNITY_PATH}" ]]; then
	echo "ERROR: UNITY_PATH does not point to an executable: ${UNITY_PATH}" >&2
	exit 1
fi

mkdir -p "$LOG_DIR" "$WIN_DIR" "$LINUX_DIR"

copy_readmes() {
	local target_dir="$1"
	[[ -f "$ROOT_DIR/README.md" ]] && cp "$ROOT_DIR/README.md" "$target_dir/"
	[[ -f "$ROOT_DIR/config.README.md" ]] && cp "$ROOT_DIR/config.README.md" "$target_dir/"
}

BUILD_METHOD=${BUILD_METHOD:-}

set +e
if [[ -z "$BUILD_METHOD" ]]; then
	echo "Building Windows64 player..."
	"$UNITY_PATH" \
		-batchmode -quit -nographics \
		-projectPath "$PROJECT_PATH" \
		-buildWindows64Player "$WIN_DIR/simulation.exe" \
		-logFile "$LOG_DIR/windows.log" "${EXTRA_UNITY_ARGS:-}"
	WIN_EXIT=$?

	echo "Building Linux64 player..."
	"$UNITY_PATH" \
		-batchmode -quit -nographics \
		-projectPath "$PROJECT_PATH" \
		-buildLinux64Player "$LINUX_DIR/simulation.x86_64" \
		-logFile "$LOG_DIR/linux.log" "${EXTRA_UNITY_ARGS:-}"
	LINUX_EXIT=$?
else
	echo "Building via custom executeMethod: $BUILD_METHOD"
	"$UNITY_PATH" \
		-batchmode -quit -nographics \
		-projectPath "$PROJECT_PATH" \
		-executeMethod "$BUILD_METHOD" \
		-logFile "$LOG_DIR/build.log" "${EXTRA_UNITY_ARGS:-}"
	METHOD_EXIT=$?
	WIN_EXIT=$METHOD_EXIT
	LINUX_EXIT=$METHOD_EXIT
fi
set -e

if [[ ${WIN_EXIT:-1} -eq 0 ]]; then
	copy_readmes "$WIN_DIR"
else
	echo "Windows build failed with exit code: ${WIN_EXIT:-1}" >&2
fi

if [[ ${LINUX_EXIT:-1} -eq 0 ]]; then
	copy_readmes "$LINUX_DIR"
else
	echo "Linux build failed with exit code: ${LINUX_EXIT:-1}" >&2
fi


zip_build() {
	local build_dir="$1"
	local zip_name="$2"
	local parent_dir
	parent_dir=$(dirname "$build_dir")
	if [[ -d "$build_dir" ]]; then
		(cd "$parent_dir" && zip -r -q "$zip_name" "$(basename "$build_dir")")
		if [[ -f "$parent_dir/$zip_name" ]]; then
			echo "Zipped $build_dir to $parent_dir/$zip_name"
		else
			echo "Failed to create zip: $parent_dir/$zip_name" >&2
		fi
	fi
}

if [[ ${WIN_EXIT:-1} -eq 0 ]]; then
	zip_build "$WIN_DIR" "simulation-windows.zip"
fi
if [[ ${LINUX_EXIT:-1} -eq 0 ]]; then
	zip_build "$LINUX_DIR" "simulation-linux.zip"
fi

echo
echo "Build summary:"
if [[ -z "$BUILD_METHOD" ]]; then
	[[ ${WIN_EXIT:-1} -eq 0 ]] && echo " - Windows: OK -> $WIN_DIR" || echo " - Windows: FAILED (see $LOG_DIR/windows.log)"
	[[ ${LINUX_EXIT:-1} -eq 0 ]] && echo " - Linux:   OK -> $LINUX_DIR" || echo " - Linux:   FAILED (see $LOG_DIR/linux.log)"
else
	[[ ${WIN_EXIT:-1} -eq 0 ]] && echo " - Windows: OK -> $WIN_DIR" || echo " - Windows: FAILED (see $LOG_DIR/build.log)"
	[[ ${LINUX_EXIT:-1} -eq 0 ]] && echo " - Linux:   OK -> $LINUX_DIR" || echo " - Linux:   FAILED (see $LOG_DIR/build.log)"
fi

if [[ ${WIN_EXIT:-1} -ne 0 || ${LINUX_EXIT:-1} -ne 0 ]]; then
	exit 2
fi
