document.addEventListener('DOMContentLoaded', () => {
    const root = document.getElementById('root');
    root.innerHTML = '<h1>PulsePanel</h1><div id="content">Loading...</div>';

    loadBlueprints();
});

async function loadBlueprints() {
    const contentDiv = document.getElementById('content');
    try {
        const response = await fetch('/api/blueprints');
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const result = await response.json();

        if (result.status === 'ok') {
            const blueprints = result.data;
            if (blueprints.length === 0) {
                contentDiv.innerHTML = '<h2>No blueprints found.</h2>';
                return;
            }

            let html = '<h2>Available Blueprints</h2><ul>';
            blueprints.forEach(bp => {
                html += `<li><strong>${bp.name}</strong> (v${bp.version}) - ${bp.description}</li>`;
            });
            html += '</ul>';
            contentDiv.innerHTML = html;
        } else {
            throw new Error(result.error?.message || 'Failed to load blueprints.');
        }
    } catch (error) {
        contentDiv.innerHTML = `<p style="color: red;">Error loading blueprints: ${error.message}</p>`;
    }
}
