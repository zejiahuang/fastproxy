using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Widget;
using Android.OS;
using Square.OkHttp3;
using Square.Picasso;
using System.Application.Services;
using System.IO;
using JFile = Java.IO.File;
using JObject = Java.Lang.Object;
using JException = Java.Lang.Exception;

using AndroidApplication = Android.App.Application;
using Size = System.Drawing.Size;
using _ThisAssembly = System.Properties.ThisAssembly;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
using AndroidMatrix = Android.Graphics.Matrix;
using JPath = Android.Graphics.Path;

namespace System.Application.UI
{
    public static class ImageLoader
    {
        const string TAG = nameof(ImageLoader);

        static readonly Lazy<Picasso> _Picasso = new(GetPicasso);

        static Picasso GetPicasso()
        {
            Picasso.Builder picassoBuilder = new(AndroidApplication.Context);
            picassoBuilder.IndicatorsEnabled(_ThisAssembly.Debuggable);
            var cacheDir = CreateDefaultCacheDir();
            var maxSize = CalculateDiskCacheSize(cacheDir);
            var client = CreateOkHttpClient(cacheDir, maxSize);
            OkHttp3Downloader downloader = new(client);
            picassoBuilder.Downloader(downloader);
            return picassoBuilder.Build();
        }

        public static Picasso Picasso => _Picasso.Value;

        #region 高效加载大型位图 https://developer.android.google.cn/topic/performance/graphics/load-bitmap?hl=zh-cn#java

        public static void SetImageSource(this ImageView imageView,
            Stream? stream,
            int targetResIdW = 0,
            int targetResIdH = 0,
            int targetW = 0,
            int targetH = 0,
            Bitmap.Config? inPreferredConfig = null)
        {
            if (stream == null || !stream.CanRead)
            {
                imageView.SetImageDrawable(null);
                return;
            }
            try
            {
                Bitmap? bitmap = null;
                if (stream.CanSeek)
                {
                    if (targetResIdW > 0)
                    {
                        if (targetResIdH <= 0) targetResIdH = targetResIdW;
                        var resources = imageView.Resources!;
                        var reqWidth = resources.GetDimensionPixelSize(targetResIdW);
                        var reqHeight = resources.GetDimensionPixelSize(targetResIdH);
                        bitmap = DecodeSampledBitmapFromStream(stream, reqWidth, reqHeight, inPreferredConfig);
                    }
                    else if (targetW > 0)
                    {
                        if (targetH <= 0) targetH = targetW;
                        bitmap = DecodeSampledBitmapFromStream(stream, targetW, targetH, inPreferredConfig);
                    }
                }
                bitmap ??= BitmapFactory.DecodeStream(stream)!;
#if DEBUG
                Log.Info(TAG,
                    $"Context: {imageView.Context!.GetType().Name}, " +
                    $"Bitmap.Width: {bitmap.Width}, " +
                    $"Bitmap.Height: {bitmap.Height}, " +
                    $"Bitmap.Config: {bitmap.GetConfig()}, " +
                    $"Bitmap.Size1: {IOPath.GetDisplayFileSizeString(bitmap.ByteCount)}, " +
                    $"Bitmap.Size2: {IOPath.GetDisplayFileSizeString(bitmap.AllocationByteCount)}.");
#endif
                imageView.SetImageBitmap(bitmap);
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "SetImageSource(Stream) catch.");
            }
        }

        static int CalculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
        {
            // Raw height and width of image
            int height = options.OutHeight;
            int width = options.OutWidth;
            int inSampleSize = 1;

            if (height > reqHeight || width > reqWidth)
            {
                int halfHeight = height / 2;
                int halfWidth = width / 2;

                // Calculate the largest inSampleSize value that is a power of 2 and keeps both
                // height and width larger than the requested height and width.
                while ((halfHeight / inSampleSize) >= reqHeight
                        && (halfWidth / inSampleSize) >= reqWidth)
                {
                    inSampleSize *= 2;
                }
            }

            return inSampleSize;
        }

        static Bitmap DecodeSampledBitmapFromStream(Stream stream, int reqWidth, int reqHeight, Bitmap.Config? inPreferredConfig = null)
        {
            // First decode with inJustDecodeBounds=true to check dimensions
            BitmapFactory.Options options = new();
            if (inPreferredConfig != null)
            {
                options.InPreferredConfig = inPreferredConfig;
            }
            options.InJustDecodeBounds = true;
            BitmapFactory.DecodeStream(stream, null, options);

            // Calculate inSampleSize
            options.InSampleSize = CalculateInSampleSize(options, reqWidth, reqHeight);

            // Decode bitmap with inSampleSize set
            options.InJustDecodeBounds = false;
            stream.Position = 0;
            return BitmapFactory.DecodeStream(stream, null, options)!;
        }

