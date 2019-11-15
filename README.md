# OrchestratorMaintenanceMode

## PowerShell

PowerShell スクリプトによる Orchestrator メンテナンス機能の利用方法を説明します。

以降の例では、Orchestrator のホスト名を orchestrator.example.com とします。

資格情報を cred.json ファイルに記憶して使用していますが、別のファイル名でも構いません。

### 準備

まず、cred.json ファイルの password の値を操作対象の Orchestrator の host テナントの admin ユーザーのパスワードに変更してください。

### Draining モードへの移行

通常運用をしている状態からメンテナンスモードに移行する場合、まず Draining モードへ移行する必要があります。

```
.\MaintenanceStart.ps1  orchestrator.example.com .\cred.json Draining
```

### Suspended モードへの移行

Draining モードに移行できたら Suspended モードへの移行が可能になります。

```
.\MaintenanceStart.ps1  orchestrator.example.com .\cred.json Suspended
```

### メンテナンスモードの終了

メンテナンスモードを終了し、通常運用に戻します。

```
.\MaintenanceEnd.ps1  orchestrator.example.com .\cred.json
```

### 状態、統計情報、前回のメンテナンスモード移行の履歴の取得

```
.\MaintenanceGet.ps1  orchestrator.example.com .\cred.json
```
