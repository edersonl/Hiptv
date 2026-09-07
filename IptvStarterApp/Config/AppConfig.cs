namespace IptvStarterApp.Config
{
    public static class AppConfig
    {
        public const string AppName = "Hiptv";

        public const string DefaultPlaylistUrl = "https://example.com/playlist.m3u";

        public const string DownloadUrl = "https://gofile.io/d/ul7CKIcf";
        public const string DownloadUrlAlt = "https://encurtador.com.br/pByg";
        public const string LiteApkUrl = "http://warwas.co/axc";
        public const string LiteApkUrlAlt = "http://pepinoplay.com/apk/lite";
        public const string ProtonVpnUrl = "https://play.google.com/store/apps/details?id=ch.protonvpn.android&hl=pt_BR";
        public const string CloudflareWarpUrl = "https://play.google.com/store/apps/details?id=com.cloudflare.onedotonedotonedotone&hl=pt_BR";

        public const string BrowserPlayerUrl = "http://www.assistir.club/";
        public const string BrowserPlayerUrlAlt = "http://8yca8t.live/web";
        public const string EpgUrl = "http://8yca8t.live/epg/212440/WuRvfk";

        public const string RegistrationDate = "12/09/2019";
        public const string ProductName = "Acesso - Pacote de 90 Dias";
        public const string UserCode = "212440";
        public const string UserPassword = "WuRvfk";

        public static readonly string[] DnsList =
        {
            "http://bunbus.org/",
            "http://koquwz.com/",
            "http://sheshy.ws/",
            "http://hah9xx.io/",
            "http://t46udq.vip/",
            "http://gemget.org/",
            "http://a1b2c3jk.info/",
            "http://k2m3aeiou.cloud/",
            "http://ubkeqzg.biz/"
        };

        public static string WarningMessage =>
            "Antes de acessar o conteúdo, use VPN. O Android pode bloquear o app externo e, em alguns casos, o CPlayer Lite é bloqueado. Conecte uma VPN antes do acesso e teste outros DNSs se necessário.";

        public static string AppInfoMessage =>
            $"Produto/Serviço: {ProductName} | Data: {RegistrationDate} | Usuário: {UserCode} | Senha: {UserPassword}";
    }
}
