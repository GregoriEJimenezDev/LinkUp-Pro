function disableSubmitButton(form) {
        if (window.jQuery && !$(form).valid()) {
            return;
        }
        const btn = form.querySelector('button[type="submit"]');
        if (btn) {
            btn.disabled = true;
            btn.classList.add('opacity-50', 'pointer-events-none');
            btn.innerHTML = 'Reenviando enlace... <i class="ml-2 size-4 animate-spin" data-lucide="loader-2"></i>';
            if (window.lucide) {
                lucide.createIcons();
            }
        }
    }