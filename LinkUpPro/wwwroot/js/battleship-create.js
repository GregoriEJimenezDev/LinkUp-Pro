document.addEventListener('DOMContentLoaded', () => {
            const searchInput = document.getElementById('friendSearch');
            if (!searchInput) return;

            searchInput.addEventListener('input', function(e) {
                const term = e.target.value.toLowerCase();
                const items = document.querySelectorAll('.friend-item');
                let visibleCount = 0;

                items.forEach(item => {
                    const name = item.querySelector('.friend-name').textContent.toLowerCase();
                    const username = item.querySelector('.friend-username').textContent.toLowerCase();
                    
                    if (name.includes(term) || username.includes(term)) {
                        item.style.display = 'flex';
                        visibleCount++;
                    } else {
                        item.style.display = 'none';
                    }
                });

                const noResults = document.getElementById('noResults');
                if (visibleCount === 0) {
                    noResults.style.display = 'flex';
                    noResults.classList.remove('hidden');
                } else {
                    noResults.style.display = 'none';
                    noResults.classList.add('hidden');
                }
            });
        });