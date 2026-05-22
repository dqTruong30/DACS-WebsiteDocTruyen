(function () {
    const body = document.body;
    const toggle = document.getElementById("themeToggle");

    if (!body.classList.contains("theme-public")) {
        return;
    }

    const storageKey = "hutechnovel-public-theme";
    const savedTheme = localStorage.getItem(storageKey);

    if (savedTheme === "light") {
        body.classList.add("theme-light");
    }

    if (toggle) {
        toggle.textContent = body.classList.contains("theme-light") ? "☀" : "◐";
        toggle.addEventListener("click", function () {
            body.classList.toggle("theme-light");
            const isLight = body.classList.contains("theme-light");
            localStorage.setItem(storageKey, isLight ? "light" : "dark");
            toggle.textContent = isLight ? "☀" : "◐";
        });
    }
    const searchForm = document.querySelector("[data-header-search]");
    const searchInput = searchForm?.querySelector("[data-header-search-input]");
    const suggestionsBox = searchForm?.querySelector("[data-header-search-suggestions]");
    const fallbackCover = "https://static.sangtacvietcdn.xyz/img/bookcover256.jpg";

    if (!searchForm || !searchInput || !suggestionsBox) {
        return;
    }

    let searchTimer = null;
    let activeController = null;

    function hideSuggestions() {
        suggestionsBox.hidden = true;
        suggestionsBox.innerHTML = "";
    }

    function createSuggestionItem(story) {
        const link = document.createElement("a");
        link.className = "header-search__suggestion";
        link.href = story.url || `/Truyen/ChiTiet/${story.id}`;

        const coverWrap = document.createElement("span");
        coverWrap.className = "header-search__suggestion-cover";

        const image = document.createElement("img");
        image.src = story.cover || fallbackCover;
        image.alt = story.title || "";
        image.loading = "lazy";
        image.onerror = () => {
            image.src = fallbackCover;
        };
        coverWrap.appendChild(image);

        const textWrap = document.createElement("span");
        const title = document.createElement("span");
        title.className = "header-search__suggestion-title";
        title.textContent = story.title || "Truyen khong ten";

        const meta = document.createElement("span");
        meta.className = "header-search__suggestion-meta";
        meta.textContent = story.author || "Dang cap nhat";

        textWrap.append(title, meta);
        link.append(coverWrap, textWrap);
        return link;
    }

    function showSuggestions(stories) {
        suggestionsBox.innerHTML = "";

        if (!stories.length) {
            hideSuggestions();
            return;
        }

        stories.forEach(story => suggestionsBox.appendChild(createSuggestionItem(story)));
        suggestionsBox.hidden = false;
    }

    async function fetchSuggestions(term) {
        activeController?.abort();
        activeController = new AbortController();

        const url = `/TimKiem/Suggestions?term=${encodeURIComponent(term)}`;
        const response = await fetch(url, {
            headers: { "X-Requested-With": "fetch" },
            signal: activeController.signal
        });

        if (!response.ok) {
            throw new Error("Suggestion request failed");
        }

        return response.json();
    }

    function queueSuggestions() {
        const term = searchInput.value.trim();
        clearTimeout(searchTimer);

        if (!term) {
            activeController?.abort();
            hideSuggestions();
            return;
        }

        searchTimer = setTimeout(async () => {
            try {
                const stories = await fetchSuggestions(term);
                if (searchInput.value.trim() === term) {
                    showSuggestions(stories);
                }
            } catch (error) {
                if (error.name !== "AbortError") {
                    hideSuggestions();
                }
            }
        }, 180);
    }

    searchInput.addEventListener("input", queueSuggestions);
    searchInput.addEventListener("focus", queueSuggestions);
    searchInput.addEventListener("keydown", event => {
        if (event.key === "Escape") {
            hideSuggestions();
            searchInput.blur();
        }
    });

    searchForm.addEventListener("submit", hideSuggestions);

    document.addEventListener("click", event => {
        if (!searchForm.contains(event.target)) {
            hideSuggestions();
        }
    });
})();
