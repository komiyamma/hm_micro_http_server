using System.Net;
using System.Diagnostics;
using System.Web;
using System.Collections.Concurrent;
using System.Runtime.InteropServices; // Win32 API呼び出し用

/// <summary>
/// 簡易HTTPサーバのエントリポイント
/// </summary>
class Program
{
    /// <summary>
    /// 拡張子ごとのMIMEタイプ辞書
    /// </summary>
    static readonly ConcurrentDictionary<string, string> Mime = new(new[]
    {
        // テキスト系
        new KeyValuePair<string,string>(".html","text/html; charset=utf-8"),
        new KeyValuePair<string,string>(".htm","text/html; charset=utf-8"),
        new KeyValuePair<string,string>(".css","text/css"),
        new KeyValuePair<string,string>(".csv","text/csv"),
        new KeyValuePair<string,string>(".txt","text/plain"),
        new KeyValuePair<string,string>(".xml","application/xml"),
        new KeyValuePair<string,string>(".json","application/json"),
        new KeyValuePair<string,string>(".js","application/javascript"),
        new KeyValuePair<string,string>(".map","application/octet-stream"),

        // 画像系
        new KeyValuePair<string,string>(".png","image/png"),
        new KeyValuePair<string,string>(".jpg","image/jpeg"),
        new KeyValuePair<string,string>(".jpeg","image/jpeg"),
        new KeyValuePair<string,string>(".gif","image/gif"),
        new KeyValuePair<string,string>(".svg","image/svg+xml"),
        new KeyValuePair<string,string>(".ico","image/x-icon"),
        new KeyValuePair<string,string>(".bmp","image/bmp"),
        new KeyValuePair<string,string>(".webp","image/webp"),
        new KeyValuePair<string,string>(".tif","image/tiff"),
        new KeyValuePair<string,string>(".tiff","image/tiff"),
        new KeyValuePair<string,string>(".apng","image/apng"),
        new KeyValuePair<string,string>(".avif","image/avif"),

        // フォント
        new KeyValuePair<string,string>(".woff","font/woff"),
        new KeyValuePair<string,string>(".woff2","font/woff2"),
        new KeyValuePair<string,string>(".ttf","font/ttf"),
        new KeyValuePair<string,string>(".otf","font/otf"),
        new KeyValuePair<string,string>(".eot","application/vnd.ms-fontobject"),

        // アーカイブ
        new KeyValuePair<string,string>(".zip","application/zip"),
        new KeyValuePair<string,string>(".tar","application/x-tar"),
        new KeyValuePair<string,string>(".gz","application/gzip"),
        new KeyValuePair<string,string>(".rar","application/vnd.rar"),
        new KeyValuePair<string,string>(".7z","application/x-7z-compressed"),
        new KeyValuePair<string,string>(".bz2","application/x-bzip2"),
        new KeyValuePair<string,string>(".xz","application/x-xz"),

        // 音声・動画
        new KeyValuePair<string,string>(".mp3","audio/mpeg"),
        new KeyValuePair<string,string>(".wav","audio/wav"),
        new KeyValuePair<string,string>(".ogg","audio/ogg"),
        new KeyValuePair<string,string>(".m4a","audio/mp4"),
        new KeyValuePair<string,string>(".flac","audio/flac"),
        new KeyValuePair<string,string>(".aac","audio/aac"),
        new KeyValuePair<string,string>(".mp4","video/mp4"),
        new KeyValuePair<string,string>(".m4v","video/x-m4v"),
        new KeyValuePair<string,string>(".webm","video/webm"),
        new KeyValuePair<string,string>(".ogv","video/ogg"),
        new KeyValuePair<string,string>(".mov","video/quicktime"),
        new KeyValuePair<string,string>(".avi","video/x-msvideo"),
        new KeyValuePair<string,string>(".wmv","video/x-ms-wmv"),
        new KeyValuePair<string,string>(".mkv","video/x-matroska"),

        // アプリケーション
        new KeyValuePair<string,string>(".pdf","application/pdf"),
        new KeyValuePair<string,string>(".doc","application/msword"),
        new KeyValuePair<string,string>(".docx","application/vnd.openxmlformats-officedocument.wordprocessingml.document"),
        new KeyValuePair<string,string>(".xls","application/vnd.ms-excel"),
        new KeyValuePair<string,string>(".xlsx","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
        new KeyValuePair<string,string>(".ppt","application/vnd.ms-powerpoint"),
        new KeyValuePair<string,string>(".pptx","application/vnd.openxmlformats-officedocument.presentationml.presentation"),
        new KeyValuePair<string,string>(".rtf","application/rtf"),
        new KeyValuePair<string,string>(".swf","application/x-shockwave-flash"),
        new KeyValuePair<string,string>(".wasm","application/wasm"),
        new KeyValuePair<string,string>(".exe","application/octet-stream"),
        new KeyValuePair<string,string>(".bin","application/octet-stream"),
        new KeyValuePair<string,string>(".dll","application/octet-stream"),
        new KeyValuePair<string,string>(".deb","application/vnd.debian.binary-package"),
        new KeyValuePair<string,string>(".rpm","application/x-rpm"),
        new KeyValuePair<string,string>(".msi","application/x-msdownload"),

        // その他
        new KeyValuePair<string,string>(".md","text/markdown"),
        new KeyValuePair<string,string>(".bat","text/plain"),
        new KeyValuePair<string,string>(".sh","text/x-shellscript"),
        new KeyValuePair<string,string>(".ps1","text/plain"),
        new KeyValuePair<string,string>(".ini","text/plain"),
        new KeyValuePair<string,string>(".conf","text/plain"),
        new KeyValuePair<string,string>(".log","text/plain"),
        new KeyValuePair<string,string>(".yaml","text/yaml"),
        new KeyValuePair<string,string>(".yml","text/yaml"),
    });

    /// <summary>
    /// Win32 API: 指定したウィンドウハンドルが有効かどうかを判定
    /// </summary>
    [DllImport("user32.dll")]
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    static extern bool IsWindow(IntPtr hWnd);

    /// <summary>
    /// プログラムのエントリポイント
    /// </summary>
    static void Main(string[] args)
    {
        string root;
        IntPtr? windowHandle = null;

        // 第1引数: ルートディレクトリ
        if (args.Length >= 1 && Directory.Exists(args[0]))
        {
            root = Path.GetFullPath(args[0]);
        }
        else
        {
            // デフォルト: 実行ディレクトリ配下のwww
            root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "www"));
            if (!Directory.Exists(root))
            {
                Console.Error.WriteLine($"www not found: {root}");
                return;
            }
        }

