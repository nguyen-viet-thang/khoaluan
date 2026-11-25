(function () {
    const searchBox = document.getElementById('search_input_box');
    const searchInput = document.getElementById('search_input');
    const searchResults = document.getElementById('search_results');
    const searchButton = document.getElementById('search');
    const closeSearch = document.getElementById('close_search');
    const clearSearch = document.getElementById('clear_search');
    const notificationsButton = document.getElementById('store-notifications');

    // Signed-in customers get a notification panel (see notifications.js); everyone else goes to the
    // notifications page (guests are sent to login first).
    if (notificationsButton && notificationsButton.dataset.hasPanel !== 'true' && notificationsButton.dataset.guestUrl) {
        notificationsButton.addEventListener('click', () => { window.location.href = notificationsButton.dataset.guestUrl; });
    }

    // Catalog sidebar: ticking a specification applies it right away.
    document.querySelectorAll('form[data-auto-submit]').forEach(form => {
        form.addEventListener('change', () => form.submit());
    });

    const searchForm = document.getElementById('search-form');
    const filtersBox = document.getElementById('search-filters');
    if (!searchBox || !searchInput || !searchResults || !searchButton || !closeSearch || !clearSearch || !searchForm || !filtersBox) {
        return;
    }

    const minPriceInput = document.getElementById('search-min-price');
    const maxPriceInput = document.getElementById('search-max-price');
    const advancedToggle = document.getElementById('search-advanced-toggle');
    const advancedBody = document.getElementById('search-advanced-body');
    let filtersLoadedFor = null;   // category id ('' = all) whose chips are on screen
    let requestId = 0;             // ignore out-of-order responses

    // Query string of everything chosen in the panel; empty values are left out to keep URLs short.
    const buildQuery = () => {
        const params = new URLSearchParams();
        for (const [key, value] of new FormData(searchForm)) {
            if (key !== 'search-price' && String(value).trim() !== '') {
                params.append(key, String(value).trim());
            }
        }
        return params;
    };

    const hasCriteria = (params) => [...params.keys()].length > 0;

    const loadFilters = async (categoryId) => {
        filtersLoadedFor = categoryId;
        const response = await fetch(`${searchForm.dataset.filtersUrl}?categoryId=${encodeURIComponent(categoryId)}`);
        if (!response.ok) {
            throw new Error('Filters failed');
        }
        // Only the latest category request may replace the chips.
        if (filtersLoadedFor === categoryId) {
            filtersBox.innerHTML = await response.text();
        }
    };

    const fetchSearchResults = _.debounce(async () => {
        const params = buildQuery();
        if (!hasCriteria(params)) {
            searchResults.innerHTML = '';
            searchResults.hidden = true;
            return;
        }

        const current = ++requestId;
        searchResults.hidden = false;
        searchResults.innerHTML = '<div class="search-empty">Đang tìm kiếm...</div>';
        try {
            const response = await fetch(`${searchForm.dataset.resultsUrl}?${params}`);
            if (!response.ok) {
                throw new Error('Search failed');
            }
            const html = await response.text();
            if (current === requestId) {
                searchResults.innerHTML = html;
            }
        } catch {
            if (current === requestId) {
                searchResults.innerHTML = '<div class="search-empty">Không thể tải kết quả tìm kiếm.</div>';
            }
        }
    }, 300);

    const openSearch = _.throttle(() => {
        searchBox.classList.add('open');
        searchInput.focus();
        if (filtersLoadedFor === null) {
            loadFilters('').catch(() => {
                filtersBox.innerHTML = '<div class="search-empty">Không thể tải bộ lọc.</div>';
            });
        }
    }, 200);

    const resetFilters = async () => {
        searchForm.querySelectorAll('#search-price input[value=""]').forEach(input => { input.checked = true; });
        minPriceInput.disabled = maxPriceInput.disabled = true;
        await loadFilters('');
    };

    const closeSearchPanel = _.throttle(() => {
        searchBox.classList.remove('open');
        searchInput.value = '';
        searchResults.hidden = true;
        searchResults.innerHTML = '';
    }, 200);

    searchForm.addEventListener('change', async (event) => {
        const target = event.target;
        if (target.name === 'categoryId') {
            // New category: its own brands and specifications (old spec choices no longer apply).
            filtersBox.querySelectorAll('input[name="spec"], input[name="brandId"]').forEach(input => { input.disabled = true; });
            try {
                await loadFilters(target.value);
            } catch {
                filtersBox.innerHTML = '<div class="search-empty">Không thể tải bộ lọc.</div>';
            }
        } else if (target.name === 'search-price') {
            const [min, max] = target.value.split('-');
            minPriceInput.value = min || '';
            maxPriceInput.value = max || '';
            minPriceInput.disabled = !min;
            maxPriceInput.disabled = !max;
        }
        fetchSearchResults();
    });

    // Enter: open the full catalog with the same criteria.
    searchForm.addEventListener('submit', (event) => {
        event.preventDefault();
        const params = buildQuery();
        window.location.href = `${searchForm.getAttribute('action')}${hasCriteria(params) ? `?${params}` : ''}`;
    });

    advancedToggle?.addEventListener('click', () => {
        const expanded = advancedToggle.getAttribute('aria-expanded') === 'true';
        advancedToggle.setAttribute('aria-expanded', String(!expanded));
        advancedBody.hidden = expanded;
    });

    document.getElementById('search-reset')?.addEventListener('click', async () => {
        try {
            await resetFilters();
        } catch {
            filtersBox.innerHTML = '<div class="search-empty">Không thể tải bộ lọc.</div>';
        }
        fetchSearchResults();
    });

    (searchButton.closest('button') ?? searchButton).addEventListener('click', openSearch);
    // Click on the dark backdrop (outside the panel) closes it.
    searchBox.addEventListener('click', (event) => {
        if (event.target === searchBox) {
            closeSearchPanel();
        }
    });
    closeSearch.addEventListener('click', closeSearchPanel);
    clearSearch.addEventListener('click', () => {
        searchInput.value = '';
        searchInput.focus();
        fetchSearchResults();
    });

    searchInput.addEventListener('input', fetchSearchResults);

    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape' && searchBox.classList.contains('open')) {
            closeSearchPanel();
        }
    });

    closeSearch.addEventListener('keydown', (event) => {
        if (event.key === 'Enter' || event.key === ' ') {
            event.preventDefault();
            closeSearchPanel();
        }
    });})();
