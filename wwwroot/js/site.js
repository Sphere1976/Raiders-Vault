(() => {
    const primaryNav = document.querySelector('[data-primary-nav]');
    const navToggle = document.querySelector('[data-nav-toggle]');
    if (primaryNav && navToggle) {
        navToggle.addEventListener('click', () => {
            const isOpen = primaryNav.classList.toggle('open');
            navToggle.setAttribute('aria-expanded', String(isOpen));
        });
    }

    const commandPanel = document.querySelector('[data-command-panel]');
    const commandSearch = document.querySelector('[data-command-search]');
    const commandItems = Array.from(document.querySelectorAll('[data-command-item]'));
    const commandEmpty = document.querySelector('[data-command-empty]');

    const setCommandPanel = isOpen => {
        if (!commandPanel) {
            return;
        }

        commandPanel.hidden = !isOpen;
        commandPanel.setAttribute('aria-hidden', String(!isOpen));
        document.body.classList.toggle('command-open', isOpen);

        if (isOpen) {
            commandSearch?.focus();
            commandSearch?.select();
        }
    };

    const filterCommands = () => {
        if (!commandSearch || !commandItems.length) {
            return;
        }

        const query = commandSearch.value.trim().toLowerCase();
        let visibleCount = 0;

        commandItems.forEach(item => {
            const haystack = `${item.textContent} ${item.dataset.keywords || ''}`.toLowerCase();
            const isMatch = !query || haystack.includes(query);
            item.hidden = !isMatch;
            if (isMatch) {
                visibleCount += 1;
            }
        });

        if (commandEmpty) {
            commandEmpty.hidden = visibleCount > 0;
        }
    };

    document.querySelectorAll('[data-command-open]').forEach(button => {
        button.addEventListener('click', () => setCommandPanel(true));
    });

    document.querySelectorAll('[data-command-close]').forEach(button => {
        button.addEventListener('click', () => setCommandPanel(false));
    });

    commandSearch?.addEventListener('input', filterCommands);

    document.addEventListener('keydown', event => {
        const key = event.key.toLowerCase();
        if ((event.ctrlKey || event.metaKey) && key === 'k') {
            event.preventDefault();
            setCommandPanel(true);
        }

        if (event.key === 'Escape' && commandPanel && !commandPanel.hidden) {
            setCommandPanel(false);
        }

        if (event.key === 'Enter' && commandPanel && !commandPanel.hidden && commandSearch === document.activeElement) {
            const firstVisibleItem = commandItems.find(item => !item.hidden);
            if (firstVisibleItem) {
                firstVisibleItem.click();
            }
        }
    });

    const scrollProgress = document.querySelector('[data-scroll-progress]');
    const scrollTopButtons = document.querySelectorAll('[data-scroll-top]');
    const updateScrollProgress = () => {
        if (!scrollProgress) {
            return;
        }

        const maxScroll = document.documentElement.scrollHeight - window.innerHeight;
        const percent = maxScroll > 0 ? Math.min(100, Math.max(0, (window.scrollY / maxScroll) * 100)) : 0;
        scrollProgress.style.setProperty('--scroll-progress', `${percent}%`);
    };

    window.addEventListener('scroll', updateScrollProgress, { passive: true });
    window.addEventListener('resize', updateScrollProgress);
    updateScrollProgress();

    scrollTopButtons.forEach(button => {
        button.addEventListener('click', () => {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    });

    const aiChat = document.querySelector('[data-ai-chat]');
    if (aiChat) {
        const form = aiChat.querySelector('[data-ai-form]');
        const input = aiChat.querySelector('[data-ai-input]');
        const thread = aiChat.querySelector('[data-ai-thread]');
        const status = aiChat.querySelector('[data-ai-status]');
        const token = aiChat.querySelector('input[name="__RequestVerificationToken"]')?.value;

        const appendMessage = (role, text, usedAi) => {
            if (!thread) {
                return;
            }

            const message = document.createElement('article');
            message.className = `assistant-message assistant-message-${role}`;

            const label = document.createElement('span');
            label.textContent = role === 'user' ? 'You' : (usedAi ? 'AI' : 'Vault');

            const body = document.createElement('p');
            body.textContent = text;

            message.append(label, body);
            thread.append(message);
            thread.scrollTop = thread.scrollHeight;
        };

        const setAiStatus = text => {
            if (status) {
                status.textContent = text;
            }
        };

        const sendPrompt = async prompt => {
            const message = prompt.trim();
            if (!message) {
                input?.focus();
                return;
            }

            appendMessage('user', message);
            if (input) {
                input.value = '';
                input.disabled = true;
            }

            form?.querySelector('button[type="submit"]')?.setAttribute('disabled', 'disabled');
            setAiStatus('Thinking through the vault data...');

            try {
                const response = await fetch('/Assistant/Ask', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': token || ''
                    },
                    body: JSON.stringify({
                        message,
                        page: document.body.dataset.pageController || 'Assistant'
                    })
                });

                const payload = await response.json();
                appendMessage('ai', payload.reply || 'I could not build a response for that prompt.', payload.usedAi === true);
                setAiStatus(payload.usedAi ? 'Answered with AI.' : 'Answered with local vault guidance.');

                if (Array.isArray(payload.suggestions)) {
                    aiChat.querySelectorAll('[data-ai-suggestion]').forEach((button, index) => {
                        if (payload.suggestions[index]) {
                            button.textContent = payload.suggestions[index];
                            button.dataset.aiSuggestion = payload.suggestions[index];
                        }
                    });
                }
            } catch {
                appendMessage('ai', 'The assistant could not be reached. Try again after the app reconnects.', false);
                setAiStatus('Connection issue.');
            } finally {
                if (input) {
                    input.disabled = false;
                    input.focus();
                }

                form?.querySelector('button[type="submit"]')?.removeAttribute('disabled');
            }
        };

        form?.addEventListener('submit', event => {
            event.preventDefault();
            sendPrompt(input?.value || '');
        });

        aiChat.querySelectorAll('[data-ai-suggestion]').forEach(button => {
            button.addEventListener('click', () => {
                const prompt = button.dataset.aiSuggestion || button.textContent || '';
                if (input) {
                    input.value = prompt;
                }
                sendPrompt(prompt);
            });
        });
    }

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

                if (card) {
                    card.dataset.itemCurrent = result.currentCount;
                    card.dataset.itemNeeded = result.needed;
                    card.dataset.itemPriorityValue = result.priority;

                    const target = Number.parseInt(card.dataset.itemTarget || '0', 10);
                    const progress = target <= 0 ? 100 : Math.min(100, Math.round((result.currentCount / target) * 100));
                    const progressBar = card.querySelector('.item-progress-rail span');

                    if (progressBar) {
                        progressBar.style.width = `${progress}%`;
                        progressBar.textContent = `${progress}%`;
                    }
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

    const detailOverlay = document.querySelector('[data-item-detail-overlay]');
    if (detailOverlay) {
        const detailPanel = detailOverlay.querySelector('.item-detail-panel');
        const closeButton = detailOverlay.querySelector('[data-item-detail-close]');
        const fields = {
            name: detailOverlay.querySelector('[data-detail-name]'),
            rarity: detailOverlay.querySelector('[data-detail-rarity]'),
            notes: detailOverlay.querySelector('[data-detail-notes]'),
            meter: detailOverlay.querySelector('[data-detail-meter]'),
            category: detailOverlay.querySelector('[data-detail-category]'),
            source: detailOverlay.querySelector('[data-detail-source]'),
            usedFor: detailOverlay.querySelector('[data-detail-used-for]'),
            stock: detailOverlay.querySelector('[data-detail-stock]'),
            priority: detailOverlay.querySelector('[data-detail-priority]'),
            value: detailOverlay.querySelector('[data-detail-value]')
        };
        let lastDetailTrigger = null;

        const setText = (element, value) => {
            if (element) {
                element.textContent = value || 'Not specified';
            }
        };

        const closeDetails = () => {
            detailOverlay.hidden = true;
            document.body.classList.remove('detail-open');
            lastDetailTrigger?.focus();
        };

        const openDetails = card => {
            lastDetailTrigger = card.querySelector('[data-item-detail]');
            const current = Number.parseInt(card.dataset.itemCurrent || '0', 10);
            const target = Number.parseInt(card.dataset.itemTarget || '0', 10);
            const needed = Number.parseInt(card.dataset.itemNeeded || '0', 10);
            const progress = target <= 0 ? 100 : Math.min(100, Math.round((current / target) * 100));
            const rarity = card.dataset.itemRarity || 'Common';

            setText(fields.name, card.dataset.itemName);
            setText(fields.rarity, rarity);
            setText(fields.notes, card.dataset.itemNotes);
            setText(fields.category, card.dataset.itemCategory);
            setText(fields.source, card.dataset.itemSource);
            setText(fields.usedFor, card.dataset.itemUsedFor);
            setText(fields.stock, `${current} of ${target} kept, ${needed} still needed`);
            setText(fields.priority, card.dataset.itemPriorityValue);
            setText(fields.value, `${card.dataset.itemSellValue || 0} per item`);

            fields.rarity?.classList.remove('common', 'rare', 'epic', 'legendary');
            fields.rarity?.classList.add(rarity.toLowerCase());

            if (fields.meter) {
                fields.meter.style.width = `${progress}%`;
                fields.meter.textContent = `${progress}%`;
            }

            detailOverlay.hidden = false;
            document.body.classList.add('detail-open');
            closeButton?.focus();
        };

        document.querySelectorAll('[data-item-detail]').forEach(button => {
            button.addEventListener('click', () => {
                const card = button.closest('[data-item-card]');
                if (card) {
                    openDetails(card);
                }
            });
        });

        closeButton?.addEventListener('click', closeDetails);
        detailOverlay.addEventListener('click', event => {
            if (!detailPanel?.contains(event.target)) {
                closeDetails();
            }
        });
        document.addEventListener('keydown', event => {
            if (event.key === 'Escape' && !detailOverlay.hidden) {
                closeDetails();
            }
        });
    }

    const databaseCards = Array.from(document.querySelectorAll('[data-item-card]'));
    const cardFilter = document.querySelector('[data-card-filter]');
    const priorityButtons = Array.from(document.querySelectorAll('[data-priority-filter]'));

    if (databaseCards.length && cardFilter) {
        let activePriority = '';

        const applyCardFilters = () => {
            const query = cardFilter.value.trim().toLowerCase();

            databaseCards.forEach(card => {
                const haystack = (card.dataset.filterText || '').toLowerCase();
                const priority = card.dataset.itemPriorityValue || '';
                const matchesQuery = !query || haystack.includes(query);
                const matchesPriority = !activePriority || priority === activePriority;

                card.hidden = !(matchesQuery && matchesPriority);
            });
        };

        cardFilter.addEventListener('input', applyCardFilters);
        priorityButtons.forEach(button => {
            button.addEventListener('click', () => {
                activePriority = button.dataset.priorityFilter || '';
                priorityButtons.forEach(item => item.classList.toggle('active', item === button && activePriority !== ''));
                applyCardFilters();
            });
        });
    }

    const compareDock = document.querySelector('[data-compare-dock]');
    const compareCount = document.querySelector('[data-compare-count]');
    const compareOverlay = document.querySelector('[data-compare-overlay]');
    const compareTable = document.querySelector('[data-compare-table]');
    const selectedCompareItems = new Map();

    if (compareDock && compareOverlay && compareTable) {
        const comparePanel = compareOverlay.querySelector('.compare-panel');

        const itemFromCard = card => ({
            id: card.dataset.itemCard,
            name: card.dataset.itemName || 'Unknown item',
            rarity: card.dataset.itemRarity || 'Common',
            category: card.dataset.itemCategory || 'Uncategorized',
            source: card.dataset.itemSource || 'Not specified',
            usedFor: card.dataset.itemUsedFor || 'Not specified',
            current: Number.parseInt(card.dataset.itemCurrent || '0', 10),
            target: Number.parseInt(card.dataset.itemTarget || '0', 10),
            needed: Number.parseInt(card.dataset.itemNeeded || '0', 10),
            priority: card.dataset.itemPriorityValue || 'Medium',
            value: Number.parseInt(card.dataset.itemSellValue || '0', 10)
        });

        const escapeHtml = value => String(value)
            .replaceAll('&', '&amp;')
            .replaceAll('<', '&lt;')
            .replaceAll('>', '&gt;')
            .replaceAll('"', '&quot;')
            .replaceAll("'", '&#039;');

        const renderCompare = () => {
            const items = Array.from(selectedCompareItems.values());
            compareDock.hidden = items.length === 0;

            if (compareCount) {
                compareCount.textContent = `${items.length} selected`;
            }

            compareTable.innerHTML = items.map(item => `
                <article class="compare-row">
                    <div>
                        <strong>${escapeHtml(item.name)}</strong>
                        <span>${escapeHtml(item.category)} // ${escapeHtml(item.rarity)}</span>
                    </div>
                    <div><small>Need</small><strong>${item.needed}</strong></div>
                    <div><small>Stock</small><strong>${item.current}/${item.target}</strong></div>
                    <div><small>Priority</small><strong>${escapeHtml(item.priority)}</strong></div>
                    <div><small>Source</small><span>${escapeHtml(item.source)}</span></div>
                    <div><small>Used For</small><span>${escapeHtml(item.usedFor)}</span></div>
                </article>
            `).join('');
        };

        document.querySelectorAll('[data-compare-item]').forEach(button => {
            button.addEventListener('click', () => {
                const card = button.closest('[data-item-card]');
                if (!card) {
                    return;
                }

                const item = itemFromCard(card);
                if (selectedCompareItems.has(item.id)) {
                    selectedCompareItems.delete(item.id);
                    card.classList.remove('is-compared');
                    button.textContent = 'Compare';
                } else {
                    selectedCompareItems.set(item.id, item);
                    card.classList.add('is-compared');
                    button.textContent = 'Selected';
                }

                renderCompare();
            });
        });

        document.querySelector('[data-open-compare]')?.addEventListener('click', () => {
            compareOverlay.hidden = false;
            document.body.classList.add('detail-open');
            compareOverlay.querySelector('[data-compare-close]')?.focus();
        });

        document.querySelector('[data-clear-compare]')?.addEventListener('click', () => {
            selectedCompareItems.clear();
            document.querySelectorAll('.database-card.is-compared').forEach(card => card.classList.remove('is-compared'));
            document.querySelectorAll('[data-compare-item]').forEach(button => {
                button.textContent = 'Compare';
            });
            renderCompare();
        });

        const closeCompare = () => {
            compareOverlay.hidden = true;
            document.body.classList.remove('detail-open');
        };

        compareOverlay.querySelector('[data-compare-close]')?.addEventListener('click', closeCompare);
        compareOverlay.addEventListener('click', event => {
            if (!comparePanel?.contains(event.target)) {
                closeCompare();
            }
        });
        document.addEventListener('keydown', event => {
            if (event.key === 'Escape' && !compareOverlay.hidden) {
                closeCompare();
            }
        });

        renderCompare();
    }

    const lootPlanList = document.querySelector('[data-loot-plan-list]');
    const lootPlanCount = document.querySelector('[data-loot-plan-count]');
    const lootPlanNeeded = document.querySelector('[data-loot-plan-needed]');
    const lootPlanValue = document.querySelector('[data-loot-plan-value]');
    const selectedLootPlanItems = new Map();

    if (lootPlanList) {
        const lootPlanKey = 'raiders-vault:database:loot-plan';

        const itemFromPlanCard = card => ({
            id: card.dataset.itemCard,
            name: card.dataset.itemName || 'Unknown item',
            rarity: card.dataset.itemRarity || 'Common',
            category: card.dataset.itemCategory || 'Uncategorized',
            source: card.dataset.itemSource || 'Not specified',
            usedFor: card.dataset.itemUsedFor || 'Not specified',
            current: Number.parseInt(card.dataset.itemCurrent || '0', 10),
            target: Number.parseInt(card.dataset.itemTarget || '0', 10),
            needed: Number.parseInt(card.dataset.itemNeeded || '0', 10),
            priority: card.dataset.itemPriorityValue || 'Medium',
            value: Number.parseInt(card.dataset.itemSellValue || '0', 10)
        });

        const escapeHtml = value => String(value)
            .replaceAll('&', '&amp;')
            .replaceAll('<', '&lt;')
            .replaceAll('>', '&gt;')
            .replaceAll('"', '&quot;')
            .replaceAll("'", '&#039;');

        const persistLootPlan = () => {
            localStorage.setItem(lootPlanKey, JSON.stringify(Array.from(selectedLootPlanItems.values())));
        };

        const renderLootPlan = () => {
            const items = Array.from(selectedLootPlanItems.values());
            const totalNeeded = items.reduce((sum, item) => sum + item.needed, 0);
            const totalValue = items.reduce((sum, item) => sum + (item.needed * item.value), 0);

            if (lootPlanCount) {
                lootPlanCount.textContent = items.length;
            }

            if (lootPlanNeeded) {
                lootPlanNeeded.textContent = totalNeeded;
            }

            if (lootPlanValue) {
                lootPlanValue.textContent = totalValue;
            }

            lootPlanList.innerHTML = items.length
                ? items.map(item => `
                    <article class="loot-plan-row">
                        <div>
                            <strong>${escapeHtml(item.name)}</strong>
                            <span>${escapeHtml(item.source)}</span>
                        </div>
                        <small>${item.needed} needed // ${escapeHtml(item.priority)}</small>
                    </article>
                `).join('')
                : '<p class="hint">Add items from cards to build a focused pre-run checklist.</p>';

            document.querySelectorAll('[data-add-loot-plan]').forEach(button => {
                const card = button.closest('[data-item-card]');
                const id = card?.dataset.itemCard;
                const selected = id && selectedLootPlanItems.has(id);
                button.textContent = selected ? 'Planned' : 'Add Plan';
                card?.classList.toggle('is-planned', Boolean(selected));
            });
        };

        const loadLootPlan = () => {
            try {
                const saved = JSON.parse(localStorage.getItem(lootPlanKey) || '[]');
                saved.forEach(item => {
                    if (item?.id) {
                        selectedLootPlanItems.set(String(item.id), item);
                    }
                });
            } catch {
                localStorage.removeItem(lootPlanKey);
            }
        };

        const csvEscape = value => `"${String(value).replaceAll('"', '""')}"`;

        const buildCsv = items => [
            ['Name', 'Rarity', 'Category', 'Source', 'Used For', 'Current', 'Target', 'Needed', 'Priority', 'Sell Value'].join(','),
            ...items.map(item => [
                item.name,
                item.rarity,
                item.category,
                item.source,
                item.usedFor,
                item.current,
                item.target,
                item.needed,
                item.priority,
                item.value
            ].map(csvEscape).join(','))
        ].join('\n');

        const copyText = async text => {
            if (navigator.clipboard?.writeText) {
                await navigator.clipboard.writeText(text);
                return;
            }

            const area = document.createElement('textarea');
            area.value = text;
            area.setAttribute('readonly', '');
            area.style.position = 'fixed';
            area.style.opacity = '0';
            document.body.append(area);
            area.select();
            document.execCommand('copy');
            area.remove();
        };

        document.querySelectorAll('[data-add-loot-plan]').forEach(button => {
            button.addEventListener('click', () => {
                const card = button.closest('[data-item-card]');
                if (!card) {
                    return;
                }

                const item = itemFromPlanCard(card);
                if (selectedLootPlanItems.has(item.id)) {
                    selectedLootPlanItems.delete(item.id);
                } else {
                    selectedLootPlanItems.set(item.id, item);
                }

                persistLootPlan();
                renderLootPlan();
            });
        });

        document.querySelector('[data-clear-loot-plan]')?.addEventListener('click', () => {
            selectedLootPlanItems.clear();
            persistLootPlan();
            renderLootPlan();
        });

        document.querySelector('[data-export-loot-plan]')?.addEventListener('click', () => {
            const items = Array.from(selectedLootPlanItems.values());
            if (!items.length) {
                return;
            }

            const blob = new Blob([buildCsv(items)], { type: 'text/csv;charset=utf-8' });
            const url = URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = 'raiders-vault-loot-plan.csv';
            document.body.append(link);
            link.click();
            link.remove();
            URL.revokeObjectURL(url);
        });

        document.querySelector('[data-copy-loot-plan]')?.addEventListener('click', async event => {
            const items = Array.from(selectedLootPlanItems.values());
            if (!items.length) {
                return;
            }

            const checklist = items
                .map(item => `- ${item.name}: need ${item.needed}, source ${item.source}, used for ${item.usedFor}`)
                .join('\n');

            await copyText(checklist);
            event.currentTarget.textContent = 'Copied';
            window.setTimeout(() => {
                event.currentTarget.textContent = 'Copy Checklist';
            }, 1400);
        });

        document.querySelector('[data-copy-route-plan]')?.addEventListener('click', async event => {
            const rows = Array.from(document.querySelectorAll('[data-route-plan] .source-cluster-card'))
                .map(card => card.innerText.trim())
                .filter(Boolean);

            if (!rows.length) {
                return;
            }

            await copyText(rows.join('\n\n'));
            event.currentTarget.textContent = 'Copied';
            window.setTimeout(() => {
                event.currentTarget.textContent = 'Copy Route Plan';
            }, 1400);
        });

        loadLootPlan();
        renderLootPlan();
    }
})();
