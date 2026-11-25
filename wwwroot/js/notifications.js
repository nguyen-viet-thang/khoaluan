// Header notification bell for signed-in customers: dropdown panel + real-time updates over SignalR.
(function () {
    const button = document.getElementById('store-notifications');
    const panel = document.getElementById('store-notify-panel');
    if (!button || !panel || button.dataset.hasPanel !== 'true') {
        return;
    }

    const badge = document.getElementById('store-notifications-badge');
    const list = document.getElementById('store-notify-list');

    function setOpen(open) {
        panel.hidden = !open;
        button.setAttribute('aria-expanded', open ? 'true' : 'false');
    }

    button.addEventListener('click', (event) => {
        event.stopPropagation();
        setOpen(panel.hidden);
    });
    document.addEventListener('click', (event) => {
        if (!panel.hidden && !panel.contains(event.target)) setOpen(false);
    });
    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape') setOpen(false);
    });

    function bumpBadge() {
        const current = parseInt(badge.textContent, 10) || 0;
        const next = current + 1;
        badge.textContent = next > 99 ? '99+' : String(next);
        badge.hidden = false;
    }

    function addToList(notification) {
        document.getElementById('store-notify-empty')?.remove();
        const item = document.createElement('li');
        const link = document.createElement('a');
        link.className = 'store-notify__item is-unread';
        link.href = notification.openUrl;
        for (const [className, text] of [['store-notify__title', notification.title], ['store-notify__message', notification.message], ['store-notify__time', notification.createdAt]]) {
            if (!text) continue;
            const span = document.createElement('span');
            span.className = className;
            span.textContent = text; // textContent: never inject server text as HTML
            link.append(span);
        }
        item.append(link);
        list.prepend(item);
        while (list.children.length > 5) list.lastElementChild.remove();
    }

    function showToast(notification) {
        const toast = document.createElement('a');
        toast.className = 'store-notify__toast';
        toast.href = notification.openUrl;
        toast.setAttribute('role', 'status');
        const title = document.createElement('strong');
        title.textContent = notification.title;
        const message = document.createElement('span');
        message.textContent = notification.message || '';
        toast.append(title, message);
        document.body.append(toast);
        setTimeout(() => toast.classList.add('is-leaving'), 7000);
        setTimeout(() => toast.remove(), 7600);
    }

    if (typeof signalR === 'undefined' || !button.dataset.hubUrl) {
        return; // real time unavailable: the panel still works with server-rendered data
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(button.dataset.hubUrl)
        .withAutomaticReconnect()
        .build();

    connection.on('NotificationReceived', (notification) => {
        bumpBadge();
        addToList(notification);
        showToast(notification);
    });

    connection.start().catch(() => { /* offline or signed out: notifications still appear on next page load */ });
})();