        #endregion

        static Drawable ErrorDrawable => new ColorDrawable(Color.DarkRed);

        static Drawable Placeholder => new ColorDrawable(new(AndroidApplication.Context.GetColorCompat(Resource.Color.md3_surface_variant)));

        static RequestCreator? GetRequestCreator(string? requestUri,
            Size targetSize = default,
            Size targetSizeResId = default,
            ScaleType scaleType = default,
            bool useErrorDrawable = true,
            bool usePlaceholder = true)
        {
            try
            {
                if (Browser2.IsHttpUrl(requestUri))
                {
                    var requestCreator = Picasso.Load(requestUri);
                    if (usePlaceholder) requestCreator = requestCreator.Placeholder(Placeholder);
                    if (useErrorDrawable) requestCreator = requestCreator.Error(ErrorDrawable);
                    var useCenterCropDefault = false;
                    if (targetSize != default)
                    {
                        if (targetSize.Width > 0)
                        {
                            if (targetSize.Height <= 0) targetSize.Height = targetSize.Width;
                            requestCreator = requestCreator.Resize(targetSize.Width, targetSize.Height);
                            useCenterCropDefault = true;
                        }
                    }
                    else if (targetSizeResId != default)
                    {
                        if (targetSizeResId.Width > 0)
                        {
                            if (targetSizeResId.Height <= 0) targetSizeResId.Height = targetSizeResId.Width;
                            requestCreator = requestCreator.ResizeDimen(targetSizeResId.Width, targetSizeResId.Height);
                            useCenterCropDefault = true;
                        }
                    }
                    if (scaleType == ScaleType.CenterCrop || (useCenterCropDefault && scaleType == default))
                    {
                        requestCreator = requestCreator.CenterCrop();
                    }
                    else if (scaleType == ScaleType.CenterInside)
                    {
                        requestCreator = requestCreator.CenterInside();
                    }
                    return requestCreator;
                }
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "GetRequestCreator catch, requestUri: {0}", requestUri);
            }

