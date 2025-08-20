using YamlDotNet.Serialization;

namespace PulsePanel.Blueprints.Models;

public class Blueprint
{
    public string Name { get; set; }
    public string Version { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public string License { get; set; }
    public Provenance Provenance { get; set; }
    public List<FileEntry> Files { get; set; }
    public Requirements Requirements { get; set; }
    public List<Token> Tokens { get; set; }
}

public class Provenance
{
    [YamlMember(Alias = "source_url")]
    public string SourceUrl { get; set; }

    [YamlMember(Alias = "source_version")]
    public string SourceVersion { get; set; }

    [YamlMember(Alias = "source_hash")]
    public string SourceHash { get; set; }

    [YamlMember(Alias = "retrieval_date")]
    public DateTime RetrievalDate { get; set; }

    [YamlMember(Alias = "retrieval_method")]
    public string RetrievalMethod { get; set; }
}

public class FileEntry
{
    public string Path { get; set; }
    public string Hash { get; set; }
}

public class Requirements
{
    public List<string> Os { get; set; }
    public List<Dependency> Dependencies { get; set; }
}

public class Dependency
{
    public string Name { get; set; }
    public string Version { get; set; }

    [YamlMember(Alias = "install_instructions")]
    public string InstallInstructions { get; set; }
}

public class Token
{
    public string Key { get; set; }
    public string Description { get; set; }
}
