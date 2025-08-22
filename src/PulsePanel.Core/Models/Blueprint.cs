using System.Collections.Generic;

namespace PulsePanel.Core.Models;

public record Blueprint
(
    string Id,
    string Name,
    string Version,
    GameDefinition GameDefinition,
    List<TemplateSpec> Templates
);
