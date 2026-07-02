const columns = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L'];
        
        function updateDirectionHint() {
            const select = document.getElementById('inputDir');
            const hint = select.options[select.selectedIndex].text.toLowerCase();
            document.getElementById('directionHint').textContent = hint;
            
            // Re-render preview si hay una celda seleccionada
            const r = document.getElementById('inputRow').value;
            const c = document.getElementById('inputCol').value;
            if (r !== "" && c !== "") {
                const btn = document.querySelector(`.board-cell[data-row="${r}"][data-col="${c}"]`);
                if (btn) selectCell(parseInt(r), parseInt(c), btn);
            }
        }
        
        function selectCell(row, col, btnElement) {
            // Quitar clase de seleccionado a todos
            document.querySelectorAll('.board-cell').forEach(el => {
                el.classList.remove('ring-2', 'ring-primary', 'ring-offset-2', 'bg-primary/20', 'border-primary');
            });
            
            // Añadir clase al seleccionado
            btnElement.classList.add('ring-2', 'ring-primary', 'ring-offset-2', 'bg-primary/20');
            
            // Actualizar inputs ocultos
            document.getElementById('inputRow').value = row;
            document.getElementById('inputCol').value = col;
            
            // Actualizar UI visual
            document.getElementById('selected-coords').textContent = columns[col] + (row + 1);
            document.getElementById('selected-coords').classList.add('text-primary', 'border-primary', 'bg-primary/10');
            document.getElementById('selected-coords').classList.remove('text-muted-foreground', 'border-dashed', 'bg-secondary/50');
            
            // Habilitar botón de envío
            document.getElementById('submitBtn').disabled = false;
        }