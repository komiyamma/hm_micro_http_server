# HmMicroHttpServer

軽量なC#製マイクロHTTPサーバー。

## 特徴

- .NET 9.0対応
- シンプルな構成
- Windows向けAOTビルド済み（dotnetランタイム不要）

## 使い方（Windows）

1. リリースファイル一式をダウンロード
2. `HmMicroHttpServer.exe` をダブルクリック、またはコマンドプロンプト/PowerShellで実行
	```pwsh
	.\HmMicroHttpServer.exe
	```
3. ブラウザで `http://localhost:8080/` などにアクセス

> .NETランタイムは不要です。AOT（Ahead-Of-Time）コンパイル済みの実行ファイルのみで動作します。

## ライセンス

本プロジェクトはCC0ライセンスです。詳細はLICENSEファイルを参照してください。
