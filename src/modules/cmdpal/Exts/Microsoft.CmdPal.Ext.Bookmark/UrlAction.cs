﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.CmdPal.Extensions.Helpers;
using Windows.System;

namespace Microsoft.CmdPal.Ext.Bookmarks;

public partial class UrlAction : InvokableCommand
{
    private bool IsContainsPlaceholder => _url.Contains('{') && _url.Contains('}');

    public string Type { get; }

    public string Url { get; }

    private readonly string _url;

    public UrlAction(string name, string url, string type)
    {
        _url = url;
        Icon = new(IconFromUrl(_url, type));
        Name = name;
        Type = type;
        Url = url;
    }

    public override CommandResult Invoke()
    {
        var target = _url;
        try
        {
            Uri? uri = GetUri(target);
            if (uri != null)
            {
                _ = Launcher.LaunchUriAsync(uri);
            }
            else
            {
                // throw new UriFormatException("The provided URL is not valid.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error launching URL: {ex.Message}");
        }

        return CommandResult.Dismiss();
    }

    internal static Uri? GetUri(string url)
    {
        Uri? uri;
        if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
        {
            if (!Uri.TryCreate("https://" + url, UriKind.Absolute, out uri))
            {
                return null;
            }
        }

        return uri;
    }

    internal static string IconFromUrl(string url, string type)
    {
        switch (type)
        {
            case "file":
                return "📄";
            case "folder":
                return "📁";
            case "web":
            default:
                // Get the base url up to the first placeholder
                var placeholderIndex = url.IndexOf('{');
                var baseString = placeholderIndex > 0 ? url.Substring(0, placeholderIndex) : url;
                try
                {
                    Uri? uri = GetUri(baseString);
                    if (uri != null)
                    {
                        var hostname = uri.Host;
                        var faviconUrl = $"{uri.Scheme}://{hostname}/favicon.ico";
                        return faviconUrl;
                    }
                }
                catch (UriFormatException)
                {
                    // return "🔗";
                }

                return "🔗";
        }
    }
}