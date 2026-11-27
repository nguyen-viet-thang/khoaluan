// Back-office helpers shared by the Admin and Staff layouts: fewer clicks, no double submits.
(function () {
    'use strict';
    if (window.Chart?.defaults?.font) {
        window.Chart.defaults.font.family = '"Nunito", system-ui, sans-serif';
    }


    // 1. Highlight the sidebar link of the current page (longest matching path wins).
    const path = window.location.pathname.toLowerCase().replace(/\/+$/, '');
    let best = null;
    let bestLength = -1;
    document.querySelectorAll('.admin-nav a[href]').forEach(link => {
        const href = new URL(link.href, window.location.origin).pathname.toLowerCase().replace(/\/+$/, '');
        const matches = path === href || path.startsWith(`${href}/`);
        if (matches && href.length > bestLength) {
            best = link;
            bestLength = href.length;
        }
    });
    best?.classList.add('active');
    best?.setAttribute('aria-current', 'page');

    // 2. "/" focuses the page's search box (unless the user is typing somewhere).
    document.addEventListener('keydown', event => {
        const target = event.target;
        const typing = target instanceof HTMLElement && (target.isContentEditable || /^(INPUT|TEXTAREA|SELECT)$/.test(target.tagName));
        if (event.key === '/' && !typing && !event.ctrlKey && !event.metaKey) {
            const search = document.querySelector('[data-hotkey="/"]') || document.querySelector('.admin-toolbar input[type="search"]');
            if (search) {
                event.preventDefault();
                search.focus();
                search.select();
            }
        }
    });

    // 3. Filter dropdowns apply immediately (no extra "Lọc" click).
    document.querySelectorAll('form.admin-toolbar select').forEach(select => {
        select.addEventListener('change', () => select.form?.requestSubmit());
    });

    // 4. Confirm prompts (data-confirm on a form or on the clicked submit button) and no double submits.
    document.addEventListener('submit', event => {
        const form = event.target;
        // Already cancelled (e.g. an inline onsubmit="return confirm(...)" answered "Cancel").
        if (!(form instanceof HTMLFormElement) || event.defaultPrevented) {
            return;
        }

        const submitter = event.submitter;
        const message = submitter?.dataset.confirm || form.dataset.confirm;
        if (message && !window.confirm(message)) {
            event.preventDefault();
            return;
        }

        if (form.method.toLowerCase() === 'post') {
            // Disable after the browser has captured the form data (a disabled button would drop its name/value).
            setTimeout(() => {
                const buttons = submitter ? [submitter] : form.querySelectorAll('button[type="submit"]');
                buttons.forEach(button => { button.disabled = true; button.classList.add('is-busy'); });
            }, 0);
        }
    });

    // Coming back with the browser's Back button restores the page from cache: make its buttons usable again.
    window.addEventListener('pageshow', () => {
        document.querySelectorAll('button.is-busy').forEach(button => { button.disabled = false; button.classList.remove('is-busy'); });
    });

    // 5. Select-all checkbox + bulk action bar.
    const checkAll = document.querySelector('[data-check-all]');
    const items = () => Array.from(document.querySelectorAll('[data-check-item]'));
    const bulkBar = document.querySelector('[data-bulk-bar]');
    const syncBulk = () => {
        const checked = items().filter(item => item.checked).length;
        if (bulkBar) {
            bulkBar.hidden = checked === 0;
            const counter = bulkBar.querySelector('[data-bulk-count]');
            if (counter) counter.textContent = String(checked);
        }
        if (checkAll) {
            checkAll.checked = checked > 0 && checked === items().length;
            checkAll.indeterminate = checked > 0 && checked < items().length;
        }
    };
    checkAll?.addEventListener('change', () => {
        items().forEach(item => { item.checked = checkAll.checked; });
        syncBulk();
    });
    document.addEventListener('change', event => {
        if (event.target instanceof HTMLElement && event.target.matches('[data-check-item]')) {
            syncBulk();
        }
    });
    syncBulk();

    // 6. Small UI actions: toggle a panel, fill a field from a chip, copy text.
    document.addEventListener('click', async event => {
        const element = event.target instanceof Element ? event.target.closest('[data-toggle-target],[data-fill-target],[data-copy]') : null;
        if (!element) {
            return;
        }

        if (element.dataset.toggleTarget) {
            const panel = document.querySelector(element.dataset.toggleTarget);
            if (panel) {
                panel.hidden = !panel.hidden;
                panel.querySelector('input,textarea')?.focus();
            }
        } else if (element.dataset.fillTarget) {
            const field = document.querySelector(element.dataset.fillTarget);
            if (field) {
                field.value = element.dataset.fillValue || '';
                field.focus();
            }
        } else if (element.dataset.copy) {
            try {
                await navigator.clipboard.writeText(element.dataset.copy);
                const original = element.innerHTML;
                element.innerHTML = '<i class="fa fa-check"></i>';
                setTimeout(() => { element.innerHTML = original; }, 1200);
            } catch {
                window.prompt('Sao chép nội dung:', element.dataset.copy);
            }
        }
    });

    // 7. Success messages fade out on their own; errors stay until the user acts.
    document.querySelectorAll('.admin-content > .alert-success').forEach(alert => {
        setTimeout(() => {
            alert.classList.add('is-leaving');
            setTimeout(() => alert.remove(), 400);
        }, 5000);
    });
})();