            return null;
        }

        public static void SetImageSource(this ImageView imageView,
            string? requestUri,
            int targetResIdW,
            int targetResIdH = 0,
            ScaleType scaleType = default)
        {
            try
            {
                var requestCreator = GetRequestCreator(requestUri, default, new(targetResIdW, targetResIdH), scaleType);

                if (requestCreator == null)
                {
                    imageView.SetImageDrawable(null);
                }
                else
                {
                    requestCreator.Into(imageView, null, e =>
                    {
                        Log.Error(TAG, e, "SetImageSource.Callback catch, requestUri: {0}", requestUri);
                    });
                }
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "SetImageSource catch, requestUri: {0}", requestUri);
            }
        }

        public enum ScaleType
        {
            Default,
            CenterCrop,
            CenterInside,
        }

        /// <summary>
        /// 以 SVG 方式加载图片（第三方图标接口返回 image/svg+xml，Picasso 无法解码）。
        /// 异步抓取 SVG 文本并光栅化为 Bitmap 后设置到 ImageView。
        /// </summary>
        public static void SetImageSourceSvg(this ImageView imageView, string? requestUri, int targetResIdW)
        {
            if (string.IsNullOrWhiteSpace(requestUri))
            {
                imageView.SetImageDrawable(null);
                return;
            }
            _ = LoadSvgAsync(imageView, requestUri!, targetResIdW);
        }

        static async Task LoadSvgAsync(ImageView imageView, string requestUri, int targetResIdW)
        {
            imageView.Tag = requestUri;
            try
            {
                var resources = imageView.Resources!;
                var px = targetResIdW > 0 ? resources.GetDimensionPixelSize(targetResIdW) : 48;
                var svg = await IHttpService.Instance.GetAsync<string>(requestUri, accept: "image/svg+xml, text/xml, text/plain;q=0.9, */*;q=0.1", cancellationToken: CancellationToken.None);
                if (imageView.Tag?.ToString() != requestUri) return; // RecyclerView 复用了该 View
                if (string.IsNullOrWhiteSpace(svg))
                {
                    imageView.SetImageDrawable(null);
                    return;
                }
                if (TryRenderSvg(svg, px, out var bitmap) && bitmap != null)
                    imageView.SetImageBitmap(bitmap);
                else
                    imageView.SetImageDrawable(null);
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "LoadSvgAsync catch, requestUri: {0}", requestUri);
                if (imageView.Tag?.ToString() == requestUri)
                    imageView.SetImageDrawable(null);
            }
        }

        /// <summary>
        /// 将 Simple Icons 风格的 SVG 字符串光栅化为指定边长的 Bitmap。
        /// 支持 viewBox + 单个 path + fill 颜色。
        /// </summary>
        static bool TryRenderSvg(string svg, int targetSizePx, out Bitmap? bitmap)
        {
            bitmap = null;
            try
            {
                var pathData = SvgPathUtil.TryGetPathData(svg);
                if (pathData == null) return false;

                SvgPathUtil.TryGetViewBox(svg, out var vbX, out var vbY, out var vbW, out var vbH);
                if (vbW <= 0 || vbH <= 0) { vbW = 24; vbH = 24; }

                var path = SvgPathUtil.ParsePath(pathData);
                if (path.IsEmpty) return false;

                var fill = SvgPathUtil.TryGetFillColor(svg);

                // 等比缩放并居中
                float scale = Math.Min((float)targetSizePx / vbW, (float)targetSizePx / vbH);
                var matrix = new AndroidMatrix();
                matrix.PostScale(scale, scale);
                matrix.PostTranslate(-vbX * scale + (targetSizePx - vbW * scale) / 2f, -vbY * scale + (targetSizePx - vbH * scale) / 2f);
                path.Transform(matrix);

                var bmp = Bitmap.CreateBitmap(targetSizePx, targetSizePx, Bitmap.Config.Argb8888);
                var canvas = new Canvas(bmp);
                canvas.DrawColor(Color.Transparent);
                var paint = new Paint(PaintFlags.AntiAlias)
                {
                    Color = fill,
                };
                paint.SetStyle(Android.Graphics.Paint.Style.Fill);
                canvas.DrawPath(path, paint);
                canvas.Dispose();
                paint.Dispose();
                bitmap = bmp;
                return true;
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "TryRenderSvg catch.");
                return false;
            }
        }

        /// <summary>
        /// Simple Icons 风格 SVG 的轻量解析器（viewBox / fill / path d）。
        /// </summary>
        static class SvgPathUtil
        {
            static readonly Regex PathDataRx = new(@"<path[^>]*\bd=""([^""]*)""", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            static readonly Regex ViewBoxRx = new(@"viewBox=""\s*([-\d.]+)\s+([-\d.]+)\s+([-\d.]+)\s+([-\d.]+)\s*""", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            static readonly Regex FillRx = new(@"<svg[^>]*\bfill=""([^""]*)""", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            static readonly Regex TokenRx = new(@"([MmLlHhVvCcSsQqTtAaZz])|(-?\d*\.?\d+(?:[eE][-+]?\d+)?)", RegexOptions.Compiled);

            public static string? TryGetPathData(string svg)
            {
                var m = PathDataRx.Match(svg);
                return m.Success ? m.Groups[1].Value : null;
            }

            public static void TryGetViewBox(string svg, out float x, out float y, out float w, out float h)
            {
                x = 0; y = 0; w = 24; h = 24;
                var m = ViewBoxRx.Match(svg);
                if (!m.Success) return;
                float.TryParse(m.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out x);
                float.TryParse(m.Groups[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out y);
                float.TryParse(m.Groups[3].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out w);
                float.TryParse(m.Groups[4].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out h);
            }

            public static Color TryGetFillColor(string svg)
            {
                var m = FillRx.Match(svg);
                if (m.Success && TryParseColor(m.Groups[1].Value, out var c)) return c;
                return Color.ParseColor("#757575");
            }

            static bool TryParseColor(string value, out Color color)
            {
                color = default;
                var v = value.Trim();
                if (v.StartsWith("#"))
                {
                    try { color = Color.ParseColor(v); return true; } catch { return false; }
                }
                switch (v.ToLowerInvariant())
                {
                    case "white": color = Color.White; return true;
                    case "black": color = Color.Black; return true;
                    case "red": color = Color.Red; return true;
                    case "green": color = Color.Green; return true;
                    case "blue": color = Color.Blue; return true;
                    case "gray":
                    case "grey": color = Color.Gray; return true;
                    case "none": return false;
                }
                return false;
            }

            public static JPath ParsePath(string d)
            {
                var path = new JPath();
                var tokens = new List<object>();
                foreach (Match m in TokenRx.Matches(d))
                {
                    if (m.Groups[1].Success) tokens.Add(m.Groups[1].Value[0]);
                    else tokens.Add(double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture));
                }

                int idx = 0, n = tokens.Count;
                char cmd = ' ';
                float cx = 0, cy = 0, startX = 0, startY = 0;
                float lastCubicCtrlX = 0, lastCubicCtrlY = 0, lastQuadCtrlX = 0, lastQuadCtrlY = 0;
                bool hasCubicCtrl = false, hasQuadCtrl = false;

                double NextNum()
                {
                    if (idx >= n) return 0;
                    var t = tokens[idx++];
                    return t is double dbl ? dbl : 0;
                }

                while (idx < n)
                {
                    var tok = tokens[idx];
                    if (tok is char c)
                    {
                        cmd = c;
                        idx++;
                        if (c == 'Z' || c == 'z')
                        {
                            path.Close();
                            cx = startX; cy = startY;
                            hasCubicCtrl = hasQuadCtrl = false;
                            continue;
                        }
                    }

                    switch (char.ToUpperInvariant(cmd))
                    {
                        case 'M':
                            {
                                var x = (float)NextNum(); var y = (float)NextNum();
                                if (cmd == 'm') { x += cx; y += cy; }
                                path.MoveTo(x, y);
                                cx = x; cy = y; startX = x; startY = y;
                                hasCubicCtrl = hasQuadCtrl = false;
                                while (idx < n && tokens[idx] is double)
                                {
                                    var x2 = (float)NextNum(); var y2 = (float)NextNum();
                                    if (cmd == 'm') { x2 += cx; y2 += cy; }
                                    path.LineTo(x2, y2);
                                    cx = x2; cy = y2;
                                }
                            }
                            break;
                        case 'L':
                            {
                                var x = (float)NextNum(); var y = (float)NextNum();
                                if (cmd == 'l') { x += cx; y += cy; }
                                path.LineTo(x, y);
                                cx = x; cy = y;
                                hasCubicCtrl = hasQuadCtrl = false;
                            }
                            break;
                        case 'H':
                            {
                                var x = (float)NextNum();
                                if (cmd == 'h') x += cx;
                                path.LineTo(x, cy);
                                cx = x;
                                hasCubicCtrl = hasQuadCtrl = false;
                            }
                            break;
                        case 'V':
                            {
                                var y = (float)NextNum();
                                if (cmd == 'v') y += cy;
                                path.LineTo(cx, y);
                                cy = y;
                                hasCubicCtrl = hasQuadCtrl = false;
                            }
                            break;
                        case 'C':
                            {
                                var x1 = (float)NextNum(); var y1 = (float)NextNum();
                                var x2 = (float)NextNum(); var y2 = (float)NextNum();
                                var x = (float)NextNum(); var y = (float)NextNum();
                                if (cmd == 'c') { x1 += cx; y1 += cy; x2 += cx; y2 += cy; x += cx; y += cy; }
                                path.CubicTo(x1, y1, x2, y2, x, y);
                                lastCubicCtrlX = x2; lastCubicCtrlY = y2;
                                hasCubicCtrl = true; hasQuadCtrl = false;
                                cx = x; cy = y;
                            }
                            break;
                        case 'S':
                            {
                                var x2 = (float)NextNum(); var y2 = (float)NextNum();
                                var x = (float)NextNum(); var y = (float)NextNum();
                                float x1, y1;
                                if (hasCubicCtrl) { x1 = 2 * cx - lastCubicCtrlX; y1 = 2 * cy - lastCubicCtrlY; }
                                else { x1 = cx; y1 = cy; }
                                if (cmd == 's') { x2 += cx; y2 += cy; x += cx; y += cy; }
                                path.CubicTo(x1, y1, x2, y2, x, y);
                                lastCubicCtrlX = x2; lastCubicCtrlY = y2;
                                hasCubicCtrl = true; hasQuadCtrl = false;
                                cx = x; cy = y;
                            }
                            break;
                        case 'Q':
                            {
                                var x1 = (float)NextNum(); var y1 = (float)NextNum();
                                var x = (float)NextNum(); var y = (float)NextNum();
                                if (cmd == 'q') { x1 += cx; y1 += cy; x += cx; y += cy; }
                                path.QuadTo(x1, y1, x, y);
                                lastQuadCtrlX = x1; lastQuadCtrlY = y1;
                                hasQuadCtrl = true; hasCubicCtrl = false;
                                cx = x; cy = y;
                            }
                            break;
                        case 'T':
                            {
                                var x = (float)NextNum(); var y = (float)NextNum();
                                float x1, y1;
                                if (hasQuadCtrl) { x1 = 2 * cx - lastQuadCtrlX; y1 = 2 * cy - lastQuadCtrlY; }
                                else { x1 = cx; y1 = cy; }
                                if (cmd == 't') { x += cx; y += cy; }
                                path.QuadTo(x1, y1, x, y);
                                lastQuadCtrlX = x1; lastQuadCtrlY = y1;
                                hasQuadCtrl = true; hasCubicCtrl = false;
                                cx = x; cy = y;
                            }
                            break;
                        case 'A':
                            {
                                var rx = (float)NextNum(); var ry = (float)NextNum();
                                var rot = (float)NextNum(); var laf = (float)NextNum(); var sf = (float)NextNum();
                                var x = (float)NextNum(); var y = (float)NextNum();
                                if (cmd == 'a') { x += cx; y += cy; }
                                AppendArc(path, cx, cy, rx, ry, rot, laf >= 0.5, sf >= 0.5, x, y);
                                cx = x; cy = y;
                                hasCubicCtrl = hasQuadCtrl = false;
                            }
                            break;
                    }
                }
                return path;
            }

            /// <summary>
            /// 将 SVG 弧线（端点参数化）转换为 Android ArcTo（中心参数化），φ=0 时精确，φ≠0 时近似。
            /// </summary>
            static void AppendArc(JPath path, float x1, float y1, float rx, float ry, float xAxisRotationDeg, bool largeArc, bool sweep, float x2, float y2)
            {
                if (rx == 0 || ry == 0)
                {
                    path.LineTo(x2, y2);
                    return;
                }
                var phi = xAxisRotationDeg * Math.PI / 180.0;
                var cosPhi = Math.Cos(phi); var sinPhi = Math.Sin(phi);

                double dx = (x1 - x2) / 2.0, dy = (y1 - y2) / 2.0;
                double x1p = cosPhi * dx + sinPhi * dy;
                double y1p = -sinPhi * dx + cosPhi * dy;

                double rxs = Math.Abs(rx), rys = Math.Abs(ry);
                double lambda = (x1p * x1p) / (rxs * rxs) + (y1p * y1p) / (rys * rys);
                if (lambda > 1)
                {
                    var s = Math.Sqrt(lambda);
                    rxs *= s; rys *= s;
                }

                double num = rxs * rxs * rys * rys - rxs * rxs * y1p * y1p - rys * rys * x1p * x1p;
                double den = rxs * rxs * y1p * y1p + rys * rys * x1p * x1p;
                double coef = (largeArc == sweep ? -1 : 1) * Math.Sqrt(Math.Max(0, num / den));
                double cxp = coef * (rxs * y1p / rys);
                double cyp = coef * (-rys * x1p / rxs);

                double cx = cosPhi * cxp - sinPhi * cyp + (x1 + x2) / 2.0;
                double cy = sinPhi * cxp + cosPhi * cyp + (y1 + y2) / 2.0;

                double ux = (x1p - cxp) / rxs, uy = (y1p - cyp) / rys;
                double vx = (-x1p - cxp) / rxs, vy = (-y1p - cyp) / rys;

                double startAngle = Math.Atan2(uy, ux);
                double delta = Angle(ux, uy, vx, vy);
                if (sweep && delta < 0) delta += 2 * Math.PI;
                if (!sweep && delta > 0) delta -= 2 * Math.PI;

                var oval = new RectF((float)(cx - rxs), (float)(cy - rys), (float)(cx + rxs), (float)(cy + rys));
                path.ArcTo(oval, (float)(startAngle * 180.0 / Math.PI), (float)(delta * 180.0 / Math.PI), false);
            }

            static double Angle(double ux, double uy, double vx, double vy)
            {
                var dot = ux * vx + uy * vy;
                var len = Math.Sqrt((ux * ux + uy * uy) * (vx * vx + vy * vy));
                if (len == 0) return 0;
                var ang = Math.Acos(Math.Max(-1, Math.Min(1, dot / len)));
                return (ux * vy - uy * vx < 0) ? -ang : ang;
            }
        }

        // https://github.com/JakeWharton/picasso2-okhttp3-downloader/blob/master/src/main/java/com/jakewharton/picasso/OkHttp3Downloader.java

        static JFile CreateDefaultCacheDir()
        {
            var cachePath = IHttpService.GetImagesCacheDirectory(null);
            JFile cache = new(cachePath);
            if (!cache.Exists())
            {
                //noinspection ResultOfMethodCallIgnored
                cache.Mkdirs();
            }
            return cache;
        }

        const int MIN_DISK_CACHE_SIZE = 5 * 1024 * 1024; // 5MB
        const int MAX_DISK_CACHE_SIZE = 50 * 1024 * 1024; // 50MB

        static long CalculateDiskCacheSize(JFile dir)
        {
            long size = MIN_DISK_CACHE_SIZE;

            try
            {
                var statFs = new StatFs(dir.AbsolutePath);
                long available = statFs.BlockCountLong * statFs.BlockSizeLong;
                // Target 2% of the total space.
                size = available / 50;
            }
            catch (Java.Lang.IllegalArgumentException)
            {
            }

            // Bound inside min/max size for disk cache.
            return Math.Max(Math.Min(size, MAX_DISK_CACHE_SIZE), MIN_DISK_CACHE_SIZE);
        }

        static OkHttpClient CreateOkHttpClient(JFile cacheDir, long maxSize)
        {
            var s = IHttpPlatformHelperService.Instance;
            var client = new OkHttpClient.Builder()
                .Cache(new(cacheDir, maxSize))
                .FollowRedirects(true)
                .FollowSslRedirects(true)
                .CallTimeout(GeneralHttpClientFactory.DefaultTimeoutMilliseconds, Java.Util.Concurrent.TimeUnit.Milliseconds)
                .AddInterceptor(chain =>
                {
                    var newRequest = chain.Request().NewBuilder()
                        .AddHeader("User-Agent", s.UserAgent)
                        .Build();
                    return chain.Proceed(newRequest);
                })
                .Build();
            return client;
        }

        static Task<Bitmap?> GetBitmapCoreAsync(string? requestUri,
            Size targetSize = default,
            Size targetSizeResId = default,
            ScaleType scaleType = default)
        {
            try
            {
                var requestCreator = GetRequestCreator(requestUri, targetSize, targetSizeResId, scaleType, useErrorDrawable: false, usePlaceholder: false);

                if (requestCreator != null)
                {
                    var tcs = new TaskCompletionSource<Bitmap?>();

                    requestCreator.Into(new TaskCompletionSourceTarget(tcs));

                    return tcs.Task;
                }
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "GetBitmapCoreAsync catch, requestUri: {0}", requestUri);
            }

            return Task.FromResult<Bitmap?>(null);
        }

        /// <summary>
        /// 从 HttpUrl 中加载图片并返回 <see cref="Bitmap"/> 实例，如果 Url 不合法或出现 <see cref="Exception"/> 将返回 <see langword="null"/>
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="targetSize">目标图片大小宽高</param>
        /// <param name="targetSizeResId">目标图片大小宽高(R.dimen)</param>
        /// <param name="scaleType">图片缩放类型</param>
        /// <returns></returns>
        public static async Task<Bitmap?> GetBitmapAsync(string? requestUri,
            Size targetSize = default,
            Size targetSizeResId = default,
            ScaleType scaleType = default)
        {
            try
            {
                var bitmap = await GetBitmapCoreAsync(requestUri, targetSize, targetSizeResId, scaleType);
                return bitmap;
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "GetBitmapAsync catch, requestUri: {0}", requestUri);
            }

            return null;
        }

        sealed class TaskCompletionSourceTarget : JObject, ITarget
        {
            readonly TaskCompletionSource<Bitmap?> tcs;

            public TaskCompletionSourceTarget(TaskCompletionSource<Bitmap?> tcs) => this.tcs = tcs;

            void ITarget.OnBitmapFailed(JException exception, Drawable _)
            {
                tcs.TrySetException(exception);
            }

            void ITarget.OnBitmapLoaded(Bitmap bitmap, Picasso.LoadedFrom _)
            {
                tcs.TrySetResult(bitmap);
            }

            void ITarget.OnPrepareLoad(Drawable _)
            {
            }
        }
    }
}