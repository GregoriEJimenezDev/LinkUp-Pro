let isAttacking = false;
function attackCell(row, col) {
    if (isAttacking) return;
    isAttacking = true;
    document.getElementById('attackRow').value = row;
    document.getElementById('attackCol').value = col;
    document.getElementById('attackForm').submit();
}
        