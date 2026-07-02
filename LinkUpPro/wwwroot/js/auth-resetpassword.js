function togglePassword(inputId, btn) {
            const input = document.getElementById(inputId);
            const icon = btn.querySelector('i');
            if (input.type === 'password') {
                input.type = 'text';
                icon.setAttribute('data-lucide', 'eye-off');
            } else {
                input.type = 'password';
                icon.setAttribute('data-lucide', 'eye');
            }
            lucide.createIcons();
        }

        function checkPasswordStrength(password) {
            let score = 0;
            if (password.length >= 8) score++;
            if (/[A-Z]/.test(password)) score++;
            if (/[0-9]/.test(password)) score++;
            if (/[^A-Za-z0-9]/.test(password)) score++;
            return score;
        }

        function updateStrengthBar(score) {
            const colors = ['bg-muted', 'bg-red-500', 'bg-orange-500', 'bg-yellow-500', 'bg-emerald-500'];
            const labels = ['', 'Débil', 'Regular', 'Buena', 'Fuerte'];
            const textColors = ['text-muted-foreground', 'text-red-500', 'text-orange-500', 'text-yellow-500', 'text-emerald-500'];

            for (let i = 1; i <= 4; i++) {
                const seg = document.getElementById('str-seg-' + i);
                seg.className = 'h-1.5 flex-1 rounded-full transition-colors duration-300 ' + (i <= score ? colors[score] : 'bg-muted');
            }

            const textEl = document.getElementById('strengthText');
            textEl.textContent = labels[score] || '';
            textEl.className = 'text-xs ' + (textColors[score] || 'text-muted-foreground');
        }

        document.getElementById('resetPassword').addEventListener('input', function () {
            const score = checkPasswordStrength(this.value);
            updateStrengthBar(score);
        });