const api = (p, opts={}) => fetch(p, Object.assign({ headers: {"Content-Type":"application/json"} }, opts));
const $ = (id) => document.getElementById(id);

async function refresh() {
  const [games, servers] = await Promise.all([
    api("/api/games").then(r=>r.json()),
    api("/api/servers").then(r=>r.json())
  ]);

  const gs = $("games"); gs.innerHTML = "";
  for (const g of games) {
    const o = document.createElement("option"); o.value = g.id; o.textContent = g.label; gs.appendChild(o);
  }

  const list = $("servers"); list.innerHTML = "";
  for (const s of servers) {
    const card = document.createElement("div"); card.className = "card";
    card.innerHTML = `
      <div class="row">
        <div><b>${s.name}</b> <small>(${s.game})</small></div>
        <div class="status ${s.status}">${s.status}</div>
      </div>
      <div class="row">
        <button data-act="install">Install/Update</button>
        <button data-act="start">${s.status === "running" ? "Restart" : "Start"}</button>
        <button data-act="stop">Stop</button>
        <button data-act="logs">Logs</button>
        <button data-act="delete" class="danger">Delete</button>
      </div>
      <pre class="logs" style="display:none;"></pre>
    `;

    card.querySelector('[data-act="install"]').onclick = async ()=>{
      await api(`/api/servers/${s.id}/install`, { method: "POST" });
      await refresh();
    };
    card.querySelector('[data-act="start"]').onclick = async ()=>{
      if (s.status === "running") {
        await api(`/api/servers/${s.id}/restart`, { method: "POST" });
      } else {
        await api(`/api/servers/${s.id}/start`, { method: "POST" });
      }
      await refresh();
    };
    card.querySelector('[data-act="stop"]').onclick = async ()=>{
      await api(`/api/servers/${s.id}/stop`, { method: "POST" });
      await refresh();
    };
    card.querySelector('[data-act="logs"]').onclick = async ()=>{
      const pre = card.querySelector(".logs");
      pre.style.display = (pre.style.display === "none" ? "block" : "none");
      if (pre.style.display === "block") {
        const txt = await api(`/api/servers/${s.id}/logs?tail=500`).then(r=>r.text());
        pre.textContent = txt;
      }
    };
    card.querySelector('[data-act="delete"]').onclick = async ()=>{
      if (confirm("Delete this server?")) {
        await api(`/api/servers/${s.id}`, { method: "DELETE" });
        await refresh();
      }
    };

    list.appendChild(card);
  }
}

$("createForm").addEventListener("submit", async (ev)=>{
  ev.preventDefault();
  const name = $("name").value.trim();
  const game = $("games").value;
  if (!name || !game) return;
  await api("/api/servers", { method: "POST", body: JSON.stringify({ name, game }) });
  $("name").value = "";
  await refresh();
});

window.addEventListener("load", refresh);
