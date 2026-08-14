document.addEventListener('DOMContentLoaded', function () {
    const distrito = document.getElementById('filterDistrito');
    const concelho = document.getElementById('filterConcelho');
    const freguesia = document.getElementById('filterFreguesia');

    if (!distrito || !concelho || !freguesia) return;

    // Obtém as URLs dos atributos data ou assume os endpoints padrão do controller
    const concelhosUrl = distrito.dataset.url || '/Imovels/ObterConcelhos';
    const freguesiasUrl = concelho.dataset.url || '/Imovels/ObterFreguesias';

    function resetSelect(select, texto) {
        select.innerHTML = `<option value="">${texto}</option>`;
        select.disabled = true;
    }

    async function preencher(select, url, texto) {
        resetSelect(select, texto);
        try {
            const response = await fetch(url);
            if (!response.ok) return;
            const items = await response.json();
            items.forEach(item => select.add(new Option(item.nome, item.id)));
            select.disabled = false;
        } catch (e) {
            console.error("Erro ao carregar dados de localização:", e);
        }
    }

    distrito.addEventListener('change', async () => {
        resetSelect(freguesia, 'Todas as freguesias');
        if (!distrito.value) {
            resetSelect(concelho, 'Todos os concelhos');
            return;
        }
        await preencher(concelho, `${concelhosUrl}?distritoId=${encodeURIComponent(distrito.value)}`, 'Todos os concelhos');
    });

    concelho.addEventListener('change', async () => {
        if (!concelho.value) {
            resetSelect(freguesia, 'Todas as freguesias');
            return;
        }
        await preencher(freguesia, `${freguesiasUrl}?concelhoId=${encodeURIComponent(concelho.value)}`, 'Todas as freguesias');
    });
});