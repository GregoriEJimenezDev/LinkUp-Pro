// Image preview
        function previewFile() {
            const preview = document.getElementById('previewImage');
            const file = document.getElementById('File').files[0];
            const reader = new FileReader();

            reader.addEventListener("load", function () {
                preview.src = reader.result;
            }, false);

            if (file) {
                reader.readAsDataURL(file);
            }
        }

        // Toggle password visibility
        function togglePassword(fieldId, btn) {
            const input = document.getElementById(fieldId);
            if (!input) return;

            const isPassword = input.type === 'password';
            input.type = isPassword ? 'text' : 'password';

            const icon = btn.querySelector('i');
            if (icon) {
                icon.setAttribute('data-lucide', isPassword ? 'eye-off' : 'eye');
                lucide.createIcons();
            }
        }

        // Password strength checker
        function checkPasswordStrength(password) {
            const container = document.getElementById('strengthContainer');
            const bar1 = document.getElementById('bar1');
            const bar2 = document.getElementById('bar2');
            const bar3 = document.getElementById('bar3');
            const bar4 = document.getElementById('bar4');
            const text = document.getElementById('strengthText');
            
            // Adjust to 3 bars since we have 3 levels: Débil, Media, Fuerte
            if (bar4) bar4.style.display = 'none'; // We only need 3 bars for the visual
            
            const bars = [bar1, bar2, bar3];
            const resetColor = 'bg-muted';

            if (!password || password.length === 0) {
                container.classList.add('hidden');
                return;
            }

            container.classList.remove('hidden');

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

        // Re-initialize lucide icons after DOM updates
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }