# PulsePanel
**Windows‑native, provenance‑first, accessibility‑driven server management panel**
_No paywalls. No feature gating. Every capability unlocked for everyone._

---

## 🚀 **Overview**

PulsePanel is an **open‑source control center** for managing game servers and services across local and remote nodes.  
It’s built for **determinism, provenance, and accessibility** — so every action is logged, every build is reproducible, and every user can participate.

**Core values:**
- **Windows‑native**: No browser bloat, no Electron overhead.
- **Provenance‑first**: Every action, blueprint, and compliance check is logged with full history.
- **Accessibility‑driven**: Dyslexia‑friendly UI, clear workflows, and onboarding for all ages.
- **Open forever**: No premium tiers, no locked features.

---

## 🛠 **Key Features**

- **Blueprint Control Center**  
  Install, update, and validate servers from reusable `.blueprint.json` templates.
- **Remote Agent Execution**  
  Run blueprints on remote nodes via a lightweight agent.
- **Job Queue**  
  Queue, monitor, and cancel multiple jobs in parallel.
- **Compliance Dashboard**  
  Scan for drift, enforce rules, and auto‑heal when enabled.
- **Provenance History**  
  Replay past actions, audit changes, and export logs.
- **Self‑Healing**  
  Detect and correct configuration drift automatically.

---

## 🏗 **Architecture**

```
                ┌───────────────────────────────┐
                │        PulsePanel.App          │
                │  (Windows Desktop Control UI)  │
                └───────────────┬────────────────┘
                                │
                                │ HTTP (API Key Auth)
                                │
                ┌───────────────▼────────────────┐
                │       PulsePanel.Agent          │
                │ (Remote Execution Service)      │
                └───────────────┬────────────────┘
                                │
                                │ Executes Blueprints
                                │ & Compliance Checks
                                │
        ┌───────────────────────▼────────────────────────┐
        │           Target Game / Service Nodes           │
        │  (Local or Remote — Windows or Cross‑Platform)  │
        └───────────────────────┬────────────────────────┘
                                │
                                │ Provenance Logs
                                │ Compliance Reports
                                │
        ┌───────────────────────▼────────────────────────┐
        │         Provenance & Compliance Storage         │
        │ (JSON History, Replay, Audit, Self‑Healing)     │
        └────────────────────────────────────────────────┘
```

### **How to Read It**
- **PulsePanel.App** — Your control center.  
  Loads blueprints, queues jobs, monitors compliance, and sends commands.
- **PulsePanel.Agent** — Lightweight listener on each managed node.  
  Runs the blueprint steps and returns logs/results.
- **Target Nodes** — The actual servers or services you’re managing.  
  Could be game servers, web services, or any blueprint‑driven workload.
- **Provenance & Compliance Storage** — Central record of everything done.  
  Enables replay, audit, and drift correction.

| Component         | Purpose |
|-------------------|---------|
| **PulsePanel.App** | Windows desktop control center (UI + orchestration) |
| **PulsePanel.Agent** | Lightweight .NET service for remote execution |
| **Blueprints**     | JSON templates defining install/config/validate steps |
| **Compliance Rules** | Optional `.compliance.json` sidecars for drift detection |
| **Provenance Log** | JSON‑based action history for replay and audit |

---

## 📦 **Installation**

### **1. Desktop App**
```powershell
git clone https://github.com/YourOrg/PulsePanel.git
cd PulsePanel/src/PulsePanel.App
dotnet restore
dotnet publish -c Release -r win10-x64 --self-contained true
```
Run the published `.exe` from the `publish` folder.

### **2. Remote Agent**
```powershell
cd PulsePanel/src/PulsePanel.Agent
dotnet restore
dotnet publish -c Release -r win10-x64 --self-contained true
```
Deploy to target node and run as a service or scheduled task.

---

## ⚙ **Configuration**

### **Desktop App** (`appsettings.json`)
```json
{
  "Blueprints": { "Path": "Assets/Blueprints" },
  "Agent": { "ApiKey": "changeme-agent-key" },
  "Compliance": { "ScanIntervalMinutes": 60 }
}
```

### **Remote Agent** (`appsettings.json`)
```json
{
  "Agent": {
    "ApiKey": "changeme-agent-key",
    "ListenUrl": "http://0.0.0.0:5070"
  }
}
```

---

## 🖥 **First Run**

1. **Add Blueprints** to `Assets/Blueprints`.
2. **Run Agent** on remote nodes with matching API key.
3. **Add Nodes** in the Nodes page (name, URL, API key).
4. **Use Control Center** to run Install/Update/Validate.
5. **Monitor Jobs** in the Job Queue.
6. **Scan Compliance** and enable Self‑Healing if desired.

---

## 🔍 **Provenance & Compliance**

- Every action is logged with:
  - Timestamp
  - Actor
  - Blueprint ID
  - Parameters
  - Result
- Compliance scans compare current state to blueprint rules.
- Drift can be corrected manually or automatically.

---

## 🤝 **Contributing**

We welcome contributions that:
- Respect the **provenance‑first** workflow.
- Follow **sequenced, testable** feature delivery.
- Include **acceptance criteria** and **UI integration**.
- Preserve **accessibility** and **open licensing**.

---

## 📜 **License**

Custom license blending:
- **Creative Commons principles**
- **Attribution lock‑in**
- **Non‑commercial terms**
- **Software enforceability**

See [`LICENSE.md`](LICENSE.md) for full terms.

---

## 🗺 **Roadmap**

- [ ] Plugin marketplace with provenance checks
- [ ] Cross‑platform agent builds
- [ ] Advanced compliance rule editor
- [ ] Blueprint sharing hub

---

## 🆘 **Support**

- **Issues**: Use GitHub Issues for bugs and feature requests.
- **Security**: Report privately
- **Docs**: See `/docs` for blueprint and compliance rule guides.

---
