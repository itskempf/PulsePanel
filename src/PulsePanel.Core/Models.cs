using YamlDotNet.Serialization;
using System;
using System.Collections.Generic;

namespace PulsePanel.Blueprints.Models
{
    public class Blueprint
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string License { get; set; } = string.Empty;
        public Provenance Provenance { get; set; } = new();
        public List<FileEntry> Files { get; set; } = new();
        public Requirements Requirements { get; set; } = new();
        public List<Token> Tokens { get; set; } = new();
    }

    public class Provenance
    {
        [YamlMember(Alias = "source_url")]
        public string SourceUrl { get; set; } = string.Empty;

        [YamlMember(Alias = "source_version")]
        public string SourceVersion { get; set; } = string.Empty;

        [YamlMember(Alias = "source_hash")]
        public string SourceHash { get; set; } = string.Empty;

        [YamlMember(Alias = "retrieval_date")]
        public DateTime RetrievalDate { get; set; }

        [YamlMember(Alias = "retrieval_method")]
        public string RetrievalMethod { get; set; } = string.Empty;
    }

    public class FileEntry
    {
        public string Path { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
    }

    public class Requirements
    {
        public List<string> Os { get; set; } = new();
        public List<Dependency> Dependencies { get; set; } = new();
    }

    public class Dependency
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;

        [YamlMember(Alias = "install_instructions")]
        public string InstallInstructions { get; set; } = string.Empty;
    }

    public class Token
    {
        public string Key { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool Required { get; set; }
        public string Default { get; set; } = string.Empty;
    }
}