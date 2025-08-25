# HmMicroHttpServer

軽量・高速なクロスプラットフォーム対応のAOTマイクロHTTPサーバーです。

## 概要

`HmMicroHttpServer` は、.NET 9 を利用して作られた、単一の実行ファイルとして動作するシンプルな静的ファイルサーバーです。AOT (Ahead-Of-Time) コンパイルにより、.NETランタイムがインストールされていない環境でも軽快に動作します。

ローカルでのWeb開発や、ファイルの簡単な共有など、様々な用途に素早く利用できます。

## 主な特徴

- **クロスプラットフォーム**: Windows, macOS, Linux で動作します。
- **.NET 9 AOT対応**: .NETランタイム不要の単一実行ファイルとしてビルド可能です。
- **ディレクトリ指定**: 任意のディレクトリをWebルートとして公開できます。
- **自動ポート選択**: 空いているポートを自動で検出し、`localhost` でリクエストを待ち受けます。
- **SPAフォールバック**: リクエストされたファイルが見つからない場合、`index.html` を返すため、SPA（Single Page Application）のホスティングにも便利です。
- **MIMEタイプ自動判定**: ファイルの拡張子に基づいて適切なMIMEタイプを返します。
- **ウィンドウハンドル監視 (Windows限定)**: 指定したウィンドウが閉じると、サーバーも自動で終了する機能を持ちます。

## 使い方

### コマンドライン引数

```
HmMicroHttpServer [公開ディレクトリ] [ウィンドウハンドル(Windows限定, 省略可)]
```

- **第1引数: `公開ディレクトリ` (省略可)**
  - 公開したいディレクトリのパスを指定します。
  - 省略した場合、実行ファイルと同じ階層にある `www` ディレクトリが公開対象となります。

- **第2引数: `ウィンドウハンドル` (Windows限定, 省略可)**
  - 監視対象のウィンドウハンドルを数値で指定します。
  - この機能は **Windowsでのみ利用可能** です。指定されたウィンドウが閉じると、HTTPサーバーも自動的に終了します。
  - 他のOSでこの引数を指定しても無視されます。

### 実行例

#### Windowsの場合

```powershell
# www フォルダを公開
.\HmMicroHttpServer.exe www

# カレントディレクトリを公開
.\HmMicroHttpServer.exe .
```

#### macOS / Linuxの場合

```bash
# www フォルダを公開
./HmMicroHttpServer www

# カレントディレクトリを公開
./HmMicroHttpServer .
```

### サーバー起動後の表示

実行すると、使用ポートと公開ディレクトリがコンソールに表示されます。

```
[INFO] Using port: 12345
Serving C:\path\to\www
 → http://localhost:12345/
```

ブラウザで表示されたURLにアクセスしてください。

## ビルド方法

このプロジェクトを自身でビルドするには、[.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) が必要です。

1.  ソースコードをクローンまたはダウンロードします。
2.  コマンドラインでプロジェクトのルートディレクトリに移動します。
3.  以下のコマンドを実行します。

```bash
# 現在のOS向けの実行ファイルをビルド・発行
dotnet publish -c Release
```

ビルドが完了すると、`bin/Release/net9.0/publish/` ディレクトリ内に実行ファイルが生成されます。（Windowsの場合は `HmMicroHttpServer.exe`、Linux/macOSの場合は `HmMicroHttpServer`）

### クロスコンパイル（他のOS向けの実行ファイルをビルドする場合）

もし他のOS向けの実行ファイルをビルドしたい場合は、`-r <RID>`オプションで[ランタイム識別子](https://learn.microsoft.com/ja-jp/dotnet/core/rid-catalog)を指定します。

```bash
# 例: Windows x64向けにビルド
dotnet publish -c Release -r win-x64

# 例: Linux x64向けにビルド
dotnet publish -c Release -r linux-x64
```
この場合、実行ファイルは `bin/Release/net9.0/<RID>/publish/` ディレクトリに生成されます。

## ライセンス

本プロジェクトはCC0ライセンスです。詳細は`LICENSE`ファイルを参照してください。
