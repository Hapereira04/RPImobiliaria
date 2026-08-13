// Ativar a notificação automaticamente se ela existir na página
document.addEventListener("DOMContentLoaded", function () {
    var toastEl = document.getElementById('emailToast');
    if (toastEl) {
        var toast = new bootstrap.Toast(toastEl);
        toast.show();
    }
});

// Lógica de mudança de Tema (Claro / Escuro)
document.addEventListener("DOMContentLoaded", () => {
    const htmlEl = document.documentElement;
    const btnToggle = document.getElementById("btnThemeToggle");
    const iconToggle = document.getElementById("themeIcon");

    function getPreferredTheme() {
        const saved = localStorage.getItem("theme");
        if (saved) return saved;
        return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
    }

    function applyTheme(theme) {
        htmlEl.setAttribute("data-bs-theme", theme);
        localStorage.setItem("theme", theme);
        if (iconToggle) {
            if (theme === "dark") {
                iconToggle.className = "bi bi-sun-fill text-warning fs-5";
            } else {
                iconToggle.className = "bi bi-moon-stars-fill text-white fs-5";
            }
        }
    }

    applyTheme(getPreferredTheme());

    window.matchMedia("(prefers-color-scheme: dark)").addEventListener("change", (e) => {
        if (!localStorage.getItem("theme")) applyTheme(e.matches ? "dark" : "light");
    });

    if (btnToggle) {
        btnToggle.addEventListener("click", () => {
            const current = htmlEl.getAttribute("data-bs-theme");
            applyTheme(current === "dark" ? "light" : "dark");
        });
    }
});