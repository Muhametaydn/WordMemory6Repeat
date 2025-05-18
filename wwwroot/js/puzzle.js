document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("guessForm");
    const board = document.getElementById("board");
    const msg = document.getElementById("msg");

    form.addEventListener("submit", async e => {
        e.preventDefault();

        const fd = new FormData(form);
        const res = await fetch("Puzzle/Guess", { method: "POST", body: fd });

        if (!res.ok) {
            msg.textContent = await res.text();
            return;
        }

        const d = await res.json();
        paintRow(d.row, fd.get("attempt").toUpperCase(), d.mask);

        if (d.finished) {
            msg.textContent = d.correct
                ? "Tebrikler 🎉"
                : `Bitti! Kelime: ${d.answer.toUpperCase()}`;
            form.remove();
        } else {
            form.reset();
            form.querySelector("input").focus();
        }
    });

    function paintRow(r, word, mask) {
        [...word].forEach((ch, i) => {
            const cell = board.querySelector(`.cell[data-row='${r}'][data-col='${i}']`);
            cell.textContent = ch;

            // 1) Önce eski renkleri temizleyelim (opsiyonel ama güvenli):
            cell.classList.remove("bg-success", "bg-warning", "bg-secondary", "text-white");

            // 2) Doğru sınıfları ayrı ayrı ekle
            if (mask[i] === 'g') {
                cell.classList.add("bg-success", "text-white");
            } else if (mask[i] === 'y') {
                cell.classList.add("bg-warning");
            } else {
                cell.classList.add("bg-secondary", "text-white");
            }
        });
    }
});