        // 第2引数: ウィンドウハンドル（省略可）
        if (args.Length >= 2 && long.TryParse(args[1], out var handleValue))
        {
            windowHandle = new IntPtr(handleValue);
        }

        // 利用可能なポートを取得し、サーバ起動
        int port = GetFreePort();
        // Console.WriteLine($"[INFO] Using port: {port}"); // ← 実際に確保したポートはRunServerで出力
        RunServer(port, root, windowHandle);
    }

    /// <summary>
    /// HTTPサーバを起動し、リクエストを受け付ける
    /// </summary>
    /// <param name="port">バインドするポート番号</param>
    /// <param name="root">公開ディレクトリ</param>
    /// <param name="windowHandle">監視対象ウィンドウハンドル（省略可）</param>
    static void RunServer(int port, string root, IntPtr? windowHandle)
    {
        HttpListener listener = new HttpListener();
        string url;
        int retry = 0;
        // ポート競合時は最大10回までリトライ
        while (true)
        {
            url = $"http://localhost:{port}/";
            listener.Prefixes.Clear();
            listener.Prefixes.Add(url);
            try
            {
                listener.Start();
                break;
            }
            catch (HttpListenerException)
            {
                if (++retry > 10) throw;
                port = GetFreePort();
                continue;
            }
        }

        Console.WriteLine($"[INFO] Using port: {port}"); // 実際に確保したポート番号を出力
        Console.WriteLine("Serving " + root);
        Console.WriteLine(" → " + url);

        var cts = new CancellationTokenSource();

        // ウィンドウハンドル監視タスク（指定がある場合のみ）
        Task? monitorTask = null;
        if (windowHandle.HasValue)
        {
            if (OperatingSystem.IsWindows())
            {
                monitorTask = Task.Run(async () =>
                {
                    while (true)
                    {
                        // ウィンドウが閉じられたらサーバも停止
                        if (!IsWindow(windowHandle.Value))
                        {
                            Console.WriteLine("[INFO] ウィンドウが閉じられたためサーバーを終了します。");
                            cts.Cancel();
                            listener.Stop();
                            break;
                        }
                        await Task.Delay(1500);
                    }
                });
            }
            else
            {
                Console.WriteLine("[WARN] ウィンドウハンドル監視はWindowsでのみサポートされているため、この引数は無視されます。");
            }
        }

        try
        {
            // リクエスト受信ループ
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    // 非同期でリクエストを受信し、Handleで処理
                    var contextTask = listener.GetContextAsync();
                    contextTask.Wait(cts.Token);
                    _ = Handle(contextTask.Result, root);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Listener error: " + ex);
                    Thread.Sleep(1000);
                }
            }
        }
        finally
        {
            listener.Close();
        }
    }

    /// <summary>
    /// 利用可能なローカルポート番号をランダムに取得
    /// </summary>
    /// <returns>空いているポート番号</returns>
    static int GetFreePort()
    {
        var used = new HashSet<int>();
        var rand = new Random();
        for (int i = 0; i < 50; i++)
        {
            int port = rand.Next(1024, 65535);
            if (used.Contains(port)) continue;
            used.Add(port);
            try
            {
                // 一時的にリッスンして空きを確認
                var l = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, port);
                l.Start();
                int found = ((System.Net.IPEndPoint)l.LocalEndpoint).Port;
                l.Stop();
                return found;
            }
            catch
            {
                // ポートが使用中など
            }
        }
        throw new Exception("空いているポートが見つかりませんでした（50回試行）");
    }

    /// <summary>
    /// HTTPリクエストを処理し、ファイルを返却
    /// </summary>
    /// <param name="ctx">リクエストコンテキスト</param>
    /// <param name="root">公開ディレクトリ</param>
    static async Task Handle(HttpListenerContext ctx, string root)
    {
        try
        {
            // URLデコードし、ルートや空パスはindex.htmlにフォールバック
            string path = HttpUtility.UrlDecode(ctx.Request.Url!.AbsolutePath);
            if (string.IsNullOrEmpty(path) || path == "/") path = "/index.html";

            // パス正規化 & ディレクトリトラバーサル防止
            string full = Path.GetFullPath(Path.Combine(root, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                ctx.Response.StatusCode = 403; ctx.Response.Close(); return;
            }

            if (!File.Exists(full))
            {
                // SPA用途などでindex.htmlにフォールバック
                full = Path.Combine(root, "index.html");
                if (!File.Exists(full)) { ctx.Response.StatusCode = 404; ctx.Response.Close(); return; }
            }

            // MIMEタイプを拡張子から判定
            string ext = Path.GetExtension(full).ToLowerInvariant();
            ctx.Response.ContentType = Mime.TryGetValue(ext, out var m) ? m : "application/octet-stream";

            ctx.Response.AddHeader("Cache-Control", "no-cache");

            // ファイルをストリームで返却
            using var fs = File.OpenRead(full);
            await fs.CopyToAsync(ctx.Response.OutputStream);
            ctx.Response.StatusCode = 200;
            ctx.Response.Close();
        }
        catch
        {
            // 例外時は500エラー
            try { ctx.Response.StatusCode = 500; ctx.Response.Close(); } catch { }
        }
    }
}
