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
            const container = document.getElementById('strengthContainer');
            const bar1 = document.getElementById('str-seg-1');
            const bar2 = document.getElementById('str-seg-2');
            const bar3 = document.getElementById('str-seg-3');
            const text = document.getElementById('strengthText');
            
            const bars = [bar1, bar2, bar3];
            const resetColor = 'bg-muted';

            if (!password || password.length === 0) {
                if(container) container.classList.add('hidden');
                return;
            }

            if(container) container.classList.remove('hidden');

            let score = 0;
            if (password.length >= 8) score++;
            if (/[A-Z]/.test(password)) score++;
            if (/[a-z]/.test(password)) score++;
            if (/[0-9]/.test(password)) score++;
            if (/[^A-Za-z0-9]/.test(password)) score++;

            let level = 0;
            let label = '';
            let color = '';

            // Débil: <= 2. Media: 3 o 4. Fuerte: 5
            if (score <= 2) {
                level = 1; label = 'Débil'; color = 'bg-red-500';
            } else if (score === 3 || score === 4) {
                level = 2; label = 'Media'; color = 'bg-orange-500';
            } else if (score === 5) {
                level = 3; label = 'Fuerte'; color = 'bg-emerald-500';
            }

            bars.forEach((bar, i) => {
                if(bar) {
                    bar.className = 'h-1 flex-1 rounded-full transition-colors duration-300 ' + (i < level ? color : resetColor);
                }
            });

            if (text) {
                text.textContent = label;
                text.className = 'text-[10px] font-medium ' + (
                    level === 1 ? 'text-red-500' :
                    level === 2 ? 'text-orange-500' :
                    'text-emerald-500'
                );
            }
        }

        document.getElementById('resetPassword').addEventListener('input', function () {
            checkPasswordStrength(this.value);
        });