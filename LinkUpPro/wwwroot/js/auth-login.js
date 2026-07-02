document.addEventListener("DOMContentLoaded", function() {
            const togglePasswordBtn = document.getElementById("togglePassword");
            const passwordInput = document.getElementById("Password");
            const eyeIcon = togglePasswordBtn.querySelector("i");
            
            togglePasswordBtn.addEventListener("click", function() {
                if (passwordInput.type === "password") {
                    passwordInput.type = "text";
                    eyeIcon.setAttribute("data-lucide", "eye-off");
                } else {
                    passwordInput.type = "password";
                    eyeIcon.setAttribute("data-lucide", "eye");
                }
                lucide.createIcons();
            });
        });

        function disableSubmit(form) {
            const btn = form.querySelector('button[type="submit"]');
            if (btn) {
                btn.disabled = true;
                const icon = btn.querySelector('i');
                if (icon) {
                    icon.setAttribute('data-lucide', 'loader-2');
                    icon.classList.add('animate-spin');
                    lucide.createIcons();
                }
            }
            return true;
        }