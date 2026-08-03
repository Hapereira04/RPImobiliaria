document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('[data-localizacao-cascata]').forEach((container) => {
        const distrito = container.querySelector('#DistritoId');
        const concelho = container.querySelector('#ConcelhoId');
        const freguesia = container.querySelector('#FreguesiaId');
        if (!distrito || !concelho || !freguesia) return;

        const reset = (select, placeholder) => {
            select.innerHTML = '';
            select.add(new Option(placeholder, ''));
            select.disabled = true;
        };

        const populate = async (select, url, placeholder) => {
            reset(select, placeholder);
            try {
                const response = await fetch(url);
                if (!response.ok) return;
                const items = await response.json();
                items.forEach((item) => select.add(new Option(item.nome, item.id)));
                select.disabled = false;
            } catch {
                // Mantém o campo indisponível enquanto não for possível obter a lista.
            }
        };

        distrito.addEventListener('change', async () => {
            reset(freguesia, 'Selecione a freguesia');
            if (!distrito.value) {
                reset(concelho, 'Selecione o concelho');
                return;
            }
            await populate(concelho, `/Imovels/ObterConcelhos?distritoId=${encodeURIComponent(distrito.value)}`, 'Selecione o concelho');
        });

        concelho.addEventListener('change', async () => {
            if (!concelho.value) {
                reset(freguesia, 'Selecione a freguesia');
                return;
            }
            await populate(freguesia, `/Imovels/ObterFreguesias?concelhoId=${encodeURIComponent(concelho.value)}`, 'Selecione a freguesia');
        });
    });
});
