function updateThemeIcons(isDark) {
        document.querySelectorAll('.theme-icon-sun').forEach(el => el.style.display = isDark ? 'inline-flex' : 'none');
        document.querySelectorAll('.theme-icon-moon').forEach(el => el.style.display = isDark ? 'none' : 'inline-flex');
    }

    function toggleTheme() {
        const root = document.documentElement;
        const isDark = root.classList.contains('dark');
        if (isDark) {
            root.classList.remove('dark');
            localStorage.setItem('theme', 'light');
            updateThemeIcons(false);
        } else {
            root.classList.add('dark');
            localStorage.setItem('theme', 'dark');
            updateThemeIcons(true);
        }
    }

    document.addEventListener('DOMContentLoaded', () => {
        const isDark = document.documentElement.classList.contains('dark') || localStorage.getItem('theme') === 'dark';
        updateThemeIcons(isDark);
    });