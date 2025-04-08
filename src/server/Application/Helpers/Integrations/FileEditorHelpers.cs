using Domain.Enums.Integrations;

namespace Application.Helpers.Integrations;

public static class FileEditorHelpers
{
    public static string ToEditorValue(this FileEditorLanguage language)
    {
        return language switch
        {
            FileEditorLanguage.Batch => "bat",
            FileEditorLanguage.Plaintext => "plaintext",
            FileEditorLanguage.CoffeeScript => "coffescript",
            FileEditorLanguage.C => "c",
            FileEditorLanguage.CPlusPlus => "cpp",
            FileEditorLanguage.CSharp => "csharp",
            FileEditorLanguage.Css => "css",
            FileEditorLanguage.Dockerfile => "dockerfile",
            FileEditorLanguage.Go => "go",
            FileEditorLanguage.Html => "html",
            FileEditorLanguage.Ini => "ini",
            FileEditorLanguage.Java => "java",
            FileEditorLanguage.JavaScript => "javascript",
            FileEditorLanguage.Lua => "lua",
            FileEditorLanguage.Markdown => "markdown",
            FileEditorLanguage.MySql => "mysql",
            FileEditorLanguage.Perl => "perl",
            FileEditorLanguage.PgSql => "pgsql",
            FileEditorLanguage.Php => "php",
            FileEditorLanguage.Powershell => "powershell",
            FileEditorLanguage.Python => "python",
            FileEditorLanguage.Rust => "rust",
            FileEditorLanguage.TypeScript => "typescript",
            FileEditorLanguage.VisualBasic => "vb",
            FileEditorLanguage.Xml => "xml",
            FileEditorLanguage.Yaml => "yaml",
            FileEditorLanguage.Json => "json",
            _ => "plaintext"
        };
    }

    public static string ToEditorValue(this FileEditorTheme theme)
    {
        return theme switch
        {
            FileEditorTheme.Default => "vs",
            FileEditorTheme.Dark => "vs-dark",
            FileEditorTheme.HighContrast => "hc-black",
            _ => "vs-dark"
        };
    }
}