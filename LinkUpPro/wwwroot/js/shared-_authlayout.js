lucide.createIcons();
        
        function updateThemeIcons(isDark) {
            document.querySelectorAll('.theme-icon-sun').forEach(el => el.style.display = isDark ? 'inline-flex' : 'none');
            document.querySelectorAll('.theme-icon-moon').forEach(el => el.style.display = isDark ? 'none' : 'inline-flex');
        }

        // Theme Toggle Logic
        const html = document.documentElement;
        
        // Check local storage or system preference
        const isCurrentlyDark = localStorage.getItem('theme') === 'dark' || (!localStorage.getItem('theme') && window.matchMedia('(prefers-color-scheme: dark)').matches);
        if (!isCurrentlyDark) {
            html.classList.remove('dark');
        } else {
            html.classList.add('dark');
        }
        
        function toggleTheme() {
            const isDark = html.classList.contains('dark');
            if (isDark) {
                html.classList.remove('dark');
                localStorage.setItem('theme', 'light');
                updateThemeIcons(false);
            } else {
                html.classList.add('dark');
                localStorage.setItem('theme', 'dark');
                updateThemeIcons(true);
            }
        }
        
        document.addEventListener('DOMContentLoaded', () => {
            const isDark = html.classList.contains('dark');
            updateThemeIcons(isDark);
        });