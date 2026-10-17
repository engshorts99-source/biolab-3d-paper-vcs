#!/usr/bin/env zsh
set -e

PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"
APP_BUNDLE="$PROJECT_DIR/Build/macOS/BioCleanroomLab.app"
MANAGED_DIR="$APP_BUNDLE/Contents/Resources/Data/Managed"
UNITY_CONTENTS="/Applications/Unity/Hub/Editor/6000.0.83f1/Unity.app/Contents"

echo "🔨 Compiling BioCleanroomLab C# scripts into Assembly-CSharp.dll..."
"$UNITY_CONTENTS/NetCoreRuntime/dotnet" "$UNITY_CONTENTS/DotNetSdkRoslyn/csc.dll" \
  -target:library \
  -out:"$MANAGED_DIR/Assembly-CSharp.dll" \
  -nostdlib \
  -r:"$MANAGED_DIR/mscorlib.dll" \
  -r:"$MANAGED_DIR/netstandard.dll" \
  -r:"$MANAGED_DIR/System.dll" \
  -r:"$MANAGED_DIR/System.Core.dll" \
  -r:"$MANAGED_DIR/UnityEngine.dll" \
  -r:"$MANAGED_DIR/UnityEngine.CoreModule.dll" \
  -r:"$MANAGED_DIR/UnityEngine.PhysicsModule.dll" \
  -r:"$MANAGED_DIR/UnityEngine.IMGUIModule.dll" \
  -r:"$MANAGED_DIR/UnityEngine.TextRenderingModule.dll" \
  -r:"$MANAGED_DIR/UnityEngine.UI.dll" \
  -r:"$MANAGED_DIR/UnityEngine.UIModule.dll" \
  -r:"$MANAGED_DIR/UnityEngine.JSONSerializeModule.dll" \
  -r:"$MANAGED_DIR/UnityEngine.ImageConversionModule.dll" \
  -r:"$MANAGED_DIR/UnityEngine.InputLegacyModule.dll" \
  -r:"$MANAGED_DIR/UnityEngine.ScreenCaptureModule.dll" \
  -r:"$MANAGED_DIR/UnityEngine.AnimationModule.dll" \
  "$PROJECT_DIR"/Assets/Scripts/Core/*.cs \
  "$PROJECT_DIR"/Assets/Scripts/Environment/*.cs \
  "$PROJECT_DIR"/Assets/Scripts/Equipment/*.cs \
  "$PROJECT_DIR"/Assets/Scripts/Player/*.cs \
  "$PROJECT_DIR"/Assets/Scripts/Protocols/*.cs \
  "$PROJECT_DIR"/Assets/Scripts/Warehouse/*.cs \
  "$PROJECT_DIR"/Assets/Scripts/PaperVCS/*.cs \
  "$PROJECT_DIR"/Assets/Scripts/UI/*.cs

echo "✅ Compilation succeeded! Launching BioCleanroomLab.app..."
open "$APP_BUNDLE"
