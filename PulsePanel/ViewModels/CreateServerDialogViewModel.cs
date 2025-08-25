
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using PulsePanel.Core.Models;
using PulsePanel.Core.Services;

namespace PulsePanel.ViewModels
{
    public sealed class TemplateVariableVM : ObservableObject
    {
        private string _name = string.Empty; public string Name { get => _name; set => SetProperty(ref _name, value); }
        private string _value = string.Empty; public string Value { get => _value; set => SetProperty(ref _value, value); }
    }

    public sealed class CreateServerDialogViewModel : ObservableObject
    {
        private readonly IProvisioningService _provision;
        private readonly IServerService _servers;
        private readonly IBlueprintService _blueprints;
        public Blueprint Blueprint { get; }
        public ObservableCollection<TemplateVariableVM> Variables { get; } = new();
        private string _instanceName = string.Empty; public string InstanceName { get => _instanceName; set => SetProperty(ref _instanceName, value); }
        private string _status = string.Empty; public string Status { get => _status; set => SetProperty(ref _status, value); }
        public AsyncCommand Create { get; }

        public CreateServerDialogViewModel(IProvisioningService provision, IServerService servers, IBlueprintService blueprints, Blueprint bp)
        {
            _provision = provision; _servers = servers; _blueprints = blueprints; Blueprint = bp;
            Create = new AsyncCommand(DoCreateAsync, ()=> !string.IsNullOrWhiteSpace(InstanceName));
            _ = LoadVarsAsync();
        }
        private async Task LoadVarsAsync()
        {
            var names = await _blueprints.GetTemplateVariablesAsync(Blueprint);
            foreach (var n in names.Distinct()) Variables.Add(new TemplateVariableVM{ Name = n, Value = string.Empty });
        }
        private async Task DoCreateAsync()
        {
            Status = "Provisioning...";
            var dict = Variables.ToDictionary(v => v.Name, v => v.Value);
            dict["SERVER_NAME"] = InstanceName;
            var inst = await _provision.ProvisionServerAsync(Blueprint, InstanceName, dict, new System.Progress<string>(p => Status = p));
            await _servers.AddAsync(inst);
            Status = "Done";
        }
    }
}
