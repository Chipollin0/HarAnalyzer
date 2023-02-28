using System.Collections.Generic;
using System.ComponentModel;

namespace HarAnalyzer.Model
{
    public class HarLog
    {
        public string? Version { get; set; }
        public HarCreator? Creator { get; set; }
        public List<HarEntry>? Entries { get; set; }
    }

    public class HarCreator
    {
        public string? Name { get; set; }
        public string? Version { get; set; }
    }

    public class HarEntry
    {
        public string? Priority { get; set; }
        public string? ResourceType { get; set; }
        public string? Connection { get; set; }
        public string? PageRef { get; set; }
        public string? ServerIpAddress { get; set; }
        public string? StartedDateTime { get; set; }
        public double? Time { get; set; }
        public string? Timings { get; set; } // todo
        public HarCache? Cache { get; set; }
        public HarInitiator? Initiator { get; set; } // 
        public HarRequest? Request { get; set; }
        public HarResponse? Response { get; set; }
    }

    public class HarRequest
    {
        public string? Method { get; set; }
        public string? Url { get; set; }
        public string? HttpVersion { get; set; }
        public List<HarHeader>? Headers { get; set; }
        public List<HarQueryPair>? Query { get; set; }
        public List<HarCookies>? Cookies { get; set; }
        public string? RedirectUrl { get; set; }
        public int? HeadersSize { get; set; }
        public int? BodySize { get; set; }
    }

    public class HarCache
    {

    }

    public class HarHeader
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
    }

    public class HarCookies
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
        public string? Path { get; set; }
        public string? Domain { get; set; }
        public string? Expires { get; set; }
        public bool? HttpOnly { get; set; }
        public bool? Secure { get; set; }
    }

    public class HarQueryPair
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
    }

    public class HarResponse
    {
        public int? Status { get; set; }
        public string? StatusText { get; set; }
        public string? HttpVersion { get; set; }
        public List<HarHeader>? Headers { get; set; }
        public List<HarCookies>? Cookies { get; set; }
        public HarText? Content { get; set; }
        public int? HeadersSize { get; set; }
        public int? BodySize { get; set; }
        public int? TransferSize { get; set; }
        public string? Error { get; set; }
    }

    public class HarText
    {
        public int? Size { get; set; }
        public string? MimeType { get; set; }
        public string? Text { get; set; }
        public string? Encoding { get; set; }
    }

    public class HarInitiator
    {

    }
}
