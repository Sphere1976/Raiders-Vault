(() => {
    const checklist = document.querySelector('[data-checklist]');
    if (checklist) {
        const key = `raiders-vault:${checklist.dataset.checklist}:state`;
        const boxes = Array.from(checklist.querySelectorAll('input[type="checkbox"]'));
        const saved = JSON.parse(localStorage.getItem(key) || '[]');

        boxes.forEach((box, index) => {
            box.checked = saved[index] === true;
            box.addEventListener('change', () => {
                localStorage.setItem(key, JSON.stringify(boxes.map(item => item.checked)));
            });
        });

        document.querySelector('[data-reset-checklist]')?.addEventListener('click', () => {
            boxes.forEach(box => box.checked = false);
            localStorage.removeItem(key);
        });
    }

    const mapBoard = document.querySelector('[data-map-board]');
    if (mapBoard) {
        const copy = {
            entry: ['Entry', 'Start quiet, confirm the active condition, and avoid committing to the center until the first container check is complete.', 'Low', 'Setup'],
            loot: ['Loot Core', 'Sweep the highest-density container cluster first, then cut away before the route gets loud.', 'Moderate', 'High'],
            blueprint: ['Blueprint Target', 'Prioritize wardrobes, containers, apartments, desks, lockers, and condition-specific pools before taking optional fights.', 'Medium', 'Very High'],
            objective: ['Objective Pocket', 'Complete the aligned quest step only after the kit and blueprint target are secured.', 'Medium', 'Medium'],
            extract: ['Extraction', 'Leave when the primary target is secured. Do not over-loot after the risk score rises.', 'Variable', 'Secure']
        };

        const title = mapBoard.querySelector('[data-zone-title]');
        const body = mapBoard.querySelector('[data-zone-copy]');
        const risk = mapBoard.querySelector('[data-zone-risk]');
        const reward = mapBoard.querySelector('[data-zone-reward]');

        mapBoard.querySelectorAll('[data-zone]').forEach(button => {
            button.addEventListener('click', () => {
                mapBoard.querySelectorAll('[data-zone]').forEach(item => item.classList.remove('active'));
                button.classList.add('active');
                const selected = copy[button.dataset.zone] || copy.entry;
                title.textContent = selected[0];
                body.textContent = selected[1];
                risk.textContent = selected[2];
                reward.textContent = selected[3];
            });
        });
    }

    const skillInspector = document.querySelector('[data-skill-inspector]');
    if (skillInspector) {
        const title = skillInspector.querySelector('[data-skill-title]');
        const copy = skillInspector.querySelector('[data-skill-copy]');
        const points = skillInspector.querySelector('[data-skill-points]');
        const requires = skillInspector.querySelector('[data-skill-requires]');

        document.querySelectorAll('[data-skill-node]').forEach(node => {
            node.addEventListener('click', () => {
                document.querySelectorAll('[data-skill-node]').forEach(item => item.classList.remove('selected'));
                node.classList.add('selected');
                title.textContent = node.dataset.title || 'Skill Node';
                copy.textContent = node.dataset.copy || 'No description available.';
                points.textContent = node.dataset.points || '--';
                requires.textContent = node.dataset.requires || '--';
            });
        });
    }

    document.querySelectorAll('[data-count-update]').forEach(form => {
        form.addEventListener('submit', async event => {
            event.preventDefault();

            const button = form.querySelector('button[type="submit"]');
            const status = form.querySelector('[data-save-status]');
            const card = form.closest('[data-item-card]');
            const formData = new FormData(form);

            if (button) {
                button.disabled = true;
                button.textContent = 'Saving';
            }

            if (status) {
                status.textContent = '';
                status.classList.remove('error');
            }

            try {
                const response = await fetch(form.action, {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                if (!response.ok) {
                    throw new Error('Save failed');
                }

                const result = await response.json();
                const needed = card?.querySelector('[data-item-needed]');
                const priority = card?.querySelector('[data-item-priority]');

                if (needed) {
                    needed.textContent = result.needed;
                }

                if (priority) {
                    priority.textContent = result.priority;
                }

                card?.classList.toggle('is-stocked', result.stocked === true);

                if (status) {
                    status.textContent = 'Saved';
                }
            } catch {
                if (status) {
                    status.textContent = 'Retry';
                    status.classList.add('error');
                }
            } finally {
                if (button) {
                    button.disabled = false;
                    button.textContent = 'Update';
                }
            }
        });
    });
})();
