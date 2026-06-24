// ===== BATTLESHIP PLACEMENT (preview & confirm) =====
(function () {
    const placementBoard = document.getElementById('placementBoard');
    if (!placementBoard) return;

    const cells = placementBoard.querySelectorAll('.cell:not(.occupied)');
    const selectedRow = document.getElementById('selectedRow');
    const selectedCol = document.getElementById('selectedCol');
    const selectedDirection = document.getElementById('selectedDirection');
    const selectedDisplay = document.getElementById('selectedCellDisplay');
    const placeBtn = document.getElementById('placeBtn');
    const shipSize = parseInt(document.querySelector('input[name="shipType"]')?.closest('form')?.querySelector('input[name="shipType"]')?.getAttribute('data-size') || '0');

    let previewCells = [];
    let currentRow = null;
    let currentCol = null;

    function getShipSize() {
        // Try to get from the model
        const sizeEl = document.querySelector('.display-6');
        if (sizeEl) {
            const match = sizeEl.textContent.match(/Size (\d+)/);
            if (match) return parseInt(match[1]);
        }
        return 0;
    }

    function clearPreview() {
        previewCells.forEach(function (el) {
            el.classList.remove('preview', 'preview-conflict');
        });
        previewCells = [];
    }

    function showPreview(row, col, direction) {
        clearPreview();
        if (!row || !col) return;

        const size = getShipSize();
        if (!size) return;

        let positions = [];
        let valid = true;

        for (let i = 0; i < size; i++) {
            let r = row, c = col;
            switch (direction) {
                case 'Up': r = row - i; break;
                case 'Down': r = row + i; break;
                case 'Left': c = col - i; break;
                case 'Right': c = col + i; break;
            }

            if (r < 1 || r > 12 || c < 1 || c > 12) {
                valid = false;
                break;
            }
            positions.push({ row: r, col: c });
        }

        if (!valid) return;

        positions.forEach(function (pos) {
            const cell = placementBoard.querySelector(
                '.board-row:nth-child(' + (pos.row) + ') .cell:nth-child(' + (pos.col + 1) + ')'
            );
            if (cell) {
                const isOccupied = cell.dataset.occupied === 'true';
                cell.classList.add(isOccupied ? 'preview-conflict' : 'preview');
                previewCells.push(cell);
            }
        });
    }

    // Cell click handler
    cells.forEach(function (cell) {
        cell.addEventListener('click', function () {
            const row = parseInt(this.dataset.row);
            const col = parseInt(this.dataset.col);
            const isOccupied = this.dataset.occupied === 'true';

            if (isOccupied) return;

            currentRow = row;
            currentCol = col;
            selectedRow.value = row;
            selectedCol.value = col;
            selectedDisplay.textContent = '(' + row + ', ' + col + ')';

            const direction = selectedDirection ? selectedDirection.value : 'Right';
            showPreview(row, col, direction);

            if (placeBtn) placeBtn.disabled = false;
        });
    });

    // Direction buttons
    document.querySelectorAll('.direction-btn').forEach(function (btn) {
        btn.addEventListener('click', function () {
            document.querySelectorAll('.direction-btn').forEach(function (b) {
                b.classList.remove('active');
            });
            this.classList.add('active');
            selectedDirection.value = this.dataset.direction;

            if (currentRow && currentCol) {
                showPreview(currentRow, currentCol, this.dataset.direction);
            }
        });
    });

    // ===== ATTACK BOARD =====
    const attackBoard = document.getElementById('attackBoard');
    if (!attackBoard) return;

    const attackCells = attackBoard.querySelectorAll('.cell:not(.hit):not(.miss)');
    const attackRow = document.getElementById('attackRow');
    const attackCol = document.getElementById('attackCol');
    const attackDisplay = document.getElementById('attackCellDisplay');
    const attackBtn = document.getElementById('attackBtn');

    attackCells.forEach(function (cell) {
        cell.addEventListener('click', function () {
            const isAttacked = this.dataset.attacked === 'true';
            if (isAttacked) return;

            attackRow.value = this.dataset.row;
            attackCol.value = this.dataset.col;
            attackDisplay.textContent = '(' + this.dataset.row + ', ' + this.dataset.col + ')';
            if (attackBtn) attackBtn.disabled = false;
        });
    });

})();
